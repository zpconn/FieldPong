#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace FieldPong
{
    /// <summary>
    /// Mimics the interface of SpriteBatch to provide functionality for drawing lines in large quantity.
    /// </summary>
    class LineBatch
    {
        #region Constants

        /// <summary>
        /// The maximum number of vertices in the LineBatch vertex array.
        /// </summary>
        const int MaxVertexCount = 512;

        #endregion

        #region Fields

        GraphicsDevice graphicsDevice;
        BasicEffect effect;
        VertexDeclaration vertexDeclaration;
        VertexPositionColor[] vertices;
        int currentIndex;
        int lineCount;

        #endregion

        #region Initialization

        /// <summary>
        /// Constructs a new LineBatch.
        /// </summary>
        public LineBatch(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("graphicsDevice");
            }
            this.graphicsDevice = graphicsDevice;

            // Create and configure the effect
            effect = new BasicEffect(graphicsDevice, null);
            effect.VertexColorEnabled = true;
            effect.TextureEnabled = false;
            effect.LightingEnabled = false;

            // Set up the matrices
            effect.World = Matrix.Identity;
            effect.View = Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up);

            // Create the vertex declaration
            vertexDeclaration = new VertexDeclaration(graphicsDevice, VertexPositionColor.VertexElements);

            // Create the vertex array
            vertices = new VertexPositionColor[MaxVertexCount];
        }

        /// <summary>
        /// Allow outside code to set up the projection matrix.
        /// </summary>
        public void SetProjection(Matrix projection)
        {
            if (effect != null)
            {
                effect.Projection = projection;
            }
        }

        #endregion

        #region Destruction

        /// <summary>
        /// Disposes of the object, cleanly releasing graphics resources.
        /// </summary>
        public void Dispose()
        {
            if (effect != null)
            {
                effect.Dispose();
                effect = null;
            }

            if (vertexDeclaration != null)
            {
                vertexDeclaration.Dispose();
                vertexDeclaration = null;
            }
        }

        #endregion

        #region Drawing

        /// <summary>
        /// Configures the object and the device to begin drawing lines.
        /// </summary>
        public void Begin()
        {
            // Reset the counters
            currentIndex = 0;
            lineCount = 0;
        }

        /// <summary>
        /// Draw a line from one point to another with the same color.
        /// </summary>
        public void DrawLine(Vector2 start, Vector2 end, Color color)
        {
            DrawLine(
                new VertexPositionColor(new Vector3(start, 0f), color),
                new VertexPositionColor(new Vector3(end, 0f), color));
        }

        /// <summary>
        /// Draw a line from one point to another with different colors at each end.
        /// </summary>
        public void DrawLine(Vector2 start, Vector2 end, Color startColor, Color endColor)
        {
            DrawLine(
                new VertexPositionColor(new Vector3(start, 0f), startColor),
                new VertexPositionColor(new Vector3(end, 0f), endColor));
        }

        /// <summary>
        /// Draw a line from one vertex to another.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public void DrawLine(VertexPositionColor start, VertexPositionColor end)
        {
            // If there's no room for another line, draw the current batch and start a new one
            if (currentIndex >= (vertices.Length - 2))
            {
                End();
                Begin();
            }

            vertices[currentIndex++] = start;
            vertices[currentIndex++] = end;

            lineCount++;
        }

        /// <summary>
        /// Ends the batch of lines, submitting them to the graphics device.
        /// </summary>
        public void End()
        {
            // If we don't have any vertices, then we can exit early
            if (currentIndex == 0)
                return;

            // Configure the graphics device and effect to render our lines
            graphicsDevice.VertexDeclaration = vertexDeclaration;
            graphicsDevice.RenderState.AlphaBlendEnable = true;
            graphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            graphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;

            // Run the effect
            effect.Begin();

            for (int i = 0; i < effect.CurrentTechnique.Passes.Count; ++i)
            {
                effect.CurrentTechnique.Passes[i].Begin();
                graphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertices, 0, lineCount);
                effect.CurrentTechnique.Passes[i].End();
            }

            effect.End();
        }

        #endregion
    }
}