---
sidebar_position: 1
slug: /
---

# ManuAPI

**ManuAPI** is a modding framework for **Classic Us** (Among Us) BepInEx mods, built on
top of [Manactor](https://github.com/TechDevOfficial/ClassicUs.Manactor). It exists so you
don't have to relearn the same IL2CPP lessons every time you build a role.

Modding an IL2CPP Unity game means every native class is a wrapper around compiled C++ —
no source, no debugger, and one wrong Harmony patch can corrupt role assignment for an
entire match. ManuAPI handles the parts every mod needs, so you can focus on what makes
your role interesting.

## What you get

- **Custom roles** — describe a role in a few properties; ManuAPI registers the IL2CPP
  type, assigns it, and keeps it synced across clients.
- **Ability buttons** — a clone of the native kill button with cooldowns, effects and
  icons, no manual UI wiring.
- **Kill manager** — host-authoritative kills that can skip the teleport, the animation,
  or the sound, independently of each other.
- **Settings menu & mod badges** — inject rows into the real options menu and show your
  mod's presence in the lobby.
- **Asset loading** — embedded sprites that survive Unity tearing them down mid-match.

:::tip Dependency
ManuAPI depends on **Manactor** for networking — RPCs, handshakes, lobby-mod detection.
Declare `[BepInDependency(ManactorPlugin.Guid)]` on your plugin; you rarely need to touch
Manactor directly.
:::

Start with [Installation](/getting-started/installation), or jump straight to
[Custom Roles](/roles/custom-roles) if you already have a plugin skeleton.
