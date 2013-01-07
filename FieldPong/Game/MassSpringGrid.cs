#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace FieldPong
{
    /// <summary>
    /// Represents a grid of points, each connected to its neighbors via ideal springs. In its equilibrium state, the
    /// points are all uniformly spaced out in a large square.
    /// </summary>
    class MassSpringGrid
    {
        #region Helper Classes

        public class Node
        {
            public Vector2 Position = new Vector2();
            public Vector2 Velocity = new Vector2();
            public Vector2 Forces = new Vector2();
        }

        #endregion

        #region Fields

        int gridSize;
        Node[,] grid;
        float nodeMass;
        float springConstant;
        float springDamping;
        Rectangle bounds;

        #endregion

        #region Properties

        /// <summary>
        /// Exposes the inner list of nodes.
        /// </summary>
        public Node[,] Nodes
        {
            get { return grid; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Constructs a new MassSpringGrid
        /// </summary>
        public MassSpringGrid(int gridSize, float nodeMass, float springConstant, float springDamping, Rectangle bounds)
        {
            this.gridSize = gridSize;
            this.nodeMass = nodeMass;
            this.springConstant = springConstant;
            this.springDamping = springDamping;
            this.bounds = bounds;

            CreateGrid();
        }

        /// <summary>
        /// Populates the node array with nodes
        /// </summary>
        private void CreateGrid()
        {
            grid = new Node[gridSize, gridSize];

            for (int x = 0; x < gridSize; ++x)
            {
                for (int y = 0; y < gridSize; ++y)
                {
                    grid[x, y] = new Node();
                }
            }

            int regionWidth = bounds.Right - bounds.Left;
            int regionHeight = bounds.Bottom - bounds.Top;

            int dx = regionWidth / gridSize;
            int dy = regionHeight / gridSize;

            int startX = bounds.X + (regionWidth - dx * gridSize) / 2;
            int startY = bounds.Y + (regionHeight - dy * gridSize) / 2;

            int px = startX;
            int py = startY;

            for (int x = 0; x < gridSize; ++x)
            {
                for (int y = 0; y < gridSize; ++y)
                {
                    grid[x, y].Position = new Vector2((float)px, (float)py);
                    py += dy;
                }

                py = startY;
                px += dx;
            }
        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// Updates the mass-spring simulation.
        /// </summary>
        public void Update(float elapsedTime)
        {
            float dampingMultiplier = (float)Math.Exp(-elapsedTime * springDamping);

            for (int x = 1; x < gridSize - 1; ++x)
            {
                for (int y = 1; y < gridSize - 1; ++y)
                {
                    Vector2 averageNeighborPos =
                        (grid[x - 1, y - 1].Position + grid[x, y - 1].Position + grid[x + 1, y - 1].Position +
                          grid[x - 1, y].Position + grid[x + 1, y].Position +
                          grid[x - 1, y + 1].Position + grid[x, y + 1].Position + grid[x + 1, y + 1].Position) / 8f;

                    grid[x, y].Forces += -springConstant * (grid[x, y].Position - averageNeighborPos);
                    grid[x, y].Velocity += (grid[x, y].Forces / nodeMass) * elapsedTime;
                    grid[x, y].Velocity *= dampingMultiplier;
                    grid[x, y].Position += grid[x, y].Velocity * elapsedTime;

                    grid[x, y].Forces = new Vector2();
                }
            }
        }

        /// <summary>
        /// Draws the grid, showing the springs connecting neighboring nodes as line segments. The color of each
        /// line segment is interpolated linearly between dimColor and brightColor depending on the length
        /// of the segment.
        /// </summary>
        public void Draw(LineBatch lineBatch, Color dimColor, Color brightColor)
        {
            float dx = (bounds.Right - bounds.Left) / gridSize;
            float dy = (bounds.Bottom - bounds.Top) / gridSize;

            Vector4 vecDimColor = new Vector4(dimColor.R, dimColor.G, dimColor.B, dimColor.A);
            Vector4 vecBrightColor = new Vector4(brightColor.R, brightColor.G, brightColor.B, brightColor.A);

            for (int x = 0; x < gridSize; ++x)
            {
                for (int y = 0; y < gridSize; ++y)
                {
                    if (x < gridSize - 1)
                    {
                        float lineLength = (grid[x + 1, y].Position - grid[x, y].Position).Length();
                        float t = (lineLength - dx) / (5 * (float)Math.Sqrt(2) * dx);

                        Vector4 modulatedColorVec = Vector4.Lerp(vecDimColor, vecBrightColor, t);
                        Color modulatedColor = new Color((byte)modulatedColorVec.X, (byte)modulatedColorVec.Y, (byte)modulatedColorVec.Z,
                                                         (byte)modulatedColorVec.W);

                        lineBatch.DrawLine(grid[x, y].Position, grid[x + 1, y].Position, modulatedColor);
                    }

                    if (y < gridSize - 1)
                    {
                        float lineLength = (grid[x, y + 1].Position - grid[x, y].Position).Length();
                        float t = (lineLength - dy) / (5 * (float)Math.Sqrt(2) * dx);

                        Vector4 modulatedColorVec = Vector4.Lerp(vecDimColor, vecBrightColor, t);
                        Color modulatedColor = new Color((byte)modulatedColorVec.X, (byte)modulatedColorVec.Y, (byte)modulatedColorVec.Z,
                                                         (byte)modulatedColorVec.W);

                        lineBatch.DrawLine(grid[x, y].Position, grid[x, y + 1].Position, modulatedColor);
                    }
                }
            }
        }

        #endregion

        #region Interaction

        /// <summary>
        /// Propagates a disturbance throughout the grid, the influence on each point being
        /// given by magnitude/(length from point to node squared).
        /// </summary>
        public void ApplyDisturbance(Vector2 point, float magnitude)
        {
            foreach (Node node in grid)
            {
                Vector2 displacement = point - node.Position;
                node.Velocity -= Vector2.Normalize(displacement) * Math.Min(magnitude / displacement.LengthSquared(), 100);
            }
        }

        /// <summary>
        /// This causes the grid to "twist" or "wrap" around a point.
        /// </summary>
        public void TwistGrid(Vector2 center, int wrapDirection, float torque)
        {
            foreach (Node node in grid)
            {
                Vector2 displacement = node.Position - center;
                Vector2 dispUnit = Vector2.Normalize(displacement);
                Vector2 perpVec = new Vector2(-dispUnit.Y, dispUnit.X) * wrapDirection;

                float magnitude = Math.Min(torque / displacement.LengthSquared(), 100);

                node.Velocity += perpVec * magnitude;
            }
        }

        #endregion
    }
}