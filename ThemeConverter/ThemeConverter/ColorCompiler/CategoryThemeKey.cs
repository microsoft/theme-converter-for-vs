using System;
using System.Collections.Generic;
using System.Text;

namespace ThemeConverter.ColorCompiler
{
    class CategoryThemeKey
    {
        public CategoryThemeKey(Guid category, Guid theme)
        {
            Category = category;
            ThemeId = theme;
        }

        public Guid Category { get; private set; }
        public Guid ThemeId { get; private set; }
    }
}
