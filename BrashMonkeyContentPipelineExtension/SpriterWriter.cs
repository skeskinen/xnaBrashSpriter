/*==========================================================================
 * Project: BrashMonkeyContentPipelineExtension
 * File: SpriterWriter.cs
 *
 *==========================================================================
 * Author:
 * Geoff "NowSayPillow" Lodder
 *==========================================================================*/

using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System.Globalization;

namespace BrashMonkeyContentPipelineExtension
{
    [ContentTypeWriter]
    public class SpriterWriter : ContentTypeWriter<SpriterShadowData>
    {
        private bool GetAttributeInt32(XElement p_element, String p_name, out Int32 p_out, Int32 p_default = 0)
        {
            if (p_element.Attribute(p_name) != null)
            {
                return Int32.TryParse(p_element.Attribute(p_name).Value, out p_out);
            }

            p_out = p_default;
            return false;
        }

        private bool GetAttributeFloat(XElement p_element, String p_name, out float p_out, float p_default = 0.0f)
        {
            if (p_element.Attribute(p_name) != null)
            {
                return float.TryParse(p_element.Attribute(p_name).Value, NumberStyles.Any, CultureInfo.InvariantCulture, out p_out);
            }

            p_out = p_default;
            return false;
        }

        private bool GetAttributeBoolean(XElement p_element, String p_name, out bool p_out, bool p_default = true)
        {
            if (p_element.Attribute(p_name) != null)
            {
                return Boolean.TryParse(p_element.Attribute(p_name).Value, out p_out);
            }

            p_out = p_default;
            return false;
        }


