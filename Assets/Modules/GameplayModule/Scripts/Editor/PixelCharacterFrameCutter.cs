#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Modules.GameplayModule.Editor
{
    // Cuts an AI-drawn animation strip (frames side by side on a flat green background) into one sprite per frame.
    // Every frame is scaled so the hat band is as wide as on the character's still drawing - so a run, a crouch and a
    // walk drawn at different sizes come out at one size - and placed with its feet on the bottom edge and the hat
    // at the centre, which is the sprite's pivot. The frames are written next to the strips, under Frames/.
    internal static class PixelCharacterFrameCutter
    {
        public const string FrameFolder = "Assets/Modules/GameplayModule/Art/Sprites/Characters/Frames";
        private const float PixelsPerUnit = 64f;
        private const int Gap = 3;

        private struct Cut
        {
            public Color32[] Pixels;
            public int Width, Height, Band, BandCentre;
        }

        // The frames of `stripPath`, scaled to match `stillPath`; empty when the strip is missing.
        public static List<Sprite> Frames(string stillPath, string stripPath, string baseName, bool keepGreen, bool upright)
        {
            var sprites = new List<Sprite>();
            if (!File.Exists(stripPath) || !File.Exists(stillPath)) return sprites;
            List<Cut> still = Slice(Load(stillPath), keepGreen);
            List<Cut> frames = Slice(Load(stripPath), keepGreen);
            if (still.Count == 0 || frames.Count == 0) return sprites;
            return Write(frames, Scale(still, frames, upright), baseName);
        }

        // How much a strip is shrunk to match the still drawing. A strip with a standing frame (idle, walk, a hit, a
        // fall) is matched by its tallest frame, which the AI keeps far better than the hat's size; a bent one (run,
        // crouch, jump) by the hat band of its first frame.
        public static float ScaleOf(string stillPath, string stripPath, bool upright, bool keepGreen)
        {
            if (!File.Exists(stripPath) || !File.Exists(stillPath)) return 0f;
            List<Cut> still = Slice(Load(stillPath), keepGreen);
            List<Cut> frames = Slice(Load(stripPath), keepGreen);
            return still.Count == 0 || frames.Count == 0 ? 0f : Scale(still, frames, upright);
        }

        private static float Scale(List<Cut> still, List<Cut> frames, bool upright)
        {
            int tallest = 1;
            foreach (Cut cut in frames) tallest = Math.Max(tallest, cut.Height);
            float byHeight = still[0].Height / (float) tallest;
            if (upright) return byHeight;
            // A bent strip is matched by the hat band, but a character with no red band, or one the AI drew elsewhere
            // (a scarf, a tie), gives a wild ratio: it is kept within what the strip's height allows - a bent frame is
            // never taller than the standing still, and never shrunk to less than 40% of it.
            float byBand = still[0].Band / Mathf.Max(1f, frames[0].Band);
            return Mathf.Clamp(byBand, byHeight * 0.4f, byHeight);
        }

        // The still drawing itself, put through the same placement so it shares the frames' pivot.
        public static Sprite Still(string stillPath, string baseName, bool keepGreen)
        {
            if (!File.Exists(stillPath)) return null;
            List<Cut> still = Slice(Load(stillPath), keepGreen);
            if (still.Count == 0) return null;
            List<Sprite> written = Write(still.GetRange(0, 1), 1f, baseName);
            return written.Count > 0 ? written[0] : null;
        }

        // One drawing with its pivot where `pivot` says (0,0.5 is the left edge's middle: an arm's shoulder).
        public static Sprite Single(string path, string baseName, bool keepGreen, Vector2 pivot)
        {
            if (!File.Exists(path)) return null;
            List<Cut> cuts = Slice(Load(path), keepGreen);
            if (cuts.Count == 0) return null;
            Cut whole = cuts[0];
            foreach (Cut cut in cuts) if (cut.Width * cut.Height > whole.Width * whole.Height) whole = cut;
            whole.BandCentre = whole.Width / 2;
            List<Sprite> written = Write(new List<Cut> {whole}, 1f, baseName, pivot);
            return written.Count > 0 ? written[0] : null;
        }

        private static Texture2D Load(string path)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.LoadImage(File.ReadAllBytes(path));
            return texture;
        }

        private static bool IsKey(Color32 c) =>
            c.a < 16 || (c.g > 170 && c.r < 130 && c.b < 130 && c.g - Math.Max(c.r, c.b) > 70);

        private static bool IsBand(Color32 c) => c.a > 200 && c.r > 120 && c.g < 70 && c.b < 70 && c.r - c.g > 80;

        // Frames are the runs of columns with any drawing in them; a sliver (a puff of smoke) joins its neighbour.
        private static List<Cut> Slice(Texture2D texture, bool keepGreen)
        {
            int w = texture.width, h = texture.height;
            Color32[] px = texture.GetPixels32();
            UnityEngine.Object.DestroyImmediate(texture);
            var filled = new bool[w];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                if (!IsKey(px[y * w + x])) { filled[x] = true; break; }

            var runs = new List<Vector2Int>();
            for (int x = 0; x < w; x++)
            {
                if (!filled[x]) continue;
                int start = x;
                int end = x;
                int empty = 0;
                while (x < w && empty <= Gap)
                {
                    if (filled[x]) { end = x; empty = 0; } else empty++;
                    x++;
                }
                runs.Add(new Vector2Int(start, end));
            }
            int widest = 0;
            foreach (Vector2Int run in runs) widest = Math.Max(widest, run.y - run.x + 1);
            for (int i = runs.Count - 1; i >= 0 && runs.Count > 1; i--)
            {
                if (runs[i].y - runs[i].x + 1 >= widest / 4) continue;
                int into = i == 0 ? 1 : i == runs.Count - 1 ? i - 1
                    : runs[i].x - runs[i - 1].y < runs[i + 1].x - runs[i].y ? i - 1 : i + 1;
                runs[into] = new Vector2Int(Math.Min(runs[into].x, runs[i].x), Math.Max(runs[into].y, runs[i].y));
                runs.RemoveAt(i);
            }

            var cuts = new List<Cut>();
            foreach (Vector2Int run in runs)
            {
                int top = -1, bottom = h;
                for (int y = 0; y < h; y++)
                for (int x = run.x; x <= run.y; x++)
                    if (!IsKey(px[y * w + x])) { bottom = Math.Min(bottom, y); top = Math.Max(top, y); }
                if (top < bottom) continue;
                int cw = run.y - run.x + 1, ch = top - bottom + 1;
                var cut = new Cut {Width = cw, Height = ch, Pixels = new Color32[cw * ch]};
                int bandMin = int.MaxValue, bandMax = -1;
                for (int y = 0; y < ch; y++)
                for (int x = 0; x < cw; x++)
                {
                    Color32 c = px[(bottom + y) * w + run.x + x];
                    if (IsKey(c)) c = new Color32(0, 0, 0, 0);
                    // The soft edge between the drawing and the green key: take it to the outline's colour.
                    else if (!keepGreen && c.g > c.r + 40 && c.g > c.b + 40) c = new Color32(20, 14, 10, 255);
                    cut.Pixels[y * cw + x] = c;
                    if (y >= ch * 0.65f && IsBand(c)) { bandMin = Math.Min(bandMin, x); bandMax = Math.Max(bandMax, x); }
                }
                cut.Band = bandMax >= 0 ? bandMax - bandMin + 1 : cw;
                cut.BandCentre = bandMax >= 0 ? (bandMin + bandMax) / 2 : cw / 2;
                cuts.Add(cut);
            }
            return cuts;
        }

        // Every frame of a set on one canvas size, feet on the bottom row, hat centre on the middle column.
        private static List<Sprite> Write(List<Cut> cuts, float scale, string baseName) =>
            Write(cuts, scale, baseName, new Vector2(0.5f, 0f));

        private static List<Sprite> Write(List<Cut> cuts, float scale, string baseName, Vector2 pivot)
        {
            Directory.CreateDirectory(FrameFolder);
            int half = 1, height = 1;
            foreach (Cut cut in cuts)
            {
                int centre = Mathf.RoundToInt(cut.BandCentre * scale);
                half = Math.Max(half, Math.Max(centre, Mathf.RoundToInt(cut.Width * scale) - centre) + 1);
                height = Math.Max(height, Mathf.RoundToInt(cut.Height * scale));
            }
            int width = half * 2;
            var paths = new List<string>();
            for (int i = 0; i < cuts.Count; i++)
            {
                Cut cut = cuts[i];
                int ow = Mathf.Max(1, Mathf.RoundToInt(cut.Width * scale));
                int oh = Mathf.Max(1, Mathf.RoundToInt(cut.Height * scale));
                int left = half - Mathf.RoundToInt(cut.BandCentre * scale);
                // Cut to its own height, feet on the bottom edge, so the frame's height says where its top is.
                int fh = Math.Min(height, oh);
                var canvas = new Color32[width * fh];
                for (int y = 0; y < oh; y++)
                for (int x = 0; x < ow; x++)
                {
                    int sx = Math.Min(cut.Width - 1, (int) (x / scale));
                    int sy = Math.Min(cut.Height - 1, (int) (y / scale));
                    int tx = left + x;
                    if (tx < 0 || tx >= width || y >= fh) continue;
                    canvas[y * width + tx] = cut.Pixels[sy * cut.Width + sx];
                }
                var texture = new Texture2D(width, fh, TextureFormat.RGBA32, false);
                texture.SetPixels32(canvas);
                texture.Apply();
                string path = $"{FrameFolder}/{baseName}_{i:00}.png";
                File.WriteAllBytes(path, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
                paths.Add(path);
            }
            // Frames left over from a longer strip drawn before.
            for (int i = cuts.Count; File.Exists($"{FrameFolder}/{baseName}_{i:00}.png"); i++)
                AssetDatabase.DeleteAsset($"{FrameFolder}/{baseName}_{i:00}.png");

            var sprites = new List<Sprite>();
            foreach (string path in paths)
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                if (AssetImporter.GetAtPath(path) is TextureImporter importer)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = PixelsPerUnit;
                    importer.filterMode = FilterMode.Point;
                    importer.mipmapEnabled = false;
                    importer.alphaIsTransparency = true;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    var settings = new TextureImporterSettings();
                    importer.ReadTextureSettings(settings);
                    settings.spriteAlignment = (int) SpriteAlignment.Custom;
                    settings.spritePivot = pivot;
                    importer.SetTextureSettings(settings);
                    importer.SaveAndReimport();
                }
                sprites.Add(AssetDatabase.LoadAssetAtPath<Sprite>(path));
            }
            return sprites;
        }
    }
}
#endif
