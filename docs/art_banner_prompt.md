You are working in the Unity project at `D:\UnityProjects\Cubes`, a 2D pixel-art game called CUBES. Design the game's banner and save every file listed below in one go, without asking me anything in between. Do not change any other file: no code, no prefab, no scene, no .meta file, and no existing image.

## Save to
`Assets/Modules/GameplayModule/Art/Incoming/Banner/` - create the folder if it is not there.

## The game
CUBES is a 1940s film-noir mafia story at night, told with shapes. The hero is a rectangle - a hitman in a dark trench coat, a fedora with a crimson band, a red tie and a cigarette - who works for the Square family and takes the city shop by shop from the rival families: the Triangles, Circles, Pentagons, Hexagons and Diamonds. Rain, neon, wet streets, smoky rooms, warm lamp light against deep shadow.

Use these as the exact reference, and keep every design, colour and proportion the same:
- the hero: `Assets/Modules/GameplayModule/Art/Sprites/Characters/SPR_Character_Rectangle.png`
- the rivals: `SPR_Character_Triangle.png`, `SPR_Character_Diamond.png`, `SPR_Character_Hexagon.png` in the same folder
- the new app icon: `Assets/Modules/GameplayModule/Art/Incoming/Icon/ICON_Cubes_512.png` - the banner must look like it belongs to the same game as this icon
- the logo lettering: `Assets/Modules/GameplayModule/Art/Incoming/UI/UI_Logo_Cubes.png`

## The scene
- A rainy street at night, wide and cinematic, seen from the side like the game. Wet pavement with neon reflections, a lamp post with a cone of warm light, a few dark building fronts with lit windows and a neon sign or two (shapes only, no readable words).
- The hero stands in the lamp light, a little off centre, gun held low at his side, cigarette smoke curling up, his long shadow stretching across the wet street.
- In the shadows behind him, two or three rival family thugs (a Triangle, a Diamond, a Hexagon) half hidden in a doorway or an alley, only their silhouettes and eyes catching the neon - a threat the viewer notices second.
- The word **CUBES** large, in the same noir title lettering as the logo file: ivory letters with a gold edge and a crimson shadow. Below it, smaller, the tagline **TAKE THE CITY. ONE SHOP AT A TIME.** in ivory. Keep both clear of the characters and readable at a glance.
- Leave calm dark space at the very top and bottom edges so nothing important is cut when a page crops it.

## Style
- Pixel art with hard pixel edges, a dark outline on characters and objects, flat shading with one highlight and one shadow tone. Characters at the same pixel size as the game sprites (the hero about 145 px tall in the 1920-wide file), scaled with nearest-neighbour in the smaller files so pixels stay crisp. No blur, no smooth gradients, no anti-aliasing, no photo textures.
- Palette: near-black #0B0806, ivory #E2D6BE, gold #C4A05C, crimson #8C161C, mint #5CC78C, coat brown #3A2418. Mostly dark, with the lamp light and two neon colours (crimson and mint) as the only bright areas.

## Files
| File | Size | Use | Layout |
|---|---|---|---|
| `BANNER_Cubes_itch_960x300.png` | 960 x 300 | itch.io page header | hero on the left third, CUBES and the tagline on the right |
| `BANNER_Cubes_GitHub_1280x400.png` | 1280 x 400 | README banner on GitHub | hero centre-left, CUBES and tagline on the right, thugs on the far left in the alley |
| `BANNER_Cubes_Wide_1920x620.png` | 1920 x 620 | the master, for any other page | same composition as the GitHub one with more street on both sides |
| `BANNER_Cubes_Social_1200x630.png` | 1200 x 630 | link preview when the page is shared | CUBES and tagline top-centre, hero below in the lamp light, thugs at the sides |
| `BANNER_Cubes_NoText_1920x620.png` | 1920 x 620 | background for future use | the same as the master with no lettering at all |

## When you are done
List every file you wrote and name any you could not make. Do not open or run Unity, and do not build or test anything.