        protected override void Write(ContentWriter p_output, SpriterShadowData p_value)
        {
            Int32 l_tmpInt = -1;
            float l_tmpFloat = 0.0f;
            bool l_tmpBool = false;

            /// Write the texture dictionary.
            /*            p_output.Write(p_value.XML.Root.Descendants("folder").Count());
                        foreach (XElement l_folder in p_value.XML.Root.Descendants("folder")) {
                            p_output.Write(l_folder.Descendants("file").Count());
                            foreach (XElement l_file in l_folder.Descendants("file")) {
                                p_output.Write(l_file.Attribute("name").Value);

                                GetAttributeInt32(l_file, "width", out l_tmpInt);
                                p_output.Write(l_tmpInt);

                                GetAttributeInt32(l_file, "height", out l_tmpInt);
                                p_output.Write(l_tmpInt);
                            }
                        }*/

            /// Write the generated Texture content.
            p_output.Write(p_value.Textures.Count());
            foreach (Texture2DContent l_texture in p_value.Textures)
            {
                p_output.WriteRawObject<Texture2DContent>(l_texture);
            }

            /// Write the rectangle location data
            p_output.Write(p_value.Rectangles.Count);
            foreach (List<Rectangle> l_list in p_value.Rectangles)
            {
                p_output.Write(l_list.Count);
                foreach (Rectangle l_rectangle in l_list)
                {
                    p_output.WriteRawObject<Rectangle>(l_rectangle);
                }
            }

            /// Tmp storage for default file pivots
            Dictionary<int, Dictionary<int, Vector2>> l_defaultPivot = new Dictionary<int, Dictionary<int, Vector2>>();
            foreach (XElement l_folder in p_value.XML.Root.Descendants("folder"))
            {
                int l_folderId;
                GetAttributeInt32(l_folder, "id", out l_folderId);
                foreach (XElement l_file in l_folder.Descendants("file"))
                {
                    int l_fileId;
                    GetAttributeInt32(l_file, "id", out l_fileId);

                    float l_tmpX, l_tmpY;
                    GetAttributeFloat(l_file, "pivot_x", out l_tmpX);
                    GetAttributeFloat(l_file, "pivot_y", out l_tmpY, 1.0f);

                    l_defaultPivot.GetOrCreate(l_folderId).Add(l_fileId, new Vector2(l_tmpX, l_tmpY));
                }
            }

            /// Write Entities.
            p_output.Write(p_value.XML.Root.Descendants("entity").Count());
            foreach (XElement l_entity in p_value.XML.Root.Descendants("entity"))
            {
                p_output.Write(l_entity.Attribute("name").Value);

                /// Write Animations.
                p_output.Write(l_entity.Descendants("animation").Count());
                foreach (XElement l_animation in l_entity.Descendants("animation"))
                {
                    p_output.Write(l_animation.Attribute("name").Value);

                    GetAttributeInt32(l_animation, "length", out l_tmpInt);
                    p_output.Write(l_tmpInt);

                    GetAttributeBoolean(l_animation, "looping", out l_tmpBool, true);
                    p_output.Write(l_tmpBool);

                    /// Write Mainline
                    foreach (XElement l_mainLine in l_animation.Descendants("mainline"))
                    {

                        /// Write Key
                        p_output.Write(l_mainLine.Descendants("key").Count());
                        foreach (XElement l_key in l_mainLine.Descendants("key"))
                        {
                            GetAttributeInt32(l_key, "time", out l_tmpInt);
                            p_output.Write(l_tmpInt);

                            /// Write Objects
                            p_output.Write(l_key.Descendants("object_ref").Count());
                            foreach (XElement l_object in l_key.Descendants("object_ref"))
                            {
                                GetAttributeInt32(l_object, "parent", out l_tmpInt, -1);
                                p_output.Write(l_tmpInt);

                                GetAttributeInt32(l_object, "timeline", out l_tmpInt);
                                p_output.Write(l_tmpInt);

                                GetAttributeInt32(l_object, "key", out l_tmpInt);
                                p_output.Write(l_tmpInt);

                                GetAttributeInt32(l_object, "z_index", out l_tmpInt);
                                p_output.Write(l_tmpInt);
                            }

                            p_output.Write(l_key.Descendants("bone_ref").Count());
                            foreach (XElement l_bone in l_key.Descendants("bone_ref"))
                            {
                                GetAttributeInt32(l_bone, "parent", out l_tmpInt, -1);
                                p_output.Write(l_tmpInt);

                                GetAttributeInt32(l_bone, "timeline", out l_tmpInt);
                                p_output.Write(l_tmpInt);

                                GetAttributeInt32(l_bone, "key", out l_tmpInt);
                                p_output.Write(l_tmpInt);
                            }
                        }
                    }

                    /// Write Timelines that aren't bones.
                    //p_output.Write(l_animation.Descendants("timeline").Where(l => l.Attribute("object_type") == null).Count());
                    //foreach (XElement l_timeLine in l_animation.Descendants("timeline").Where(l => l.Attribute("object_type") == null)) {
                    p_output.Write(l_animation.Descendants("timeline").Count());
                    foreach (XElement l_timeLine in l_animation.Descendants("timeline"))
                    {

                        p_output.Write(l_entity.Attribute("name").Value);

                        /// Write Key                       
                        p_output.Write(l_timeLine.Descendants("key").Count());
                        foreach (XElement l_key in l_timeLine.Descendants("key"))
                        {
                            GetAttributeInt32(l_key, "time", out l_tmpInt);
                            p_output.Write(l_tmpInt);

                            GetAttributeInt32(l_key, "spin", out l_tmpInt, 1);
                            p_output.Write(l_tmpInt);

                            if (l_key.Descendants("object").Count() > 0)
                            {
                                p_output.Write(0);///TimelineType.object

                                foreach (XElement l_object in l_key.Descendants("object"))
                                {
                                    int l_tmpFolder, l_tmpFile;
                                    GetAttributeInt32(l_object, "folder", out l_tmpFolder);
                                    p_output.Write(l_tmpFolder);

                                    GetAttributeInt32(l_object, "file", out l_tmpFile);
                                    p_output.Write(l_tmpFile);

                                    GetAttributeFloat(l_object, "x", out l_tmpFloat);
                                    p_output.Write(l_tmpFloat);

                                    GetAttributeFloat(l_object, "y", out l_tmpFloat);
                                    p_output.Write(l_tmpFloat);


                                    if (!GetAttributeFloat(l_object, "pivot_x", out l_tmpFloat))
                                    {
                                        l_tmpFloat = l_defaultPivot[l_tmpFolder][l_tmpFile].X;
                                    }
                                    p_output.Write(l_tmpFloat);

                                    if (!GetAttributeFloat(l_object, "pivot_y", out l_tmpFloat))
                                    {
                                        l_tmpFloat = l_defaultPivot[l_tmpFolder][l_tmpFile].Y;
                                    }
                                    p_output.Write(l_tmpFloat);

                                    GetAttributeFloat(l_object, "angle", out l_tmpFloat);
                                    p_output.Write(l_tmpFloat);

                                    GetAttributeFloat(l_object, "scale_x", out l_tmpFloat, 1.0f);
                                    p_output.Write(l_tmpFloat);

                                    GetAttributeFloat(l_object, "scale_y", out l_tmpFloat, 1.0f);
                                    p_output.Write(l_tmpFloat);

                                    GetAttributeFloat(l_object, "a", out l_tmpFloat, 1.0f);
                                    p_output.Write(l_tmpFloat);
                                }
                            }
                            else
                            {
                                p_output.Write(1);///TimelineType.bone

                                foreach (XElement l_bone in l_key.Descendants("bone"))
                                {
                                    GetAttributeFloat(l_bone, "x", out l_tmpFloat);
                                    p_output.Write(l_tmpFloat);

                                    GetAttributeFloat(l_bone, "y", out l_tmpFloat);
                                    p_output.Write(l_tmpFloat);

                                    GetAttributeFloat(l_bone, "pivot_x", out l_tmpFloat);
                                    p_output.Write(l_tmpFloat);

                                    GetAttributeFloat(l_bone, "pivot_y", out l_tmpFloat, 1.0f);
                                    p_output.Write(l_tmpFloat);

                                    GetAttributeFloat(l_bone, "angle", out l_tmpFloat);
                                    p_output.Write(l_tmpFloat);

                                    GetAttributeFloat(l_bone, "scale_x", out l_tmpFloat, 1.0f);
                                    p_output.Write(l_tmpFloat);

                                    GetAttributeFloat(l_bone, "scale_y", out l_tmpFloat, 1.0f);
                                    p_output.Write(l_tmpFloat);
                                }
                            }
                        }
                    }
                }
            }
        }

        public override string GetRuntimeType(TargetPlatform p_targetPlatform)
        {
            return typeof(BrashMonkeySpriter.CharacterModel).AssemblyQualifiedName;
        }

        public override string GetRuntimeReader(TargetPlatform p_targetPlatform)
        {
            return typeof(BrashMonkeySpriter.Content.SpriterReader).AssemblyQualifiedName;
        }
    }
}
