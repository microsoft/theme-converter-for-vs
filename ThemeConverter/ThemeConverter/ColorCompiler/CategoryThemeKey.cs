// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace ThemeConverter.ColorCompiler
{
    internal class CategoryThemeKey
    {
        public CategoryThemeKey(Guid category, Guid theme)
        {
            Category = category;
            ThemeId = theme;
        }

        public Guid Category { get; private set; }
        public Guid ThemeId { get; private set; }

        public override bool Equals(object obj)
        {
            CategoryThemeKey other = obj as CategoryThemeKey;
            if (other == null)
            {
                return false;
            }

            return Category == other.Category && ThemeId == other.ThemeId;
        }

        public override int GetHashCode()
        {
            return Category.GetHashCode() ^ ThemeId.GetHashCode();
        }
    }
}
