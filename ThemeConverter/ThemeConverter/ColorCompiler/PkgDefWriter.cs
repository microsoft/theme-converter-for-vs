using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;


namespace ThemeConverter.ColorCompiler
{
    /// <summary>
    /// Writes a ColorManager out to a pkgdef file.
    /// </summary>
    public class PkgDefWriter
    {
        private readonly ColorManager _colorManager;
        public PkgDefWriter(ColorManager manager)
        {
            Validate.IsNotNull(manager, "manager");
            _colorManager = manager;
        }

        public void SaveThemeRegistration(string fileName)
        {
            string tempPath = Path.GetTempFileName();
            PkgDefFileReader reader = null;
            try
            {
                PkgDefItem item = new PkgDefItem();
                using (PkgDefFileWriter writer = new PkgDefFileWriter(tempPath, true))
                {
                    foreach (ColorTheme theme in _colorManager.Themes)
                    {
                        if (theme.IsBuiltInTheme)
                            continue;

                        item.SectionName = string.Format(CultureInfo.InvariantCulture, @"$RootKey$\Themes\{0:B}", theme.ThemeId);
                        item.ValueDataType = PkgDefValueType.PKGDEF_VALUE_STRING;

                        item.ValueName = "@";
                        item.ValueDataString = theme.Name;
                        writer.Write(item);

                        item.ValueName = "Name";
                        item.ValueDataString = theme.Name;
                        writer.Write(item);

                        if (theme.FallbackId != Guid.Empty)
                        {
                            item.ValueName = nameof(theme.FallbackId);
                            item.ValueDataString = theme.FallbackId.ToString("B");
                            writer.Write(item);
                        }
                    }
                }

                EnsureDirectoryExists(Path.GetDirectoryName(fileName));

                File.Copy(tempPath, fileName, overwrite: true);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
                File.Delete(tempPath);
            }
        }

        public void SaveToFile(string fileName)
        {
            string tempPath = Path.GetTempFileName();
            PkgDefFileReader reader = null;
            try
            {
                // PkgDefFileReader does not support zero-length or non-existant files,
                // so if the target file does not already exist, simply write the contents
                // of this new theme to the file without reading the file.
                FileInfo sourceFile = new FileInfo(fileName);
                if (sourceFile.Exists && sourceFile.Length > 0)
                {
                    reader = new PkgDefFileReader(fileName);
                }

                using (PkgDefFileWriter writer = new PkgDefFileWriter(tempPath, true))
                {
                    Dictionary<CategoryThemeKey, List<ColorEntry>> entries = new Dictionary<CategoryThemeKey, List<ColorEntry>>();

                    foreach (ColorEntry entry in _colorManager.Themes.SelectMany(t => t.Colors))
                    {
                        CategoryThemeKey key = new CategoryThemeKey(entry.Name.Category.Id, entry.Theme.ThemeId);
                        List<ColorEntry> keyEntries;
                        if (!entries.TryGetValue(key, out keyEntries))
                        {
                            keyEntries = new List<ColorEntry>();
                            entries[key] = keyEntries;
                        }

                        keyEntries.Add(entry);
                    }

                    PkgDefItem item = new PkgDefItem();
                    item.ValueDataBinary = new byte[1000000];

                    // There's only a reader if the target file already exists on disk
                    if (reader != null)
                    {
                        try
                        {
                            while (reader.Read(ref item))
                            {
                                if (item.ValueDataType == PkgDefItem.PkgDefValueType.Binary && item.ValueName == "Data")
                                {
                                    if (!PopulateThemeCategoryInfo(entries, ref item))
                                    {
                                        continue;
                                    }
                                }

                                writer.Write(item);
                            }
                        }
                        catch
                        {
                            // Eat any. Likely due to an error reading the existing file on disk.
                        }
                    }

                    foreach (KeyValuePair<CategoryThemeKey, List<ColorEntry>> unsavedSet in entries)
                    {
                        if (unsavedSet.Value.Any(e => !e.IsEmpty))
                        {
                            ColorCategory category = _colorManager.CategoryIndex[unsavedSet.Key.Category];
                            item.SectionName = string.Format(CultureInfo.InvariantCulture, @"$RootKey$\Themes\{0:B}\{1}", unsavedSet.Key.ThemeId, category.Name);
                            item.ValueName = null;
                            item.ValueDataType = PkgDefValueType.PKGDEF_VALUE_STRING;
                            writer.Write(item);

                            item.ValueName = "Data";
                            item.ValueDataType = PkgDefValueType.PKGDEF_VALUE_BINARY;
                            PopulateThemeCategoryInfo(ref item, category.Id, unsavedSet.Value);
                            writer.Write(item);
                        }
                    }
                }

                EnsureDirectoryExists(Path.GetDirectoryName(fileName));

                File.Copy(tempPath, fileName, overwrite: true);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
                File.Delete(tempPath);
            }
        }

        private void EnsureDirectoryExists(string directory)
        {
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private bool PopulateThemeCategoryInfo(Dictionary<CategoryThemeKey, List<ColorEntry>> unsavedEntries, ref PkgDefItem item)
        {
            Regex FindCategoryNameExpression = new Regex(@"\$RootKey\$\\Themes\\(?'name'[^\\]*)\\(?'categoryName'[^\\]*)", RegexOptions.Singleline);

            Match match = FindCategoryNameExpression.Match(item.SectionName);
            if (match.Success)
            {
                string categoryNameString = match.Groups["categoryName"].Value;
                Guid category = _colorManager.Categories.Where(c => c.Name == categoryNameString).Select(c => c.Id).First();

                string themeNameString = match.Groups["name"].Value;
                Guid themeName;
                if (Guid.TryParse(themeNameString, out themeName))
                {
                    CategoryThemeKey categoryThemeKey = new CategoryThemeKey(category, themeName);
                    List<ColorEntry> colorEntries;
                    if (unsavedEntries.TryGetValue(categoryThemeKey, out colorEntries))
                    {
                        PopulateThemeCategoryInfo(ref item, category, colorEntries);

                        unsavedEntries.Remove(categoryThemeKey);

                        return true;
                    }
                }
            }

            return false;
        }

        private void PopulateThemeCategoryInfo(ref PkgDefItem item, Guid category, List<ColorEntry> colorEntries)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (VersionedBinaryWriter versionedWriter = new VersionedBinaryWriter(stream))
                {
                    CategoryCollectionRecord record = new CategoryCollectionRecord();

                    CategoryRecord categoryRecord = new CategoryRecord(category);
                    record.Categories.Add(categoryRecord);


                    foreach (ColorEntry entry in colorEntries)
                    {
                        if (!entry.IsEmpty)
                        {
                            ColorRecord colorRecord = CreateColorRecord(entry);
                            categoryRecord.Colors.Add(colorRecord);
                        }
                    }

                    versionedWriter.WriteVersioned(PkgDefConstants.ExpectedVersion, (checkedWriter, version) =>
                    {
                        record.Write(checkedWriter);
                    });
                }

                byte[] newBytes = stream.ToArray();
                item.ValueDataBinaryLength = newBytes.Length;
                newBytes.CopyTo(item.ValueDataBinary, 0);
            }
        }

        private ColorRecord CreateColorRecord(ColorEntry entry)
        {
            ColorRecord colorRecord = new ColorRecord(entry.Name.Name);
            colorRecord.BackgroundType = entry.BackgroundType;
            colorRecord.Background = entry.BackgroundSource;
            colorRecord.ForegroundType = entry.ForegroundType;
            colorRecord.Foreground = entry.ForegroundSource;
            return colorRecord;
        }
    }
}
