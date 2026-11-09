You are working in the Unity project at `D:\UnityProjects\Cubes`, a 2D side-scrolling pixel-art action game called CUBES. Draw every object in the list below in one go, without asking me anything in between, and save each one as its own PNG under the exact file name given. Do not change any other file: no code, no prefab, no scene, no .meta file, and no existing image.

## Where to save
`Assets/Modules/GameplayModule/Art/Incoming/Props/` - create the folder if it is not there.

## The game and its look
- CUBES is a 1940s film-noir mafia story at night: rain on the windows, neon outside, smoky rooms, warm lamp light against deep shadow. The hero is a hitman who lives in a shabby flat over a bar and takes contracts against rival families in diners, laundromats, pawn shops, gyms, cinemas, distilleries, docks, hotels, casinos, a police precinct and a penthouse.
- The rooms are seen from the side as a cutaway, like a dollhouse. The back walls are now calm and empty: plain wallpaper, brick or tiles, a couple of windows and lamps. **Your objects are what stands in front of that wall and fills the room**, so they carry the detail the walls no longer have - but each one must still read clearly at a glance, as one solid object, because the player shoots, hides behind, kicks open or uses them.
- Pixel art with hard pixel edges. A 1-pixel near-black outline around every object. Flat shading: one base tone, one highlight tone on the top-left, one shadow tone on the bottom-right. No blur, no gradients, no anti-aliasing, no dithering noise, no photo textures.
- Palette: near-black #0B0806, ivory #E2D6BE, gold #C4A05C, crimson #8C161C, mint #5CC78C, dark wood #3A2418, mid wood #6B4428, steel #4A5560, dark teal #1E3A3A. Keep the colours a little muted and dark, like the existing characters, so that characters always stand out in front.
- Match the existing characters exactly in pixel size and detail level: open `Assets/Modules/GameplayModule/Art/Sprites/Characters/SPR_Character_Rectangle.png` - a character is about **145 px tall** in this scale. Every size below is given in that same scale, so a desk is about half a character tall. Do not draw objects bigger or with finer pixels than that.

## Rules for every image
- Exactly one object per file, seen straight from the side (no perspective, no top-down, no 3/4 view), standing on the bottom edge of the image.
- Fully transparent background. No ground shadow, no floor, no wall behind, no text, no letters, no numbers, no logos - except where an item says so.
- Crop tightly: the object touches the bottom edge and has at most 2 px empty margin on the other sides.
- The object faces right, or is symmetric.
- Where an item has states (closed/open, whole/broken, on/off), every state is its own file of **exactly the same canvas size**, with the object in the same place, so the game can swap them without anything jumping.

## The objects

### Level fixtures (used in every contract level)
| File | Size (px, w x h) | Draw |
|---|---|---|
| `PROP_Door_Closed.png` | 64 x 150 | a heavy 1940s interior wooden door in its frame, panelled, brass knob, small frosted glass pane at head height |
| `PROP_Door_Open.png` | 64 x 150 | the same frame with the door swung open toward the viewer - only the edge of the door visible at the hinge side, dark room beyond |
| `PROP_Door_Broken.png` | 64 x 150 | the same frame, door kicked in: hanging from one hinge, splintered |
| `PROP_StairDoor.png` | 64 x 150 | a stairwell door: metal fire door, darker, with a small wired-glass window and a brass push bar; above it a small plaque area left blank |
| `PROP_Exit.png` | 64 x 150 | a back-alley exit door, dark green metal, a small lamp above it (the game writes EXIT itself) |
| `PROP_Crate.png` | 96 x 64 | cover: a sturdy wooden shipping crate with metal corners and stencil-like marks (no readable letters) |
| `PROP_Crate_Broken.png` | 96 x 64 | the same crate shot to pieces, half collapsed |
| `PROP_Vent.png` | 48 x 32 | a metal air vent grille at floor level, with screws |
| `PROP_Vent_Open.png` | 48 x 32 | the same vent with the grille hanging open, dark duct inside |
| `PROP_LightSwitch.png` | 16 x 24 | a 1940s round brass light switch on a small wall plate |
| `PROP_Desk.png` | 112 x 70 | a heavy office desk with drawers, dark wood |
| `PROP_Computer.png` | 64 x 56 | sits on the desk: an old green-screen terminal monitor with a chunky keyboard in front, screen glowing dark mint |
| `PROP_Computer_Off.png` | 64 x 56 | the same, screen dark |
| `PROP_Document.png` | 32 x 24 | a manila folder with papers sticking out and a red string tie |

