using BepInEx.Configuration;
using System.Linq;
using UnityEngine;

namespace BoatStatusHUD
{
    public class InstrumentsPanel : HUDPanel
    {
        private ConfigEntry<bool> _requireChipLog;
        private ConfigEntry<bool> _requireCompass;

        private bool _hasChipLog = false;
        private bool _hasCompass = false;
        private float _nextCheckTime = 0f;
        private bool _first = true;

        public override void BindConfig(ConfigFile config)
        {
            IsEnabled = config.Bind("HUDSections", "ShowInstruments", true, "Master switch for the Instruments panel.");

            _requireChipLog = config.Bind("EquipmentRequirements", "RequireChipLogForSpeed", true, "Speed appears only with a chip log aboard.");
            _requireCompass = config.Bind("EquipmentRequirements", "RequireCompassForHeading", true, "Heading appears only with a compass aboard.");
        }

        public override void UpdateTelemetry(BoatDamage currentBoat)
        {
            if (!IsEnabled.Value || currentBoat == null) return;

            if (Time.time >= _nextCheckTime || _first)
            {
                _first = false;
                _nextCheckTime = Time.time + 1f;

                _hasChipLog = !_requireChipLog.Value;
                _hasCompass = !_requireCompass.Value;

                Transform[] boatObjects = currentBoat.GetComponentsInChildren<Transform>(true);
                CheckTransforms(boatObjects);

                if ((_requireCompass.Value && !_hasCompass) || (_requireChipLog.Value && !_hasChipLog))
                {
                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        Transform[] playerObjects = player.GetComponentsInChildren<Transform>(true);
                        CheckTransforms(playerObjects);
                    }
                }
            }
        }

        private void CheckTransforms(Transform[] transforms)
        {
            foreach (var t in transforms)
            {
                string nameLower = t.name.ToLower();

                if (_requireCompass.Value && (nameLower.Contains("compass") || nameLower.Contains("bussola")))
                {
                    _hasCompass = true;
                }

                if (_requireChipLog.Value && (nameLower.Contains("chip log") || nameLower.Contains("chiplog")))
                {
                    _hasChipLog = true;
                }
            }
        }

        public override void DrawLines(GUIStyle defaultStyle, BoatDamage currentBoat)
        {
            if (!IsEnabled.Value || currentBoat == null) return;

            Rigidbody boatRigidbody = currentBoat.GetComponent<Rigidbody>();
            if (boatRigidbody == null) return;

            GUIStyle subCardStyle = new GUIStyle(GUI.skin.box);
            subCardStyle.normal.background = Utils.MakeTexture(2, 2, BackgroundColor);
            subCardStyle.margin = new RectOffset(0, 0, 0, 8);
            subCardStyle.padding = new RectOffset(12, 12, 12, 12);
            subCardStyle.stretchWidth = false;
            subCardStyle.stretchHeight = false;

            GUILayout.BeginVertical(subCardStyle);

            // 1. Vessel Speed (Frame-to-Frame)
            if (_hasChipLog)
            {
                Vector3 rawVelocity = boatRigidbody.velocity;
                float forwardSpeed = Vector3.Dot(rawVelocity, currentBoat.transform.forward);
                float speedInKnots = forwardSpeed * 1.94384f;
                if (Mathf.Abs(speedInKnots) < 0.1f) speedInKnots = 0f;

                DrawHUDLine($"<color={ColorLabel}>Speed: <b>{speedInKnots:F1} kts</b></color>", defaultStyle);
            }
            else
            {
                DrawHUDLine($"<color={ColorMuted}>Speed: [Requires Chip Log]</color>", defaultStyle);
            }

            // 2. Heading (Frame-to-Frame)
            if (_hasCompass)
            {
                float headingDegrees = currentBoat.transform.eulerAngles.y;
                if (headingDegrees < 0) headingDegrees += 360f;

                DrawHUDLine($"<color={ColorLabel}>Heading: <b>{headingDegrees:F0}°</b></color>", defaultStyle);
            }
            else
            {
                DrawHUDLine($"<color={ColorMuted}>Heading: [Requires Compass]</color>", defaultStyle);
            }

            float heelAngle = Vector3.Angle(currentBoat.transform.up, Vector3.up);

            float localZ = currentBoat.transform.localEulerAngles.z;
            if (localZ > 180f) localZ -= 360f;

            string sideIndicator = "";
            if (heelAngle > 0.5f)
            {
                // Se Z for positivo, o barco está inclinado para Bombordo (Port)
                // Se Z for negativo, o barco está inclinado para Estibordo (Starboard)
                sideIndicator = localZ > 0f ? "P" : "S";
            }

            // Se passar do limite de segurança do barco, o texto fica vermelho
            string colorHeel = (heelAngle > currentBoat.safeAngleLimit) ? ColorDanger : ColorLabel;

            DrawHUDLine($"<color={ColorLabel}>Heel Angle:</color> <color={colorHeel}><b>{heelAngle:F1}°{sideIndicator}</b> / {currentBoat.safeAngleLimit:F0}°</color>", defaultStyle);

            GUILayout.EndVertical();
        }
    }
}