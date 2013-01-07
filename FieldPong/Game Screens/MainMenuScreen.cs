#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace FieldPong
{
    class MainMenuScreen : MenuScreen
    {
        #region Fields

        Sprite logoSprite;

        #endregion

        #region Initialization

        /// <summary>
        /// Populates the menu with items.
        /// </summary>
        public MainMenuScreen()
            : base()
        {
            MenuEntries.Add("Play");
            MenuEntries.Add("Quit");
        }

        /// <summary>
        /// Loads in the logo sprite.
        /// </summary>
        public override void LoadGraphicsContent(bool loadAllContent)
        {
            if (loadAllContent)
            {
                logoSprite = Sprite.CreateStatic(ScreenManager.Content, "Content/logo");
            }
        }

        #endregion

        #region Handle Input

        /// <summary>
        /// Responds to user menu selections.
        /// </summary>
        protected override void OnSelectEntry(int entryIndex)
        {
            switch (entryIndex)
            {
                case 0: // Play
                    LoadingScreen.Load(ScreenManager, LoadGameplayScreen);
                    break;

                case 1: // Quit
                    OnCancel();
                    break;
            }
        }

        /// <summary>
        /// Adds the GameplayScreen to the screen stack.
        /// </summary>
        void LoadGameplayScreen(object sender, EventArgs e)
        {
            // Add the game screen
            GameplayScreen gameScreen = new GameplayScreen();

            gameScreen.ScreenManager = ScreenManager;
            gameScreen.Initialize();

            ScreenManager.AddScreen(gameScreen);
        }

        /// <summary>
        /// Displays a confirmation message asking if the user wants to exit.
        /// </summary>
        protected override void OnCancel()
        {
            ScreenManager.Game.Exit();
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draws the logo above the menu.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            logoSprite.Draw(ScreenManager.SpriteBatch, new Vector2(), 0.0f, 1.0f, new Color(255, 255, 255, 255), new Vector2());

            base.Draw(gameTime);
        }

        #endregion
    }
}