### Breakable props (shot, they react)
| File | Size | Draw |
|---|---|---|
| `PROP_CeilingLamp.png` | 48 x 64 | an industrial ceiling lamp: cord from the top edge, enamel shade, warm bulb - drawn hanging, top of cord at the top edge |
| `PROP_CeilingLamp_Fallen.png` | 48 x 24 | the same lamp crashed on the floor, shade dented, bulb broken |
| `PROP_Bottles.png` | 48 x 40 | a small cluster of whiskey and gin bottles and a glass, on nothing (they stand on a counter or shelf the game draws) |
| `PROP_Bottles_Broken.png` | 48 x 40 | the same, smashed: necks and shards |
| `PROP_Monitor.png` | 56 x 48 | a bulky 1940s TV/radio-console monitor on short legs, screen glowing |
| `PROP_Monitor_Broken.png` | 56 x 48 | the same with a shattered dark screen |
| `PROP_Barrel.png` | 44 x 64 | an explosive oil drum: crimson with an ivory band and a black diamond hazard mark (no text) |
| `PROP_Plant.png` | 40 x 72 | a potted palm in a terracotta pot |
| `PROP_Plant_Broken.png` | 40 x 72 | the same, pot burst, soil spilled, plant flopped over |

### Room furniture (dressing for every level - place-specific)
| File | Size | Draw |
|---|---|---|
| `PROP_Table_Round.png` | 80 x 60 | a small round table with a tablecloth (diner, casino, hotel) |
| `PROP_Chair.png` | 36 x 70 | a bentwood chair, facing right |
| `PROP_Stool.png` | 28 x 50 | a chrome diner bar stool with a red seat |
| `PROP_Counter.png` | 160 x 70 | a bar or diner counter, chrome edge, wood front |
| `PROP_Shelf.png` | 96 x 110 | a free-standing wooden shelf with jars, boxes and a few books |
| `PROP_FilingCabinet.png` | 44 x 100 | a steel four-drawer filing cabinet |
| `PROP_WashingMachine.png` | 64 x 70 | a 1940s front-loading laundromat washer, round porthole door |
| `PROP_PunchingBag.png` | 40 x 100 | a hanging leather punching bag on a chain, top of chain at top edge |
| `PROP_SlotMachine.png` | 48 x 100 | a one-armed bandit slot machine, chrome and red |
| `PROP_RouletteTable.png` | 140 x 64 | a casino roulette table with the wheel at one end, green felt |
| `PROP_Still.png` | 90 x 120 | a copper distillery still with a coiled pipe |
| `PROP_Piano.png` | 120 x 90 | an upright piano, dark wood |
| `PROP_Sofa.png` | 120 x 64 | a leather chesterfield sofa, dark red |
| `PROP_Coatstand.png` | 32 x 110 | a wooden coat stand with a hat and a trench coat |
| `PROP_FloorLamp.png` | 36 x 120 | a tall floor lamp with a fringed shade, glowing |
| `PROP_Clock_Grandfather.png` | 44 x 140 | a grandfather clock, dark wood, brass pendulum |
| `PROP_Clock_Wall.png` | 32 x 32 | a round wall clock, ivory face, hands only, no numbers |
| `PROP_Telephone.png` | 24 x 20 | a black candlestick telephone |
| `PROP_Typewriter.png` | 36 x 24 | a black typewriter with a sheet of paper |
| `PROP_Radio.png` | 40 x 36 | a wooden table radio with a glowing dial |
| `PROP_Safe.png` | 56 x 64 | a heavy black safe with a gold dial |

### The hideout (the hero's flat)
| File | Size | Draw |
|---|---|---|
| `PROP_Bed.png` | 170 x 56 | a narrow iron bed with a thin mattress, a rumpled grey blanket and one pillow |
| `PROP_Wardrobe.png` | 80 x 140 | a tall dark-wood wardrobe with a mirror on one door, doors closed |
| `PROP_Wardrobe_Open.png` | 80 x 140 | the same with both doors open, suits on hangers inside |
| `PROP_WeaponLocker.png` | 72 x 130 | a steel gun locker, doors closed, a padlock |
| `PROP_WeaponLocker_Open.png` | 72 x 130 | the same open: pistols, a shotgun and a rifle on pegs |
| `PROP_UpgradeBench.png` | 130 x 80 | a gunsmith's workbench: vice, tools on a pegboard strip, gun parts, a desk lamp |
| `PROP_CassettePlayer.png` | 72 x 48 | a chunky tape deck / cassette player with big keys and two visible reels, on its own (it stands on a table) |
| `PROP_CassetteTable.png` | 90 x 60 | the small wooden side table the cassette player stands on |
| `PROP_MissionBoard.png` | 110 x 90 | a cork board on an easel stand with pinned photos, notes and red string (no readable text) |
| `PROP_FamilyBoard.png` | 110 x 90 | a dark chalkboard on an easel stand with six empty frames in two rows, for the rival families' marks |
| `PROP_Couch.png` | 130 x 64 | a worn brown couch with a sagging cushion |
| `PROP_DeskLamp.png` | 28 x 36 | a green banker's desk lamp, glowing |
| `PROP_Rug.png` | 200 x 12 | a threadbare patterned rug seen edge-on, lying flat on the floor |
| `PROP_Cat.png` | 48 x 32 | a black alley cat sitting, facing right, yellow eyes (just one pose) |
| `PROP_Whiskey.png` | 20 x 28 | a bottle of whiskey and a tumbler |
| `PROP_RecordStack.png` | 32 x 32 | a stack of records and cassette tapes |

## When you are done
List every file you wrote, grouped by the sections above, and name any you could not make. Do not open or run Unity, and do not build or test anything.
