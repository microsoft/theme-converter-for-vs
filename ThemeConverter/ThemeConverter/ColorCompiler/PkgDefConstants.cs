// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;

namespace ThemeConverter.ColorCompiler
{
    internal static class PkgDefConstants
    {
        public const int MaxBinaryBlobSize = 1000000;
        public const string DataValueName = "Data";
        public static readonly Regex FindThemeExpression = new Regex(@"\$RootKey\$\\Themes\\(?'name'[^\\]*)", RegexOptions.Singleline);
        public static readonly Regex FindCategoryNameExpression = new Regex(@"\$RootKey\$\\Themes\\(?'name'[^\\]*)\\(?'categoryName'[^\\]*)", RegexOptions.Singleline);
        public const int ExpectedVersion = 11;
    }
}
