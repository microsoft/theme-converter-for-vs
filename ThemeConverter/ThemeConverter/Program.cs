namespace ThemeConverter
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using Mono.Options;
    using Newtonsoft.Json.Linq;
    using ThemeConverter.ColorCompiler;

#pragma warning disable IDE0051 // Remove unused private members
    internal sealed class Program
    {
        private static Guid DarkThemeId = new Guid("{1ded0138-47ce-435e-84ef-9ec1f439b749}");

        private static string ThemeName = string.Empty;
        private static Lazy<Dictionary<string, ColorKey[]>> ScopeMappings = new Lazy<Dictionary<string, ColorKey[]>>(ParseMapping.CreateScopeMapping());
        private static Lazy<Dictionary<string, string>> CategoryGuids = new Lazy<Dictionary<string, string>>(ParseMapping.CreateCategoryGuids());
        private static Lazy<Dictionary<string, string>> VSCTokenFallback = new Lazy<Dictionary<string, string>>(ParseMapping.CreateVSCTokenFallback());

        /// <summary>
        /// args[0]: path to the json
        /// args[1]: installation path to the target instance or null
        /// </summary>
        /// <param name="args"></param>
        static int Main(string[] args)
        {
            try
            {
                string inputPath = null, outputPath = null, targetVS = null;
                bool showHelp = false;

                var options = new OptionSet()
                {
                    {"i|input=", Resources.Input, i => inputPath = i },
                    {"o|output=", Resources.Output, o => outputPath = o },
                    {"t|targetVS=", Resources.TargetVS, t => targetVS = t },
                    {"h|help",  Resources.Help, h => showHelp = h != null},
                };

                options.Parse(args);

                if (showHelp)
                {
                    Console.WriteLine(Resources.HelpHeader);
                    options.WriteOptionDescriptions(Console.Out);
                    return 0;
                }

                // Check for duplicate mappings
                ParseMapping.CheckDuplicateMapping();

                if (inputPath == null || !IsValidPath(inputPath))
                {
                    throw new ApplicationException(Resources.InputNotExistException);
                }

                if (outputPath == null)
                {
                    outputPath = GetDirName(inputPath);
                }

                if (targetVS != null && !Directory.Exists(targetVS))
                {
                    throw new ApplicationException(Resources.TargetVSNotExistException);
                }


                Convert(inputPath, outputPath, targetVS);

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine(ex);
                return -1;
            }
        }

        private static string GetDirName(string path)
        {
            return Directory.Exists(path) ? path : Path.GetDirectoryName(path);
        }

        private static bool IsValidPath(string path)
        {
            return Directory.Exists(path) || File.Exists(path);
        }

        private static void ShowHelpText()
        {
            try
            {
                Console.WriteLine(Resources.HelpHeader);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void Convert(string sourcePath, string pkgdefOutputPath, string deployInstall)
        {
            if (Directory.Exists(sourcePath))
            {
                bool converted = false;
                foreach (var sourceFile in Directory.EnumerateFiles(sourcePath, "*.json"))
                {
                    ConvertFile(sourceFile, pkgdefOutputPath, deployInstall);
                    converted = true;
                }

                if (!converted)
                {
                    throw new ApplicationException(Resources.NoJSONFoundException);
                }
            }
            else
            {
                ConvertFile(sourcePath, pkgdefOutputPath, deployInstall);
            }

            if (!string.IsNullOrEmpty(deployInstall))
            {
                LaunchVS(deployInstall);
            }
        }

        /// <summary>
        /// Convert the theme file and patch the pkgdef to the target VS if specified.
        /// </summary>
        /// <param name="filePath">Theme file path</param>
        /// <param name="pkgdefOutputPath">Output path</param>
        /// <param name="deployInstall">Installation path to the taget VS</param>
        private static void ConvertFile(string filePath, string pkgdefOutputPath, string deployInstall)
        {
            Console.WriteLine($"Converting {filePath}");
            Console.WriteLine();

            string fileName = Path.GetFileName(filePath);
            int extensionStart = fileName.LastIndexOf(".");
            ThemeName = fileName.Substring(0, extensionStart);

            // Parse VS Code theme file.
            var text = File.ReadAllText(filePath);
            var jobject = JObject.Parse(text);
            var theme = jobject.ToObject<ThemeFileContract>();

            // Group colors by category.
            var colorCategories = GroupColorsByCategory(theme);

            // Compile VS theme.
            string pkgdefFilePath = CompileVsTheme(theme, colorCategories);

            // Deploy pkgdef to specified folder
            if (!string.IsNullOrEmpty(pkgdefOutputPath))
            {
                Directory.CreateDirectory(pkgdefOutputPath);
                File.Copy(pkgdefFilePath, Path.Combine(pkgdefOutputPath, $"{ThemeName}.pkgdef"), overwrite: true);
            }

            if (!string.IsNullOrEmpty(deployInstall))
            {
                File.Copy(pkgdefFilePath, Path.Combine(deployInstall, $@"Common7\IDE\CommonExtensions\Platform\{ThemeName}.pkgdef"), overwrite: true);
            }
        }

        #region Compile VS Theme

        /// <summary>
        /// Generate the pkgdef from the theme.
        /// </summary>
        /// <param name="theme">The theme object from the json file</param>
        /// <param name="colorCategories">Colors grouped by category</param>
        /// <returns>Path to the generated pkgdef</returns>
        private static string CompileVsTheme(
           ThemeFileContract theme,
           Dictionary<string, Dictionary<string, SettingsContract>> colorCategories)
        {
            using (TempFileCollection tempFileCollection = new TempFileCollection())
            {
                string tempThemeFile = tempFileCollection.AddExtension("vstheme");

                using (var writer = new StreamWriter(tempThemeFile))
                {
                    writer.WriteLine($"<Themes>");

                    Guid themeGuid = Guid.NewGuid();

                    if (theme.Type == "dark")
                    {
                        writer.WriteLine($"    <Theme Name=\"{ThemeName}\" GUID=\"{themeGuid:B}\" FallbackId=\"{DarkThemeId:B}\">");
                    }
                    else // light theme will fallback to VS light theme by default
                    {
                        writer.WriteLine($"    <Theme Name=\"{ThemeName}\" GUID=\"{themeGuid:B}\">");
                    }

                    foreach (var category in colorCategories)
                    {
                        writer.WriteLine($"        <Category Name=\"{category.Key}\" GUID=\"{CategoryGuids.Value[category.Key]}\">");

                        foreach (var color in category.Value)
                        {
                            if (color.Value.Foreground is not null || color.Value.Background is not null)
                            {
                                WriteColor(writer, color.Key, color.Value.Foreground, color.Value.Background);
                            }
                        }

                        writer.WriteLine($"        </Category>");
                    }

                    writer.WriteLine($"    </Theme>");
                    writer.WriteLine($"</Themes>");

                }

                // Compile the pkgdef
                XmlFileReader reader = new XmlFileReader(tempThemeFile);
                ColorManager manager = reader.ColorManager;

                string tempPkgdef = tempFileCollection.AddExtension("pkgdef", keepFile: true);
                FileWriter.SaveColorManagerToFile(manager, tempPkgdef, true);

                return tempPkgdef;
            }
        }

        private static void LaunchVS(string deployInstall)
        {
            string vsPath = Path.Combine(deployInstall, @"Common7\IDE\devenv.exe");
            var updateConfigProcess = Process.Start(vsPath, "/updateconfiguration");
            Console.WriteLine("Running UpdateConfiguration.");
            Console.WriteLine();
            updateConfigProcess.WaitForExit();
            if (updateConfigProcess.ExitCode != 0)
                throw new ApplicationException("Fatal error running devenv.exe /updateconfiguration");

            // Launch Visual Studio to the themes page.
            Console.WriteLine("Launching Visual Studio.");
            Process.Start(vsPath);
        }
        #endregion Compile VS Theme

        #region Translate VS Theme

        /// <summary>
        /// Method description.
        /// </summary>
        /// <param name="theme">Parameter description.</param>
        /// <returns>Return description.</returns>
        private static Dictionary<string, Dictionary<string, SettingsContract>> GroupColorsByCategory(ThemeFileContract theme)
        {
            // category -> colorKeyName => color value 
            var colorCategories = new Dictionary<string, Dictionary<string, SettingsContract>>();
            // category -> colorKeyName -> assigned by VSC token
            var assignBy = new Dictionary<string, Dictionary<string, string>>();

            Dictionary<string, bool> keyUsed = new Dictionary<string, bool>();
            foreach (string key in ScopeMappings.Value.Keys)
            {
                keyUsed.Add(key, false);
            }

            // Add the editor colors
            foreach (var ruleContract in theme.TokenColors)
            {
                foreach (var scopeName in ruleContract.ScopeNames)
                {
                    string[] scopes = scopeName.Split(',');
                    foreach (var scopeRaw in scopes)
                    {
                        var scope = scopeRaw.Trim();
                        foreach (string key in ScopeMappings.Value.Keys)
                        {
                            if (key.StartsWith(scope) && scope != "")
                            {
                                if (ScopeMappings.Value.TryGetValue(key, out var colorKeys))
                                {
                                    keyUsed[key] = true;
                                    AssignEditorColors(colorKeys, scope, ruleContract, ref colorCategories, ref assignBy);
                                }
                            }
                        }
                    }
                }
            }

            // for keys that were not used during hierarchical assigning, check if there's any fallback that we can use...
            foreach (string key in keyUsed.Keys)
            {
                if (!keyUsed[key])
                {
                    if (VSCTokenFallback.Value.TryGetValue(key, out var fallbackToken))
                    {
                        // if the fallback is foreground, assign it like a shell color
                        if (fallbackToken == "foreground" && theme.Colors.ContainsKey("foreground"))
                        {
                            if (ScopeMappings.Value.TryGetValue(key, out var colorKeys))
                            {
                                AssignShellColors(theme.Colors["foreground"], colorKeys, ref colorCategories);
                            }
                        }

                        foreach (var ruleContract in theme.TokenColors)
                        {
                            foreach (var scopeName in ruleContract.ScopeNames)
                            {
                                string[] scopes = scopeName.Split(',');
                                foreach (var scopeRaw in scopes)
                                {
                                    var scope = scopeRaw.Trim();

                                    if ((fallbackToken.StartsWith(scope) && scope != ""))
                                    {
                                        if (ScopeMappings.Value.TryGetValue(key, out var colorKeys))
                                        {
                                            AssignEditorColors(colorKeys, scope, ruleContract, ref colorCategories, ref assignBy);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // token => VS Opacity, background token
            Dictionary<string, (float, string)> editorOverlayTokens = new Dictionary<string, (float, string)>{{"editor.lineHighlightBorder",     (1.0f,  "editor.background") },
                                                                                                              {"editor.lineHighlightBackground", (0.25f, "editor.background") },
                                                                                                              {"editorBracketMatch.border",      (1.0f,  "editor.background") },
                                                                                                              {"editorBracketMatch.background",  (1.0f,  "editor.background") },
                                                                                                              {"minimapSlider.background",       (1.0f,  "minimap.background") } };

            // Add the shell colors
            foreach (var color in theme.Colors)
            {
                if (ScopeMappings.Value.TryGetValue(color.Key.Trim(), out var colorKeyList))
                {
                    if (!TryGetColorValue(theme, color.Key, out string colorValue))
                    {
                        continue;
                    }

                    // calculate the actual border color for editor overlay colors
                    if (editorOverlayTokens.ContainsKey(color.Key) && TryGetColorValue(theme, editorOverlayTokens[color.Key].Item2, out string backgroundColor))
                    {
                        colorValue = GetCompoundColor(colorValue, backgroundColor, editorOverlayTokens[color.Key].Item1);
                    }

                    AssignShellColors(colorValue, colorKeyList, ref colorCategories);
                }
            }

            return colorCategories;
        }

        private static bool TryGetColorValue(ThemeFileContract theme, string token, out string colorValue)
        {
            theme.Colors.TryGetValue(token, out colorValue);

            string key = token;

            while (colorValue == null)
            {
                if (VSCTokenFallback.Value.TryGetValue(key, out var fallbackToken))
                {
                    key = fallbackToken;
                    theme.Colors.TryGetValue(key, out colorValue);
                }
                else
                {
                    break;
                }
            }

            return colorValue != null;
        }

        /// <summary>
        /// Compute what is the compound color of 2 overlayed colors with transparency
        /// </summary>
        /// <param name="VSOpacity">What is the opacity that VS will use when displaying this color</param>
        /// <returns></returns>
        private static string GetCompoundColor(string overlayColor, string baseColor, float VSOpacity = 1)
        {
            overlayColor = ReviseColor(overlayColor);
            baseColor = ReviseColor(baseColor);
            float overlayA = (float)System.Convert.ToInt32(overlayColor.Substring(0, 2), 16) / 255;
            float overlayR = System.Convert.ToInt32(overlayColor.Substring(2, 2), 16);
            float overlayG = System.Convert.ToInt32(overlayColor.Substring(4, 2), 16);
            float overlayB = System.Convert.ToInt32(overlayColor.Substring(6, 2), 16);

            float baseA = (float)System.Convert.ToInt32(baseColor.Substring(0, 2), 16) / 255;
            float baseR = System.Convert.ToInt32(baseColor.Substring(2, 2), 16);
            float baseG = System.Convert.ToInt32(baseColor.Substring(4, 2), 16);
            float baseB = System.Convert.ToInt32(baseColor.Substring(6, 2), 16);

            float R = (overlayA / VSOpacity) * overlayR + (1 - overlayA / VSOpacity) * baseA * baseR;
            float G = (overlayA / VSOpacity) * overlayG + (1 - overlayA / VSOpacity) * baseA * baseG;
            float B = (overlayA / VSOpacity) * overlayB + (1 - overlayA / VSOpacity) * baseA * baseB;

            R = Math.Clamp(R, 0, 255);
            G = Math.Clamp(G, 0, 255);
            B = Math.Clamp(B, 0, 255);

            return $"{(int)R:X2}{(int)G:X2}{(int)B:X2}FF";
        }

        private static void AssignEditorColors(ColorKey[] colorKeys,
                                        string scope,
                                        RuleContract ruleContract,
                                        ref Dictionary<string, Dictionary<string, SettingsContract>> colorCategories,
                                        ref Dictionary<string, Dictionary<string, string>> assignBy)
        {
            foreach (var colorKey in colorKeys)
            {
                if (!colorCategories.TryGetValue(colorKey.CategoryName, out var rulesList))
                {
                    rulesList = new Dictionary<string, SettingsContract>();
                    colorCategories[colorKey.CategoryName] = rulesList;
                }

                if (!assignBy.TryGetValue(colorKey.CategoryName, out var assignList))
                {
                    assignList = new Dictionary<string, string>();
                    assignBy[colorKey.CategoryName] = assignList;
                }

                if (rulesList.ContainsKey(colorKey.KeyName))
                {
                    if (scope.StartsWith(assignList[colorKey.KeyName]) && ruleContract.Settings.Foreground != null)
                    {
                        rulesList[colorKey.KeyName] = ruleContract.Settings;
                        assignList[colorKey.KeyName] = scope;
                    }
                }
                else
                {
                    rulesList.Add(colorKey.KeyName, ruleContract.Settings);
                    assignList.Add(colorKey.KeyName, scope);
                }
            }
        }

        private static void AssignShellColors(string colorValue, ColorKey[] colorKeys, ref Dictionary<string, Dictionary<string, SettingsContract>> colorCategories)
        {
            foreach (var colorKey in colorKeys)
            {
                if (!colorCategories.TryGetValue(colorKey.CategoryName, out var rulesList))
                {
                    // token name to colors
                    rulesList = new Dictionary<string, SettingsContract>();
                    colorCategories[colorKey.CategoryName] = rulesList;
                }

                if (!rulesList.TryGetValue(colorKey.KeyName, out var colorSetting))
                {
                    colorSetting = new SettingsContract();
                    rulesList.Add(colorKey.KeyName, colorSetting);
                }

                if (colorKey.isBackground)
                {
                    colorSetting.Background = colorValue;
                }
                else
                {
                    colorSetting.Foreground = colorValue;
                }
            }
        }

        #endregion Translate VS Theme

        #region Write VS Theme

        private static void WriteColor(StreamWriter writer, string colorKeyName, string foregroundColor, string backgroundColor)
        {
            writer.WriteLine($"            <Color Name=\"{colorKeyName}\">");

            if (backgroundColor is not null)
            {
                writer.WriteLine($"                <Background Type=\"CT_RAW\" Source=\"{ReviseColor(backgroundColor)}\"/>");
            }

            if (foregroundColor is not null)
            {
                writer.WriteLine($"                <Foreground Type=\"CT_RAW\" Source=\"{ReviseColor(foregroundColor)}\"/>");
            }

            writer.WriteLine($"            </Color>");
        }

        private static string ReviseColor(string color)
        {
            var revisedColor = color.Trim('#');
            switch (revisedColor.Length)
            {
                case 3:
                    {
                        string r = revisedColor.Substring(0, 1);
                        string g = revisedColor.Substring(1, 1);
                        string b = revisedColor.Substring(2, 1);
                        revisedColor = string.Format("FF{0}{0}{1}{1}{2}{2}", r, g, b);
                        break;
                    }
                case 4:
                    {
                        string r = revisedColor.Substring(0, 1);
                        string g = revisedColor.Substring(1, 1);
                        string b = revisedColor.Substring(2, 1);
                        string a = revisedColor.Substring(3, 1);
                        revisedColor = string.Format("{0}{0}{1}{1}{2}{2}{3}{3}", a, r, g, b);
                        break;
                    }
                case 6:
                    {
                        revisedColor = $"FF{revisedColor}";
                        break;
                    }
                case 8:
                    {
                        // go from RRGGBBAA to AARRGGBB
                        revisedColor = string.Format("{0}{1}", revisedColor.Substring(6), revisedColor.Substring(0, 6));
                        break;
                    }
                default:
                    break;
            }
            return revisedColor;
        }

        #endregion Write VS Theme
    }

    internal sealed class ColorKey
    {
        public ColorKey(string categoryName, string keyName, string backgroundOrForeground)
        {
            this.CategoryName = categoryName;
            this.KeyName = keyName;
            this.Aspect = backgroundOrForeground;

            if (backgroundOrForeground.Equals("Background", StringComparison.OrdinalIgnoreCase))
            {
                isBackground = true;
            }
            else
            {
                isBackground = false;
            }
        }

        public string CategoryName { get; }

        public string KeyName { get; }

        public string Aspect { get; }

        public bool isBackground { get; }

        public override string ToString()
        {
            return this.CategoryName + "&" + this.KeyName + "&" + this.Aspect;
        }
    }
}
