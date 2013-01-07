#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace FieldPong
{
    /// <summary>
    /// This screen is displayed after a game over. It shows the player his final score and gives him the option
    /// of quitting or returning to the main menu.
    /// </summary>
    class GameOverScreen : MenuScreen
    {
        #region Fields

        int finalLevel;

        #endregion

        #region Initialization

        /// <summary>
        /// Populates the menu with items and records the player's final level.
        /// </summary>
        /// <param name="finalLevel"></param>
        public GameOverScreen(int finalLevel)
            : base()
        {
            this.finalLevel = finalLevel;

            MenuEntries.Add("Main Menu");
            MenuEntries.Add("Quit");
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
                case 0: // Main Menu
                    LoadingScreen.Load(ScreenManager, LoadMainMenu);
                    break;

                case 1: // Quit
                    OnCancel();
                    break;
            }
        }

        /// <summary>
        /// This closes the game!
        /// </summary>
        protected override void OnCancel()
        {
            ScreenManager.Game.Exit();
        }

        /// <summary>
        /// Loads up the main menu.
        /// </summary>
        void LoadMainMenu(object sender, EventArgs e)
        {
            ScreenManager.AddScreen(new MainMenuScreen());
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draws the player's final level above the menu items.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin();

            ScreenManager.SpriteBatch.DrawString(ScreenManager.Font, "Game Over! Your final level was " + finalLevel.ToString() + ".",
                new Vector2(1024 / 4, 768 / 4), Color.Green);

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }

        #endregion
    }
}