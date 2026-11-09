You are working in the Unity project at `D:\UnityProjects\Cubes`, a 2D pixel-art game called CUBES. Make every image in the list below in one go, without asking me anything in between, and save each one under the exact path and file name given. Do not change any other file: no code, no prefab, no scene, no .meta file, and nothing under `Art/Sprites/Characters/`.

## Look of the whole game
- 1940s film noir, a mafia city at night: rain, neon signs, wet streets, smoky rooms, warm lamp light against deep shadow.
- Pixel art with hard pixel edges, a 1-pixel dark outline on objects, flat shading with one highlight and one shadow tone. No blur, no smooth gradients, no photo textures.
- Palette: near-black #0B0806, ivory #E2D6BE, gold #C4A05C, crimson #8C161C, mint #5CC78C, plus each place's own accent colour.
- Match the existing characters in `Assets/Modules/GameplayModule/Art/Sprites/Characters/` (open `SPR_Character_Rectangle.png`): same pixel size and the same level of detail. A character is about 96 px tall in this pixel scale.
- No text, letters, logos or numbers in any image unless the item asks for them. No characters or people in any background.

## 1. Scenes (side-scrolling level backgrounds)
Save to `Assets/Modules/GameplayModule/Art/Incoming/Scenes/`. Each place gets three images, seen from the side as a cutaway of the interior, 1920x1080, made so they can sit behind the gameplay:
- `BG_<Place>_Far.png` - the far layer: what is seen through windows, or the city skyline, sky and rain. Fully opaque.
- `BG_<Place>_Wall.png` - the back wall of the rooms: wallpaper, panelling, windows, framed pictures, signs, pipes, lamps on the wall. Transparent where the window shows the far layer.
- `BG_<Place>_Floor.png` - a tileable floor and ceiling strip for that place, 512x128, seamless left to right.

The places, with what each one should feel like:
| Place | Mood |
|---|---|
| Hideout | the hero's shabby flat over a bar: peeling wallpaper, a desk lamp, a radio, blinds with neon light coming through |
| NightShift | an office building at night, a security desk, long corridors, one light left on |
| Diner | a 24-hour diner, chrome counter, red stools, checkered floor, pie case, neon OPEN glow outside |
| Laundromat | rows of washing machines, steam, flickering tube lights, tiled walls |
| PawnShop | cluttered shelves, guitars, clocks, a caged counter, dusty light |
| Gym | a boxing gym: ring ropes, punching bags, lockers, sweat-stained brick |
| Cinema | a picture palace: red velvet, gold trim, a lobby with a candy counter, a projector beam |
| Distillery | copper stills, barrels, pipes, a warehouse loft, a green glow |
| Docks | a warehouse on the pier: crates, chains, a crane, fog and the harbour behind |
| Hotel | a grand hotel: carpeted halls, a brass lift, room doors, chandeliers |
| Casino | a smoky gambling hall: green tables, a roulette wheel, gold mirrors, red carpet |
| Precinct | a police station: filing cabinets, a holding cell, a wanted board, cold blue light |
| Penthouse | the boss's penthouse: marble, a bar, a huge window over the lit city, gold and black |

Also in the same folder:
- `BG_CityMap.png` - 1920x1080, the whole city seen from above at night as a stylised map: streets, river and docks, districts in muted colours, ready for shop pins to be placed on it. No labels.
- `BG_MainMenu.png` - 1920x1080, the title screen backdrop: a rainy street corner under a lamp post, neon signs, a car's tail lights, room on the left third for the menu buttons.
- `BG_Loading.png` - 1920x1080, a dark, calm image for the loading screen: a desk with a tape recorder, a glass of whiskey and smoke in a lamp's cone.

## 2. Menus and on-screen UI
Save to `Assets/Modules/GameplayModule/Art/Incoming/UI/`. Every UI piece is drawn empty, so the game writes its own text on it, on a transparent background. Panels are drawn so their middle can stretch: plain inner area, all the decoration in the corners and edges (9-slice ready).
- `UI_Logo_Cubes.png` - the game's logo, the word CUBES, noir title lettering in ivory and crimson with a gold edge, 1024x384.
- `UI_Button.png`, `UI_Button_Hover.png`, `UI_Button_Pressed.png` - a menu button, 384x96: a dark plaque with a thin gold frame; hover glows mint, pressed is darker and set in.
- `UI_Panel.png` - a general window panel, 512x512: a dark leather or wooden board with a gold inner frame and corner ornaments.
- `UI_Header.png` - the bar across the top of the screen during play, 1920x170: dark, with a thin mint line along the bottom.
- `UI_Terminal.png` - the in-game computer screen frame, 1024x768: an old green-screen monitor bezel, glass empty.
- `UI_MenuRow.png` and `UI_MenuRow_Selected.png` - one row of a list, 768x72: plain dark, and the same highlighted in gold.
- `UI_PhoneCall.png` - the call card along the bottom, 1200x260: a dark card with a crimson top line, an empty square on the left for the caller's face.
- `UI_Phone.png` - a 1940s black telephone, 160x160.
- `UI_NowPlaying.png` - the "now playing" cassette card, 480x160: a cassette tape with two empty reels and an empty label.
- `UI_Reel.png` - one cassette reel to spin, 64x64.
- `UI_TapeDeck.png` - the tape-deck window, 900x520: a wooden cassette deck with two empty reel windows, an empty label area, and empty spaces where the buttons go.
- `UI_DeckButton_Play.png`, `UI_DeckButton_Pause.png`, `UI_DeckButton_Previous.png`, `UI_DeckButton_Next.png`, `UI_DeckButton_Close.png` - chunky deck keys, 96x96, each with its symbol.
- `UI_Knob.png` - a volume knob seen from the front, 128x128.
- `UI_Station.png` - the panel for a place on the map, 640x420, like an index card pinned to a board.
- `UI_MapPin.png`, `UI_MapPin_Owned.png`, `UI_MapPin_Rival.png` - a map pin, 64x96: neutral ivory, the player's mint, an enemy's crimson.
- `UI_Banner.png` - a wide ribbon for big messages across the middle of the screen, 1400x200.
- `UI_Stick_Ring.png` and `UI_Stick_Knob.png` - the on-screen joystick, 256x256 and 128x128, see-through dark with a gold rim.
- `UI_TouchButton.png` - a round on-screen button, 160x160, see-through dark with a gold rim, empty in the middle.
- `UI_Icon_Use.png`, `UI_Icon_Jump.png`, `UI_Icon_Melee.png`, `UI_Icon_Reload.png`, `UI_Icon_Swap.png`, `UI_Icon_Song.png` - small icons for those buttons, 64x64, ivory: a hand, an up arrow, a fist, a bullet with a turning arrow, two crossed guns, a music note.
- `UI_Icon_Money.png`, `UI_Icon_Heart.png`, `UI_Icon_Ammo.png`, `UI_Icon_Star.png`, `UI_Icon_Lock.png` - HUD icons, 48x48.

## When you are done
List every file you wrote, grouped by folder, and name any you could not make. Do not open or run Unity, and do not build or test anything.
