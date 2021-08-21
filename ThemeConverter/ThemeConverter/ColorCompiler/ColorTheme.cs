using System;
using System.Collections.ObjectModel;
using System.Text;

namespace ThemeConverter.ColorCompiler
{
    public class ColorTheme
    {
        private ColorEntryCollection _colors;
        private Dictionary<ColorName, ColorEntry> _index;

        public ColorTheme(Guid themeId)
        {
            ThemeId = themeId;
        }

        public Guid ThemeId
        {
            get;
            private set;
        }

        public bool IsBuiltInTheme
        {
            get;
            set;
        }

        public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(ColorTheme));

        public string Name
        {
            get
            {
                return (string)GetValue(NameProperty);
            }
            set
            {
                SetValue(NameProperty, value);
            }
        }

        public static readonly DependencyProperty FallbackIdProperty = DependencyProperty.Register("FallbackId", typeof(Guid), typeof(ColorTheme));

        public Guid FallbackId
        {
            get
            {
                return (Guid)GetValue(FallbackIdProperty);
            }
            set
            {
                SetValue(FallbackIdProperty, value);
            }
        }

        public IList<ColorEntry> Colors
        {
            get
            {
                return _colors = _colors ?? new ColorEntryCollection(this);
            }
        }

        public IDictionary<ColorName, ColorEntry> Index
        {
            get
            {
                return _index = _index ?? new Dictionary<ColorName, ColorEntry>();
            }
        }

        class ColorEntryCollection : ObservableCollection<ColorEntry>
        {
            private ColorTheme _theme;

            public ColorEntryCollection(ColorTheme theme)
            {
                this._theme = theme;
            }
        }
    }
}
