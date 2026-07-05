using BepInEx.Configuration;
using UnityEngine;

namespace BoatStatusHUD
{
    public abstract class HUDPanel
    {
        protected ConfigEntry<bool> IsEnabled;

        protected readonly Color BackgroundColor = new Color(0.29f, 0.35f, 0.41f, 0.80f);

        protected readonly string ColorLabel = "#bd7f2e";

        protected readonly string ColorMuted = "#515964";

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