using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace ThemeConverter.ColorCompiler
{
    public class ColorRow : DependencyObject
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

        public bool IsMatch(string text)
        {
            if (Name.Name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (Name.Category.Name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            foreach (ColorEntry entry in Manager.Themes.Select(theme => GetOrCreateEntry(theme)))
            {
                if (ColorEntry.Format(entry.BackgroundType, entry.BackgroundSource, entry.ForegroundType, entry.ForegroundSource).IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsMatch(string[] texts)
        {
            return texts.All(text => this.IsMatch(text));
        }
    }
}
