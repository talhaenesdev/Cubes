You are working in the Unity project at `D:\UnityProjects\Cubes`, a 2D pixel-art game called CUBES. Design the on-screen touch controls for the phone version and save every file listed below in one go, without asking me anything in between. Do not change any other file: no code, no prefab, no scene, no .meta file, and no existing image.

## Save to
`Assets/Modules/GameplayModule/Art/Incoming/Touch/` - create the folder if it is not there.

## The game and where these sit
CUBES is a 1940s film-noir mafia side-scroller at night: rain, neon, smoky rooms, warm lamp light against deep shadow. The hero is a trench-coat rectangle hitman (see `Assets/Modules/GameplayModule/Art/Sprites/Characters/SPR_Character_Rectangle.png` and the app icon `Assets/Modules/GameplayModule/Art/Incoming/Icon/ICON_Cubes_512.png`).

On a phone the controls float over the game: a movement stick in the bottom-left corner, and a cluster of round action buttons in the bottom-right corner. The game is played over them for the whole run, so they must:
- read clearly over both dark rooms and bright lit walls,
- stay out of the way: semi-transparent dark body, so the game shows through, with a solid, crisp rim and symbol,
- look like they belong to the game - 1940s noir objects, not generic flat mobile circles.

## The look
Each button is a **1940s brass-rimmed button**, like the keys of an old radio, a jukebox or a telephone dial:
- a round body of smoked dark glass (near-black #0B0806 at about 55% opacity), so the room behind shows through,
- a thick polished brass/gold rim (#C4A05C, with one lighter highlight on the top-left and one darker shadow on the bottom-right),
- a 1-pixel near-black outline outside the rim, so it reads on bright walls,
- the symbol in the middle in solid ivory #E2D6BE with a near-black outline, big and bold, readable at a glance with a thumb half over it,
- a small accent per button as a thin inner ring in its colour (listed below).

Pixel art with hard pixel edges, flat shading with one highlight and one shadow tone. Draw each piece on a small pixel grid and scale it up with nearest-neighbour so every pixel stays a crisp square: a 256 px button is drawn on a 64 px grid and scaled 4x. No blur, no smooth gradients, no anti-aliasing, no drop shadows outside the piece, no text except the letters asked for.

Every piece is centred on a fully transparent square canvas. Every pressed version has **exactly the same canvas size** as its normal version and the object in the same place, so the game can swap them without anything jumping.

## The movement stick
| File | Size | Draw |
|---|---|---|
| `TOUCH_Stick_Base.png` | 512 x 512 | the stick's base: a large round smoked-glass dial with a brass rim, like an old telephone rotary dial seen from the front - four small brass arrow notches at top, bottom, left and right, faint radial lines between them. The middle stays empty for the knob. |
| `TOUCH_Stick_Knob.png` | 256 x 256 | the knob the thumb drags: a solid domed brass knob with a darker ring of grip ridges, a bright highlight on the top-left, clearly opaque so it stands out over the base |
| `TOUCH_Stick_Knob_Pressed.png` | 256 x 256 | the same knob held down: slightly darker, the highlight smaller, a mint #5CC78C glow ring just inside the canvas edge |

## The action buttons (bottom right)
| Normal | Pressed | Symbol | Accent ring |
|---|---|---|---|
| `TOUCH_Button_Use.png` | `TOUCH_Button_Use_Pressed.png` | an open hand, palm forward, with a small letter E in the lower right | mint #5CC78C |
| `TOUCH_Button_Jump.png` | `TOUCH_Button_Jump_Pressed.png` | a bold up arrow, like a vintage lift-call arrow | ivory |
| `TOUCH_Button_Melee.png` | `TOUCH_Button_Melee_Pressed.png` | a clenched fist seen from the side, with brass knuckles | crimson #8C161C |
| `TOUCH_Button_Reload.png` | `TOUCH_Button_Reload_Pressed.png` | a revolver cylinder seen from the front with a curved arrow around it | gold |
| `TOUCH_Button_Swap.png` | `TOUCH_Button_Swap_Pressed.png` | two pistols crossed, or a pistol and a shotgun with a two-way arrow between them | gold |
| `TOUCH_Button_Song.png` | `TOUCH_Button_Song_Pressed.png` | a music note over a small cassette tape | mint |

All of them 256 x 256. The pressed version: the body a little darker and pushed in (the rim's highlight moves to the bottom-right, the symbol shifts 1 grid pixel down), and the accent ring glows brighter, so the thumb sees it took.

## Extras
| File | Size | Draw |
|---|---|---|
| `TOUCH_Button_Blank.png` | 256 x 256 | the button with no symbol and no accent ring, for any future action |
| `TOUCH_Button_Blank_Pressed.png` | 256 x 256 | its pressed version |
| `TOUCH_Preview.png` | 1920 x 1080 | a mock-up of the phone screen: a dark game room in the background, the stick base with its knob in the bottom-left, and the six buttons in a fan-shaped cluster in the bottom-right (Use biggest, nearest the thumb; Jump and Melee next to it; Reload, Swap and Song smaller and further out), so I can see them together at real size |

## When you are done
List every file you wrote and name any you could not make. Do not open or run Unity, and do not build or test anything.
