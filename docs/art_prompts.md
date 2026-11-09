# CUBES - AI art prompts

Every prompt below starts with the **style block**: paste it in front of each one, so the whole game comes out in
one look. Generate each item as its own image. When an image is ready, save it with the **file name** given and drop
it into `Assets/Modules/GameplayModule/Art/Incoming/` - then tell Claude "yerleştir" and it wires them into the
prefabs.

## Rules for every image

- **Transparent background** (PNG with alpha). No floor, no shadow on the ground, no frame, no text.
- **Side view, facing right** (the game flips it for left).
- **Pixel art, hard edges, no anti-aliasing, no blur, no gradients.** A limited palette.
- **One object per image, centred**, with a few empty pixels around it.
- Sizes are the *pixel-art* size. If the AI only makes big images, that is fine: ask for the look of that pixel
  size and Claude scales it down with nearest-neighbour so the pixels stay sharp.

## Style block (paste first, every time)

```
Pixel art game sprite, 1940s film noir, side-scrolling stealth shooter. The characters are geometric shapes
(a rectangle, a triangle, a circle...) with small hats, coats and simple arms and legs - charming, readable,
not realistic. Palette: near-black (#0B0806), ivory (#E2D6BE), gold (#C4A05C), crimson (#8C161C), plus the
item's own family colour. Strong silhouette, 1-pixel dark outline, flat shading with one highlight tone and one
shadow tone. Transparent background, no ground, no text, no watermark. Side view, facing right.
```

---

## 1. Characters (64 x 96 px each, standing pose)

The shape is the body. Every family wears its colour on the hat band, tie or scarf.

