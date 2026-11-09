# CUBES - one brief for every prefab

Copy everything inside the box below and paste it into the image AI **once**. It holds the style and the whole list,
so the AI makes every prefab's image in order, one image each. When it stops, write `continue`. Save each image with
the file name the AI writes above it, and drop them into `Assets/Modules/GameplayModule/Art/Incoming/`.

Prefabs that have nothing to draw (the Roots, the Connector, screens built from the panels below) are not listed.

````
I am making a 2D pixel-art game called CUBES. Please create ONE SEPARATE IMAGE FOR EVERY ITEM in the numbered list
below, in order. Above each image write only its file name. Do not merge items into one image. If you have to stop,
I will write "continue" and you go on from the next number. Keep exactly the same style for all of them.

STYLE (every image):
- Pixel art game sprite, 1940s film noir, side-scrolling stealth shooter, side view, facing right.
- Characters are geometric shapes (a rectangle, a triangle, a circle...) with small hats, coats and simple arms and
  legs - charming and readable, not realistic.
- Palette: near-black #0B0806, ivory #E2D6BE, gold #C4A05C, crimson #8C161C, plus the item's own colour.
- 1-pixel dark outline, flat shading with one highlight and one shadow tone, hard pixel edges, no blur, no gradients.
- Transparent background; if you cannot, a flat pure green #00FF00 background. One object only, centred, a little
  empty space around it. No ground, no cast shadow, no text, no labels, no watermark, no frame.
- Sizes are the pixel-art size to imitate; the image itself may be larger.

CHARACTERS (64x96, standing, facing right)
1. PB_Character_Rectangle - the hero: a tall upright ivory rectangle in a long dark trench coat and a fedora with a crimson band, collar up, cigarette.
2. PB_Character_Rectangle_Crouch - the same hero crouched low behind cover.
3. PB_Character_Rectangle_Aim - the same hero standing, arm straight out aiming a revolver.
4. PB_Character_Square - a square-bodied friendly henchman in a dark hooded coat with a green #5CC78C scarf.
5. PB_Character_Triangle - a triangle-bodied mobster, point up, pinstripe suit, red #D93D33 tie, flat cap, mean brows.
6. PB_Character_Circle - a round circle-bodied mobster, waistcoat, blue #528FEB bow tie, bowler hat.
7. PB_Character_Pentagon - a pentagon-bodied mobster, long purple #A366E0 coat with a fur collar, wide-brimmed hat.
8. PB_Character_Hexagon - a hexagon-bodied brute, rolled sleeves, orange #F29933 vest, bald, broad.
9. PB_Character_Diamond - a diamond-bodied (rhombus, point up) dandy, cream suit, gold #EBD64D tie, cane.
10. PB_Character_Civilian - a small plain circle-bodied civilian in a cardigan, hands up, scared.
11. PB_Character_Boss - a heavy circle-bodied shop owner in an apron, moustache, sleeves rolled.
12. PB_Character_Dead - a generic shape lying flat on its back, arms out (96x40).

WEAPONS (side view, pointing right)
13. PB_Weapon_Revolver - six-shot revolver, dark steel, wooden grip (32x16).
14. PB_Weapon_Pistol - 1940s semi-automatic pistol, black (28x16).
15. PB_Weapon_SilencedPistol - pistol with a long cylindrical silencer (40x16).
16. PB_Weapon_Shotgun - pump shotgun, wooden stock (56x16).
17. PB_Weapon_Smg - Thompson submachine gun with a drum magazine (56x24).
18. PB_Weapon_Rifle - bolt-action rifle, long, wooden (64x16).
19. PB_WeaponPickup - a small glowing gold outline ring that marks a weapon lying on the floor (32x12).
20. PB_CashPickup - a bundle of dollar bills with a paper band (16x12).
21. PB_Bullet - a short bright yellow-white bullet streak (12x3).

LEVEL FIXTURES
22. PB_Door - wooden interior door, closed, brass handle (24x72).
23. PB_Door_Open - the same door swung open, seen edge-on (24x72).
24. PB_StairDoor - stairwell door with a small wired window and an up/down arrow plate (32x72).
25. PB_Vent - wall air vent grille (32x20).
26. PB_LightSwitch - brass wall light switch (12x16).
27. PB_Exit - back-alley exit door with a red glowing lamp above it (40x72).
28. PB_Computer - desk with a typewriter-style computer terminal, green glowing screen (32x32).
29. PB_Document - a manila folder stamped with a red mark (16x12).
30. PB_Crate - wooden shipping crate with stencil marks, used as cover (48x40).

BREAKABLE PROPS
31. PB_Prop_Barrel - red oil barrel with a hazard mark (32x44).
32. PB_Prop_Bottles - row of liquor bottles on a little shelf (32x24).
33. PB_Prop_CeilingLamp - hanging ceiling lamp on a chain, metal shade, warm light (24x32).
34. PB_Prop_Monitor - 1940s radio / teletype terminal with a green screen (32x28).
35. PB_Prop_Plant - potted plant in a ceramic pot (28x44).

