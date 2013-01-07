#region Using statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Physics = FarseerGames.FarseerPhysics;
#endregion

namespace FieldPong
{
    /// <summary>
    /// This behavior causes an actor to be sucked in by the gravity of all actors possessing the 
    /// BlackHoleBehavior behavior.
    /// </summary>
    class InteractWithBlackHolesBehavior : ActorBehavior
    {
        #region Fields

        const float GravitationalConstant = 9990; // Gravity is an amazingly weak force...

        #endregion

        #region Initialization

        /// <summary>
        /// This creates a new instance of InteractWithBlackHolesBehavior.
        /// </summary>
        public InteractWithBlackHolesBehavior(ActorManager actorManager)
            : base(actorManager)
        {
        }

        #endregion

        #region Update

        /// <summary>
        /// This computes the net gravitational force on the actor by all the black hole actors and then applies this force
        /// to the actor's physics body.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            Vector2 gravForce = ComputeGravitationalForce();
            parentActor.PhysicsBody.ApplyForce(gravForce);
        }

        /// <summary>
        /// This finds the gravitational force the actor.
        /// </summary>
        private Vector2 ComputeGravitationalForce()
        {
            // First get all those actors who generate gravity.
            List<Actor> blackHoleActors = actorManager.FindActorsWithBehavior(typeof(BlackHoleBehavior));

            // Now compute the net gravity from them all.

            Vector2 netForce = new Vector2();

            foreach (Actor actor in blackHoleActors)
            {
                Vector2 disp = actor.PhysicsBody.Position - parentActor.PhysicsBody.Position;
                float distance = disp.Length();
                disp = Vector2.Normalize(disp);

                Vector2 force = disp * GravitationalConstant * actor.PhysicsBody.Mass * parentActor.PhysicsBody.Mass /
                                (distance); // Dividing by just distance instead of its square gives the force a 
                                            // longer radius of action, which is important for a game like this.
                netForce += force;
            }

            return netForce;
        }

        #endregion
    }
}