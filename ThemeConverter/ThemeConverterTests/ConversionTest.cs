// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace ThemeConverterTests
{
    using System;
    using System.IO;
    using System.Reflection;
    using FluentAssertions;
    using ThemeConverter;
    using Xunit;

    public class ConversionTest
    {
        private static readonly string ResultsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "TestResults", DateTime.Now.ToString("yyyy-dd-MM-HHmmss"));
        private static readonly string ThemesFolderPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestFiles");

        // TODO: this is bare minimum to get infrastructure going
        // - add a valid json theme
        // - validate contents of generated pkgdef file, at least partially
        //   - asserting exact file contents would make this impossible to maintain or trust
        //     but we could at least validate the .pkgdef header and the various keys for known categories are present
        // - converter is not really resilient when it comes to invalid files, sometimes wrong output without exception,
        //   sometimes NRE...

        [Fact]
        public void Invalid_NoColors()
        {
            Assert.Throws<NullReferenceException>(() =>
            {
                ConvertTheme("Invalid_NoColors.json");
            });
        }

        [Fact]
        public void Invalid_MissingCriticalColors()
        {
            string pkgdefPath = ConvertTheme("Invalid_MissingCriticalColors.json");
            File.Exists(pkgdefPath).Should().BeTrue();
        }

        private static string ConvertTheme(string testFileName)
        {
            return Converter.ConvertFile(Path.Combine(ThemesFolderPath, testFileName), ResultsFolderPath);
        }
    }
}
