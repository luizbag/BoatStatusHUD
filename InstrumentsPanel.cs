using BepInEx.Configuration;
using System.Linq;
using UnityEngine;

namespace BoatStatusHUD
{
    public class InstrumentsPanel : HUDPanel
    {
        private ConfigEntry<bool> _requireChipLog;
        private ConfigEntry<bool> _requireCompass;
        private ConfigEntry<bool> _requireWindIndicator;

        private bool _hasChipLog = false;
        private bool _hasCompass = false;
        private bool _hasWindIndicator = false;
        private float _nextCheckTime = 0f;

        public override void BindConfig(ConfigFile config)
        {
            IsEnabled = config.Bind("HUDSections", "ShowInstruments", true, "Master switch for the Instruments panel.");

            _requireChipLog = config.Bind("EquipmentRequirements", "RequireChipLogForSpeed", true, "Speed appears only with a chip log aboard.");
            _requireCompass = config.Bind("EquipmentRequirements", "RequireCompassForHeading", true, "Heading appears only with a compass aboard.");
            _requireWindIndicator = config.Bind("EquipmentRequirements", "RequireWindIndicatorForWind", true, "Wind appears only with a windicator on the ship.");
        }

        public override void UpdateTelemetry(BoatDamage currentBoat)
        {
            if (!IsEnabled.Value || currentBoat == null) return;

            if (Time.time >= _nextCheckTime)
            {
                _nextCheckTime = Time.time + 1f;

                _hasChipLog = !_requireChipLog.Value;
                _hasCompass = !_requireCompass.Value;
                _hasWindIndicator = !_requireWindIndicator.Value;

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

                if (_requireWindIndicator.Value && (nameLower.Contains("windicator") || nameLower.Contains("tell tale") || nameLower.Contains("tell_tale")))
                {
                    _hasWindIndicator = true;
                }
            }
        }

        public override void DrawLines(GUIStyle defaultStyle, BoatDamage currentBoat)
        {
            if (!IsEnabled.Value || currentBoat == null) return;

            Rigidbody boatRigidbody = currentBoat.GetComponent<Rigidbody>();
            if (boatRigidbody == null) return;

            GUIStyle cardStyle = new GUIStyle(GUI.skin.box);
            cardStyle.normal.background = Utils.MakeTexture(2, 2, BackgroundColor);
            cardStyle.margin = new RectOffset(0, 0, 0, 10);
            cardStyle.padding = new RectOffset(10, 10, 10, 10);

            GUIStyle labelStyle = new GUIStyle(defaultStyle);

            GUILayout.BeginVertical(cardStyle);

            // 1. Vessel Speed
            if (_hasChipLog)
            {
                Vector3 rawVelocity = boatRigidbody.velocity;
                float forwardSpeed = Vector3.Dot(rawVelocity, currentBoat.transform.forward);
                float speedInKnots = forwardSpeed * 1.94384f;
                if (Mathf.Abs(speedInKnots) < 0.1f) speedInKnots = 0f;

                DrawHUDLine($"<color={ColorLabel}>Vessel Speed: </color> <b>{speedInKnots:F1} kts</b>", labelStyle);
            }
            else
            {
                DrawHUDLine($"<color={ColorMuted}>Vessel Speed: [Requires Chip Log]</color>", labelStyle);
            }

            // 2. Heading
            if (_hasCompass)
            {
                float headingDegrees = currentBoat.transform.eulerAngles.y;
                if (headingDegrees < 0) headingDegrees += 360f;

                DrawHUDLine($"<color={ColorLabel}>Heading: </color> <b>{headingDegrees:F0}°</b>", labelStyle);
            }
            else
            {
                DrawHUDLine($"<color={ColorMuted}>Heading: [Requires Compass]</color>", labelStyle);
            }

            // 3. Wind Speed
            if (_hasWindIndicator)
            {
                float windSpeedInKnots = Wind.currentWind.magnitude * 1.94384f;
                DrawHUDLine($"<color={ColorLabel}>Wind Speed: </color> <b>{windSpeedInKnots:F1} kts</b>", labelStyle);
            }
            else
            {
                DrawHUDLine($"<color={ColorMuted}>Wind Speed: [No Wind Indicator]</color>", labelStyle);
            }

            // 4. Heel Angle
            float heelAngle = Vector3.Angle(currentBoat.transform.up, Vector3.up);
            string colorHeel = (heelAngle > currentBoat.safeAngleLimit) ? ColorDanger : ColorLabel;
            DrawHUDLine($"<color={colorHeel}>Heel Angle: {heelAngle:F1}° / {currentBoat.safeAngleLimit:F0}°</color>", labelStyle);

            GUILayout.EndVertical();
        }
    }
}