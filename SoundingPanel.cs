using BepInEx.Configuration;
using UnityEngine;

namespace BoatStatusHUD
{
    public class SoundingPanel : HUDPanel
    {
        private float _seaDepth = 300f;
        private float _keelClearance = 300f;
        private float _nextCheckTime = 0f;

        public SoundingPanel() : base() { }

        public override void BindConfig(ConfigFile config)
        {
            IsEnabled = config.Bind("HUDSections", "ShowSounding", true, "Master switch for the Sounding panel.");
        }

        public override void UpdateTelemetry(BoatDamage currentBoat)
        {
            if (!IsEnabled.Value || currentBoat == null) return;

            if (Time.time >= _nextCheckTime)
            {
                _nextCheckTime = Time.time + 0.5f;

                Vector3 origin = GameState.lastBoat.position;
                Vector3 direction = Vector3.down;
                float maxDistance = 300f;

                if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance))
                {
                    _seaDepth = hit.distance;

                    /* 1.5m baseline draft adjustment for standard hulls */
                    _keelClearance = _seaDepth - 1.5f;

                    if (_seaDepth < 0f) _seaDepth = 0f;
                    if (_keelClearance < 0f) _keelClearance = 0f;
                }
                else
                {
                    _seaDepth = 300f;
                    _keelClearance = 300f;
                }
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

            DrawHUDLine($"<color={ColorLabel}>Sea Depth: </color>{_seaDepth:F1}m", defaultStyle);

            if (_keelClearance <= 0.1f)
            {
                DrawHUDLine($"<color={ColorDanger}>Keel Clearance: </color><b>{_keelClearance:F1}m</b>", defaultStyle);
            }
            else if (_keelClearance <= 2.0f)
            {
                DrawHUDLine($"<color=#bd7f2e>Keel Clearance: </color><b>{_keelClearance:F1}m</b>", defaultStyle);
            }
            else
            {
                DrawHUDLine($"<color={ColorLabel}>Keel Clearance: </color>{_keelClearance:F1}m", defaultStyle);
            }

            GUILayout.EndVertical();
        }
    }
}