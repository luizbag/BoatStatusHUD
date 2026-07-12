using BepInEx.Configuration;
using System.Reflection;
using UnityEngine;

namespace BoatStatusHUD
{
    public class CargoPanel : HUDPanel
    {
        private enum BurdenLevel { Light, Normal, Loaded, Overloaded }

        private float _calculatedDeadweight = 0f;
        private float _nextCheckTime = 0f;
        
        private ConfigFile _configFile;
        private string _lastCachedBoatName = "";
        private ConfigEntry<float> _activeMaxCapacity;
        private ConfigEntry<float> _activeMaxFreeboard;

        /* Cached float to read current flood percentage dynamically in DrawLines */
        private float _currentWaterLevel = 0f;

        public override void BindConfig(ConfigFile config)
        {
            IsEnabled = config.Bind("HUDSections", "ShowCargo", true, "Master switch for the Cargo panel.");
            _configFile = config;
        }

        public override void UpdateTelemetry(BoatDamage currentBoat)
        {
            if (!IsEnabled.Value || currentBoat == null) return;

            /* Extract normalized water level frame-to-frame for smooth UI updates */
            _currentWaterLevel = currentBoat.waterLevel;

            if (Time.time >= _nextCheckTime)
            {
                _nextCheckTime = Time.time + 1f;

                if (currentBoat.name != _lastCachedBoatName && _configFile != null)
                {
                    _lastCachedBoatName = currentBoat.name;
                    SetupActiveBoatConfig(currentBoat.name);
                }

                BoatMass boatMass = currentBoat.GetComponent<BoatMass>();
                if (boatMass != null)
                {
                    FieldInfo partsMassField = typeof(BoatMass).GetField("partsMass", BindingFlags.Instance | BindingFlags.NonPublic);
                    FieldInfo itemsListField = typeof(BoatMass).GetField("itemsOnBoat", BindingFlags.Instance | BindingFlags.NonPublic);

                    float totalCargoWeight = 0f;

                    if (partsMassField != null)
                    {
                        totalCargoWeight += (float)partsMassField.GetValue(boatMass);
                    }

                    if (itemsListField != null)
                    {
                        var itemsList = itemsListField.GetValue(boatMass) as System.Collections.Generic.List<ItemRigidbody>;
                        if (itemsList != null)
                        {
                            foreach (ItemRigidbody item in itemsList)
                            {
                                if (item != null && item.GetCurrentInventorySlot() == null)
                                {
                                    totalCargoWeight += item.GetBody().mass;
                                }
                            }
                        }
                    }

                    if (GameState.lastBoat == currentBoat.gameObject && GameState.currentBoat != null && GameState.currentBoat.parent == currentBoat.transform)
                    {
                        totalCargoWeight += 160f;
                    }

                    /* Convert internal water level and capacity metrics straight into active lbs */
                    float bilgeWaterWeight = currentBoat.waterLevel * currentBoat.waterUnitsCapacity;
                    totalCargoWeight += bilgeWaterWeight;

                    _calculatedDeadweight = totalCargoWeight;
                }
            }
        }

        private void SetupActiveBoatConfig(string boatName)
        {
            ShipProfile profile = Utils.GetProfile(boatName);
            
            string sectionName = $"Cargo - {((profile != null) ? profile.DisplayName : boatName)}";
            float defaultCapacity = (profile != null) ? profile.MaxCapacity : 4000f;
            float defaultFreeboard = (profile != null) ? profile.MaxFreeboard : 1.5f;

            _activeMaxCapacity = _configFile.Bind(sectionName, "MaxCapacity", defaultCapacity, "Maximum safe carrying capacity in lbs.");
            _activeMaxFreeboard = _configFile.Bind(sectionName, "MaxFreeboard", defaultFreeboard, "Target empty freeboard height in meters.");
        }

        public override void DrawLines(GUIStyle defaultStyle, BoatDamage currentBoat)
        {
            if (!IsEnabled.Value || currentBoat == null || _activeMaxCapacity == null) return;

            BeginPanel();

            DrawHUDLine($"<color={ColorLabel}>Deadweight:</color> {_calculatedDeadweight:F0}lbs", defaultStyle);

            float maxFreeboard = _activeMaxFreeboard.Value;
            float maxCapacity = _activeMaxCapacity.Value;

            /* Freeboard drops linearly based on weight, and hits absolute zero if waterLevel reaches 1.0 (sunk) */
            float weightEffect = (_calculatedDeadweight / maxCapacity) * (maxFreeboard * 0.5f);
            float freeboardMeters = maxFreeboard - weightEffect - (_currentWaterLevel * maxFreeboard);
            if (freeboardMeters < 0f) freeboardMeters = 0f;

            string colorFreeboard = ColorLabel;
            if (freeboardMeters <= 0.3f || _currentWaterLevel > 0.7f) colorFreeboard = ColorDanger; 
            else if (freeboardMeters <= 0.6f || _currentWaterLevel > 0.3f) colorFreeboard = "#bd7f2e"; 

            DrawHUDLine($"<color={colorFreeboard}>Freeboard:</color> {freeboardMeters:F1}m", defaultStyle);

            float capacityPercentage = _calculatedDeadweight / maxCapacity;
            string burdenText;
            string colorBurden;

            if (capacityPercentage > 1.0f || _currentWaterLevel >= 0.9f)
            {
                burdenText = BurdenLevel.Overloaded.ToString();
                colorBurden = ColorDanger;
            }
            else if (capacityPercentage > 0.75f || _currentWaterLevel > 0.4f)
            {
                burdenText = BurdenLevel.Loaded.ToString();
                colorBurden = "#bd7f2e";
            }
            else if (capacityPercentage > 0.25f)
            {
                burdenText = BurdenLevel.Normal.ToString();
                colorBurden = "#ffffff";
            }
            else
            {
                burdenText = BurdenLevel.Light.ToString();
                colorBurden = ColorMuted;
            }

            DrawHUDLine($"<color={ColorLabel}>Burden:</color> <color={colorBurden}>{burdenText}</color>", defaultStyle);

            EndPanel();
        }
    }
}