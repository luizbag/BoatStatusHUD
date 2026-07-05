using BepInEx.Configuration;
using UnityEngine;

namespace BoatStatusHUD
{
    public abstract class HUDPanel
    {
        protected ConfigEntry<bool> IsEnabled;

        protected readonly Color BackgroundColor = new Color(0.047f, 0.035f, 0.020f, 0.34f);

        protected readonly string ColorLabel = "#F5EEDCE0";

        protected readonly string ColorMuted = "#A19B8EE0";

        protected readonly string ColorDanger = "#ff3333";

        public abstract void BindConfig(ConfigFile config);

        public abstract void UpdateTelemetry(BoatDamage currentBoat);

        public abstract void DrawLines(GUIStyle defaultStyle, BoatDamage currentBoat);

        protected void DrawHUDLine(string text, GUIStyle style)
        {
            GUILayout.Label(text, style);
        }
    }
}