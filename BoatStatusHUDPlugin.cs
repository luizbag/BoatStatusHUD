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
                _panels = new List<HUDPanel>
                {
                    new InstrumentsPanel()
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
            if (!_hudVisible.Value || !GameState.playing || GameState.lastBoat == null) return;

            BoatDamage activeBoat = GameState.lastBoat.GetComponent<BoatDamage>();

            Rect hudRect = new Rect(20, 20, 310, 500);

            GUIStyle panelStyle = new GUIStyle(GUI.skin.box);
            panelStyle.normal.background = Utils.MakeTexture(2, 2, new Color(0.29f, 0.35f, 0.41f, 0.70f));
            panelStyle.border = new RectOffset(4, 4, 4, 4);

            GUI.Box(hudRect, "", panelStyle);

            GUILayout.BeginArea(new Rect(hudRect.x + 12, hudRect.y + 12, hudRect.width - 24, hudRect.height - 24));

            GUIStyle baseStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                richText = true
            };

            foreach (var panel in _panels)
            {
                panel.DrawLines(baseStyle, activeBoat);
            }

            GUILayout.EndArea();
        }

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