#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace FieldPong
{
    /// <summary>
    /// A lightning box is a just a rectangle where the edges are drawn as streaks of lightning.
    /// </summary>
    class LightningBox
    {
        #region Fields

        Rectangle rectangle;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the rectangle this LightningBox represents.
        /// </summary>
        public Rectangle Rectangle
        {
            get { return rectangle; }
            set { rectangle = value; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// This constructs a new LightningBox determined by the position and size of the specified rectangle.
        /// </summary>
        public LightningBox(Rectangle rectangle)
        {
            this.rectangle = rectangle;
        }

        #endregion

        #region Draw

        /// <summary>
        /// A streak of lightning is drawn simply by piecing together a long string of small line segments, 
        /// each of which is slightly rotated.
        /// </summary>
        private void DrawLightningStreak(LineBatch lineBatch, Color color, Vector2 start, Vector2 end, int numSegments,
                                         float deviation)
        {
            Random randomGen = new Random();

            Vector2 disp = end - start;
            Vector2 unitDisp = Vector2.Normalize(disp);
            Vector2 stepVector = unitDisp * disp.Length() / (float)numSegments;
            Vector2 perpVec = new Vector2(-stepVector.Y, stepVector.X);
            perpVec.Normalize();

            Vector2 startPoint = start;
            Vector2 endPoint = new Vector2();

            for (int i = 0; i < numSegments; ++i)
            {
                endPoint = start + stepVector * (i + 1);

                // If this is an intermediate segment, apply an offset perpendicular to the line connecting start to end.
                if (i < numSegments - 1)
                {
                    float random = (float)randomGen.NextDouble();
                    float offset = (random >= 0.5f ? 1 : -1) * random * deviation;

                    endPoint += perpVec * offset;
                }

                lineBatch.DrawLine(startPoint, endPoint, color);

                startPoint = endPoint;
            }
        }

        /// <summary>
        /// This draws the box with streaks of lightning representing its edges.
        /// </summary>
        public void Draw(LineBatch lineBatch, Color color, int numSegments, float deviation)
        {
            // Let's compute the coordinates of all the vertices to make the line drawing simpler.

            Vector2 upperLeft = new Vector2(rectangle.Left, rectangle.Top);
            Vector2 upperRight = new Vector2(rectangle.Right, rectangle.Top);
            Vector2 lowerLeft = new Vector2(rectangle.Left, rectangle.Bottom);
            Vector2 lowerRight = new Vector2(rectangle.Right, rectangle.Bottom);

            // Now draw the lightning streaks.

            DrawLightningStreak(lineBatch, color, upperLeft, upperRight, numSegments, deviation);
            DrawLightningStreak(lineBatch, color, upperRight, lowerRight, numSegments, deviation);
            DrawLightningStreak(lineBatch, color, lowerRight, lowerLeft, numSegments, deviation);
            DrawLightningStreak(lineBatch, color, lowerLeft, upperLeft, numSegments, deviation);
        }

        #endregion
    }
}