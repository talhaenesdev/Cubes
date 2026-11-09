You are working in the Unity project at `D:\UnityProjects\Cubes`, a 2D pixel-art side-scroller called CUBES. Redraw the back-wall image of every place listed below in one go, without asking me anything in between, and overwrite each file at the exact path given. Do not change any other file: no code, no prefab, no scene, no .meta file, and no other image.

## What is wrong with the current walls
The current `BG_<Place>_Wall.png` images are full of furniture and objects - punching bags, lockers, boxing rings, counters, stools, tables, shelves, pie cases, barrels. The game places its own doors, crates, cover, stairs, computers and characters in front of the wall, so the drawn objects clash with them and the scene reads as clutter. The new walls must be calm, empty backdrops.

## Rules for every wall
- Size 1920x1080, PNG. The image is one storey of a building seen from the inside, side view: the bottom edge is the floor line, the top edge is the ceiling line. Fill the whole height - no transparent band at the top or bottom.
- Seamless left to right: the left and right edges must match exactly, because the image repeats along the building.
- Draw ONLY the wall surface and things fixed flat onto it:
  - the wall material (wallpaper pattern, brick, wood panelling, tiles, plaster, marble - per place below),
  - a lower wainscot or skirting band,
  - a thin cornice or moulding under the ceiling,
  - 2 windows at most, and their inside (glass) is fully transparent so the far layer shows through,
  - 1-2 wall lamps or sconces with a soft pool of light,
  - at most one small flat item such as a framed picture, a sign or a pipe run.
- NOTHING standing on the floor or in front of the wall: no furniture, no counters, no tables, no chairs, no stools, no machines, no lockers, no bags, no barrels, no crates, no shelves with goods, no doors, no people.
- Large calm areas: a quiet, low-contrast pattern. Keep it dark and slightly desaturated so characters and objects in front read clearly - the wall's brightest pixels should be the lamp light, nothing else.
- Same pixel art style and pixel size as the existing images: hard pixel edges, 1-pixel dark outline on the few objects, flat shading with one highlight and one shadow tone. No blur, no gradients, no text or letters.
- Palette base: near-black #0B0806, ivory #E2D6BE, gold #C4A05C, crimson #8C161C, mint #5CC78C, plus the place's accent below.

## Files (save to `Assets/Modules/GameplayModule/Art/Incoming/Scenes/`)
| File | Wall |
|---|---|
| `BG_Hideout_Wall.png` | faded dark-red damask wallpaper, a few peeling patches, wooden wainscot, one window with blinds, one desk-lamp-style wall sconce |
| `BG_NightShift_Wall.png` | plain office wall, grey-green paint, a chair rail, one window, one cold tube light |
| `BG_Diner_Wall.png` | cream and crimson tiled wall, chrome strip trim, one big window, one round wall lamp |
| `BG_Laundromat_Wall.png` | pale green square tiles, a tube light, a small notice board, one window |
| `BG_PawnShop_Wall.png` | dark wood panelling, one barred window, one hanging-bulb sconce, one small clock on the wall |
| `BG_Gym_Wall.png` | plain red-brown brick, a pipe running along the top, one arched window, one cage lamp |
| `BG_Cinema_Wall.png` | red velvet wall panels with gold trim, one gold sconce, one framed poster with no text |
| `BG_Distillery_Wall.png` | old brick with copper pipes along the top, one round window, green-tinted lamp |
| `BG_Docks_Wall.png` | corrugated warehouse wall, wooden beams, one high window, one cage lamp |
| `BG_Hotel_Wall.png` | purple striped wallpaper, dark wood wainscot, one brass sconce, one framed painting |
| `BG_Casino_Wall.png` | dark green wallpaper with gold diamond pattern, gold moulding, one gold mirror, one sconce |
| `BG_Precinct_Wall.png` | two-tone painted wall (dark blue below, grey above), one window with bars, one cold lamp |
| `BG_Penthouse_Wall.png` | black marble with gold lines, one tall window, one gold sconce |

Do not redraw the `_Far` or `_Floor` images, the city map, the menus or any UI piece.

## When you are done
List every file you wrote and name any you could not make. Do not open or run Unity, and do not build or test anything.
