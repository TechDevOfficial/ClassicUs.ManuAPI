---
sidebar_position: 8
---

# API Reference

Everything in these docs, in one table.

| Namespace | Type | What it's for |
|---|---|---|
| Roles | `CustomRole` / `CustomImpostorRole` / `CustomCrewmateRole` | Describe a role's identity and team behaviour. |
| Roles | `RoleRegistry` | Register and sync a role across the lobby. |
| Events | `GameEvents` | Subscribe to murder, report, meeting, vent, task and connection events. |
| Game Modes | `CustomGameMode` / `GameModeRegistry` | Register and synchronise host-selected custom modes. |
| Abilities | `CustomAbility` | A ticking, self-resetting ability button. |
| Abilities | `AbilityButtonGrid` | Known-good bottom-left button slots. |
| Kills | `KillManager` / `KillRequest` | Host-authoritative kills with independently toggleable side effects. |
| Options | `SettingsMenuAPI` / `SettingsMenuBuilder` | Rows in the real in-game settings menu. |
| UI | `ModBadgeAPI` | Loaded-mod list entry and lobby tag. |
| Assets | `LoadableSprite` / `AssetBundleManager` | Embedded PNGs and asset bundles that survive teardown. |
| Assets | `LoadableSound` | Embedded WAV clips with the same self-healing loading as sprites. |
| Assets | `SpatialAudio` | Play a clip at a world position with real distance-based attenuation. |
