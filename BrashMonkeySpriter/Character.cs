/*==========================================================================
 * Project: BrashMonkeySpriter
 * File: Mainline.cs
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

using BrashMonkeySpriter.Spriter;

namespace BrashMonkeySpriter {
    public class CharacterModel : List<Entity> {
        public List<Texture2D> Textures { get; internal protected set; }
        public List<List<Rectangle>> Rectangles { get; internal protected set; }

        public CharacterModel()
            : base() {

            Textures = new List<Texture2D>();
            Rectangles = new List<List<Rectangle>>();
        }

        public Entity this[string name] {
            get { return this.FirstOrDefault(x => x.Name == name); }
        }

        public CharaterAnimator CreateAnimator(String p_entity) {
            return new CharaterAnimator(this, p_entity);
        }
    }

    public class CharaterAnimator {
        protected struct RenderMatrix {
            public float Alpha;
            public SpriteEffects Effects;
            public int File;
            public int Folder;
            public Vector2 Location;
            public Vector2 Pivot;
            public float Rotation;
            public Vector2 Scale;
            public int ZOrder;

            public RenderMatrix(AnimationTransform p_at) {
                Alpha = p_at.Alpha;
                Location = p_at.Location;
                Rotation = p_at.Rotation;
                Scale = p_at.Scale;

                Effects = SpriteEffects.None;
                File = 0;
                Folder = 0;
                Pivot = Vector2.Zero;
                ZOrder = 0;
            }
        }

        protected struct AnimationTransform {
            public float Alpha;
            public Vector2 Location;
            public Single Rotation;
            public Vector2 Scale;

            public AnimationTransform(Vector2 p_location, float p_rotation, Vector2 p_scale) {
                Alpha = 1.0f;
                Location = p_location;
                Rotation = p_rotation;
                Scale = p_scale;
            }
        }

        protected Color m_color = Color.White;
        public virtual Color Color {
            get { return m_color; }
            set { m_color = value; }
        }

        protected bool m_flipX = false, m_flipY = false;
        public virtual bool FlipX {
            get { return m_flipX; }
            set { m_flipX = value; }
        }

        public virtual bool FlipY {
            get { return m_flipY; }
            set { m_flipY = value; }
        }

        protected Vector2 m_location = Vector2.Zero;
        public virtual Vector2 Location {
            get { return m_location; }
            set { m_location = value; }
        }

        protected float m_rotate = 0.0f;
        public virtual float Rotation {
            get { return m_rotate; }
            set { m_rotate = value; }
        }

        protected float m_scale = 1.0f;
        public virtual float Scale {
            get { return m_scale; }
            set { m_scale = value; }
        }

        protected Entity m_entity = null;
        protected Animation m_current;
        protected int m_elapsedTime = 0;

        protected List<RenderMatrix> m_renderList;
        protected List<Texture2D> m_tx;
        protected List<List<Rectangle>> m_rect;

        protected Dictionary<int, AnimationTransform> m_boneTransforms;

        public delegate void AnimationEndedHandler();
        public event AnimationEndedHandler AnimationEnded;

        public CharaterAnimator(CharacterModel p_model, String p_entity) {
            m_entity = p_model[p_entity];
            m_tx = p_model.Textures;
            m_rect = p_model.Rectangles;

            m_boneTransforms = new Dictionary<int, AnimationTransform>();

            ChangeAnimation(0);
        }

        public void ChangeAnimation(String p_name) {
            m_current = m_entity[p_name];
            m_renderList = new List<RenderMatrix>(m_current.MainLine[0].Body.Count);
        }

        public void ChangeAnimation(int p_index) {
            m_current = m_entity[p_index];
            m_renderList = new List<RenderMatrix>(m_current.MainLine[0].Body.Count);
        }

        protected AnimationTransform GetFrameTransition(Reference p_ref) {
            Timeline l_timeline = m_current.TimeLines[p_ref.Timeline];
            
            // Find the current frame. 
            // The one referenced by mainline is not neccesarily the correct one
            // I guess the Spriter editor sometimes messes things up
            // I'm not sure how to reproduce this problem but better safe than sorry? For the reference XSpriter does something similar

            int l_keyCur = l_timeline.KeyAtOrBefore(m_elapsedTime);

            int l_thisTime = m_elapsedTime - l_timeline.Keys[l_keyCur].Time;
            int l_keyNext;
            int l_nextTime;
            // Find the next frame.
            if ((l_keyCur + 1) < l_timeline.Keys.Count) {
                l_keyNext = l_keyCur + 1;
                l_nextTime = l_timeline.Keys[l_keyNext].Time;
            }
            else if (m_current.Looping)
            {
                // Assume that there is a frame at time=0
                l_keyNext = 0;
                l_nextTime = m_current.Length;
            }
            else
            {
                l_keyNext = l_keyCur;
                l_nextTime = m_current.Length;
            }

            //  Figure out where we are in the timeline...
            l_nextTime = l_nextTime - l_timeline.Keys[l_keyCur].Time;

            TimelineKey l_now = l_timeline.Keys[l_keyCur];
            TimelineKey l_next = l_timeline.Keys[l_keyNext];

            /// Tween EVERYTHING... Gonna have to add an option for it not to...
            /// Rotations are handled differently depending on which way they're supposed to spin
            AnimationTransform l_render = new AnimationTransform();
            if ((l_now.Spin == SpinDirection.Clockwise) && ((l_next.Rotation - l_now.Rotation) < 0.0f)) {
                l_render.Rotation = MathHelper.Lerp(l_now.Rotation, (l_next.Rotation + MathHelper.TwoPi), (float)l_thisTime / (float)l_nextTime);
            } else if ((l_now.Spin == SpinDirection.CounterClockwise) && ((l_next.Rotation - l_now.Rotation) > 0.0f)) {
                l_render.Rotation = MathHelper.Lerp(l_now.Rotation, (l_next.Rotation - MathHelper.TwoPi), (float)l_thisTime / (float)l_nextTime);
            } else {
                l_render.Rotation = MathHelper.Lerp(l_now.Rotation, l_next.Rotation, (float)l_thisTime / (float)l_nextTime);
            }

            l_render.Scale = Vector2.Lerp(l_now.Scale, l_next.Scale, (float)l_thisTime / (float)l_nextTime);
            l_render.Location = Vector2.Lerp(l_now.Location, l_next.Location, (float)l_thisTime / (float)l_nextTime);
            l_render.Alpha = MathHelper.Lerp(l_now.Alpha, l_next.Alpha, (float)l_thisTime / (float)l_nextTime);

            // So, how far are we between frames?
            return l_render;
        }

        protected AnimationTransform ApplyTransform(AnimationTransform p_baseTransform, Vector2 p_scale, float p_rotation, Vector2 p_location, float p_alpha) {
            //  Create a tranformation matrix so we can find out the location of the bone \ body
            Matrix l_matrix =   Matrix.CreateScale(p_baseTransform.Scale.X, p_baseTransform.Scale.Y, 0) *
                                Matrix.CreateRotationZ(p_baseTransform.Rotation) *
                                Matrix.CreateTranslation(p_baseTransform.Location.X, p_baseTransform.Location.Y, 0);

            //  Apply the scaling, rotation and tranform matrix to current structure
            AnimationTransform l_result = new AnimationTransform();
            l_result.Scale = p_baseTransform.Scale * p_scale;
            l_result.Rotation = p_baseTransform.Rotation + p_rotation;
            l_result.Location = Vector2.Transform(p_location, l_matrix);
            l_result.Alpha = p_baseTransform.Alpha * p_alpha;

            return l_result;
        }

        protected AnimationTransform ApplyBoneTransforms(MainlineKey p_main, Reference p_reference) {
            if (p_reference.BoneId >= 0 && m_boneTransforms.ContainsKey(p_reference.BoneId))
            {
                return m_boneTransforms[p_reference.BoneId];
            }

            AnimationTransform l_frame = GetFrameTransition(p_reference);

            // Apply transforms from self and/or parent
            var l_transform = ApplyTransform(
                (p_reference.Parent != -1) ? ApplyBoneTransforms(p_main, p_main.Bones[p_reference.Parent]) : new AnimationTransform(Vector2.Zero, MathHelper.ToRadians(Rotation), new Vector2(Math.Abs(Scale))),
                l_frame.Scale,
                l_frame.Rotation,
                l_frame.Location,
                l_frame.Alpha
            );
            if (p_reference.BoneId >= 0)
                m_boneTransforms.Add(p_reference.BoneId, l_transform);
            return l_transform;
        }

        public void Update(GameTime p_gameTime) {
            m_renderList.Clear();
            m_boneTransforms.Clear();
            
            m_elapsedTime += p_gameTime.ElapsedGameTime.Milliseconds;
            if (m_elapsedTime > m_current.Length) {
                if (AnimationEnded != null)
                    AnimationEnded();
                if (m_current.Looping) {
                    m_elapsedTime -= m_current.Length;
                } else {
                    m_elapsedTime = m_current.Length;
                }
            }

            int l_frame = 0;
            for (int l_i = 0; (l_i < m_current.MainLine.Count); l_i++) {
                if (m_elapsedTime >= m_current.MainLine[l_i].Time) {
                    l_frame = l_i;
                }
            }

            Vector2 l_flip = new Vector2(m_flipX ? -1.0f : 1.0f, m_flipY ? -1.0f : 1.0f);

            MainlineKey l_mainline = m_current.MainLine[l_frame];
            
            for (int l_i = 0; l_i < l_mainline.Body.Count; l_i++) {
                TimelineKey l_key = m_current.TimeLines[l_mainline.Body[l_i].Timeline].Keys[l_mainline.Body[l_i].Key];
                // check if file for this object is missing, and if so skip calculating transforms
                if (m_rect[l_key.Folder][l_key.File].Width == 0)
                {
                    continue;
                }

                RenderMatrix l_render = new RenderMatrix(ApplyBoneTransforms(l_mainline, l_mainline.Body[l_i]));

                l_render.File = l_key.File;
                l_render.Folder = l_key.Folder;
                l_render.Pivot = l_key.Pivot;

                l_render.Location = Location + Vector2.Multiply(l_render.Location, l_flip);
                    
                if (m_flipX) {
                    l_render.Effects |= SpriteEffects.FlipHorizontally;
                    l_render.Pivot.X = m_rect[l_key.Folder][l_render.File].Width - l_render.Pivot.X;
                }

                if (m_flipY) {
                    l_render.Effects |= SpriteEffects.FlipVertically;
                    l_render.Pivot.Y = m_rect[l_key.Folder][l_render.File].Height - l_render.Pivot.Y;
                }

                if (m_flipX != m_flipY) {
                    l_render.Rotation *= -1.0f;
                }

                l_render.ZOrder = l_mainline.Body[l_i].ZOrder;

                m_renderList.Add(l_render);
            }
        }

        public void Draw(SpriteBatch p_spriteBatch) {
            p_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone);
            foreach (RenderMatrix l_render in m_renderList) {
                p_spriteBatch.Draw(
                    m_tx[l_render.Folder],
                    l_render.Location,
                    m_rect[l_render.Folder][l_render.File],
                    m_color * l_render.Alpha,
                    l_render.Rotation,
                    l_render.Pivot, 
                    l_render.Scale,
                    l_render.Effects,
                    /*(float)l_render.ZOrder*/0.0f
                );
            }
            p_spriteBatch.End();
        }
    }
}
