# Unity Interactable System

## Overview

This project contains a modular interactable system for Unity that allows players to focus on objects, see tooltips, interact with them, and view detailed object information. It includes multiple interactable types and a centralized `GameService` managing core services like input, UI, sound, and events.

---

## Script Setup

- **Interactables**
  - `IInteractables.cs` — Interface all interactables implement.
  - `DoorInteractable.cs`, `SwitchInteractable.cs`, `LetterInteractable.cs`, `Photointeractable.cs` — Example interactable implementations.

- **Services**
  - `GameService.cs` — Singleton managing global services.
  - `EventService.cs` — Event broadcasting system.
  - `InputService.cs` — Handles input (ensure this matches your input system) or change it to your version.
  - `SoundService.cs` — Manages sound playback.
  - `UIService.cs` — Controls UI prompts for interaction.

- **UI**
  - `ObjectViewer.cs` — Manages object viewing UI and controls.

---

## Unity Scene Setup


1. **Setup Interactable Objects**:
   - Add any of the interactable scripts (e.g., `DoorInteractable`, `LetterInteractable`) to your GameObjects.
   - Ensure the GameObjects have a `Collider` and `Renderer` component.
   - Configure the display name, description, and tooltip fields in the inspector.

2. **Setup Camera & Raycasting**:
   - Attach your main camera to the `Interactor` script (if you have one for raycasting interactions).

3. **Layer Management**:
   - Assign your interactable objects to a specific layer  "Interactable".

4. **To view the object**: 
   - Use a dedicated camera configured specifically for rendering interactable objects. Follow these steps:

   - Culling Mask: Set the camera's culling mask to include only the InteractObject layer.

   - Layer Assignment: Ensure all interactable objects are assigned to the InteractObject layer.

   - Render Texture: Assign a RenderTexture to the camera's Output Target Texture field.

   - Camera Depth: Set the camera's Depth to 2 to control rendering order.

   - Field of View: Set the camera's FOV to 28.

   - Clipping Planes: Configure the Near clipping plane to 0.01 and the Far clipping plane to 100.

   - Camera Layer: Assign the camera itself to the InspectObject layer to properly isolate it in the scene.

     This setup ensures the dedicated camera renders only the interactable objects cleanly and efficiently for your object viewer.

---

## Usage

- When the player looks at an interactable object within range:
  - The tooltip text displays the distance or interaction hint.
  - Pressing the interact key (e.g., "E") triggers the object's interaction (`OnInteract()`).
  - The `ObjectViewer` shows detailed information and allows rotation/zoom.

- UI prompts show and hide automatically via events from `GameService`.

---

## Important Notes & Tips

- **Event Subscription**  
  The `UIService` subscribes to events from `EventService` to show/hide interaction UI. Ensure `UIService` is properly initialized via `GameService`.

- **Input System**  
  Replace or extend `InputService` to handle your game's input. The current system checks for `InteractPressed`.

- **Materials & Colors**  
  Interactables change color on focus/interact for feedback. Customize materials or shaders as needed.

- **Camera Controls**  
  The `ObjectViewer` script controls camera orbit and zoom around the viewed object.

- **Scripts should be assigned in the Inspector**  
  Most services require references set in the inspector to work correctly.

---

## Common Gotchas

- Always assign the **correct layers** to interactable objects and set the layer mask accordingly.
- Assign all service references in `GameService` inspector to avoid `NullReferenceException`.
- Ensure your interactable GameObjects have colliders and renderers.

---

## Extending the System

- Add new interactable types by implementing `IInteractables`.
- Expand `GameService` to include more global managers.
- Customize the UI to add more interaction feedback.

---
