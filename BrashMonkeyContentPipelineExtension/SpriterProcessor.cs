/*==========================================================================
 * Project: BrashMonkeyContentPipelineExtension
 * File: SpriterProcessor.cs
 *
 *==========================================================================
 * Author:
 * Geoff "NowSayPillow" Lodder
 *==========================================================================*/

using System;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace BrashMonkeyContentPipelineExtension {
    [ContentProcessor(DisplayName = "SCML - BrashMonkey Spriter Animation")]
    public class SpriterProcessor : ContentProcessor<XDocument, SpriterShadowData> {
        public override SpriterShadowData Process(XDocument p_input, ContentProcessorContext p_context) {
            return BuildSpriteSheet(p_input, p_context);
        }

        /// <summary>
        /// Convert sprites into sprite sheet object
        /// (Basically from XNA SpriteSheetSample project)
        /// </summary>
        public SpriterShadowData BuildSpriteSheet(XDocument p_input, ContentProcessorContext p_context) {
            SpriterShadowData l_return = new SpriterShadowData();
            l_return.Rectangles = new List<List<Rectangle>>();
            l_return.Textures = new List<Texture2DContent>();
            l_return.XML = p_input;

            String p_fileName = (new List<XElement>(l_return.XML.Root.Descendants("File")))[0].Attribute("path").Value;

            foreach (XElement l_folder in l_return.XML.Root.Descendants("folder")) {
                List<BitmapContent> l_sourceSprites = new List<BitmapContent>();

                Texture2DContent l_outputTexture = new Texture2DContent();
                List<Rectangle> l_outputRectangles = new List<Rectangle>();

                foreach (XElement l_file in l_folder.Descendants("file")) {
                    ExternalReference<TextureContent> l_textureReference = new ExternalReference<TextureContent>(p_fileName + @"\" + l_file.Attribute("name").Value);
                    TextureContent texture = p_context.BuildAndLoadAsset<TextureContent, TextureContent>(l_textureReference, "TextureProcessor");
                    l_sourceSprites.Add(texture.Faces[0][0]);
                }

                // Pack all the sprites onto a single texture.
                BitmapContent l_packedSprites = SpritePacker.PackSprites(l_sourceSprites, l_outputRectangles, p_context);
                l_outputTexture.Mipmaps.Add(l_packedSprites);

                //  Add the data to the return type
                l_return.Rectangles.Add(l_outputRectangles);
                l_return.Textures.Add(l_outputTexture);
            }

            return l_return;
        }
    }
}