HIDEOUT
36. PB_Desk - heavy dark wooden desk with drawers, papers and an ashtray on top (96x48).
37. PB_DeskClock - small 1940s alarm clock, dark case, empty amber digital face (32x20).
38. PB_Wardrobe - tall wooden wardrobe, one door ajar, a trench coat inside (56x88).
39. PB_WeaponLocker - green metal weapon locker with a rifle rack (48x80).
40. PB_UpgradeBench - workbench with tools, a vice and gun parts (80x48).
41. PB_CassettePlayer - chunky cassette tape deck with two reels and big buttons (48x28).
42. PB_MissionBoard - cork board with a city map, pins and red string (72x56).
43. PB_FamilyBoard - cork board with five shape-family photos joined by red string (72x56).
44. PB_Cat - small black cat walking, yellow eyes (32x20).
45. PB_Neon - neon sign of a rectangle, a heart and a square, glowing pink and gold (64x32).
46. PB_Phone - a 1940s black telephone held in a hand (24x24).

EFFECTS (each a horizontal strip of frames, left to right)
47. PB_FX_Muzzle - player muzzle flash, yellow-white, 3 frames of 16x16.
48. PB_FX_GuardMuzzle - enemy muzzle flash, orange-red, 3 frames of 16x16.
49. PB_FX_Explosion - barrel explosion, orange fire and black smoke, 6 frames of 64x64.
50. PB_FX_ImpactBlood - dark red blood burst, 4 frames of 16x16.
51. PB_FX_ImpactWall - plaster chips bursting from a wall, 4 frames of 16x16.
52. PB_FX_MetalSparks - bullet sparks on metal, 3 frames of 16x16.
53. PB_FX_Glass - glass shards bursting, 4 frames of 16x16.
54. PB_FX_Splinters - wood splinters bursting, 4 frames of 16x16.
55. PB_FX_Dirt - dirt and soil puff from a plant pot, 4 frames of 16x16.
56. PB_FX_CeilingDebris - plaster falling from a ceiling, 4 frames of 16x24.
57. PB_FX_FloorDust - small dust puff at the feet, 4 frames of 16x8.
58. PB_FX_DoorKick - splinters and dust from a kicked door, 4 frames of 24x24.
59. PB_FX_Melee - a white swoosh arc of a punch, 3 frames of 24x24.
60. PB_FX_ShellLoad - a brass revolver shell, single sprite (4x6).
61. PB_Decal_BulletHole - a bullet hole in a wall (6x6).
62. PB_Decal_Blood - a dark red blood pool on the floor (32x8).
63. PB_Decal_Scorch - a black scorch mark from an explosion (48x16).

INTERFACE
64. PB_UI_MapPin - a tiny top-down shop building seen from above, plain grey flat roof (92x70).
65. PB_UI_CityMap - top-down night map of a 1940s harbour city: harbour at the bottom, a river on the right, two parks, streets in a grid, no labels, dark and muted (1280x900, opaque).
66. PB_UI_SpeechBubble - ivory comic speech bubble with a dark outline and a tail at the bottom left, empty (48x32).
67. PB_UI_BannerPanel - a long thin dark panel with gold Art Deco corners, empty (480x64).
68. PB_UI_StationPanel - a dark menu window with a gold Art Deco frame, empty (480x360).
69. PB_UI_PhoneCallPanel - a card showing a hand holding a black 1940s phone receiver, dark background (240x120).
70. PB_UI_NowPlayingPanel - a cassette tape seen from the front with two round reels and a blank label (160x100).
71. PB_UI_TapeDeckPanel - the front panel of a tape deck with play, stop, next and previous buttons and a volume knob (480x200).
72. PB_UI_TerminalPanel - a green-on-black typewriter terminal screen with a metal bezel, empty (480x320).
73. PB_UI_TouchControlsPanel - a set of round semi-transparent touch buttons: a joystick ring, a jump arrow, and lettered circles with no letters (each 64x64).
74. PB_UI_MenuRow - a dark horizontal row bar with a thin gold underline, empty (480x40).
75. PB_UI_Icons - five small gold icons: a map pin, a wrench, a coat hanger, a bullet, a music note (16x16 each).
76. PB_UI_AppIcon - the hero rectangle in his fedora, close-up portrait, crimson background, gold rim (512x512, opaque).
77. PB_UI_Logo - the word CUBES in a bold Art Deco pixel font, ivory with a gold edge (320x96).

BACKGROUNDS (opaque, 640x360, dark and low contrast, rainy night, no characters, no text)
78. PB_Level_Hideout - a cosy attic apartment with brick walls and a round window.
79. PB_Level_NightShift - an office building interior at night.
80. PB_Level_Laundromat - washing machines and steam.
81. PB_Level_Diner - counter, stools and neon.
82. PB_Level_PawnShop - cluttered shelves, guitars, clocks.
83. PB_Level_Gym - boxing ring and punch bags.
84. PB_Level_Cinema - red seats and a projector beam.
85. PB_Level_Hotel - grand lobby with a chandelier.
86. PB_Level_Distillery - copper stills and barrels.
87. PB_Level_Casino - roulette tables and green felt.
88. PB_Level_Precinct - police desks, filing cabinets, blinds.
89. PB_Level_Penthouse - a skyline window and a grand piano.
90. PB_Level_Docks - harbour cranes, containers and the sea.
91. PB_Level_Wall - a tileable dark wallpaper wall section (64x64).
92. PB_Level_Floor - a tileable wooden floor strip (64x16).

Start with number 1.
````
