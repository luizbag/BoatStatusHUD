using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoatStatusHUD
{
    public static class Utils
    {
        private static Dictionary<string, ShipProfile> _profileCache = new Dictionary<string, ShipProfile>();

        public static Texture2D MakeTexture(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i) pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        public static void ParseProfilesString(string profilesRawData)
        {
            _profileCache.Clear();

            if (string.IsNullOrEmpty(profilesRawData)) return;

            string[] pairs = profilesRawData.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string pair in pairs)
            {
                string[] keyValue = pair.Split('=');
                if (keyValue.Length == 2)
                {
                    string key = keyValue[0].Trim();
                    if (float.TryParse(keyValue[1].Trim(), out float capacity))
                    {
                        float estimatedFreeboard = 1.0f + (capacity / 10000f);
                        if (estimatedFreeboard > 2.8f) estimatedFreeboard = 2.8f;

                        _profileCache[key.ToLower()] = new ShipProfile
                        {
                            Key = key,
                            DisplayName = key,
                            MaxCapacity = capacity,
                            MaxFreeboard = estimatedFreeboard
                        };
                    }
                }
            }
        }

        public static ShipProfile GetProfile(string gameObjectName)
        {
            if (string.IsNullOrEmpty(gameObjectName)) return null;

            string lowerName = gameObjectName.ToLower();
            foreach (var kvp in _profileCache)
            {
                if (lowerName.Contains(kvp.Key)) return kvp.Value;
            }
            return null;
        }
    }
}