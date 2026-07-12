using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace BoatStatusHUD
{
    [BepInPlugin("com.luizbag.sailwind.boatstatushud", "BoatStatusHUD", "0.0.0")]
    public class BoatStatusHUDPlugin : BaseUnityPlugin
    {
        // Global static tracking of the active vessel's core damage script
        public static BoatDamage CurrentBoatDamage;
        private ConfigEntry<bool> _hudVisible;
        private ConfigEntry<KeyCode> _hudToggleKey;
        private List<HUDPanel> _panels;

        private void Awake()
        {
            try
            {
                /* Single source of truth for all ship profiles inside the main BepInEx .cfg */
                var shipProfilesConfig = Config.Bind("ShipProfiles", "Profiles",
                    "Kakam=400;Cog=700;Dhow=800;Junk=4000;Sanbuq=6000;Brig=10000;Jong=17000;" +
                    "Gallus=400;Caelanor=2000;Clipper=10000;Gloriana=12000;Chronian=17000;Leopard=20000;" +
                    "Shroud Small=600;Shroud Large=9000;BOAT CUTTER=250;DNG=150",
                    "List of ship profiles and their max carrying capacities (Format: Key=Capacity;)");

                Utils.ParseProfilesString(shipProfilesConfig.Value);

                _panels = new List<HUDPanel>
                {
                    new InstrumentsPanel(),
                    new CargoPanel(),
                    new SoundingPanel(),
                    new StatusPanel(),
                    new WeatherPanel()
                };

                Info.Metadata.GetType().GetProperty("Version")?.SetValue(Info.Metadata, new System.Version(PluginInfo.Version));

                _hudVisible = Config.Bind("HUDSections", "HudVisible", true, "Master switch for the whole overlay.");
                _hudToggleKey = Config.Bind("HUDSections", "HudToggleKey", KeyCode.F8, "The key that flips the HUD visibility at sea.");

                foreach (var panel in _panels)
                {
                    panel.BindConfig(Config);
                }

                var harmony = new Harmony(PluginInfo.GUID);
                harmony.PatchAll();
                Logger.LogInfo($"[{PluginInfo.Name}] Telemetry system successfully initialized.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"[{PluginInfo.Name}] Critical failure hook mapping: {ex.Message}");
            }
        }

        private void OnGUI()
        {
            if (!_hudVisible.Value || !GameState.playing || GameState.justStarted || GameState.lastBoat == null) return;

            BoatDamage activeBoat = GameState.lastBoat.GetComponent<BoatDamage>();

            GUIStyle mainContainerStyle = new GUIStyle(GUI.skin.box);
            mainContainerStyle.normal.background = Utils.MakeTexture(2, 2, new Color(0.29f, 0.35f, 0.41f, 0.25f));
            mainContainerStyle.padding = new RectOffset(10, 10, 10, 10);

            /* ALLOW EXPANSION: Let the main container track sub-panels growth */
            mainContainerStyle.stretchWidth = true;
            mainContainerStyle.stretchHeight = true;

            /* Enforce global font configurations setup by HUDPanel overrides */
            GUIStyle baseStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = HUDPanel.FontSize,
                richText = true,
                wordWrap = false,
                clipping = TextClipping.Overflow
            };

            /* FIX: We anchor the layout at (20, 20) using a horizontal block instead of a rigid Area width */
            GUILayout.BeginHorizontal();
            GUILayout.Space(20f); // Margin left

            GUILayout.BeginVertical();
            GUILayout.Space(20f); // Margin top

            /* The vertical layout now wraps the style and auto-expands perfectly to the right and bottom */
            GUILayout.BeginVertical(mainContainerStyle, GUILayout.MinWidth(200f), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            foreach (var panel in _panels)
            {
                panel.DrawLines(baseStyle, activeBoat);
            }

            GUILayout.EndVertical();
            GUILayout.EndVertical();

            /* Closes the master horizontal anchor alignment */
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
/*
        private void OnGUI()
        {
            if (!_hudVisible.Value || !GameState.playing || GameState.justStarted || GameState.lastBoat == null) return;

            BoatDamage activeBoat = GameState.lastBoat.GetComponent<BoatDamage>();

            GUILayout.BeginArea(new Rect(20, 20, 200, Screen.height));

            GUIStyle mainContainerStyle = new GUIStyle(GUI.skin.box);
            mainContainerStyle.normal.background = Utils.MakeTexture(2, 2, new Color(0.29f, 0.35f, 0.41f, 0.25f));
            mainContainerStyle.padding = new RectOffset(10, 10, 10, 10);

            mainContainerStyle.stretchWidth = false;
            mainContainerStyle.stretchHeight = false;

            GUILayout.BeginVertical(mainContainerStyle);

            GUIStyle baseStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                richText = true,
                wordWrap = false,
                clipping = TextClipping.Overflow
            };

            foreach (var panel in _panels)
            {
                panel.DrawLines(baseStyle, activeBoat);
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
*/
        private void Update()
        {
            if (!GameState.playing || GameState.justStarted) return;

            if (Input.GetKeyDown(_hudToggleKey.Value))
            {
                _hudVisible.Value = !_hudVisible.Value;
            }

            if (_hudVisible.Value && GameState.lastBoat != null)
            {
                BoatDamage activeBoat = GameState.lastBoat.GetComponent<BoatDamage>();

                foreach (var panel in _panels)
                {
                    panel.UpdateTelemetry(activeBoat);
                }
            }
        }
    }
}