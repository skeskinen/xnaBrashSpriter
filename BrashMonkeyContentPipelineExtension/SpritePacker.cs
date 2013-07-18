/*==========================================================================
 * Project: BrashMonkeyContentPipelineExtension
 * File: SpritePacker.cs
 *
 * Pack a list of sprites into a single texture, taken from the XNA project
 * "SpriteSheetSample";
 *
 *==========================================================================
 * Author:
 * Microsoft Game Technology Group
 * Copyright (C) Microsoft Corporation. All rights reserved.
 *==========================================================================*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace BrashMonkeyContentPipelineExtension {
    /// <summary>
    /// Helper for arranging many small sprites into a single larger sheet.
    /// </summary>
    public static class SpritePacker {
        /// <summary>
        /// Packs a list of sprites into a single big texture,
        /// recording where each one was stored.
        /// </summary>
        public static BitmapContent PackSprites(IList<BitmapContent> p_sourceSprites, ICollection<Rectangle> p_outputSprites, ContentProcessorContext p_context) {
            if (p_sourceSprites.Count == 0)
                throw new InvalidContentException("There are no sprites to arrange");

            // Build up a list of all the sprites needing to be arranged.
            List<ArrangedSprite> l_sprites = new List<ArrangedSprite>();

            for (int i = 0; i < p_sourceSprites.Count; i++) {
                ArrangedSprite l_sprite = new ArrangedSprite();

                // Include a single pixel padding around each sprite, to avoid
                // filtering problems if the sprite is scaled or rotated.
                l_sprite.Width = p_sourceSprites[i].Width + 2;
                l_sprite.Height = p_sourceSprites[i].Height + 2;

                l_sprite.Index = i;

                l_sprites.Add(l_sprite);
            }

            // Sort so the largest sprites get arranged first.
            l_sprites.Sort(CompareSpriteSizes);

            // Work out how big the output bitmap should be.
            int l_outputWidth = GuessOutputWidth(l_sprites);
            int l_outputHeight = 0;
            int l_totalSpriteSize = 0;

            // Choose positions for each sprite, one at a time.
            for (int i = 0; i < l_sprites.Count; i++) {
                PositionSprite(l_sprites, i, l_outputWidth);

                l_outputHeight = Math.Max(l_outputHeight, l_sprites[i].Y + l_sprites[i].Height);

                l_totalSpriteSize += l_sprites[i].Width * l_sprites[i].Height;
            }

            // Sort the sprites back into index order.
            l_sprites.Sort(CompareSpriteIndices);

            p_context.Logger.LogImportantMessage(
                "Packed {0} sprites into a {1}x{2} sheet, {3}% efficiency",
                l_sprites.Count, l_outputWidth, l_outputHeight,
                l_totalSpriteSize * 100 / l_outputWidth / l_outputHeight);

            return CopySpritesToOutput(l_sprites, p_sourceSprites, p_outputSprites, l_outputWidth, l_outputHeight);
        }


        /// <summary>
        /// Once the arranging is complete, copies the bitmap data for each
        /// sprite to its chosen position in the single larger output bitmap.
        /// </summary>
        static BitmapContent CopySpritesToOutput(List<ArrangedSprite> p_sprites, IList<BitmapContent> p_sourceSprites, ICollection<Rectangle> p_outputSprites, int p_width, int p_height) {
            BitmapContent l_output = new PixelBitmapContent<Color>(p_width, p_height);

            foreach (ArrangedSprite l_sprite in p_sprites) {
                BitmapContent l_source = p_sourceSprites[l_sprite.Index];

                int l_x = l_sprite.X;
                int l_y = l_sprite.Y;

                int l_width = l_source.Width;
                int l_height = l_source.Height;

                // Copy the main sprite data to the output sheet.
                BitmapContent.Copy(l_source, new Rectangle(0, 0, l_width, l_height), l_output, new Rectangle(l_x + 1, l_y + 1, l_width, l_height));

                // Copy a border strip from each edge of the sprite, creating
                // a one pixel padding area to avoid filtering problems if the
                // sprite is scaled or rotated.
                BitmapContent.Copy(l_source, new Rectangle(0, 0, 1, l_height), l_output, new Rectangle(l_x, l_y + 1, 1, l_height));

                BitmapContent.Copy(l_source, new Rectangle(l_width - 1, 0, 1, l_height), l_output, new Rectangle(l_x + l_width + 1, l_y + 1, 1, l_height));

                BitmapContent.Copy(l_source, new Rectangle(0, 0, l_width, 1), l_output, new Rectangle(l_x + 1, l_y, l_width, 1));

                BitmapContent.Copy(l_source, new Rectangle(0, l_height - 1, l_width, 1), l_output, new Rectangle(l_x + 1, l_y + l_height + 1, l_width, 1));

                // Copy a single pixel from each corner of the sprite,
                // filling in the corners of the one pixel padding area.
                BitmapContent.Copy(l_source, new Rectangle(0, 0, 1, 1), l_output, new Rectangle(l_x, l_y, 1, 1));

                BitmapContent.Copy(l_source, new Rectangle(l_width - 1, 0, 1, 1), l_output, new Rectangle(l_x + l_width + 1, l_y, 1, 1));

                BitmapContent.Copy(l_source, new Rectangle(0, l_height - 1, 1, 1), l_output, new Rectangle(l_x, l_y + l_height + 1, 1, 1));

                BitmapContent.Copy(l_source, new Rectangle(l_width - 1, l_height - 1, 1, 1), l_output, new Rectangle(l_x + l_width + 1, l_y + l_height + 1, 1, 1));

                // Remember where we placed this sprite.
                p_outputSprites.Add(new Rectangle(l_x + 1, l_y + 1, l_width, l_height));
            }

            return l_output;
        }


        /// <summary>
        /// Internal helper class keeps track of a sprite while it is being arranged.
        /// </summary>
        class ArrangedSprite {
            public int Index;

            public int X;
            public int Y;

            public int Width;
            public int Height;
        }


        /// <summary>
        /// Works out where to position a single sprite.
        /// </summary>
        static void PositionSprite(List<ArrangedSprite> p_sprites, int p_index, int p_outputWidth) {
            int l_x = 0;
            int l_y = 0;

            while (true) {
                // Is this position free for us to use?
                int intersects = FindIntersectingSprite(p_sprites, p_index, l_x, l_y);

                if (intersects < 0) {
                    p_sprites[p_index].X = l_x;
                    p_sprites[p_index].Y = l_y;

                    return;
                }

                // Skip past the existing sprite that we collided with.
                l_x = p_sprites[intersects].X + p_sprites[intersects].Width;

                // If we ran out of room to move to the right,
                // try the next line down instead.
                if (l_x + p_sprites[p_index].Width > p_outputWidth) {
                    l_x = 0;
                    l_y++;
                }
            }
        }


        /// <summary>
        /// Checks if a proposed sprite position collides with anything
        /// that we already arranged.
        /// </summary>
        static int FindIntersectingSprite(List<ArrangedSprite> p_sprites, int p_index, int p_x, int p_y) {
            int l_width = p_sprites[p_index].Width;
            int l_height = p_sprites[p_index].Height;

            for (int i = 0; i < p_index; i++) {
                if (p_sprites[i].X >= p_x + l_width)
                    continue;

                if (p_sprites[i].X + p_sprites[i].Width <= p_x)
                    continue;

                if (p_sprites[i].Y >= p_y + l_height)
                    continue;

                if (p_sprites[i].Y + p_sprites[i].Height <= p_y)
                    continue;

                return i;
            }

            return -1;
        }


        /// <summary>
        /// Comparison function for sorting sprites by size.
        /// </summary>
        static int CompareSpriteSizes(ArrangedSprite p_a, ArrangedSprite p_b) {
            int l_aSize = p_a.Height * 1024 + p_a.Width;
            int l_bSize = p_b.Height * 1024 + p_b.Width;

            return l_bSize.CompareTo(l_aSize);
        }


        /// <summary>
        /// Comparison function for sorting sprites by their original indices.
        /// </summary>
        static int CompareSpriteIndices(ArrangedSprite p_a, ArrangedSprite p_b) {
            return p_a.Index.CompareTo(p_b.Index);
        }


        /// <summary>
        /// Heuristic guesses what might be a good output width for a list of sprites.
        /// </summary>
        static int GuessOutputWidth(List<ArrangedSprite> p_sprites) {
            // Gather the widths of all our sprites into a temporary list.
            List<int> l_widths = new List<int>();

            foreach (ArrangedSprite sprite in p_sprites) {
                l_widths.Add(sprite.Width);
            }

            // Sort the widths into ascending order.
            l_widths.Sort();

            // Extract the maximum and median widths.
            int l_maxWidth = l_widths[l_widths.Count - 1];
            int l_medianWidth = l_widths[l_widths.Count / 2];

            // Heuristic assumes an NxN grid of median sized sprites.
            int width = l_medianWidth * (int)Math.Round(Math.Sqrt(p_sprites.Count));

            // Make sure we never choose anything smaller than our largest sprite.
            return Math.Max(width, l_maxWidth);
        }
    }
}
