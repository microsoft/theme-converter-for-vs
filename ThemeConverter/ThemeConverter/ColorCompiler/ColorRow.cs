// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace ThemeConverter.ColorCompiler
{
    internal class ColorRow
    {
        public ColorRow(ColorManager manager, ColorName name)
        {
            Manager = manager;
            Name = name;
        }

        public ColorName Name
        {
            get;
            private set;
        }

        private ColorManager Manager
        {
            get;
            set;
        }

        public ColorEntry GetOrCreateEntry(ColorTheme theme)
        {
            return theme.Manager.GetOrCreateEntry(theme.ThemeId, Name);
        }
    }
}