| File name | Prompt (after the style block) |
|---|---|
| `CHR_Rectangle.png` | The hero: a tall upright ivory rectangle in a long dark trench coat and a fedora with a crimson band, collar up, cigarette, a revolver in a holster. Calm and charismatic. |
| `CHR_Square.png` | A square-bodied friendly henchman in a dark hooded coat with a muted green (#5CC78C) scarf. Loyal, stocky. |
| `CHR_Triangle.png` | A triangle-bodied mobster, point up, in a pinstripe suit and a red (#D93D33) tie, flat cap, mean eyebrows. |
| `CHR_Circle.png` | A round circle-bodied mobster in a waistcoat and a blue (#528FEB) bow tie, bowler hat, friendly smile. |
| `CHR_Pentagon.png` | A pentagon-bodied mobster in a long purple (#A366E0) coat with a fur collar, wide-brimmed hat. |
| `CHR_Hexagon.png` | A hexagon-bodied brute in rolled sleeves and an orange (#F29933) vest, bald, broad. |
| `CHR_Diamond.png` | A diamond-bodied (rhombus, point up) dandy in a cream suit with a gold-yellow (#EBD64D) tie and a cane. |
| `CHR_Civilian.png` | A small plain circle-bodied civilian in a cardigan, no hat, scared expression, hands up. |
| `CHR_Boss.png` | A shop owner: a heavy circle-bodied man in an apron over a shirt, moustache, sleeves rolled. |

Optional extra poses for each character, same size, same file name with a suffix:
`_Crouch` (crouched behind cover), `_Aim` (arm out, aiming a revolver), `_Dead` (lying flat, 96 x 40 px).

## 2. Weapons (side view, pointing right)

| File name | Size | Prompt |
|---|---|---|
| `WPN_Revolver.png` | 32 x 16 | A six-shot revolver, dark steel, wooden grip. |
| `WPN_Pistol.png` | 28 x 16 | A 1940s semi-automatic pistol, black. |
| `WPN_Shotgun.png` | 56 x 16 | A pump shotgun, wooden stock. |
| `WPN_SilencedPistol.png` | 40 x 16 | A 1940s pistol with a long cylindrical silencer. |
| `WPN_Smg.png` | 56 x 24 | A Thompson submachine gun with a drum magazine. |
| `WPN_Rifle.png` | 64 x 16 | A bolt-action rifle, long, wooden. |
| `PKP_Cash.png` | 16 x 12 | A small bundle of dollar bills with a band, dropped on the floor. |

## 3. Props and fixtures

| File name | Size | Prompt |
|---|---|---|
| `PRP_Crate.png` | 48 x 40 | A wooden shipping crate with stencilled marks, used as cover. |
| `PRP_Barrel.png` | 32 x 44 | A red oil barrel with a hazard mark - it explodes. |
| `PRP_Bottles.png` | 32 x 24 | A row of liquor bottles on a little shelf. |
| `PRP_Plant.png` | 28 x 44 | A potted plant in a ceramic pot. |
| `PRP_Monitor.png` | 32 x 28 | A 1940s radio / teletype terminal with a glowing green screen. |
| `PRP_Lamp.png` | 24 x 32 | A hanging ceiling lamp, metal shade, warm light. |
| `FIX_Door.png` | 24 x 72 | A wooden interior door, closed, brass handle. |
| `FIX_DoorOpen.png` | 24 x 72 | The same door swung open, seen edge-on. |
| `FIX_StairDoor.png` | 32 x 72 | A stairwell door with a small window and an up/down arrow plate. |
| `FIX_Vent.png` | 32 x 20 | A wall air vent grille. |
| `FIX_LightSwitch.png` | 12 x 16 | A brass wall light switch. |
| `FIX_Exit.png` | 40 x 72 | A back-alley exit door with a red glowing EXIT lamp above it (the word may be a glow only). |
| `FIX_Computer.png` | 32 x 32 | A desk with a typewriter-style computer terminal. |

## 4. The hideout

| File name | Size | Prompt |
|---|---|---|
| `HID_Desk.png` | 96 x 48 | A heavy dark wooden desk with drawers, papers and an ashtray on top. |
| `HID_DeskClock.png` | 32 x 20 | A small 1940s bedside alarm clock with a dark case and an empty amber digital face (the game draws the digits). |
| `HID_Wardrobe.png` | 56 x 88 | A tall wooden wardrobe, one door ajar, a trench coat inside. |
| `HID_WeaponLocker.png` | 48 x 80 | A green metal weapon locker with a rifle rack. |
| `HID_UpgradeBench.png` | 80 x 48 | A workbench with tools, a vice and gun parts. |
| `HID_CassettePlayer.png` | 48 x 28 | A chunky cassette tape deck with two reels and big buttons. |
| `HID_MissionBoard.png` | 72 x 56 | A cork board with a city map, pins and red string. |
| `HID_FamilyBoard.png` | 72 x 56 | A cork board with five shape-family photos and red string between them. |
| `HID_Cat.png` | 32 x 20 | A small black cat walking, yellow eyes. |
| `HID_Neon.png` | 64 x 32 | A neon sign: a rectangle, a heart and a square, glowing pink and gold. |

## 5. Backgrounds (one per location, 640 x 360 px, **opaque**, no characters)

Drawn behind the level, so keep it dark and low-contrast.

```
Pixel art background, 1940s noir city at night, rain, side view, dark and low contrast so characters in front
stay readable, no characters, no text, 640x360.
```

Then add one of: `BG_Laundromat` (washing machines, steam), `BG_Diner` (counter, stools, neon), `BG_PawnShop`
(cluttered shelves, guitars), `BG_Gym` (boxing ring, punch bags), `BG_Cinema` (red seats, projector beam),
`BG_Hotel` (lobby, chandelier), `BG_Distillery` (copper stills, barrels), `BG_Casino` (roulette, green felt),
`BG_Precinct` (desks, filing cabinets, blinds), `BG_Penthouse` (city skyline window, piano), `BG_Docks` (cranes,
containers, sea), `BG_NightShift` (an office building at night), `BG_Hideout` (a cosy attic apartment, brick walls).

## 6. Effects (small sprite sheets, frames side by side in one row)

| File name | Frame size x frames | Prompt |
|---|---|---|
| `FX_MuzzleFlash.png` | 16 x 16 x 3 | A muzzle flash, yellow-white, 3 frames growing then fading. |
| `FX_Explosion.png` | 64 x 64 x 6 | A barrel explosion, orange fire and black smoke, 6 frames. |
| `FX_Blood.png` | 16 x 16 x 4 | A small dark red blood splat, 4 frames. |
| `FX_Sparks.png` | 16 x 16 x 3 | Bullet sparks on metal, 3 frames. |
| `FX_Smoke.png` | 32 x 32 x 4 | A small puff of grey smoke, 4 frames. |
| `DCL_BulletHole.png` | 6 x 6 | A bullet hole in a wall. |
| `DCL_BloodPool.png` | 32 x 8 | A dark blood pool on the floor. |

## 7. Interface

| File name | Size | Prompt |
|---|---|---|
| `UI_MapPin.png` | 92 x 70 | A tiny top-down shop building seen from above, a flat roof left plain grey (the game tints it with the family colour), on transparent background. |
| `UI_CityMap.png` | 1280 x 900 | A top-down pixel-art night map of a 1940s harbour city: a harbour at the bottom, a river on the right third, two parks, streets in a grid, no labels, dark and muted. |
| `UI_SpeechBubble.png` | 48 x 32 | A comic speech bubble, ivory with a dark outline and a tail at the bottom left, empty inside. |
| `UI_Icon_Pin.png`, `UI_Icon_Wrench.png`, `UI_Icon_Hanger.png`, `UI_Icon_Bullet.png`, `UI_Icon_Note.png` | 16 x 16 | A single gold pixel-art icon: a map pin / a wrench / a coat hanger / a bullet / a music note. |
| `UI_AppIcon.png` | 512 x 512, opaque | The game's icon: the rectangle hero in his fedora, close up, crimson background, gold rim. |
