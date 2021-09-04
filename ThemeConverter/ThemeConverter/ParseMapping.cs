﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections.Generic;

namespace ThemeConverter
{
    internal class ParseMapping
    {
        private static Dictionary<string, ColorKey[]> ScopeMappings = new Dictionary<string, ColorKey[]>();
        private static Dictionary<string, string> CategoryGuids = new Dictionary<string, string>();
        private static Dictionary<string, string> ThemeNameGuids = new Dictionary<string, string>();
        private static Dictionary<string, string> VSCTokenFallback = new Dictionary<string, string>();
        private static List<string> MappedVSTokens = new List<string>();

        public static void CheckDuplicateMapping()
        {
            var contents = System.IO.File.ReadAllText("TokenMappings.json");
            var file = JObject.Parse(contents);
            var colors = file["tokenColors"];

            var addedMappings = new List<string>();

            foreach (var color in colors)
            {
                var VSCToken = color["VSC Token"];
                string key = VSCToken.ToString();

                var VSTokens = color["VS Token"];

                foreach (var VSToken in VSTokens)
                {
                    if (addedMappings.Contains(VSToken.ToString()))
                    {
                        Console.WriteLine(key + ": " + VSToken.ToString());
                    }
                    else
                    {
                        addedMappings.Add(VSToken.ToString());
                    }
                }
            }

            Console.WriteLine();
        }

        public static Dictionary<string, ColorKey[]> CreateScopeMapping()
        {
            var contents = System.IO.File.ReadAllText("TokenMappings.json");

            // JObject.Parse will skip JSON comments by default
            var file = JObject.Parse(contents);

            var colors = file["tokenColors"];
            foreach (var color in colors)
            {
                var VSCToken = color["VSC Token"];
                string key = VSCToken.ToString();

                var VSTokens = color["VS Token"];
                List<ColorKey> values = new List<ColorKey>();
                foreach (var VSToken in VSTokens)
                {
                    string[] colorKey = VSToken.ToString()?.Split("&");
                    ColorKey newColorKey;
                    if (colorKey.Length > 2)
                    {
                        newColorKey = new ColorKey(colorKey[0], colorKey[1], colorKey[2]);
                    }
                    else
                    {
                        // if not specified, default to foreground
                        newColorKey = new ColorKey(colorKey[0], colorKey[1], "Foreground");
                    }
                    values.Add(newColorKey);
                    MappedVSTokens.Add(string.Format("{0}&{1}&{2}", newColorKey.CategoryName, newColorKey.KeyName, newColorKey.Aspect));
                }

                ScopeMappings.Add(key, values.ToArray());
            }

            CheckForMissingVSTokens();
           
            return ScopeMappings;
        }

        private static void CheckForMissingVSTokens()
        {
            if (MappedVSTokens.Count > 0)
            {
                var text = File.ReadAllText("VSTokens.json");
                var jobject = JArray.Parse(text);
                var availableTokens = jobject.ToObject<List<string>>();

                var missingVSTokens = new List<string>();

                foreach (var token in availableTokens)
                {
                    if (!MappedVSTokens.Contains(token))
                    {
                        missingVSTokens.Add(token);
                    }
                }

                string json = JsonConvert.SerializeObject(missingVSTokens, Formatting.Indented);
                File.WriteAllText("MissingVSTokens.json", json);
            }
        }

        public static Dictionary<string, string> CreateCategoryGuids()
        {
            var contents = System.IO.File.ReadAllText("CategoryGuid.json");
            var file = JsonConvert.DeserializeObject<JObject>(contents);

            foreach (var item in file)
            {
                CategoryGuids.Add(item.Key, item.Value.ToString());
            }

            return CategoryGuids;
        }

        public static Dictionary<string, string> CreateThemeNameGuids()
        {
            var contents = System.IO.File.ReadAllText("ThemeNameGuid.json");
            var file = JsonConvert.DeserializeObject<JObject>(contents);

            foreach (var item in file)
            {
                ThemeNameGuids.Add(item.Key, item.Value.ToString());
            }

            return ThemeNameGuids;
        }

        public static Dictionary<string, string> CreateVSCTokenFallback()
        {
            var contents = System.IO.File.ReadAllText("VSCTokenFallback.json");
            var file = JsonConvert.DeserializeObject<JObject>(contents);

            foreach (var item in file)
            {
                VSCTokenFallback.Add(item.Key, item.Value.ToString());
            }

            return VSCTokenFallback;
        }
    }
}
