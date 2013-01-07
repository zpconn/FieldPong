#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace FieldPong
{
    /// <summary>
    /// Represents a single animation frame that is displayed for a certain amount of time.
    /// </summary>
    public struct AnimationFrame
    {
        #region Fields

        /// <summary>
        /// The length of time this frame should be displayed
        /// </summary>
        public TimeSpan DelayTime;


        /// <summary>
        /// The name of the content file, without extension, representing the texture to be used for this frame.
        /// </summary>
        public string TextureName;

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new AnimationFrame.
        /// </summary>
        public AnimationFrame(TimeSpan delayTime, string textureName)
        {
            DelayTime = delayTime;
            TextureName = textureName;
        }

        #endregion
    }


    /// <summary>
    /// Specifies the directions in which an animation can be updated.
    /// </summary>
    public enum AnimationDirection
    {
        Forward,
        Backward,
    }


    /// <summary>
    /// An Animation stores and plays a sequence of AnimationFrames.
    /// </summary>
    public class Animation
    {
        #region Fields

        List<AnimationFrame> frames = new List<AnimationFrame>();
        List<Texture2D> frameTextures = new List<Texture2D>();
        int currentFrameIndex = 0;
        TimeSpan accumulator = TimeSpan.Zero;
        bool running = true;
        AnimationDirection direction = AnimationDirection.Forward;
        ContentManager content;
        bool repeat = true;
        float width = 0.0f;
        float height = 0.0f;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the index of the current frame the animation is displaying.
        /// </summary>
        public int CurrentFrameIndex
        {
            get { return currentFrameIndex; }
        }


        /// <summary>
        /// Gets the total number of frames in the animation.
        /// </summary>
        public int NumFrames
        {
            get { return frames.Count; }
        }


        /// <summary>
        /// Indicates whether the animation is being updated or not.
        /// </summary>
        public bool Running
        {
            get { return running; }
            set { running = value; }
        }


        /// <summary>
        /// Specifies the direction in which the animation is played.
        /// </summary>
        public AnimationDirection Direction
        {
            get { return direction; }
            set { direction = value; }
        }


        /// <summary>
        /// Indicates whether the animation will loop once it has played through completely.
        /// </summary>
        public bool Repeat
        {
            get { return repeat; }
            set { repeat = value; }
        }


        /// <summary>
        /// Exposes the list of frames as an array so outside code can examine it. The returned array is a copy of the interal
        /// master list so that the master list can only be modified through the Animation class interface.
        /// </summary>
        public AnimationFrame[] Frames
        {
            get { return frames.ToArray(); }
        }

        /// <summary>
        /// Gets or sets the width of the rendered sprite.
        /// </summary>
        public float Width
        {
            get { return width; }
            set { width = value; }
        }

        /// <summary>
        /// Gets or sets the height of rendered sprite.
        /// </summary>
        public float Height
        {
            get { return height; }
            set { height = value; }
        }

        /// <summary>
        /// Gets the width of the texture image for the current frame.
        /// </summary>
        public float SourceWidth
        {
            get { return frameTextures[currentFrameIndex].Width; }
        }

        /// <summary>
        /// Gets the height of the texture image for the current frame.
        /// </summary>
        public float SourceHeight
        {
            get { return frameTextures[currentFrameIndex].Height; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new Animation. Allows outside code to specify the ContentManager to be used for loading textures.
        /// If width and height are both zero, then this just uses the texture size.
        /// </summary>
        public Animation(ContentManager content, float width, float height)
        {
            this.content = content;
            this.width = width;
            this.height = height;
        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// Updates the current animation frame.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            // Don't update the timers and current frame if the animation is paused or if there aren't any frames 
            // to update in the first place
            if (!running || frames.Count <= 0)
                return;

            // Use an accumulation method to make the timing as accurate as possible
            accumulator += gameTime.ElapsedGameTime;
            if (accumulator > frames[currentFrameIndex].DelayTime)
            {
                UpdateAnimationFrame();
                accumulator -= frames[currentFrameIndex].DelayTime;
            }
        }

        /// <summary>
        /// This resets the animation so that the current frame is the initial one.
        /// </summary>
        public void Reset()
        {
            // Don't do anything if there aren't any frames
            if (frames.Count <= 0)
                return;

            currentFrameIndex = 0;

            // We need to restart the timer as well
            accumulator = TimeSpan.Zero;
        }

        /// <summary>
        /// Draws the current animation frame with the provided transformation. This will only use the provided scale
        /// value if the user did not provide custom width and height values earlier.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Vector2 position, float rotation, float scale, Color color,
                         Vector2 origin)
        {
            if (frames.Count <= 0)
                return;

            Rectangle sourceRectangle;
            float scaleModified = scale; // The scale value might need to be modified if we use our own width and height. See below.

            if (width > 0 && height > 0)
            {
                // We're going to use our own custom width and height for the texture.

                // Compute the scaling factor for later on. This is necessary so the whole sprite image
                // fits within the new width and height.

                scaleModified = width / frameTextures[currentFrameIndex].Width;
            }

            sourceRectangle = new Rectangle(0, 0, frameTextures[currentFrameIndex].Width,
                frameTextures[currentFrameIndex].Height);

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            spriteBatch.Draw(frameTextures[currentFrameIndex], position, sourceRectangle, color, rotation, origin, scaleModified,
                 SpriteEffects.None, 0.0f);

            spriteBatch.End();
        }


        /// <summary>
        /// Moves the current frame to the next, depending on the direction of animation.
        /// </summary>
        private void UpdateAnimationFrame()
        {
            if (direction == AnimationDirection.Forward)
            {
                currentFrameIndex++;

                if (currentFrameIndex >= frames.Count)
                {
                    if (repeat)
                        currentFrameIndex = 0;
                    else
                        currentFrameIndex--;
                }
            }
            else if (direction == AnimationDirection.Backward)
            {
                currentFrameIndex--;

                if (currentFrameIndex < 0)
                {
                    if (repeat)
                        currentFrameIndex = frames.Count - 1;
                    else
                        currentFrameIndex++;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a new frame and loads its associated texture into memory using the content manager passed
        /// to the animation in the constructor.
        /// </summary>
        public void AddFrame(AnimationFrame newFrame)
        {
            frames.Add(newFrame);
            frameTextures.Add(content.Load<Texture2D>(newFrame.TextureName));
        }


        /// <summary>
        /// Removes a frame and its associated texture.
        /// </summary>
        public void RemoveFrame(int index)
        {
            frames.RemoveAt(index);
            frameTextures.RemoveAt(index);
        }

        #endregion
    }

    /// <summary>
    /// A sprite is the visual representation of an object on the screen. It contains a collection of animations
    /// and provides support for playing them and switching among them. A sprite does not store its position on the 
    /// screen and other transformation information; this information corresponds to the internal representation
    /// of the object, whereas a sprite respresents the external representation of the object. This specific information
    /// is the job of the actor system to take care of.
    /// </summary>
    public class Sprite
    {
        #region Fields

        Dictionary<string, Animation> animations = new Dictionary<string, Animation>();
        Animation currentAnimation = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the animation currently playing for this sprite.
        /// </summary>
        public Animation CurrentAnimation
        {
            get { return currentAnimation; }
        }

        /// <summary>
        /// This is how wide the final rendered sprite will be. This is not the width of the source image.
        /// </summary>
        public float Width
        {
            get
            {
                if (currentAnimation != null)
                    return currentAnimation.Width;

                return 0;
            }
        }

        /// <summary>
        /// This is how tall the final rendered sprite will be. This is not the height of the source image.
        /// </summary>
        public float Height
        {
            get
            {
                if (currentAnimation != null)
                    return currentAnimation.Height;

                return 0;
            }
        }

        /// <summary>
        /// Gets the width of the texture image for the current frame.
        /// </summary>
        public float SourceWidth
        {
            get { return currentAnimation.SourceWidth; }
        }

        /// <summary>
        /// Gets the height of the texture image for the current frame.
        /// </summary>
        public float SourceHeight
        {
            get { return currentAnimation.SourceHeight; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes a new Sprite.
        /// </summary>
        public Sprite()
        {
        }

        /// <summary>
        /// This method creates a new Sprite that has a single animation consisting of a single frame displaying
        /// a specified texture indefinitely. In other words, this can be used to quickly create a static sprite
        /// that doesn't need to use the animation framework. The animation it creates is named "defaultAnim."
        /// </summary>
        public static Sprite CreateStatic(ContentManager content, string textureName)
        {
            Sprite newSprite = new Sprite();

            Animation defaultAnim = new Animation(content, 0, 0);
            defaultAnim.AddFrame(new AnimationFrame(TimeSpan.FromSeconds(1.0f), textureName));

            newSprite.AddAnimation("defaultAnim", defaultAnim);
            newSprite.SetCurrentAnimation("defaultAnim");

            return newSprite;
        }

        /// <summary>
        /// This method creates a new Sprite that has a single animation consisting of a single frame displaying
        /// a specified texture indefinitely. In other words, this can be used to quickly create a static sprite
        /// that doesn't need to use the animation framework. The animation it creates is named "defaultAnim."
        /// </summary>
        public static Sprite CreateStatic(ContentManager content, string textureName, float width, float height)
        {
            Sprite newSprite = new Sprite();

            Animation defaultAnim = new Animation(content, width, height);
            defaultAnim.AddFrame(new AnimationFrame(TimeSpan.FromSeconds(1.0f), textureName));

            newSprite.AddAnimation("defaultAnim", defaultAnim);
            newSprite.SetCurrentAnimation("defaultAnim");

            return newSprite;
        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// Updates the current animation of the sprite.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if (currentAnimation == null)
                return;

            currentAnimation.Update(gameTime);
        }


        /// <summary>
        /// Draws the current animation with the provided transformation.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Vector2 position, float rotation, float scale, Color color,
                         Vector2 origin)
        {
            if (currentAnimation == null)
                return;

            currentAnimation.Draw(spriteBatch, position, rotation, scale, color, origin);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a new animation to the internal and associates it with the provided name.
        /// </summary>
        public void AddAnimation(string name, Animation newAnimation)
        {
            animations.Add(name, newAnimation);
        }


        /// <summary>
        /// Removes the named animation. If the animation to remove is the current animation, the current animation
        /// is set to null.
        /// </summary>
        /// <param name="name"></param>
        public void RemoveAnimation(string name)
        {
            if (!animations.ContainsKey(name))
            {
                throw new ArgumentException("There is no animation of the provided name in the internal animation list.");
            }

            Animation animToRemove = animations[name];

            if (currentAnimation == animToRemove)
                currentAnimation = null;

            animations.Remove(name);
        }


        /// <summary>
        /// Sets the current animation to the one corresponding to the name provided.
        /// </summary>
        public void SetCurrentAnimation(string name)
        {
            if (!animations.ContainsKey(name))
            {
                throw new ArgumentException("There is no animation of the provided name in the internal animation list.");
            }

            currentAnimation = animations[name];
        }

        #endregion
    }
}