// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.IO;

namespace ThemeConverter.ColorCompiler
{
    internal abstract class FileWriter
    {
        private readonly ColorManager _colorManager;
        protected FileWriter(ColorManager manager)
        {
            _colorManager = manager;
        }

        protected ColorManager ColorManager { get { return _colorManager; } }

        public abstract void SaveToFile(string fileName);

        public static void SaveColorManagerToFile(ColorManager manager, string fileName, bool registerTheme = false)
        {
            string extension = Path.GetExtension(fileName);
            if (string.Equals(extension, ".xml", StringComparison.OrdinalIgnoreCase))
            {
                XmlFileWriter writer = new XmlFileWriter(manager);
                writer.SaveToFile(fileName);
            }
            else if (string.Equals(extension, ".pkgdef", StringComparison.OrdinalIgnoreCase))
            {
                PkgDefWriter writer = new PkgDefWriter(manager);
                writer.SaveToFile(fileName);
            }
            else
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, "Invalid file extension '{0}'. Only XML files and PKGDEF files are allowed.", extension));
            }
        }
    }
}