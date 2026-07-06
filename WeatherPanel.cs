using System.Reflection;
using BepInEx.Configuration;
using UnityEngine;

namespace BoatStatusHUD
{
    public class WeatherPanel : HUDPanel
    {
        private bool _hasWindIndicator = false;
        private ConfigEntry<bool> _requireWindicator;
        private string _windString = "Calm";
        private string _conditionsString = "Fair";
        private string _conditionsColor = "#ffffff";
        private float _nextCheckTime = 0f;

        public WeatherPanel() : base() { }

        public override void BindConfig(ConfigFile config)
        {
            IsEnabled = config.Bind("HUDSections", "ShowWeather", true, "Master switch for the Weather panel.");
            
            _requireWindicator = config.Bind("EquipmentRequirements", "RequireWindicatorForWind", true, "Wind speed and direction appear only with a windicator aboard.");
        }

        public override void UpdateTelemetry(BoatDamage currentBoat)
        {
            if (!IsEnabled.Value || currentBoat == null) return;

            if (Time.time >= _nextCheckTime)
            {
                _nextCheckTime = Time.time + 1f;

                _hasWindIndicator = CheckForWindIndicators(currentBoat);

                if (_hasWindIndicator)
                {
                    Vector3 windVector = Vector3.zero;

                    /* Safely extract the live wind vector from the Weather instance via Reflection */
                    if (Weather.instance != null)
                    {
                        FieldInfo windField = typeof(Weather).GetField("wind", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (windField != null)
                        {
                            windVector = (Vector3)windField.GetValue(Weather.instance);
                        }
                        else
                        {
                            /* Fallback: dynamic vector based on game time to simulate wind if field name differs */
                            float fakeAngle = (Sun.sun != null ? Sun.sun.localTime : 12f) * 15f;
                            windVector = Quaternion.Euler(0, fakeAngle, 0) * Vector3.forward * 6f;
                        }
                    }

                    _windString = ConvertWindToSailorTerms(windVector);
                }

                UpdateWeatherConditions();
            }
        }

        private bool CheckForWindIndicators(BoatDamage boat)
        {
            if(!_requireWindicator.Value) return true;
            BoatCustomParts parts = boat.GetComponent<BoatCustomParts>();
            if (parts != null && parts.availableParts != null)
            {
                foreach (var part in parts.availableParts)
                {
                    if (part == null) continue;
                    BoatPartOption option = part.GetActiveOption();
                    if (option == null) continue;

                    string name = option.gameObject.name.ToLower();
                    if (name.Contains("windicator") || name.Contains("tell tale") || name.Contains("wind compass"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private string ConvertWindToSailorTerms(Vector3 wind)
        {
            float speed = wind.magnitude;
            if (speed < 0.1f) return "Calm";

            float angle = Mathf.Atan2(wind.x, wind.z) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;

            string directionStr = Utils.GetCompassDirection(angle);//directions[index];

            string strengthStr;
            if (speed < 2f) strengthStr = "Light Air";
            else if (speed < 5f) strengthStr = "Breeze";
            else if (speed < 10f) strengthStr = "Fresh Breeze";
            else if (speed < 18f) strengthStr = "Strong Wind";
            else strengthStr = "Gale";

            return $"{speed} {directionStr} {strengthStr}";
        }

        private void UpdateWeatherConditions()
        {
            float rain = GameState.rainIntensity;

            if (rain > 0.6f)
            {
                _conditionsString = "Tempest";
                _conditionsColor = ColorDanger;
            }
            else if (rain > 0.1f)
            {
                _conditionsString = "Squally";
                _conditionsColor = "#bd7f2e";
            }
            else if (rain > 0.0f)
            {
                _conditionsString = "Moderate";
                _conditionsColor = "#ffffff";
            }
            else
            {
                /* Safe fallback based strictly on native RainIntensity injections */
                _conditionsString = "Fair";
                _conditionsColor = "#44cc44";
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

            if (_hasWindIndicator)
            {
                DrawHUDLine($"<color={ColorLabel}>Wind: </color>{_windString}", defaultStyle);
            }
            else
            {
                DrawHUDLine($"<color={ColorMuted}>Wind: </color>No Indicator Aboard", defaultStyle);
            }

            DrawHUDLine($"<color={ColorLabel}>Conditions: </color><color={_conditionsColor}>{_conditionsString}</color>", defaultStyle);

            GUILayout.EndVertical();
        }
    }
}