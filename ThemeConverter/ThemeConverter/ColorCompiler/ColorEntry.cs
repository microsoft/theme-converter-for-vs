// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Drawing;

namespace ThemeConverter.ColorCompiler
{
    internal class ColorEntry
    {
        public ColorEntry(ColorName name)
        {
            Name = name;
        }

        public ColorName Name { get; set; }

        public ColorTheme Theme { get; set; }

        public bool IsEmpty { get; set; }

        public Color Background { get; set; }

        public uint BackgroundSource { get; set; }

        public __VSCOLORTYPE BackgroundType { get; set; }

        public Color Foreground { get; set; }

        public uint ForegroundSource { get; set; }

        public __VSCOLORTYPE ForegroundType { get; set; }

        public static Color FromRgba(uint rgba)
        {
            byte alpha = (byte)(rgba >> 24);
            byte blue = (byte)(rgba >> 16);
            byte green = (byte)(rgba >> 8);
            byte red = (byte)rgba;
            return Color.FromArgb(alpha, red, green, blue);
        }
    }
}
