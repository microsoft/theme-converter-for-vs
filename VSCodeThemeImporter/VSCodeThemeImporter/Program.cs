namespace VSCodeThemeImporter
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using Newtonsoft.Json.Linq;

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
                Console.WriteLine("VSCodeThemeImporter v0.0.2");
                Console.WriteLine("A utility that converts VSCode theme json file(s) to VS pkgdef file(s).");
                Console.WriteLine("For Microsoft internal usage only.");
                Console.WriteLine();

                if (args.Length < 1)
                {
                    Console.WriteLine("Usage: VSCodeThemeImporter.exe <theme_json_file_path> <vs_install_dir>");
                    Console.WriteLine("       or");
                    Console.WriteLine("       VSCodeThemeImporter.exe <theme_json_folder_path> <pkgdef_out_dir>");
                    Console.WriteLine();
                    return -1;
                }

                // Check for duplicate mappings
                ParseMapping.CheckDuplicateMapping();

                if (Directory.Exists(args[0]))
                {
                    if (args.Length < 2)
                        throw new ApplicationException($"Specify pkgdef output folder.");

                    string pkgdefOutputPath = args[1];
                    Directory.CreateDirectory(pkgdefOutputPath);

                    foreach (var vscodeThemePath in Directory.EnumerateFiles("ImportedThemes", "*.json"))
                    {
                        Convert(vscodeThemePath, pkgdefOutputPath, null);
                    }
                }
                else if (File.Exists(args[0]))
                {
                    string filePath = args[0];

                    string deployInstall = args.Length > 1 ? args[1] : GetVsInstallPath();
                    if (!Directory.Exists(deployInstall))
                        throw new ApplicationException($"VS install dir does not exist: {deployInstall}");

                    Convert(filePath, null, deployInstall);
                }
                else
                {
                    throw new ApplicationException($"Specify a theme json file or a folder containing theme json files.");
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
            // Compile theme.
            var compilerPath = Path.Combine(deployInstall, @"VSSDK\VisualStudioIntegration\Tools\Bin\VsixColorCompiler.exe");
            var colorCompilerProcess = Process.Start(compilerPath, $"/registerTheme after.vstheme");
            colorCompilerProcess.WaitForExit();
            if (colorCompilerProcess.ExitCode != 0)
                throw new ApplicationException("Fatal error running VsixColorCompiler.exe");

            return "after.pkgdef";
        }

        private static void InstallThemeAndLaunch(string pkgdefFilePath, string deployInstall)
        {
            // NOTE: this wasn't working with the experimental instance so this is a version that works with the
            // install directory, so long as you run as ADMIN.

            // Deploy to Visual Studio.
            File.Copy(pkgdefFilePath, Path.Combine(deployInstall, $@"Common7\IDE\CommonExtensions\Platform\{ThemeName}.pkgdef"), overwrite: true);

            string vsPath = Path.Combine(deployInstall, @"Common7\IDE\devenv.exe");
            var updateConfigProcess = Process.Start(vsPath, "/updateconfiguration");
            updateConfigProcess.WaitForExit();
            if (updateConfigProcess.ExitCode != 0)
                throw new ApplicationException("Fatal error running devenv.exe /updateconfiguration");

            // Launch Visual Studio to the themes page.
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
            if (ThemeNameGuids.Value.TryGetValue(ThemeName, out string knownGuid))
            {
                return Guid.Parse(knownGuid);
            }
            else
            {
                Guid newThemeGuid = Guid.NewGuid();
                string guidString = newThemeGuid.ToString("B");
                string newGuidString = "{ffffffff" + guidString.Substring(guidString.IndexOf('-'));

                return Guid.Parse(newGuidString);
            }
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

            int totalToken = theme.TokenColors.Length;
            int usedVSCToken = 0;
            int scopeUsed = 0;

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
                                    scopeUsed = 1;
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
                            }
                        }

                        usedVSCToken += scopeUsed;
                    }
                }
            }

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
                            && fallbackToken != null)
                        {
                            colorValue = fallbackColor;
                        }
                        else
                        {
                            continue;
                        }
                    }


                    foreach (var colorKey in colorKeyList)
                    {
                        SettingsContract colorSetting;

                        if (!colorCategories.TryGetValue(colorKey.CategoryName, out var rulesList))
                        {
                            // token name to colors
                            rulesList = new Dictionary<string, SettingsContract>();
                            colorCategories[colorKey.CategoryName] = rulesList;
                        }

                        if (!rulesList.TryGetValue(colorKey.KeyName, out var existingSetting))
                        {
                            colorSetting = new SettingsContract();
                            rulesList.Add(colorKey.KeyName, colorSetting);
                        }
                        else
                        {
                            colorSetting = existingSetting;
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
            }

            //double transferRate = (double)usedVSCToken / (double)totalToken;
            //Console.WriteLine(transferRate);

            return colorCategories;
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
