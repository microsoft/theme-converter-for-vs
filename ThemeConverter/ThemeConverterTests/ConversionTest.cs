// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace ThemeConverterTests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using FluentAssertions;
    using ThemeConverter;
    using Xunit;

    public class ConversionTest
    {
        private const string DarkThemeFallback = "{1ded0138-47ce-435e-84ef-9ec1f439b749}";
        private const string LightThemeFallback = "{de3dbbcd-f642-433c-8353-8f1df4370aba}";

        /// <summary>
        /// The minimum set of categories that should be present in the case of
        /// a complete theme successful conversion.
        /// </summary>
        private static readonly string[] ExpectedCategoryNames =
        {
            "Text Editor Language Service Items",
            "Roslyn Text Editor MEF Items",
            "Text Editor Text Marker Items",
            "Cider",
            "CommonControls",
            "CommonDocument",
            "Diagnostics",
            "Environment",
            "Header",
            "IntelliTrace",
            "ManifestDesigner",
            "NewProjectDialog",
            "NotificationBubble",
            "PackageManifestEditor",
            "ProjectDesigner",
            "SharePointTools",
            "ThemedDialog",
            "TreeView",
            "UserNotifications",
            "VisualStudioInstaller",
            "VSSearch",
            "Find",
            "Output Window",
            "StartPage",
            "ThemedUtilityDialog",
            "Text Editor Text Manager Items",
            "WebClient Diagnostic Tools",
            "UserInformation",
            "InfoBar",
            "ClientDiagnosticsMemory",
            "CodeSenseControls",
            "GraphDocumentColors",
            "GraphicsDesigners",
            "InformationBadge",
            "Promotion",
            "TaskRunnerExplorerControls",
            "TeamExplorer",
            "WelcomeExperience",
            "ClientDiagnosticsTimeline",
            "SearchControl",
            "ACDCOverview",
            "Editor Tooltip",
            "CodeSense",
            "Command Window",
            "Find Results",
            "Immediate Window",
            "Locals",
            "Package Manager Console",
            "Performance Tips",
            "Watch",
            "ApplicationInsights",
            "ClientDiagnosticsTools",
            "DetailsView",
            "DiagnosticsHub",
            "NavigateTo",
            "VersionControl",
            "WorkItemEditor",
            "ProgressBar",
            "UnthemedDialog",
        };

        private static readonly Regex GuidRegex = new Regex("[({][a-fA-F0-9]{8}-([a-fA-F0-9]{4}-){3}[a-fA-F0-9]{12}[})]", RegexOptions.Compiled);
        private static readonly Regex DataRegex = new Regex("^\"Data\"=hex:[a-fA-F0-9]+(,[a-fA-F0-9]+)*", RegexOptions.Compiled);

        private static readonly string ResultsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "TestResults", DateTime.Now.ToString("yyyy-MM-dd-HHmmss"));
        private static readonly string ThemesFolderPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestFiles");

        [Fact]
        public void Incomplete_NoColors()
        {
            string pkgdefPath = ConvertTheme("Incomplete_NoColors.json");
            File.Exists(pkgdefPath).Should().BeTrue();

            string[] lines = File.ReadAllLines(pkgdefPath);
            ValidateGeneralThemeInformation(lines, "Incomplete_NoColors", LightThemeFallback);

            lines.Where(l => l.Contains("\"Data\"=hex:")).Should().BeEmpty();
        }

        [Fact]
        public void Incomplete_MissingCriticalColors()
        {
            string pkgdefPath = ConvertTheme("Incomplete_MissingCriticalColors.json");
            File.Exists(pkgdefPath).Should().BeTrue();

            string[] lines = File.ReadAllLines(pkgdefPath);
            ValidateGeneralThemeInformation(lines, "Incomplete_MissingCriticalColors", LightThemeFallback);

            lines.Where(l => l.Contains("\"Data\"=hex:")).Count().Should().Be(1);
        }

        [Fact]
        public void Complete_Dark()
        {
            ConvertAndValidateCompleteTheme("Complete_Dark.json", DarkThemeFallback);
        }

        [Fact]
        public void Complete_Light()
        {
            ConvertAndValidateCompleteTheme("Complete_Light.json", LightThemeFallback);
        }

        private static void ConvertAndValidateCompleteTheme(string testFileName, string themeFallbackGuid)
        {
            string pkgdefPath = ConvertTheme(testFileName);
            File.Exists(pkgdefPath).Should().BeTrue();

            string themeName = Path.GetFileNameWithoutExtension(testFileName);
            string[] lines = File.ReadAllLines(pkgdefPath);

            // We check for a limited set of things to be correct in order to keep the test low maintenance.
            // This unfortunately doesn't include verifying the actual colors in the pkgdef.
            string themeGuid = ValidateGeneralThemeInformation(lines, themeName, themeFallbackGuid);
            ValidateThemeCategories(lines, themeGuid, ExpectedCategoryNames);
        }

        private static string ValidateGeneralThemeInformation(string[] lines, string themeName, string themeFallbackGuid)
        {
            // Extract the theme guid from the first line so we can later search for lines that contain it
            Match m = GuidRegex.Match(lines[0]);
            m.Success.Should().BeTrue();
            string themeGuid = m.Groups[0].Value;

            // Check the general theme information
            lines[0].Should().Be($"[$RootKey$\\Themes\\{themeGuid}]");
            lines[1].Should().Be($"@=\"{themeName}\"");
            lines[2].Should().Be($"\"Name\"=\"{themeName}\"");
            lines[3].Should().Be($"\"FallbackId\"=\"{themeFallbackGuid}\"");

            return themeGuid;
        }

        private static void ValidateThemeCategories(string[] lines, string themeGuid, string[] categoryNames)
        {
            // Check that all expected categories are found
            foreach (string categoryName in categoryNames)
            {
                // Ensure category line is present
                string categoryLine = lines.SingleOrDefault(l => l == $"[$RootKey$\\Themes\\{themeGuid}\\{categoryName}]");
                categoryLine.Should().NotBeNull($"{categoryName} not found");
                int categoryLineIndex = Array.IndexOf(lines, categoryLine);

                // Next line is data line
                string dataLine = lines[categoryLineIndex + 1];
                Match m = DataRegex.Match(dataLine);
                m.Success.Should().BeTrue();
            }
        }

        private static string ConvertTheme(string testFileName)
        {
            return Converter.ConvertFile(Path.Combine(ThemesFolderPath, testFileName), ResultsFolderPath);
        }
    }
}
