#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace FieldPong
{
    /// <summary>
    /// A starfield represented by single-pixel "stars" with parallax scrolling.
    /// </summary>
    class Starfield
    {
        #region Constants

        /// <summary>
        /// The maximum speed that the stars move at, in pixels per second.
        /// </summary>
        const float StarVelocity = 32.0f;

        #endregion

        #region Fields

        Random random;
        Vector2[] starPositions;
        byte[] starDepths;
        Vector2 targetPosition;
        Vector2 currentPosition;

        #endregion

        #region Initialization

        public Starfield(int count, Rectangle bounds)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException("count");

            random = new Random();
            currentPosition = targetPosition = new Vector2(
                bounds.Left + (bounds.Right - bounds.Left) / 2,
                bounds.Top + (bounds.Bottom - bounds.Top) / 2);
            starPositions = new Vector2[count];
            starDepths = new byte[count];

            for (int i = 0; i < count; ++i)
            {
                starPositions[i] = new Vector2(
                    random.Next(bounds.Left, bounds.Right),
                    random.Next(bounds.Top, bounds.Bottom));
                starDepths[i] = (byte)random.Next(1, 255);
            }
        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// Updates the starfield
        /// </summary>
        public void Update(float elapsedTime)
        {
            if (targetPosition != currentPosition)
            {
                Vector2 movement = targetPosition - currentPosition;

                // If the displacement is within a unit square in each direction, then we're close enough
                if (movement.LengthSquared() < 1.414f) // approximation of sqrt(2)
                {
                    currentPosition = targetPosition;
                    return;
                }

                // Move the current position
                movement = Vector2.Normalize(movement) * (StarVelocity * elapsedTime);
                currentPosition += movement;

                // Move each star, scaling its motion by its depth
                for (int i = 0; i < starPositions.Length; ++i)
                {
                    starPositions[i] += movement * (float)starDepths[i] / 255f;
                }
            }
        }

        /// <summary>
        /// Renders the starfield.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Texture2D spriteTexture)
        {
            if (spriteBatch == null)
                throw new ArgumentNullException("spriteBatch");

            if (spriteTexture == null)
                throw new ArgumentNullException("spriteTexture");

            for (int i = 0; i < starPositions.Length; ++i)
            {
                Color starColor = new Color(starDepths[i], starDepths[i], starDepths[i]);
                spriteBatch.Draw(spriteTexture, new Rectangle(
                    (int)starPositions[i].X, (int)starPositions[i].Y, 1, 1), starColor);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Assign the target position for the starfield to scroll to
        /// </summary>
        public void SetTargetPosition(Vector2 targetPosition)
        {
            this.targetPosition = targetPosition;
        }

        #endregion
    }
}