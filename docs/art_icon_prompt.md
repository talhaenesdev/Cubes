You are working in the Unity project at `D:\UnityProjects\Cubes`, a 2D pixel-art game called CUBES. Design the game's new app icon and save every file listed below in one go, without asking me anything in between. Do not change any other file: no code, no prefab, no scene, no .meta file, and no existing image.

## Save to
`Assets/Modules/GameplayModule/Art/Incoming/Icon/` - create the folder if it is not there.

## The game
CUBES is a 1940s film-noir mafia story at night, told with shapes. The hero is a rectangle - a hitman in a dark trench coat, a fedora with a crimson band, a red tie and a cigarette - who works for the Square family and takes the city shop by shop from the rival families: the Triangles, Circles, Pentagons, Hexagons and Diamonds. Rain, neon, smoky rooms, warm lamp light against deep shadow. Open `Assets/Modules/GameplayModule/Art/Sprites/Characters/SPR_Character_Rectangle.png` to see the hero exactly as he is drawn in the game.

## What the icon shows
- The hero's head and shoulders, from the front or a slight three-quarter turn: the flat rectangular face, the fedora with its crimson band pulled low, the coat collar turned up, the red tie, a cigarette with a thin curl of smoke. Same design, colours and character as the game sprite, drawn bigger and cleaner.
- Behind him, a simple dark night: a few rain streaks and one soft neon glow (crimson or mint) so the silhouette stands out. No detailed background.
- Optional, small and subtle: the shape of a rival family (a triangle or a diamond) reflected in the hat band or as a faint neon sign behind him.
- It must read instantly at 48 px on a phone home screen: one strong silhouette, high contrast, the hat and the face clearly recognisable. No thin details that turn to noise when small.

## Style
- Pixel art with hard pixel edges, a dark outline, flat shading with one highlight and one shadow tone. Chunky pixels - the icon is drawn on a 64x64 pixel grid and scaled up with nearest-neighbour, so every pixel stays a crisp square. No blur, no smooth gradients, no anti-aliasing, no photo textures.
- Palette: near-black #0B0806, ivory #E2D6BE, gold #C4A05C, crimson #8C161C, mint #5CC78C, coat brown #3A2418.
- No text, no letters, no logo, no game name - except in the logo file at the end.

## Files
| File | Size | What |
|---|---|---|
| `ICON_Cubes_64.png` | 64 x 64 | the master pixel drawing, full square, opaque background |
| `ICON_Cubes_1024.png` | 1024 x 1024 | the same scaled up 16x, nearest-neighbour, opaque - used for the store and Windows |
| `ICON_Cubes_512.png` | 512 x 512 | the same at 8x |
| `ICON_Cubes_Round_1024.png` | 1024 x 1024 | the same art inside a circle, transparent outside the circle - for round launcher icons |
| `ICON_Cubes_Foreground_1024.png` | 1024 x 1024 | Android adaptive icon foreground: only the hero (head, hat, shoulders, smoke) on a transparent background, kept inside the central 66% safe zone (about 680 x 680 in the middle) so no launcher mask cuts the hat |
| `ICON_Cubes_Background_1024.png` | 1024 x 1024 | Android adaptive icon background: only the dark rainy night with the neon glow, opaque, nothing important near the edges |
| `ICON_Cubes_itch_630x500.png` | 630 x 500 | the itch.io cover image: the hero on the left half, the word CUBES in noir title lettering (ivory with a gold edge and a crimson shadow) on the right half |

## When you are done
List every file you wrote and name any you could not make. Do not open or run Unity, and do not build or test anything.
