using BepInEx.Configuration;
using UnityEngine;

namespace BoatStatusHUD
{
    public abstract class HUDPanel
    {
        protected ConfigEntry<bool> IsEnabled;

        protected readonly Color BackgroundColor = new Color(0.047f, 0.035f, 0.020f, 0.85f);

        protected readonly string ColorLabel = "#F5EEDCE0";

        protected readonly string ColorMuted = "#A19B8EE0";

        protected readonly string ColorDanger = "#ff3333";

        public static string FontName { get; set; } = "auto";

        public static int FontSize { get; set; } = 13;

        public abstract void BindConfig(ConfigFile config);

        public abstract void UpdateTelemetry(BoatDamage currentBoat);

        public abstract void DrawLines(GUIStyle defaultStyle, BoatDamage currentBoat);

        protected void DrawHUDLine(string text, GUIStyle style)
        {
            /* Enforce live layout overrides onto the style struct right before rendering the label */
            if (style != null)
            {
                style.fontSize = FontSize;
                style.richText = true;

                if (FontName.ToLower() == "auto")
                {
                    style.font = null; // Forces Unity to inherit Sailwind's beautiful native ledger font
                }
                else if (style.font == null || style.font.name != FontName)
                {
                    style.font = Font.CreateDynamicFontFromOSFont(FontName, FontSize);
                }
            }

            GUILayout.Label(text, style);
        }
    }
}