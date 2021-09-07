namespace ThemeConverter
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using Newtonsoft.Json.Linq;
    using ThemeConverter.ColorCompiler;

#pragma warning disable IDE0051 // Remove unused private members
    internal sealed class Program
    {
        private const string BackgroundColorCategory = "Text Editor Text Manager Items";
        private const string TooltipCategory = "Editor Tooltip";
        private static Guid DarkThemeId = new Guid("{1ded0138-47ce-435e-84ef-9ec1f439b749}");

        private static string ThemeName = string.Empty;
        private static Lazy<Dictionary<string, ColorKey[]>> ScopeMappings = new Lazy<Dictionary<string, ColorKey[]>>(ParseMapping.CreateScopeMapping());
        private static Lazy<Dictionary<string, string>> CategoryGuids = new Lazy<Dictionary<string, string>>(ParseMapping.CreateCategoryGuids());
        private static Lazy<Dictionary<string, string>> ThemeNameGuids = new Lazy<Dictionary<string, string>>(ParseMapping.CreateThemeNameGuids());
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
                if (args.Length < 1)
                {
                    ShowHelpText();
                    return -1;
                }

                // Check for duplicate mappings
                ParseMapping.CheckDuplicateMapping();

                switch (args[0])
                {
                    case "-patch":
                        if (args.Length != 3)
                        {
                            throw new ApplicationException("Invalid input, use '-help' to see sample usage.");
                        }

                        PatchTheme(args[1], args[2]);
                        break;
                    case "-convert":
                        if (args.Length < 2 || args.Length > 3)
                        {
                            throw new ApplicationException("Invalid input, use '-help' to see sample usage.");
                        }

                        string targetPath;
                        if (args.Length == 2)
                        {
                            targetPath = Directory.Exists(args[1]) ? args[1] : Path.GetDirectoryName(args[1]);
                        }
                        else
                        {
                            targetPath = args[2];
                        }

                        ConvertTheme(args[1], targetPath);
                        break;
                    default:
                        ShowHelpText();
                        break;
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine(ex);
                return -1;
            }
        }

        private static void ShowHelpText()
        {
            try
            {
                Console.WriteLine(Resources.HelpText);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void PatchTheme(string sourcePath, string deployInstall)
        {
            if (!Directory.Exists(deployInstall))
            {
                throw new ApplicationException($"VS install dir does not exist: {deployInstall}");
            }

            if (Directory.Exists(sourcePath))
            {
                foreach (var vscodeThemePath in Directory.EnumerateFiles(sourcePath, "*.json"))
                {
                    Convert(vscodeThemePath, null, deployInstall);
                }
            }
            else if (File.Exists(sourcePath))
            {
                Convert(sourcePath, null, deployInstall);
            }
            else
            {
                throw new ApplicationException($"Specify a theme json file or a folder containing theme json files.");
            }
        }

        private static void ConvertTheme(string sourcePath, string targetPath)
        {
            Directory.CreateDirectory(targetPath);

            if (Directory.Exists(sourcePath))
            {
                foreach (var vscodeThemePath in Directory.EnumerateFiles(sourcePath, "*.json"))
                {
                    Convert(vscodeThemePath, targetPath, null);
                }
            }
            else if (File.Exists(sourcePath))
            {
                Convert(sourcePath, targetPath, null);
            }
            else
            {
                throw new ApplicationException($"Specify a theme json file or a folder containing theme json files.");
            }
        }

        private static void Convert(string filePath, string pkgdefOutputPath, string deployInstall)
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

            // Write VS theme.
            TranslateVsTheme(theme, colorCategories);

            // Compile VS theme.
            var pkgdefFilePath = CompileTheme(deployInstall);

            // Deploy pkgdef to specified folder
            if (!string.IsNullOrEmpty(pkgdefOutputPath))
            {
                File.Copy(pkgdefFilePath, Path.Combine(pkgdefOutputPath, $"{ThemeName}.pkgdef"), overwrite: true);
            }

            // Deploy pkgdef to specified VS install dir
            if (!string.IsNullOrEmpty(deployInstall))
            {
                InstallThemeAndLaunch(pkgdefFilePath, deployInstall);
            }

            Console.WriteLine();
        }

        #region Compile VS Theme
        private static string CompileTheme(string deployInstall)
        {
            ColorManager manager = OpenXML("after.vstheme");

            FileWriter.SaveColorManagerToFile(manager, "after.pkgdef", true);

            //// Compile theme.
            //var compilerPath = Path.Combine(deployInstall, @"VSSDK\VisualStudioIntegration\Tools\Bin\VsixColorCompiler.exe");
            //var colorCompilerProcess = Process.Start(compilerPath, $"/registerTheme after.vstheme");
            //colorCompilerProcess.WaitForExit();
            //if (colorCompilerProcess.ExitCode != 0)
            //    throw new ApplicationException("Fatal error running VsixColorCompiler.exe");

            return "after.pkgdef";
        }

        static ColorManager OpenXML(string fileName)
        {
            XmlFileReader reader = new XmlFileReader(fileName);
            return reader.ColorManager;
        }

        private static void InstallThemeAndLaunch(string pkgdefFilePath, string deployInstall)
        {
            // NOTE: this wasn't working with the experimental instance so this is a version that works with the
            // install directory, so long as you run as ADMIN.

            // Deploy to Visual Studio.
            File.Copy(pkgdefFilePath, Path.Combine(deployInstall, $@"Common7\IDE\CommonExtensions\Platform\{ThemeName}.pkgdef"), overwrite: true);

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

        private static string GetVsInstallPath()
        {
            JToken token = GetVSWhereOutput();

            if (!token.HasValues)
                throw new ApplicationException("Unable to detect VS install dir, you should pass it in.");

            var installationPath = token[0]["installationPath"].ToString();
            return installationPath;
        }

        private static JToken GetVSWhereOutput()
        {
            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.FileName = @"C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe";
            process.StartInfo.Arguments = "-format json";
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            var output = process.StandardOutput.ReadToEnd();

            var token = JToken.Parse(output);
            return token;
        }

        #endregion Compile VS Theme

        #region Translate VS Theme

        private static void TranslateVsTheme(
            ThemeFileContract theme,
            Dictionary<string, Dictionary<string, SettingsContract>> colorCategories)
        {
            using var writer = new StreamWriter(File.Create("after.vstheme"));

            writer.WriteLine($"<Themes>");

            Guid themeGuid = GetThemeGuid();

            if (theme.Type == "dark")
            {
                writer.WriteLine($"    <Theme Name=\"{ThemeName}\" GUID=\"{themeGuid:B}\" FallbackId=\"{DarkThemeId:B}\">");
            }
            else // light theme will fallback to VS light theme by default
            {
                writer.WriteLine($"    <Theme Name=\"{ThemeName}\" GUID=\"{themeGuid:B}\">");
            }

            bool wroteMainEditorColors = false;
            int totalVSToken = 0;

            foreach (var category in colorCategories)
            {
                writer.WriteLine($"        <Category Name=\"{category.Key}\" GUID=\"{CategoryGuids.Value[category.Key]}\">");

                foreach (var color in category.Value)
                {
                    totalVSToken++;
                    if (color.Value.Foreground is not null || color.Value.Background is not null)
                    {
                        WriteColor(writer, color.Key, color.Value.Foreground, color.Value.Background);
                    }
                }

                // Write the editor background color.
                if (category.Key == BackgroundColorCategory)
                {
                    theme.Colors.TryGetValue("editor.foreground", out var foregroundColor);
                    theme.Colors.TryGetValue("editor.background", out var backgroundColor);

                    WriteColor(writer, "Plain Text", foregroundColor, backgroundColor);
                    wroteMainEditorColors = true;
                }

                writer.WriteLine($"        </Category>");
            }

            // We didn't encounter the background category, so write it now.
            if (!wroteMainEditorColors)
            {
                theme.Colors.TryGetValue("editor.background", out var backgroundColor2);
                theme.Colors.TryGetValue("editor.foreground", out var foregroundColor2);
                writer.WriteLine($"        <Category Name=\"{BackgroundColorCategory}\" GUID=\"{CategoryGuids.Value[BackgroundColorCategory]}\">");
                WriteColor(writer, "Plain Text", foregroundColor2, backgroundColor2);
                writer.WriteLine($"        </Category>");
                totalVSToken++;
            }

            //Console.WriteLine("VS: ", totalVSToken);
            writer.WriteLine($"    </Theme>");
            writer.WriteLine($"</Themes>");
        }

        private static Guid GetThemeGuid()
        {
            // if (ThemeNameGuids.Value.TryGetValue(ThemeName, out string knownGuid))
            // {
            //     return Guid.Parse(knownGuid);
            // }

            return Guid.NewGuid();
        }

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

            Dictionary<string, float> editorOverlayTokens = new Dictionary<string, float>{{"editor.lineHighlightBorder", 1.0f },
                                                                                          {"editor.lineHighlightBackground", 0.25f },
                                                                                          {"editorBracketMatch.border", 1.0f},
                                                                                          {"editorBracketMatch.background", 1.0f } };

            // Add the shell colors
            foreach (var color in theme.Colors)
            {
                if (ScopeMappings.Value.TryGetValue(color.Key.Trim(), out var colorKeyList))
                {
                    string colorValue = color.Value;

                    if (color.Value == null)
                    {
                        if (VSCTokenFallback.Value.TryGetValue(color.Key, out var fallbackToken)
                            && fallbackToken != null
                            && theme.Colors.TryGetValue(fallbackToken, out var fallbackColor)
                            && fallbackColor != null)
                        {
                            colorValue = fallbackColor;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    // calculate the actual border color for editor overlay colors
                    if (editorOverlayTokens.ContainsKey(color.Key) && theme.Colors.TryGetValue("editor.background", out string editorBackground))
                    {
                        colorValue = GetCompoundColor(colorValue, editorBackground, editorOverlayTokens[color.Key]);
                    }

                    AssignShellColors(colorValue, colorKeyList, ref colorCategories);
                }
            }

            return colorCategories;
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
