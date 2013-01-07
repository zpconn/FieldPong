#region Using statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Physics = FarseerGames.FarseerPhysics;
#endregion

namespace FieldPong
{
    /// <summary>
    /// This behavior causes an actor to persist in the world for a limited duration, killing it once the timer expires.
    /// This also will make the actor smoothly fade in and out according to its lifetime.
    /// </summary>
    class LiveTemporarilyBehavior : ActorBehavior
    {
        #region Fields

        TimeSpan lifetime;
        TimeSpan timePassed = TimeSpan.Zero;

        TimeSpan fadeDuration;
        TimeSpan fadeInTimer = TimeSpan.Zero;
        TimeSpan fadeOutTimer = TimeSpan.Zero;

        #endregion

        #region Initialization

        /// <summary>
        /// This creates a new LiveTemporarilyBehavior and sets how long the actor is to live.
        /// </summary>
        public LiveTemporarilyBehavior(ActorManager actorManager, TimeSpan lifetime, TimeSpan fadeDuration)
            : base(actorManager)
        {
            this.lifetime = lifetime;
            this.fadeDuration = fadeDuration;
        }

        #endregion

        #region Update

        /// <summary>
        /// This updates the timer, and if it has exceeded the lifetime value specified upon creation of this behavior
        /// the actor is killed.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            if (fadeInTimer < fadeDuration)
            {
                fadeInTimer += gameTime.ElapsedGameTime;
                parentActor.Alpha = (float)(fadeInTimer.TotalSeconds / fadeDuration.TotalSeconds) * 255.0f;
            }

            if ((lifetime.TotalSeconds - timePassed.TotalSeconds) <= fadeDuration.TotalSeconds)
            {
                fadeOutTimer += gameTime.ElapsedGameTime;
                parentActor.Alpha = 255.0f - (float)(fadeOutTimer.TotalSeconds / (fadeDuration.TotalSeconds)) * 255.0f;
            }

            if (timePassed > lifetime)
            {
                parentActor.Kill();
            }

            timePassed += gameTime.ElapsedGameTime;
        }

        #endregion
    }
}