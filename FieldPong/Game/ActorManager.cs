#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace FieldPong
{
    /// <summary>
    /// Owns and manages all the game actors in the game world.
    /// </summary>
    class ActorManager
    {
        #region Fields

        Dictionary<string, Actor> actorTemplates = new Dictionary<string, Actor>();
        List<Actor> liveActors = new List<Actor>();
        List<Actor> newActors = new List<Actor>();

        #endregion

        #region Update

        /// <summary>
        /// Updates all alive actors and removes dead actors from the world.
        /// </summary>
        public void Update(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Remove dead actors

            List<Actor> deadActors = new List<Actor>();

            foreach (Actor actor in liveActors)
            {
                if (!actor.Alive)
                    deadActors.Add(actor);
            }

            foreach (Actor deadActor in deadActors)
            {
                liveActors.Remove(deadActor);
            }

            deadActors.Clear();

            // Update alive actors

            foreach (Actor actor in liveActors)
            {
                actor.Update(gameTime, spriteBatch);
            }

            // Now add in whatever actors might have been created by some actor.Update() call.

            foreach (Actor actor in newActors)
            {
                liveActors.Add(actor);
            }

            newActors.Clear();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Unloads all actor data and removes all the physics objects from the simulator.
        /// </summary>
        public void Unload()
        {
            // First kill all the actors.

            foreach (Actor actor in liveActors)
            {
                actor.Kill();
            }

            foreach (Actor actor in newActors)
            {
                actor.Kill();
            }

            // Now clear all the lists.
            liveActors.Clear();
            newActors.Clear();
            actorTemplates.Clear();
        }

        /// <summary>
        /// Registers a new template.
        /// </summary>
        public void AddActorTemplate(Actor template, string name)
        {
            actorTemplates[name] = template;
        }

        /// <summary>
        /// Creates a live actor active in the game world based off a registered actor template.
        /// </summary>
        public Actor InstantiateTemplate(string templateName)
        {
            if (!actorTemplates.ContainsKey(templateName))
                throw new ArgumentException("In ActorManager.InstantiateTemplate(), 'templateName' must refer to a valid " +
                                            "registered Actor template.", "templateName");

            Actor clone = actorTemplates[templateName].Clone();
            newActors.Add(clone);

            return clone;
        }

        /// <summary>
        /// This examines all the live actors and returns a list of references to just those possessing a certain behavior.
        /// </summary>
        public List<Actor> FindActorsWithBehavior(Type behaviorType)
        {
            // First make sure the passed in type is derived from ActorBehavior
            if (!behaviorType.IsSubclassOf(typeof(ActorBehavior)))
                throw new ArgumentException("The first parameter of ActorManager.FindActorsWithBehavior must be a type that derives from " +
                                            "ActorBehavior.", "behaviorType");

            // Now search through all the live actors and retain references to those that have the desired behavior.

            List<Actor> actorsWithBehavior = new List<Actor>();

            foreach (Actor actor in liveActors)
            {
                ActorBehavior behavior = actor.GetBehavior(behaviorType);

                if (behavior != null)
                    actorsWithBehavior.Add(actor);
            }

            return actorsWithBehavior;
        }

        #endregion
    }
}