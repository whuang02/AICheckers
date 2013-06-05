//BoardTile.cs
//Written by Wei Wei Huang

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AICheckers
{
    /// <summary>
    /// Class to represent a tile on the gameboard of the checker board
    /// </summary>
    class BoardTile
    {
        //int value to declare the width of a tile
        public static int TILE_WIDTH = 100;
        //int value to declare the height of a tile
        public static int TILE_HEIGHT = 100;
        //2D vector to keep track of the position of the tile on the board i.e (1,1)
        public Vector2 position;
        //Status of the vector - used to determine if a tile should be marked
        //  A tile is marked to show the user possible moves by a piece
        private TileStatus status;
        //Field to keep track of the occupied status of the tile
        private PieceColor isOccupied;
        //Rectangle area for the tile - used to mouse clicks
        private Rectangle tileArea;

        /// <summary>
        /// Constructor for the BoardTile Class
        /// </summary>
        /// <param name="pos">The position to set the tile</param>
        public BoardTile(Vector2 pos)
        {
            position = pos;
            status = TileStatus.NONE;
            isOccupied = PieceColor.None;
            tileArea = new Rectangle((int)pos.X * TILE_WIDTH, (int)pos.Y * TILE_HEIGHT, TILE_WIDTH, TILE_HEIGHT);
        }
        /// <summary>
        /// Copy constructor for the BoardTile class
        /// This constructor is used for the generation of temporary states
        /// </summary>
        /// <param name="tile">The tile to copy</param>
        public BoardTile(BoardTile tile)
        {
            position = new Vector2(tile.position.X, tile.position.Y);
            status = tile.status;
            isOccupied = tile.isOccupied;
            tileArea = tile.tileArea;
        }

        public TileStatus getStatus()
        {
            return status;
        }
        public void setStatus(TileStatus s)
        {
            status = s;
        }


        /// <summary>
        /// Functions to set/unset the occupancy status of the tile
        /// </summary>
        /// <param name="color">The color that will occupy this tile</param>
        public void occupy(PieceColor color)
        {
            isOccupied = color;
        }
        public void unOccupy()
        {
            isOccupied = PieceColor.None;
        }

        public PieceColor getOccupiedStatus()
        {
            return isOccupied;
        }

        /// <summary>
        /// Function to determine if this tile intersects with the given rectangle
        /// </summary>
        /// <param name="rectangle">The rectangle to test intersection against</param>
        /// <returns>true for intersection, false for no intersection</returns>
        public bool intersects(Rectangle rectangle)
        {
            return tileArea.Intersects(rectangle);
        }

        /// <summary>
        /// This function is called by CheckersGame.cs for when a tile has to be drawn
        /// </summary>
        /// <param name="spriteBatch">The spritebatch to draw to</param>
        /// <param name="texture">The texture to draw</param>
        public void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            if (status != TileStatus.NONE)
            {
                spriteBatch.Draw(texture, position * BoardTile.TILE_WIDTH, Color.White);
            }
        }
    }
}
