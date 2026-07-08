# ManuAPI v1.3.24

First public release of ManuAPI, a modding framework for Classic Us (Among Us) BepInEx mods built on top of Manactor. This changelog consolidates every change from the initial 1.3.3 compatibility pass up to this release.

## Roles

- Custom role framework (`CustomRole`, `CustomImpostorRole`, `CustomCrewmateRole`) with automatic IL2CPP type registration, retrying safely until the type is ready.
- `RoleRegistry.Register` handles role assignment, team-based candidate selection with configurable count/chance, and syncing the assignment to every client.
- Native role text, description, team color, and kill-ability text are patched so custom roles display exactly like native ones in the intro cutscene, task list, settings menu, and exile screen.
- Fixed custom role types not registering under Classic Us 2026.7.4 due to a false-positive in Il2CppInterop's `ClassInjector.RewriteType` (it treated any type merely *named* like a BCL type, e.g. `SystemTypes`, as needing remapping).
- Fixed custom roles not being assignable in fast game starts (e.g. freeplay) by force-flushing pending IL2CPP registrations right before `RoleManager.AssignRolesForTeam` runs.
- Fixed custom role assignment being silently reverted shortly after being set: the native `PlayerControl.SetRole` call (part of the game's own role-sync flow) would overwrite the assignment a moment later. A guard now blocks the native call outright for any player with a custom role assigned, instead of reactively fighting it.
- Fixed a fatal CLR crash on the second match of a session: cached `RoleBehaviour` references from the previous (now-destroyed) `RoleManager` were read after being invalidated. All cached role state is now cleared on game end/restart.
- Removed an overly broad `TaskFolder` patch that corrupted vanilla role assignment mid-match; freeplay role-folder registration now uses a narrower, safe hook.

## Abilities

- `CustomAbility` base class: clone-of-the-kill-button ability buttons with cooldowns, timed effects, icons, and visibility/activation checks — no manual UI wiring required.
- `AbilityButtonGrid`: fixed, known-good bottom-left slot positions so multiple ability buttons from different mods line up instead of overlapping.
- Ability buttons now reset themselves automatically on death, restart, and game-end (`HudManager.Start`, `AmongUsClient.OnGameEnd`/`ExitGame`, `EndGameManager.NextGame`/`Exit`) instead of every mod re-implementing the same five Harmony patches.
- Fixed ability icons silently reverting to the native kill sprite after touching mod settings: `LoadableAsset<T>` now retries after a failed or Unity-destroyed load instead of permanently caching the failure, and `AbilityButton` re-applies its icon every tick instead of once at creation.

## Kills

- `KillManager` / `KillRequest`: a full, host-authoritative re-implementation of the murder flow that never touches the native `RpcMurderPlayer`/kill-animation path.
- Independently toggleable side effects: `TeleportKiller`, `CreateDeadBody` (colored to match the victim, spawned from the killer's own `KillAnimations` prefab), `PlayKillSound`, and `ShowKillAnimation` (shown only to the victim's own client, purely cosmetic).
- Replaces an earlier reactive approach (pausing/re-snapping the killer's `NetTransform` after the fact), which fought the native kill animation and produced visible rubber-banding.

## Settings & UI

- `SettingsMenuAPI` / `SettingsMenuBuilder`: inject toggle/numeric rows into the real in-game settings menu.
- Rows are now positioned using the native `SettingMenu.RepositionChildren()` after injection, so they land directly after vanilla options instead of using stale manual layout math.
- `ModBadgeAPI`: register a loaded-mod badge and a lobby prelobby tag.

## Assets

- `LoadableSprite`, `LoadableBundleAsset<T>`, `LoadableEmbeddedBundleAsset<T>`: embedded-resource and asset-bundle loading with retry-on-failure caching.
- Generalized asset loading beyond sprites: `AssetUtils.LoadAudioClipFromWavBytes`/`LoadAudioClipFromEmbeddedResource` and a new `LoadableSound` class, so mods can embed and play custom sounds with the same self-healing loader.

## Documentation

- Added a full documentation site (Docusaurus, served from `/docs`) covering installation, plugin setup, custom roles, ability buttons, the kill manager, settings menu, mod badges, and asset loading — with real examples pulled from a production mod.

## Dependencies

- Requires `ClassicUs.Manactor` 1.0.26+ and `ClassicUs.GameLibs` 2026.7.4.1, targeting Classic Us 2026.7.4.
