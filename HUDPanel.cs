using System;
using BepInEx.Configuration;
using UnityEngine;

namespace BoatStatusHUD
{
    public abstract class HUDPanel
    {
        protected ConfigEntry<bool> IsEnabled;

        protected readonly Color BackgroundColor = new Color(0.047f, 0.035f, 0.020f, 0.30f);

        protected readonly string ColorLabel = "#F5EEDCE0";

        protected readonly string ColorMuted = "#A19B8EE0";

        protected readonly string ColorDanger = "#ff3333";

        public static Font Font { get; private set; }

        public static int FontSize { get; set; } = 14;

        protected HUDPanel()
        {
            Font = GetGameFont();
        }

        protected void BeginPanel()
        {
            GUIStyle subCardStyle = new GUIStyle(GUI.skin.box);
            subCardStyle.normal.background = Utils.MakeTexture(2, 2, BackgroundColor);
            subCardStyle.margin = new RectOffset(0, 0, 0, 8);
            subCardStyle.padding = new RectOffset(12, 12, 12, 12);

            /* ALLOW DYNAMIC EXPANSION: Allows the container to grow seamlessly to the right and down */
            subCardStyle.stretchWidth = true;
            subCardStyle.stretchHeight = true;

            /* If you want to define a comfortable minimum width so it doesn't look too squished initially */
            GUILayout.BeginVertical(subCardStyle, GUILayout.MinWidth(200), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        }

        protected void EndPanel()
        {
            GUILayout.EndVertical();
        }

        private Font GetGameFont()
        {
            Font gameFont = null;

            UnityEngine.Object[] fonts = Resources.FindObjectsOfTypeAll(typeof(Font));
            for (int i = 0; i < fonts.Length; i++)
            {
                Font f = fonts[i] as Font;
                if (f == null) continue;
                string n = f.name ?? "";
                if (n.IndexOf("Arial", StringComparison.OrdinalIgnoreCase) < 0 && n.Length > 0)
                {
                    gameFont = f;
                    break;
                }
            }
            return gameFont;
        }

        public abstract void BindConfig(ConfigFile config);

        public abstract void UpdateTelemetry(BoatDamage currentBoat);

        public abstract void DrawLines(GUIStyle defaultStyle, BoatDamage currentBoat);

        protected void DrawHUDLine(string text, GUIStyle style)
        {
            /* Enforce live layout overrides onto the style struct right before rendering the label */
            if (style != null)
            {
                style.fontSize = FontSize;
                style.font = Font;
                style.richText = true;
            }

            GUILayout.Label(text, style);
        }
    }
}