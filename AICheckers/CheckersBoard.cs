//CheckersBoard.cs
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
    /// Class to represent a checker board for a checkers game
    /// </summary>
    class CheckersBoard
    {
        //int value that states the max number of horizontal tiles
        public static int MAX_HORIZONTAL_TILES = 6;
        //int value that states the max number of vertical tiles
        public static int MAX_VERTICAL_TILES = 6;
        //2D vector to that holds the position of the board for it to be drawn on the UI
        private Vector2 position;
        //A 2D array of all the tiles on the board
        private BoardTile[,] tiles;

        /// <summary>
        /// Default constructor for the checkers board
        /// </summary>
        public CheckersBoard()
        {
            position = Vector2.Zero;
            tiles = new BoardTile[6,6];

            for (int x = 0; x < MAX_HORIZONTAL_TILES; x++)
            {
                for (int y = 0; y < MAX_VERTICAL_TILES; y++)
                {
                    Vector2 tilePosition = new Vector2(x, y);
                    tiles[x,y] = new BoardTile(tilePosition);
                }
            }
        }

        /// <summary>
        /// Copy constructor for the checkers board.
        /// This constructor is used for creating temporary states used in the AI.
        /// </summary>
        /// <param name="board">A board to copy</param>
        public CheckersBoard(CheckersBoard board)
        {
            tiles = new BoardTile[6, 6];
            for (int x = 0; x < MAX_HORIZONTAL_TILES; x++)
            {
                for (int y = 0; y < MAX_VERTICAL_TILES; y++)
                {
                    BoardTile copyTile = board.tiles[x, y];
                    tiles[x, y] = new BoardTile(copyTile);
                }
            }
        }

        public BoardTile[,] getTiles(){
            return tiles;
        }
        
        /// <summary>
        /// Function to get a tile on the board given a 2D vector(x,y)
        /// </summary>
        /// <param name="vector">A 2D vector containing the x and y position of the tile</param>
        /// <returns>The tile at the x,y position of the vector or null if not found</returns>
        public BoardTile getTileAt(Vector2 vector)
        {
            try
            {
                return tiles[(int)vector.X, (int)vector.Y];
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Function to get a tile on the board given an x and y coordinate
        /// </summary>
        /// <param name="x">The x position of the tile to get</param>
        /// <param name="y">The y position of the tile to get</param>
        /// <returns>The tile at the x,y position or null if not found</returns>
        public BoardTile getTileAt(int x, int y)
        {
            try
            {
                return tiles[x, y];
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// This function clears any markings that any tiles may currently have
        /// </summary>
        public void clearMarkings()
        {
            for (int x = 0; x < MAX_HORIZONTAL_TILES; x++)
            {
                for (int y = 0; y < MAX_VERTICAL_TILES; y++)
                {
                    tiles[x, y].setStatus(TileStatus.NONE);
                }
            }
        }

        /// <summary>
        /// This function is called by CheckersGame.cs for when a board needs to be drawn.
        /// </summary>
        /// <param name="spriteBatch">The spritebatch to draw to</param>
        /// <param name="texture">The texture to draw</param>
        public void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            spriteBatch.Draw(texture, position * BoardTile.TILE_HEIGHT, Color.White);            
        }
    }
}
