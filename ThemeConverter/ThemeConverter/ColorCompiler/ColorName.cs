// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace ThemeConverter.ColorCompiler
{
    internal class ColorName
    {
        readonly ColorCategory _category;
        readonly string _name;

        public ColorName(ColorCategory category, string name)
        {
            _category = category;
            _name = name;
        }

        public ColorCategory Category
        {
            get
            {
                return _category;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public override bool Equals(object obj)
        {
            ColorName other = obj as ColorName;
            if (other == null)
            {
                return false;
            }

            return Object.Equals(Category, other.Category) && Object.Equals(Name, other.Name);
        }

        public override int GetHashCode()
        {
            return Name == null ? 0 : Name.GetHashCode();
        }
    }

}

