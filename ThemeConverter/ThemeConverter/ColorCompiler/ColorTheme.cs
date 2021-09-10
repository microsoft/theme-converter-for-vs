// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace ThemeConverter.ColorCompiler
{
    internal class ColorTheme
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

        public string Name { get; set; }

        public Guid FallbackId { get; set; }

        public ColorManager Manager { get; set; }

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

        class ColorEntryCollection : OwnershipCollection<ColorEntry>
        {
            private ColorTheme _theme;

            public ColorEntryCollection(ColorTheme theme)
            {
                this._theme = theme;
            }

            protected override void TakeOwnership(ColorEntry item)
            {
                if (item.Theme != null)
                {
                    throw new InvalidOperationException("Color entry can only belong to one theme");
                }

                item.Theme = _theme;
                _theme.Index[item.Name] = item;
            }

            protected override void LoseOwnership(ColorEntry item)
            {
                _theme.Index.Remove(item.Name);
                item.Theme = null;
            }
        }
    }
}
