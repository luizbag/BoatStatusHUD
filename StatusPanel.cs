using BepInEx.Configuration;
using UnityEngine;

namespace BoatStatusHUD
{
    public class StatusPanel : HUDPanel
    {
        private float _hullIntegrityPercent = 100f;
        private float _bilgeWaterPercent = 0f;
        private float _nextCheckTime = 0f;

        public StatusPanel() : base() { }

        public override void BindConfig(ConfigFile config)
        {
            IsEnabled = config.Bind("HUDSections", "ShowStatus", true, "Master switch for the Status panel.");
        }

        public override void UpdateTelemetry(BoatDamage currentBoat)
        {
            if (!IsEnabled.Value || currentBoat == null) return;

            if (Time.time >= _nextCheckTime)
            {
                _nextCheckTime = Time.time + 0.5f;

                /* Hull damage is 0 (clean) to 1 (destroyed). Invert for integrity. */
                _hullIntegrityPercent = (1f - currentBoat.hullDamage) * 100f;
                if (_hullIntegrityPercent < 0f) _hullIntegrityPercent = 0f;

                /* Water level is 0 to 1 inside the bilges */
                _bilgeWaterPercent = currentBoat.waterLevel * 100f;
            }
        }

        public override void DrawLines(GUIStyle defaultStyle, BoatDamage currentBoat)
        {
            if (!IsEnabled.Value || currentBoat == null) return;

            GUIStyle subCardStyle = new GUIStyle(GUI.skin.box);
            subCardStyle.normal.background = Utils.MakeTexture(2, 2, BackgroundColor);
            subCardStyle.margin = new RectOffset(0, 0, 0, 8);
            subCardStyle.padding = new RectOffset(12, 12, 12, 12);
            subCardStyle.stretchWidth = false;
            subCardStyle.stretchHeight = false;

            GUILayout.BeginVertical(subCardStyle);

            /* Hull Integrity Line with conditional styling for the label */
            string hullLabelColor = ColorLabel;
            if (_hullIntegrityPercent <= 30f) hullLabelColor = ColorDanger;
            else if (_hullIntegrityPercent <= 75f) hullLabelColor = "#bd7f2e";

            DrawHUDLine($"<color={hullLabelColor}>Hull Integrity: </color>{_hullIntegrityPercent:F1}%", defaultStyle);

            /* Bilge Water Line with conditional styling for the label */
            string bilgeLabelColor = ColorLabel;
            if (_bilgeWaterPercent >= 50f) bilgeLabelColor = ColorDanger;
            else if (_bilgeWaterPercent > 15f) bilgeLabelColor = "#bd7f2e";

            DrawHUDLine($"<color={bilgeLabelColor}>Bilge Water: </color>{_bilgeWaterPercent:F1}%", defaultStyle);

            GUILayout.EndVertical();
        }
    }
}