/*==========================================================================
 * Project: BrashMonkeyContentPipelineExtension
 * File: SpriterReader.cs
 *
 *==========================================================================
 * Author:
 * Geoff "NowSayPillow" Lodder
 *==========================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using BrashMonkeySpriter;
using BrashMonkeySpriter.Spriter;

namespace BrashMonkeySpriter.Content {
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content
    /// Pipeline to read the specified data type from binary .xnb format.
    /// 
    /// Unlike the other Content Pipeline support classes, this should
    /// be a part of your main game project, and not the Content Pipeline
    /// Extension Library project.
    /// </summary>
    public class SpriterReader : ContentTypeReader<CharacterModel> {
        protected override CharacterModel Read(ContentReader p_input, CharacterModel p_character) {
            p_character = new CharacterModel();
/*            
            /// ------------------- Texture Dictionary -------------------------------
            Int32 l_folderCount = p_input.ReadInt32();///Number of Folders
            for (int l_fIter = 0; l_fIter < l_folderCount; l_fIter++) {
                List<String> l_list = new List<String>();
                
                Int32 l_fileCount = p_input.ReadInt32();///Number of Files
                for (int l_fIter2 = 0; l_fIter2 < l_fileCount; l_fIter2++) {
                    l_list.Add(p_input.ReadString());///Name
                    p_input.ReadInt32();///Width
                    p_input.ReadInt32();///Height
                }

                p_character.FileNames.Add(l_list);
            }
            // Load textures
            for (int l_i = 0; l_i < p_character.FileNames.Count; l_i++) {
                List<Texture2D> l_list = new List<Texture2D>();

                for (int l_j = 0; l_j < p_character.FileNames[l_i].Count; l_j++) {
                    l_list.Add(p_input.ContentManager.Load<Texture2D>(p_character.FileNames[l_i][l_j].Substring(0, p_character.FileNames[l_i][l_j].Length - 4)));
                }

                p_character.Textures.Add(l_list);
            }
*/

            Int32 l_textureCount = p_input.ReadInt32();///Number of Textures
            for (int l_tIter = 0; l_tIter < l_textureCount; l_tIter++) {
                Texture2D l_texture = p_input.ReadRawObject<Texture2D>();
                p_character.Textures.Add(l_texture);
            }

            Int32 l_rectCount1 = p_input.ReadInt32();///Number of RectangleLists
            for (int l_rIter1 = 0; l_rIter1 < l_rectCount1; l_rIter1++) {
                List<Rectangle> l_list = new List<Rectangle>();
                
                Int32 l_rectCount2 = p_input.ReadInt32();///Number of Rectangles in that list
                for (int l_rIter2 = 0; l_rIter2 < l_rectCount2; l_rIter2++) {
                    l_list.Add(p_input.ReadRawObject<Rectangle>());
                }

                p_character.Rectangles.Add(l_list);
            }

            /// ------------------- Entities -------------------------------
            Int32 l_entityCount = p_input.ReadInt32();///Number of Entities
            for (int l_eIter = 0; l_eIter < l_entityCount; l_eIter++) {
                Entity l_entity = new Entity();

                l_entity.Name = p_input.ReadString();///Name

                /// ------------------- Animations -------------------------------
                Int32 l_animationCount = p_input.ReadInt32();///Number of Animations
                for (int l_aIter = 0; l_aIter < l_animationCount; l_aIter++) {
                    Animation l_animation = new Animation();

                    l_animation.Name = p_input.ReadString();///Name
                    l_animation.Length = p_input.ReadInt32();//length
                    l_animation.Looping = p_input.ReadBoolean();//looping
                    l_animation.MainLine = new List<MainlineKey>();
                    l_animation.TimeLines = new TimelineList();

                    /// ------------------- Mainline -------------------------------
                    Int32 l_keyCount = p_input.ReadInt32();///Number of Keyframes
                    for (int l_kIter = 0; l_kIter < l_keyCount; l_kIter++) {
                        MainlineKey l_key = new MainlineKey();

                        l_key.Time = p_input.ReadInt32();///Time
                        l_key.Body = new List<Reference>();
                        l_key.Bones = new List<Reference>();

                        /// ------------------- Object_refs -------------------------------
                        Int32 l_objectCount = p_input.ReadInt32();///Number of Objects
                        for (int l_oIter = 0; l_oIter < l_objectCount; l_oIter++) {
                            Reference l_body = new Reference();

                            l_body.Parent = p_input.ReadInt32();//parent
                            l_body.Timeline = p_input.ReadInt32();//timeline
                            l_body.Key = p_input.ReadInt32();//key
                            l_body.ZOrder = p_input.ReadInt32();//z_index

                            l_key.Body.Add(l_body);
                        }

                        /// ------------------- bone_ref -------------------------------
                        Int32 l_boneCount = p_input.ReadInt32();///Number of Objects
                        for (int l_bIter = 0; l_bIter < l_boneCount; l_bIter++) {
                            Reference l_bone = new Reference();

                            l_bone.Parent = p_input.ReadInt32();//parent
                            l_bone.Timeline = p_input.ReadInt32();//timeline
                            l_bone.Key = p_input.ReadInt32();//key

                            l_key.Bones.Add(l_bone);
                        }

                        l_animation.MainLine.Add(l_key);
                    }

                    /// ------------------- Timelines -------------------------------
                    Int32 l_timelineCount = p_input.ReadInt32();///Number of Timelines
                    for (int l_tIter = 0; l_tIter < l_timelineCount; l_tIter++) {
                        Timeline l_timeline = new Timeline();
                        l_timeline.Keys = new List<TimelineKey>();

                        l_timeline.Name = p_input.ReadString();///Name

                        /// ------------------- Frame -------------------------------
                        l_keyCount = p_input.ReadInt32();///Number of Keyframes
                        for (int l_kIter = 0; l_kIter < l_keyCount; l_kIter++) {
                            TimelineKey l_key = new TimelineKey();

                            l_key.Time = p_input.ReadInt32();//time
                            l_key.Spin = p_input.ReadInt32() >= 0 ? SpinDirection.CounterClockwise : SpinDirection.Clockwise;//spin

                            l_key.Type = (TimelineType)p_input.ReadInt32();//type

                            if (l_key.Type == TimelineType.Body) {
                                l_key.Folder = p_input.ReadInt32();//folder
                                l_key.File = p_input.ReadInt32();//file

                                //  Sprite Location (Spriter saves the rotations backwards.)
                                float l_locationX = p_input.ReadSingle(), l_locationY = p_input.ReadSingle();//location
                                l_key.Location = new Vector2(l_locationX, -l_locationY);

                                //  Sprite Pivot Point (Spriter saves the "Y" backwards. We need to flip it.)
                                float l_pivotX = p_input.ReadSingle(), l_pivotY = p_input.ReadSingle();//pivot
                                l_key.Pivot = new Vector2(l_pivotX * p_character.Rectangles[l_key.Folder][l_key.File].Width, (1.0f - l_pivotY) * p_character.Rectangles[l_key.Folder][l_key.File].Height);

                                l_key.Rotation = -MathHelper.ToRadians(p_input.ReadSingle());//rotation

                                float l_scaleX = p_input.ReadSingle(), l_scaleY = p_input.ReadSingle();//scale
                                l_key.Scale = new Vector2(l_scaleX, l_scaleY);

                                l_key.Alpha = p_input.ReadSingle();//alpha
                            } else {
                                l_key.Folder = l_key.File = -1;//File & folder are useless.

                                float l_locationX = p_input.ReadSingle(), l_locationY = p_input.ReadSingle();//location
                                l_key.Location = new Vector2(l_locationX, -l_locationY);

                                float l_pivotX = p_input.ReadSingle(), l_pivotY = p_input.ReadSingle();//pivot
                                l_key.Pivot = new Vector2(l_pivotX, l_pivotY);

                                //  Rotation (Spriter saves the rotations backwards.
                                l_key.Rotation = -MathHelper.ToRadians(p_input.ReadSingle());//rotation

                                float l_scaleX = p_input.ReadSingle(), l_scaleY = p_input.ReadSingle();//scale
                                l_key.Scale = new Vector2(l_scaleX, l_scaleY);

                                l_key.Alpha = 1.0f; // Useless alpha
                            }

                            l_timeline.Keys.Add(l_key);
                        }

                        l_animation.TimeLines.Add(l_timeline);
                    }

                    l_entity.Add(l_animation);
                }

                p_character.Add(l_entity);
            }

            return p_character;
        }
    }
}
