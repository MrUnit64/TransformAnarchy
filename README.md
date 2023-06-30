# Transform Anarchy
A mod created to add advanced building to Parkitect.

## Cloning
When you clone, create a local directory called `Libs/` next to the `sln` file. For easiest install, copy all .DLL's from `\Parkitect\Parkitect_Data\Managed\` along with [0Harmony](https://github.com/pardeike/Harmony/releases/tag/v2.2.2.0) into this folder.

## Build Events
There are build events that copy mod `dll`, `preview.png` and `img/` to `Documents/Parkitect/Mods/TransformAnarchy`. If you don't have that folder created on that exact path, the build will fail at copying files.

## Asset bundle
This repo uses an Asset Bundle called `ta_assets` located within `\Res\`. It contains all Sprites, along with the prefabs for the UI and the Positional and Rotational gizmo. The files required to create this Asset Bundle are not present within this repository. Please extract the information from this Asset Bundle with a program like Asset Studio in order to modify it.

# Attribution
- Icons are from Google's [Material Icons](https://github.com/google/material-design-icons/) repo.
- The method `ClosestPointsOnTwoLines` inside of `/TransformAnarchy/Gizmo/PositionalGizmoComponent.cs` was adapted from [Unity3DRuntimeTransformGizmo](https://github.com/HiddenMonk/Unity3DRuntimeTransformGizmo/tree/master)
- The file `ui_infopip_circle.png` inside of `ra_assets` was sourced directly from Parkitect.

# Modding in Parkitect
Feel free to view this repository to learn about how Parkitect mods are constructed!

This repo contains examples of:
- Keybind handling
- Loading assets from Asset Bundles
- Instantiation into the game
- Harmony patches (on both public and protected methods)
- Harmony traversal, accessing and setting private fields and members.