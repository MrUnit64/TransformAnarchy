# RotationAnarchy
A mod created to freely rotate objects in Parkitect.

## Cloning
When you clone, create a local directory called Libs/ next to the `sln` file. Copy the following dlls (All from `\Parkitect\Parkitect_Data\Managed\` with the exception of `0Harmony`) into this folder.

- 0Harmony
- Parkitect
- UnityEngine
- UnityEngine.CoreModule
- UnityEngine.ImageConversionModule
- UnityEngine.IMGUIModule
- UnityEngine.InputModule
- UnityEngine.InputLegacyModule
- UnityEngine.TextRenderingModule
- UnityEngine.UI
- UnityEngine.UIModule
- UnityEngine.PhysicsModule.dll

## Build Events
There are build events that copy mod `dll`, `preview.png` and `img/` to `Documents/Parkitect/Mods/RotationAnarchy`. If you don't have that folder created on that exact path, the build will fail at copying files.

## Terms
- **Placement mode** - when user selects a deco to place, and has only hotkey control over rotation.
- **Gizmo mode** - when user either selects already existing deco, or switches to gizmo while in placement mode, the control of the transformation of the object is given to the gizmo.
- **Active toggle** - deactivates the mod functionality, reverting to default parki rotation, if switched mid RA operation, should reset the model rotation to default.
- **Coordinates** - collective term for local/world coordinate system in which RA operates currently. 
