#region Using statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Physics = FarseerGames.FarseerPhysics;
#endregion

namespace FieldPong
{
    /// <summary>
    /// This behavior causes an actor to die the instant it collides with another actor's physics body.
    /// </summary>
    class DieUponCollisionBehavior : ActorBehavior
    {
        #region Initialization

        /// <summary>
        /// Initializes a new instance of DieUponCollisionBehavior.
        /// </summary>
        public DieUponCollisionBehavior(ActorManager actorManager)
            : base(actorManager)
        {
        }

        /// <summary>
        /// This addes the custom collision handler to the physics geometry.
        /// </summary>
        public override void Initialize()
        {
            parentActor.PhysicsGeom.CollisionHandler =
                new Physics.Collisions.Geom.CollisionHandlerDelegate(CollisionHandler);
        }

        #endregion

        #region Update

        /// <summary>
        /// This just kills the actor if it's hit something.
        /// </summary>
        public bool CollisionHandler(Physics.Collisions.Geom geom1, Physics.Collisions.Geom geom2, Physics.Collisions.ContactList contacts)
        {
            parentActor.Kill();

            return true;
        }

        /// <summary>
        /// This updates the behavior.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // Nothing to do!
        }

        #endregion
    }
}