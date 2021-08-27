using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace ThemeConverter.ColorCompiler
{
    public class ColorRow
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
