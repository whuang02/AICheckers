//CheckersPiece.cs
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
    /// This class represents a checkers piece in a checkers game
    /// </summary>
    class CheckersPiece
    {
        //2D vectors for position of the piece, destination of the piece(during a move), and the origin of the piece
        public Vector2 position, destination, origin;
        //Float to determine the angle of rotation the marking should be at
        public float rotation;
        //Float to denote the rotation speed
        public const float ROTATION_SPEED = 0.005f;
        //Color of the checkers piece
        private PieceColor color;
        //List of possible moves for this piece
        private List<GameMove> possibleMoves;
        //Boolean to mark if this piece was captured (used for piece removal)
        private bool isCaptured;
        //Boolean to keep track of if this piece was jumped over for finding sequential jumps
        private bool justJumpedOver = false;

        /// <summary>
        /// Constructor for a Checkers piece.
        /// </summary>
        /// <param name="p">The position for the piece</param>
        /// <param name="c">The color for the piece</param>
        public CheckersPiece(Vector2 p, PieceColor c)
        {
            position = p;
            color = c;
            isCaptured = false;
            possibleMoves = new List<GameMove>();
            destination = Vector2.Zero;
            //Rotation and origin is used for the animation marking of active pieces
            rotation = 0;
            origin = new Vector2(BoardTile.TILE_WIDTH * 0.5f, BoardTile.TILE_HEIGHT * 0.5f);
        }
        /// <summary>
        /// Copy constructor for a checkers piece.
        /// This constructor is used mainly for creating new temporary states for running the alpha-beta
        /// </summary>
        /// <param name="piece">The piece to copy</param>
        public CheckersPiece(CheckersPiece piece)
        {
            position = new Vector2(piece.position.X, piece.position.Y);
            color = piece.color;
            isCaptured = piece.isCaptured;
            destination = new Vector2(piece.destination.X, piece.destination.Y);
            possibleMoves = new List<GameMove>();
            foreach (GameMove move in piece.possibleMoves)
            {
                possibleMoves.Add(new GameMove(move));
            }
        }

        /// <summary>
        /// Function handles the updating of checkers piece.
        /// This function is called whenever a successful move is done.
        /// This function will update the piece's position on the board, and change who's turn it is.
        /// </summary>
        /// <param name="gameState">The gamestate to update</param>
        public void Update(CheckersGameState gameState)
        {
            if (destination != Vector2.Zero)
            {
                position = destination;
                destination = Vector2.Zero;               

                if (gameState.turnColor == PieceColor.Black)
                {
                    gameState.changePlayerTurn(PieceColor.White);
                }
                else
                {
                    gameState.changePlayerTurn(PieceColor.Black);
                }
            }
        }

        /// <summary>
        /// Function to draw the game piece. This function is called by Draw() in CheckersGame.cs
        /// </summary>
        /// <param name="spriteBatch">The spritebatch to draw to</param>
        /// <param name="texture">The texture to draw</param>
        public void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            spriteBatch.Draw(texture, position * BoardTile.TILE_WIDTH, Color.White);
        }

        /// <summary>
        /// Function draws the marking of the active checkers piece. 
        /// This function is called by Draw() in CheckersGame.cs
        /// </summary>
        /// <param name="spriteBatch">The spritebatch to draw to</param>
        /// <param name="texture">The texture to draw</param>
        /// <param name="rotate">Whether or not to rotate</param>
        public void Draw(SpriteBatch spriteBatch, Texture2D texture, bool rotate = true)
        {
            spriteBatch.Draw(texture, position * BoardTile.TILE_WIDTH + origin, null, Color.White,
                        rotation, origin, 1.0f, SpriteEffects.None, 0.0f);
        }

        public PieceColor getColor()
        {
            return color;
        }

        public List<GameMove> getPossibleMoves()
        {
            return possibleMoves;
        }

        public bool getCaptureStatus()
        {
            return isCaptured;
        }

        /// <summary>
        /// Function to capture a checkerpiece given the piece, and its tile position
        /// </summary>
        /// <param name="piece">The piece to be captured</param>
        /// <param name="tile">The tile the piece is on</param>
        public void Capture(CheckersPiece piece, BoardTile tile)
        {
            tile.unOccupy();
            piece.isCaptured = true;
        }

        /// <summary>
        /// Function that will recursively find all the jump sequences possible by this piece
        /// </summary>
        /// <param name="tilePosition">The position of the tile to check jumps from</param>
        /// <param name="gameBoard">The gameboard</param>
        /// <param name="gameState">The current gamestate</param>
        /// <returns>A list of possible jumps</returns>
        public List<GameMove> getJumps(Vector2 tilePosition, CheckersBoard gameBoard, CheckersGameState gameState)
        {
            List<GameMove> jumps = new List<GameMove>();
            //Get all the possible tiles that could result with a jump
            BoardTile topLeftTile = gameBoard.getTileAt((int)tilePosition.X - 1, (int)tilePosition.Y - 1);
            BoardTile topRightTile = gameBoard.getTileAt((int)tilePosition.X + 1, (int)tilePosition.Y - 1);
            BoardTile bottomLeftTile = gameBoard.getTileAt((int)tilePosition.X - 1, (int)tilePosition.Y + 1);
            BoardTile bottomRightTile = gameBoard.getTileAt((int)tilePosition.X + 1, (int)tilePosition.Y + 1);
            BoardTile topLeftJumpTile = gameBoard.getTileAt((int)tilePosition.X - 2, (int)tilePosition.Y - 2);
            BoardTile topRightJumpTile = gameBoard.getTileAt((int)tilePosition.X + 2, (int)tilePosition.Y - 2);
            BoardTile bottomLeftJumpTile = gameBoard.getTileAt((int)tilePosition.X - 2, (int)tilePosition.Y + 2);
            BoardTile bottomRightJumpTile = gameBoard.getTileAt((int)tilePosition.X + 2, (int)tilePosition.Y + 2);
            
            //Set the enemy color
            PieceColor enemyColor;
            if (this.color == PieceColor.Black)
                enemyColor = PieceColor.White;
            else
                enemyColor = PieceColor.Black;

            //The below four if-elses will basically determine if there was a piece of the enemy color was jumped over 
            //by moving to a tile 2 diagonal tiles away. If there was a jump, it will mark the piece that it just jumped over
            //and then recursively look for any other following jumps from that tile. 
            if (topLeftTile != null && topLeftTile.getOccupiedStatus() == enemyColor)
            {
                //Get the piece that was jumped over
                CheckersPiece adjacentPiece = gameState.getPiece(enemyColor, topLeftTile.position);
                if (topLeftJumpTile != null && topLeftJumpTile.getOccupiedStatus() == PieceColor.None && adjacentPiece.justJumpedOver == false)
                {
                    //mark the piece as jumped over
                    adjacentPiece.justJumpedOver = true;
                    //Find any jumps from the new tile position
                    List<GameMove> sequenceJumps = getJumps(topLeftJumpTile.position, gameBoard, gameState);

                    //If there was no following jump then add the jump to the list of jump
                    if (sequenceJumps.Count == 0)
                    {
                        GameMove newJump = (new GameMove(this, topLeftJumpTile.position));
                        newJump.capturedPieces.Add(adjacentPiece);
                        jumps.Add(newJump);
                    }
                    //Else for each following jump, add the piece the first jump captured to the list of captured pieces
                    else
                    {
                        foreach (GameMove jump in sequenceJumps)
                        {
                            //GameMove newJump = (new GameMove(this, jump.destinationPosition));
                            //jump.capturedPieces = jump.capturedPieces;
                            jump.capturedPieces.Add(adjacentPiece);
                            jumps.Add(jump);
                        }
                    }
                }
            }
            if (topRightTile != null && topRightTile.getOccupiedStatus() == enemyColor)
            {
                CheckersPiece adjacentPiece = gameState.getPiece(enemyColor, topRightTile.position);
                if (topRightJumpTile != null && topRightJumpTile.getOccupiedStatus() == PieceColor.None && adjacentPiece.justJumpedOver == false)
                {
                    //get any jumps from that tile
                    adjacentPiece.justJumpedOver = true;
                    List<GameMove> sequenceJumps = getJumps(topRightJumpTile.position, gameBoard, gameState);

                    if (sequenceJumps.Count == 0)
                    {
                        GameMove newJump = (new GameMove(this, topRightJumpTile.position));
                        newJump.capturedPieces.Add(adjacentPiece);
                        jumps.Add(newJump);
                    }
                    else
                    {
                        foreach (GameMove jump in sequenceJumps)
                        {
                            GameMove newJump = (new GameMove(this, jump.destinationPosition));
                            newJump.capturedPieces = jump.capturedPieces;
                            newJump.capturedPieces.Add(adjacentPiece);
                            jumps.Add(newJump);
                        }
                    }
                }
            }
            if (bottomLeftTile != null && bottomLeftTile.getOccupiedStatus() == enemyColor)
            {
                CheckersPiece adjacentPiece = gameState.getPiece(enemyColor, bottomLeftTile.position);
                if (bottomLeftJumpTile != null && bottomLeftJumpTile.getOccupiedStatus() == PieceColor.None && adjacentPiece.justJumpedOver == false)
                {
                    //get any jumps from that tile
                    adjacentPiece.justJumpedOver = true;
                    List<GameMove> sequenceJumps = getJumps(bottomLeftJumpTile.position, gameBoard, gameState);

                    if (sequenceJumps.Count == 0)
                    {
                        GameMove newJump = (new GameMove(this, bottomLeftJumpTile.position));
                        newJump.capturedPieces.Add(adjacentPiece);
                        jumps.Add(newJump);
                    }
                    else
                    {
                        foreach (GameMove jump in sequenceJumps)
                        {
                            GameMove newJump = (new GameMove(this, jump.destinationPosition));
                            newJump.capturedPieces = jump.capturedPieces;
                            newJump.capturedPieces.Add(adjacentPiece);
                            jumps.Add(newJump);
                        }
                    }
                }
            }
            if (bottomRightTile != null && bottomRightTile.getOccupiedStatus() == enemyColor)
            {
                CheckersPiece adjacentPiece = gameState.getPiece(enemyColor, bottomRightTile.position);
                if (bottomRightJumpTile != null && bottomRightJumpTile.getOccupiedStatus() == PieceColor.None && adjacentPiece.justJumpedOver == false)
                {
                    //get any jumps from that tile
                    adjacentPiece.justJumpedOver = true;
                    List<GameMove> sequenceJumps = getJumps(bottomRightJumpTile.position, gameBoard, gameState);

                    if (sequenceJumps.Count == 0)
                    {
                        GameMove newJump = (new GameMove(this, bottomRightJumpTile.position));
                        newJump.capturedPieces.Add(adjacentPiece);
                        jumps.Add(newJump);
                    }
                    else
                    {
                        foreach (GameMove jump in sequenceJumps)
                        {
                            GameMove newJump = (new GameMove(this, jump.destinationPosition));
                            newJump.capturedPieces = jump.capturedPieces;
                            newJump.capturedPieces.Add(adjacentPiece);
                            jumps.Add(newJump);
                        }
                    }
                }
            }
            
            return jumps;
        }

        /// <summary>
        /// Function that will find all regular moves for this piece
        /// A regular move is a non-jump move
        /// </summary>
        /// <param name="tilePosition">The position of the tile to check jumps from</param>
        /// <param name="gameBoard">The gameboard</param>
        /// <param name="gameState">The current gamestate</param>
        /// <returns>A list of possible regular moves</returns>
        public List<GameMove> getRegularMoves(Vector2 tilePosition, CheckersBoard gameBoard, CheckersGameState gameState)
        {
            List<GameMove> regularMoves = new List<GameMove>();                      

            //Below is the logic for if the checkers piece is black
            if (color == PieceColor.Black)
            {
                //Get the two possible tiles a black piece could move to
                BoardTile topLeftTile = gameBoard.getTileAt((int)tilePosition.X - 1, (int)tilePosition.Y - 1);
                BoardTile topRightTile = gameBoard.getTileAt((int)tilePosition.X + 1, (int)tilePosition.Y - 1);

                //If the tile exists on the board, add that tile as a possible move
                if (topLeftTile != null && topLeftTile.getOccupiedStatus() == PieceColor.None)
                {
                    regularMoves.Add(new GameMove(this, topLeftTile.position));
                }
                if (topRightTile != null && topRightTile.getOccupiedStatus() == PieceColor.None)
                {
                    regularMoves.Add(new GameMove(this, topRightTile.position));
                }
            }

            //Below is the logic for if the checkers piece is white
            if (color == PieceColor.White)
            {
                //Get the two possible tiles a black piece could move to
                BoardTile bottomLeftTile = gameBoard.getTileAt((int)tilePosition.X - 1, (int)tilePosition.Y + 1);
                BoardTile bottomRightTile = gameBoard.getTileAt((int)tilePosition.X + 1, (int)tilePosition.Y + 1);

                //If the tile exists on the board, add that tile as a possible move
                if (bottomLeftTile != null && bottomLeftTile.getOccupiedStatus() == PieceColor.None)
                {
                    regularMoves.Add(new GameMove(this, bottomLeftTile.position));
                }
                if (bottomRightTile != null && bottomRightTile.getOccupiedStatus() == PieceColor.None)
                {
                    regularMoves.Add(new GameMove(this, bottomRightTile.position));
                }
            }
            return regularMoves;
        }

        /// <summary>
        /// Function that will determine all possible moves for the piece
        /// </summary>
        /// <param name="gameBoard">The gameboard</param>
        /// <param name="gameState">The current state of the game</param>
        /// <param name="noShow">Whether or not to mark the board</param>
        /// <returns>Type of move the piece has (NONE, MOVE, JUMP)</returns>
        public TileStatus determineMoves(CheckersBoard gameBoard, CheckersGameState gameState, bool noShow = false)
        {
            TileStatus foundMove = TileStatus.NONE;
            possibleMoves.Clear();

            Vector2 tilePosition = new Vector2(position.X, position.Y);

            List<GameMove> jumps = getJumps(tilePosition, gameBoard, gameState);
            //Look for any jumps by the piece
            if (jumps.Count != 0)
            {
                foundMove = TileStatus.JUMP;
                possibleMoves = jumps;
                gameState.jumpExists = true;
                //Reset that the piece was just jumped over
                //Necessary for multi-jumps
                if (color == PieceColor.Black)
                {
                    foreach (CheckersPiece piece in gameState.whiteGamePieces)
                    {
                        piece.justJumpedOver = false;
                    }
                }
                else if (color == PieceColor.White)
                {
                    foreach (CheckersPiece piece in gameState.blackGamePieces)
                    {
                        piece.justJumpedOver = false;
                    }
                }
            }
            //If no jump was found in the context of the turn find possible regular moves for the piece
            else if (gameState.jumpExists == false)
            {
                List<GameMove> regularMoves = getRegularMoves(tilePosition, gameBoard, gameState);
                if (regularMoves.Count != 0)
                {
                    foundMove = TileStatus.MOVE;
                    possibleMoves = regularMoves;
                }
            }
            if (noShow == false)
            {
                gameBoard.clearMarkings();
                if (foundMove != TileStatus.NONE)
                {
                    foreach (GameMove move in possibleMoves)
                    {
                        gameBoard.getTileAt(move.destinationPosition).setStatus(foundMove);
                    }
                }
            }
            return foundMove;
        }

        /// <summary>
        /// This method determines whether or not a rectangle at point (x,y) intersects this checkers piece
        /// </summary>
        /// <param name="rectangle">A rectangle to check intersection against</param>
        /// <returns>True for intersection, false for no intersection</returns>
        public bool intersects(Rectangle rectangle)
        {
            Rectangle squareRect = new Rectangle((int)position.X * BoardTile.TILE_WIDTH, 
                                                 (int)position.Y * BoardTile.TILE_HEIGHT, 
                                                 BoardTile.TILE_WIDTH, BoardTile.TILE_HEIGHT);
            return rectangle.Intersects(squareRect);
        }

        /// <summary>
        /// This method will move this (CheckersPiece) to the tile that as passed in
        /// </summary>
        /// <param name="tile">The tile to move the piece to</param>
        public void moveTo(BoardTile tile)
        {
            destination = tile.position;
            tile.occupy(color);
        }
    }
}
