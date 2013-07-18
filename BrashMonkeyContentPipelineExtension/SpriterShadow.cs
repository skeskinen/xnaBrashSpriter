/*==========================================================================
 * Project: BrashMonkeyContentPipelineExtension
 * File: SpriterShadow.cs
 *
 *==========================================================================
 * Author:
 * Geoff "NowSayPillow" Lodder
 *==========================================================================*/

using System.Xml.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace BrashMonkeyContentPipelineExtension {
    public class SpriterShadowData {
        public List<List<Rectangle>> Rectangles;
        public List<Texture2DContent> Textures;
        public XDocument XML;
    }
}
