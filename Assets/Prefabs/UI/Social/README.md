This folder contains UI-related placeholder notes for the SocialHub scene.

How to use:
- Add the SocialUIManager component to an empty GameObject in the SocialHub scene (or simply leave it in the scene as a child of the SocialHub root). It will build an on-screen compose UI at runtime.
- Requires Unity UI (UnityEngine.UI). The script builds a minimal set of controls at runtime so no binary prefab is needed.

Next improvements:
- Replace runtime-built UI with designer-made prefabs (Buttons, InputField, styled panels) using Kenney UI assets.
- Add localization for UI labels.
