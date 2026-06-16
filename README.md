# BoatStatusHUD

A lightweight, real-time telemetry HUD overlay for **Sailwind**. Since the game features a highly immersive, diegetic UI with no floating health bars, this mod introduces an elegant Immediate Mode GUI (IMGUI) overlay to help you monitor your ship's structural state during long trading voyages.

## Features

- **Hull Integrity:** Displays your ship's overall health percentage.
- **Hull Damage:** Tracks structural failure caused by physical collisions with docks or rocks.
- **Bilge Water:** Real-time monitoring of flooding/water level accumulation inside the hold.
- **Caulk/Oakum:** Monitored condition of your hull seals.
- **Dynamic Color Coding:** The HUD automatically switches typography colors from **White** (Safe) to **Yellow** (Warning) when damage/water exceeds 15%, and **Red** (Critical/Sunk) if the vessel is lost or integrity reaches 0%.

## Installation

### Automatic (Recommended)
1. Install **r2modman** or the **Thunderstore App**.
2. Search for `BoatStatusHUD` in the Sailwind mod catalogue.
3. Click **Download**.

### Manual Installation
1. Ensure **BepInEx 5** is correctly installed in your Sailwind directory.
2. Extract the `BoatStatusHUD.dll` into your `Sailwind/BepInEx/plugins/` folder.
3. Launch the game using your mod manager or BepInEx environment.

## License
This project is open-source and available under the MIT License.