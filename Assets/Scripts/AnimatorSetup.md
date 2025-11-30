# Animator setup for `SimpleCharacterController`

Follow these steps to drive your idle and walk animations using the controller's parameters:

1. **Assign the Animator**
   - Drag the player's `Animator` onto the `Animator` field of `SimpleCharacterController` (or leave it empty to let the script find one on child objects at runtime).

2. **Wire the speed float (optional but recommended)**
   - In the inspector, set **Speed Parameter** to the float your blend tree or transitions read (e.g., `Speed`).
   - The controller updates it with damped planar speed every frame, allowing smooth idle–walk blending.

3. **Mark when the character is moving**
   - Set **Walking Bool Parameter** to a bool in your Animator (e.g., `IsWalking`).
   - The controller toggles it when planar speed crosses **Walk Speed Threshold** (default `0.1`).

4. **Fire discrete transitions (optional)**
   - Set **Start Walking Trigger** (e.g., `StartWalk`) and **Stop Walking Trigger** (e.g., `StopWalk`).
   - These fire once when movement starts or stops, after resetting the opposite trigger to avoid stuck states.

5. **Test in Play Mode**
   - Press Play and move with WASD; adjust **Walk Speed Threshold** or **Speed Damp Time** if transitions feel too sensitive or sluggish.

6. **Animator graph expectations**
   - Idle ↔ Walk: use the bool or triggers for discrete transitions, or use the speed float to blend.
   - Ensure exit conditions reference the same parameters you configured above.
