You are working in the Unity project at `D:\UnityProjects\Cubes`. Generate the pixel-art animation strips for every character listed below and save each one into `Assets/Modules/GameplayModule/Art/Sprites/Characters/` under the exact file name given. Do it for all characters in one go, without asking me anything in between. Do not change any other file: no code, no prefab, no scene, no .meta file.

## Reference
For each character, the still drawing `Assets/Modules/GameplayModule/Art/Sprites/Characters/SPR_Character_<Name>.png` is the exact reference. Keep the same design, colours, clothes, proportions, outline thickness and pixel size. The finished Rectangle strips (`SPR_Character_Rectangle_*.png`) in the same folder show the style, layout and poses to copy; use them as the template for every move.

## Rules for every image
- Side view, facing right.
- All frames of one move side by side in one horizontal strip, with clear background space between frames.
- Flat pure green #00FF00 background. Nothing else in the image: no ground shadow, no text, no frame borders, no numbers.
- The feet stand on the same baseline in every frame, and the character is the same size in every frame and in every strip.
- PNG, about 2000 px wide; the character should be about as tall as in the Rectangle strips.
- Square is green itself: draw it in a clearly darker green than the #00FF00 background so the two never touch in colour.
- Never overwrite an existing `SPR_Character_<Name>.png` still, and never touch any `SPR_Character_Rectangle_*` file.

## The moves (file name suffix → what to draw)
| Suffix | Frames | Draw |
|---|---|---|
| `_Idle` | 4 | standing idle, subtle breathing, hands relaxed or in pockets |
| `_Walk` | 6 | walk cycle, feet clearly alternating |
| `_Run` | 6 | run cycle, leaning forward, long strides |
| `_WalkBackward` | 6 | still facing right, stepping backwards carefully |
| `_Crouch` | 2 | crouched still; crouched mid-step |
| `_CrouchWalk` | 6 | crouched sneak walk, small careful steps |
| `_IdleArmed` | 4 | idle breathing, NO arms and NO sleeves visible at all, clean body silhouette from shoulder down |
| `_WalkArmed` | 6 | walk cycle, no arms or sleeves visible |
| `_RunArmed` | 6 | run cycle, no arms or sleeves visible |
| `_CrouchArmed` | 2 | crouched, no arms or sleeves visible |
| `_CrouchWalkArmed` | 6 | crouched sneak walk, no arms or sleeves visible |
| `_Arm` | 1 | a single straight arm pointing right: sleeve in the character's clothing colour, closed fist at the end, nothing else in the image |
| `_Jump` | 3 | take-off, in the air, landing |
| `_Melee` | 4 | a punch forward |
| `_Kick` | 4 | a forward kick, as if kicking a door open |
| `_Hurt` | 2 | flinching back from a hit |
| `_Death` | 4 | hit, knees buckling, falling, lying on the ground |

## Characters and which moves each needs
- **Square, Triangle, Circle, Pentagon, Hexagon, Diamond** (enemies): every move in the table.
- **ShopOwner**: `_Idle`, `_Walk`, `_Run`, `_Crouch`, `_CrouchArmed`, `_Arm`, `_Hurt`, `_Death`.
- **Civilian**: `_Idle`, `_Walk`, `_Run`, `_Crouch`, `_Hurt`, `_Death`.

The file name is always `SPR_Character_<Name><Suffix>.png`, for example `SPR_Character_Triangle_WalkArmed.png` or `SPR_Character_Civilian_Hurt.png`.

## When you are done
List every file you wrote, grouped by character, and name any you could not make. Do not run Unity, any build or any test.
