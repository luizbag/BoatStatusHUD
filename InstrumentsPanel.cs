using BepInEx.Configuration;
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

        public override void BindConfig(ConfigFile config)
        {
            IsEnabled = config.Bind("HUDSections", "ShowInstruments", true, "Master switch for the Instruments panel.");

            _requireChipLog = config.Bind("EquipmentRequirements", "RequireChipLogForSpeed", true, "Speed appears only with a chip log aboard.");
            _requireCompass = config.Bind("EquipmentRequirements", "RequireCompassForHeading", true, "Heading appears only with a compass aboard.");
        }

        public override void UpdateTelemetry(BoatDamage currentBoat)
        {
            if (!IsEnabled.Value || currentBoat == null) return;

            if (Time.time >= _nextCheckTime)
            {
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
                if (t == null) continue;
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

            BeginPanel();

            // 1. Vessel Speed
            if (_hasChipLog)
            {
                Vector3 rawVelocity = boatRigidbody.velocity;
                float forwardSpeed = Vector3.Dot(rawVelocity, currentBoat.transform.forward);
                float speedInKnots = forwardSpeed * 1.94384f;
                if (Mathf.Abs(speedInKnots) < 0.1f) speedInKnots = 0f;

                DrawHUDLine($"<color={ColorLabel}>Speed: </color>{speedInKnots:F1}kts", defaultStyle);
            }
            else
            {
                DrawHUDLine($"<color={ColorMuted}>Speed: </color>[Requires Chip Log]", defaultStyle);
            }

            // 2. Heading with Integrated Compass Point
            if (_hasCompass)
            {
                float headingDegrees = currentBoat.transform.eulerAngles.y;
                if (headingDegrees < 0) headingDegrees += 360f;

                string directionLetter = Utils.GetCompassDirection(headingDegrees);
                DrawHUDLine($"<color={ColorLabel}>Heading: </color>{headingDegrees:F0}° {directionLetter}", defaultStyle);
            }
            else
            {
                DrawHUDLine($"<color={ColorMuted}>Heading: </color>[Requires Compass]", defaultStyle);
            }

            // 3. Heel Angle
            float heelAngle = Vector3.Angle(currentBoat.transform.up, Vector3.up);
            float localZ = currentBoat.transform.localEulerAngles.z;
            if (localZ > 180f) localZ -= 360f;

            string sideIndicator = "";
            if (heelAngle > 0.5f)
            {
                sideIndicator = localZ > 0f ? "P" : "S";
            }

            string labelColor = (heelAngle > currentBoat.safeAngleLimit) ? ColorDanger : ColorLabel;
            DrawHUDLine($"<color={labelColor}>Heel Angle: </color>{heelAngle:F1}° {sideIndicator}/{currentBoat.safeAngleLimit:F0}°", defaultStyle);

            EndPanel();
        }
    }
}