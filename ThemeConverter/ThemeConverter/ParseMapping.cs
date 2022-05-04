// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ThemeConverter
{
    internal class ParseMapping
    {
        private static Dictionary<string, ColorKey[]> ScopeMappings = new Dictionary<string, ColorKey[]>();
        private static Dictionary<string, string> CategoryGuids = new Dictionary<string, string>();
        private static Dictionary<string, string> VSCTokenFallback = new Dictionary<string, string>();
        private static Dictionary<string, (float, string)> OverlayMapping = new Dictionary<string, (float, string)>();
        private static List<string> MappedVSTokens = new List<string>();
        private const string TokenColorsName = "tokenColors";
        private const string VSCTokenName = "VSC Token";
        private const string VSTokenName = "VS Token";
        private const string TokenMappingFileName = "TokenMappings.json";

        public static void CheckDuplicateMapping(Action<string> reportFunc)
        {
            var contents = System.IO.File.ReadAllText(TokenMappingFileName);
            var file = JObject.Parse(contents);
            var colors = file[TokenColorsName];

            var addedMappings = new List<string>();

            foreach (var color in colors)
            {
                var VSCToken = color[VSCTokenName];
                string key = VSCToken.ToString();

                var VSTokens = color[VSTokenName];

                foreach (var VSToken in VSTokens)
                {
                    if (addedMappings.Contains(VSToken.ToString()))
                    {
                        reportFunc(key + ": " + VSToken.ToString());
                    }
                    else
                    {
                        addedMappings.Add(VSToken.ToString());
                    }
                }
            }
        }

        public static Dictionary<string, ColorKey[]> CreateScopeMapping()
        {
            var contents = System.IO.File.ReadAllText(TokenMappingFileName);

            // JObject.Parse will skip JSON comments by default
            var file = JObject.Parse(contents);

            var colors = file[TokenColorsName];
            foreach (var color in colors)
            {
                var VSCToken = color[VSCTokenName];
                string key = VSCToken.ToString();

                var VSTokens = color[VSTokenName];
                List<ColorKey> values = new List<ColorKey>();
                foreach (var VSToken in VSTokens)
                {
                    string[] colorKey = VSToken.ToString()?.Split("&");
                    ColorKey newColorKey;
                    switch(colorKey.Length)
                    {
                        case 2: // category & token name (by default foreground)
                            newColorKey = new ColorKey(colorKey[0], colorKey[1], "Foreground");
                            break;
                        case 3: // category & token name & aspect
                            newColorKey = new ColorKey(colorKey[0], colorKey[1], colorKey[2]);
                            break;
                        case 4: // category & token name & vsc opacity & vscode background
                            newColorKey = new ColorKey(colorKey[0], colorKey[1], "Foreground", colorKey[2], colorKey[3]);
                            break;
                        case 5: // category & token name & aspect & vsc opacity & vscode background
                            newColorKey = new ColorKey(colorKey[0], colorKey[1], colorKey[2], colorKey[3], colorKey[4]);
                            break;
                        default:
                            throw new Exception("Invalid mapping format");
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

        public static Dictionary<string, (float, string)> CreateOverlayMapping()
        {
            var contents = System.IO.File.ReadAllText("OverlayMapping.json");
            OverlayMapping = JsonConvert.DeserializeObject<Dictionary<string, (float, string)>>(contents);
            return OverlayMapping;
        }
    }
}
