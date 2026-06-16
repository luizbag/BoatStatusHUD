using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace BoatStatusHUD
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class BoatStatusHUDPlugin : BaseUnityPlugin
    {
        public static class PluginInfo
        {
            public const string GUID = "com.luizbag.sailwind.boatstatushud";
            public const string Name = "BoatStatusHUD";
            public const string Version = "1.0.0";
        }

        // Global static tracking of the active vessel's core damage script
        public static BoatDamage CurrentBoatDamage;

        private void Awake()
        {
            try
            {
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
            if (!GameState.playing || GameState.justStarted || GameState.lastBoat == null)
            {
                return;
            }

            BoatDamage activeBoatDamage = GameState.lastBoat.GetComponent<BoatDamage>();
            Rigidbody boatRigidbody = GameState.lastBoat.GetComponent<Rigidbody>();
            if (activeBoatDamage == null || boatRigidbody == null) return;

            // --- CÁLCULOS DE NAVEGAÇÃO E FÍSICA ---
            Vector3 rawVelocity = boatRigidbody.velocity;
            float forwardSpeedMetersPerSecond = Vector3.Dot(rawVelocity, GameState.lastBoat.forward);
            float speedInKnots = forwardSpeedMetersPerSecond * 1.94384f;
            if (Mathf.Abs(speedInKnots) < 0.1f) speedInKnots = 0f;

            float heelAngle = Vector3.Angle(GameState.lastBoat.up, Vector3.up);
            float currentHullDamage = activeBoatDamage.hullDamage * 100f;
            float currentBilgeWater = activeBoatDamage.waterLevel * 100f;

            float factoryBaseMass = (float)AccessTools.Field(typeof(BoatDamage), "baseMass").GetValue(activeBoatDamage);
            float cargoWeightInPounds = (boatRigidbody.mass - factoryBaseMass) * 2.20462f;
            if (cargoWeightInPounds < 0f) cargoWeightInPounds = 0f;

            float displacementMeters = (boatRigidbody.mass - factoryBaseMass) / (factoryBaseMass * 0.5f);
            if (displacementMeters < 0f) displacementMeters = 0f;
            float emptyVesselDraft = 0.65f;
            float currentDraftMeters = emptyVesselDraft + displacementMeters;

            // --- CÁLCULO DA VELOCIDADE DO VENTO CORRIGIDO ---
            // Acedemos diretamente ao campo estático 'currentWind' da classe Wind que descobriu
            float windSpeedInKnots = Wind.currentWind.magnitude * 1.94384f;

            float totalWaterDepth = 999f;
            float keelClearance = 999f;

            RaycastHit hit;
            if (Physics.Raycast(GameState.lastBoat.position, Vector3.down, out hit, 200f))
            {
                totalWaterDepth = hit.distance;
                keelClearance = totalWaterDepth - currentDraftMeters;
                if (keelClearance < 0f) keelClearance = 0f;
            }

            // --- CONFIGURAÇÃO DA INTERFACE (IMGUI) ---
            Rect hudContainer = new Rect(20, 60, 270, 270);
            GUI.Box(hudContainer, "");

            GUIStyle defaultStyle = new GUIStyle();
            defaultStyle.fontSize = 13;
            defaultStyle.fontStyle = FontStyle.Bold;
            defaultStyle.normal.textColor = Color.white;

            // Métodos de estilo granular
            GUIStyle GetStyleForAngle(float angle, float limit) => 
                new GUIStyle(defaultStyle) { normal = { textColor = (angle > limit) ? Color.yellow : Color.white } };

            GUIStyle GetStyleForDamage(float damage) => 
                new GUIStyle(defaultStyle) { normal = { textColor = (damage >= 95f) ? Color.red : (damage > 15f) ? Color.yellow : Color.white } };

            GUIStyle GetStyleForWater(float water, bool sunk) => 
                new GUIStyle(defaultStyle) { normal = { textColor = sunk ? Color.red : (water > 15f) ? Color.yellow : Color.white } };

            GUIStyle GetStyleForClearance(float clearance) => 
                new GUIStyle(defaultStyle) { normal = { textColor = (clearance <= 0.1f) ? Color.red : (clearance < 2.0f) ? Color.yellow : Color.white } };

            // Renderizador linha por linha
            float startX = 32f;
            float startY = 65f;
            float lineHeight = 15f;
            int currentLine = 0;

            void DrawHUDLine(string text, GUIStyle style)
            {
                GUI.Label(new Rect(startX, startY + (currentLine * lineHeight), 250, 20), text, style);
                currentLine++;
            }

            // Bloco 1: Instrumentos de Navegação
            DrawHUDLine("[ VESSEL INSTRUMENTS ]", defaultStyle);
            DrawHUDLine("---------------------------", defaultStyle);
            DrawHUDLine($"Vessel Speed   : {speedInKnots:F1} kts", defaultStyle);
            DrawHUDLine($"Wind Speed     : {windSpeedInKnots:F1} kts", defaultStyle); 
            DrawHUDLine($"Heel Angle     : {heelAngle:F1}° / {activeBoatDamage.safeAngleLimit:F0}°", GetStyleForAngle(heelAngle, activeBoatDamage.safeAngleLimit));
            DrawHUDLine($"Cargo Weight   : {cargoWeightInPounds:F0} lbs", defaultStyle);
            DrawHUDLine("---------------------------", defaultStyle);

            // Bloco 2: Hidrodinâmica e Flutuação
            DrawHUDLine($"Waterline Sink : +{displacementMeters:F2} m", defaultStyle);
            DrawHUDLine($"Current Draft  : {currentDraftMeters:F2} m", defaultStyle);
            DrawHUDLine("---------------------------", defaultStyle);

            // Bloco 3: Sonar / Profundidade
            DrawHUDLine($"Sea Depth      : {(totalWaterDepth > 190f ? "Deep Ocean" : totalWaterDepth.ToString("F1") + " m")}", defaultStyle);
            string clearanceText = keelClearance > 190f ? "Safe" : keelClearance.ToString("F1") + " m";
            DrawHUDLine($"Keel Clearance : {clearanceText}", GetStyleForClearance(keelClearance));
            DrawHUDLine("---------------------------", defaultStyle);

            // Bloco 4: Integridade Estrutural (Avisos Individuais)
            DrawHUDLine($"Hull Integrity : {(100f - currentHullDamage):F1}%", GetStyleForDamage(currentHullDamage));
            DrawHUDLine($"Bilge Water    : {currentBilgeWater:F1}%", GetStyleForWater(currentBilgeWater, activeBoatDamage.sunk));
        }
    }
}