//CheckersGameState.cs
//Written by Wei Wei Huang

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace AICheckers
{
    /// <summary>
    /// This class contains all of the information of a state of a checkers game.
    /// This class keeps track of:
    /// state of the board, the state of each tile, the state of all game pieces,
    /// the color of the player and ai, whose turn it is, whose the winner, the active piece for the game,
    /// and finally the number of turns that has occurred in the game.
    /// </summary>
    class CheckersGameState
    {
        //Int that states the maximum pieces both players can have
        private const int MAX_PIECES = 6;
        //The checker board for the game
        public CheckersBoard gameBoard;
        //The a list of black game piece, white game pieces, and moveable pieces for the current turn
        public List<CheckersPiece> blackGamePieces, whiteGamePieces, moveablePieces;
        //The color for the current turn, the winner, the player, and the ai
        public PieceColor turnColor, winner, playerColor, aiColor;
        //The active checkers piece at the moment
        public CheckersPiece activeGamePiece;
        //Boolean field to signify if the player has to jump this move
        public bool jumpExists;
        //Integer to keep track of turns that has passed - used in evaluation
        public int numTurnsPassed;

        /// <summary>
        /// Constructor for a state.
        /// A new game state is initialized by passing in the color the player is playing as.
        /// </summary>
        /// <param name="playerClr">The color for the player</param>
        public CheckersGameState(PieceColor playerClr)
        {
            gameBoard = new CheckersBoard();
            whiteGamePieces = new List<CheckersPiece>();
            blackGamePieces = new List<CheckersPiece>();
            turnColor = PieceColor.None;
            winner = PieceColor.None;
            jumpExists = false;

            //If a player color is passed in, the blow will initialize all the white and black pieces.
            if (playerClr != PieceColor.None)
            {
                Vector2 blackPosition;
                Vector2 whitePosition;
                for (int index = 0; index < MAX_PIECES; index++)
                {
                    //calculate the position for the black piece and the white piece
                    if (index < 3)
                    {
                        blackPosition = new Vector2(index * 2f, 5 - (index / 3));
                        whitePosition = new Vector2(index * 2f + 1, index / 3);
                    }
                    else
                    {
                        blackPosition = new Vector2((index * 2f + 1) - 6, 5 - (index / 3));
                        whitePosition = new Vector2((index * 2f) - 6, index / 3);
                    }

                    //Create the pieces with the calculated positions and add them the the list
                    CheckersPiece blackPiece = new CheckersPiece(new Vector2(blackPosition.X, blackPosition.Y), PieceColor.Black);
                    CheckersPiece whitePiece = new CheckersPiece(new Vector2(whitePosition.X, whitePosition.Y), PieceColor.White);
                    blackGamePieces.Add(blackPiece);
                    whiteGamePieces.Add(whitePiece);

                    //Set the tiles to be occupied
                    gameBoard.getTileAt(blackPosition).occupy(PieceColor.Black);
                    gameBoard.getTileAt(whitePosition).occupy(PieceColor.White);
                }
                //Set the player and ai colors
                if (playerClr == PieceColor.Black)
                {
                    playerColor = PieceColor.Black;
                    aiColor = PieceColor.White;
                }
                else
                {
                    playerColor = PieceColor.White;
                    aiColor = PieceColor.Black;
                }
            }
        }

        /// <summary>
        /// Copy constructor to make a new copy of this current state
        /// This constructor is used to create temporary states in the ai.
        /// </summary>
        /// <param name="gameState">The state to be copied</param>
        public CheckersGameState(CheckersGameState gameState)
        {
            whiteGamePieces = new List<CheckersPiece>();
            blackGamePieces = new List<CheckersPiece>();
            moveablePieces = new List<CheckersPiece>();
            gameBoard = new CheckersBoard(gameState.gameBoard);
            foreach (CheckersPiece piece in gameState.blackGamePieces)
            {
                CheckersPiece newPiece = new CheckersPiece(piece);
                //find the active game piece
                if (gameState.activeGamePiece != null && newPiece.position == gameState.activeGamePiece.position)
                    activeGamePiece = newPiece;
                blackGamePieces.Add(newPiece);
            }
            foreach (CheckersPiece piece in gameState.whiteGamePieces)
            {
                CheckersPiece newPiece = new CheckersPiece(piece);
                //find the active game piece
                if (gameState.activeGamePiece != null &&  newPiece.position == gameState.activeGamePiece.position)
                    activeGamePiece = newPiece;
                whiteGamePieces.Add(newPiece);
            }
            foreach (CheckersPiece piece in gameState.moveablePieces)
            {
                CheckersPiece newPiece = new CheckersPiece(piece);
                moveablePieces.Add(newPiece);
            }
            playerColor = gameState.playerColor;
            aiColor = gameState.aiColor;
            turnColor = gameState.turnColor;
            jumpExists = gameState.jumpExists;
            winner = gameState.winner;
            numTurnsPassed = gameState.numTurnsPassed;
        }        

        /// <summary>
        /// This function will return a checkers piece given its color and position
        /// The color is used to speed up the search
        /// </summary>
        /// <param name="color">The color of the piece</param>
        /// <param name="position">The position of the piece</param>
        /// <returns>A checker piece</returns>
        public CheckersPiece getPiece(PieceColor color, Vector2 position)
        {
            if (color == PieceColor.White)
            {
                foreach (CheckersPiece piece in whiteGamePieces)
                {
                    if (piece.position == position)
                        return piece;
                }
            }
            else if (color == PieceColor.Black)
            {
                foreach (CheckersPiece piece in blackGamePieces)
                {
                    if (piece.position == position)
                        return piece;
                }
            }
            return null;
        }
        
        /// <summary>
        /// This function is used to remove all captured pieces
        /// </summary>
        public void removeCapturedPieces()
        {
            //If it is white's turn, remove all captured black pieces            
            if (turnColor == PieceColor.White)
            {
                for (int index = 0; index < blackGamePieces.Count(); index++)
                {
                    if (blackGamePieces[index].getCaptureStatus() == true)
                    {
                        gameBoard.getTileAt(blackGamePieces[index].position).unOccupy();
                        blackGamePieces.RemoveAt(index);
                        index--;
                    }
                }
            }
            //else remove all captured white pieces
            else
            {
                for (int index = 0; index < whiteGamePieces.Count(); index++)
                {
                    if (whiteGamePieces[index].getCaptureStatus() == true)
                    {
                        gameBoard.getTileAt(whiteGamePieces[index].position).unOccupy();
                        whiteGamePieces.RemoveAt(index);
                        index--;
                    }
                }
            }
        }

        /// <summary>
        /// This method gets all of the moveable checkers pieces for the turn.
        /// This method is called everytime the a turn passes by changePlayerTurn().
        /// This method also takes in the list of pieces that could possibly move.
        /// </summary>
        /// <param name="pieces">A list of pieces that could possibly move(i.e black pieces or white pieces)</param>
        /// <returns>A list of moveable checker pieces for the turn</returns>
        private List<CheckersPiece> getMoveablePieces(List<CheckersPiece> pieces)
        {
            TileStatus move = TileStatus.NONE;
            List<CheckersPiece> moveablePieces = new List<CheckersPiece>();
            bool canOnlyJump = false;

            //Initially, this loop will find all the pieces that has a valid move
            //and add it to the list of moveable pieces. However, if a jump move was encountered,
            //then all previous pieces will be removed(since they are not jump moves), and only pieces 
            //with a jump move will be added to the list of moveable pieces.
            foreach (CheckersPiece piece in pieces)
            {
                move = piece.determineMoves(gameBoard, this, true);
                if (canOnlyJump == true)
                {
                    if (move == TileStatus.JUMP)
                    {
                        moveablePieces.Add(piece);
                    }
                }
                else
                {
                    if (move == TileStatus.JUMP)
                    {
                        moveablePieces.Clear();
                        moveablePieces.Add(piece);
                        canOnlyJump = true;
                    }
                    else if (move == TileStatus.MOVE)
                    {
                        moveablePieces.Add(piece);
                    }
                }
            }
            return moveablePieces;
        }

        /// <summary>
        /// This function is used to determine if the game is over, and sets the winner.
        /// This function will be called everytime turnColor changes.
        /// </summary>
        /// <returns>The color of the winner</returns>
        public PieceColor getWinner()
        {
            //Below will only run the logic if a player has yet to be found
            if (winner == PieceColor.None)
            {
                //The next 2 if/else will check if a player has won by capturing all the pieces
                if (blackGamePieces.Count() == 0)
                {
                    winner = PieceColor.White;
                }
                else if (whiteGamePieces.Count() == 0)
                {
                    winner = PieceColor.Black;
                }
                //This statement will determine if a player because the other player has no more moves
                else if (moveablePieces.Count == 0)
                {
                    if (turnColor == PieceColor.White)
                    {
                        winner = PieceColor.Black;
                    }
                    else if (turnColor == PieceColor.Black)
                    {
                        winner = PieceColor.White;
                    }
                }
            }
            return winner;
        }

        /// <summary>
        /// This function is used to change whose turn it is in the game.
        /// This function is called by the update of a checkers piece.
        ///     -- After a checkers piece has finished moving, it call this.
        /// </summary>
        /// <param name="color">The color of the next turn</param>
        public void changePlayerTurn(PieceColor color)
        {
            //Turn changed, increment the num of turns that has passed
            numTurnsPassed++;
            turnColor = color;
            jumpExists = false; //reset jumpExist to false, so next player is limited to jump moves
            //Get the moveable pieces for the next player
            if (color == PieceColor.Black)
            {
                moveablePieces = getMoveablePieces(blackGamePieces);
            }
            else if (color == PieceColor.White)
            {
                moveablePieces = getMoveablePieces(whiteGamePieces);
            }
            //Check if the game is over
            getWinner();
        }

        /// <summary>
        /// This function is for handling any checker piece actions.
        /// Function take a checkers piece as parameter, and set it as the active game piece.
        /// Function will also make the piece determine all its possible moves.
        /// </summary>
        /// <param name="activePiece">A piece to do an action on</param>
        public void doGamePieceAction(CheckersPiece activePiece)
        {
            if (activePiece.getCaptureStatus() == false && activePiece.getColor() == turnColor)
            {
                activeGamePiece = activePiece;
                activePiece.determineMoves(gameBoard, this);
            }
        }

        /// <summary>
        /// This function is for handling any tile actions.
        /// Function takes a tile as a parameter. If the tile is marked as a possible move,
        /// then the function will apply the logic necessary for the move.
        /// </summary>
        /// <param name="tile">A tile to do an action on</param>
        /// <returns>Whether or not the tile was valid for move</returns>
        public bool doTileAction(BoardTile tile)
        {
            bool actionDone = false;
            List<GameMove> availableMoves = new List<GameMove>();
            //Apply logic only if the tile is marked for a possible action (i.e. MOVE, JUMP)
            if (tile.getStatus() != TileStatus.NONE)
            {
                //Get the moves for the active piece
                availableMoves = activeGamePiece.getPossibleMoves();
                //The below loop finds the move that the tile correlates to
                foreach (GameMove move in availableMoves)
                {
                    //Below is the logic for when the move is found
                    if (tile.position == move.destinationPosition)
                    {
                        //If the tile is marked for jump, also capture the pieces
                        //  specified in the move and remove them
                        if (tile.getStatus() == TileStatus.JUMP)
                        {
                            //Capture the pieces specified in the move
                            foreach (CheckersPiece capturedPiece in move.capturedPieces)
                            {
                                var piece = getPiece(capturedPiece.getColor(), capturedPiece.position);
                                activeGamePiece.Capture(piece, gameBoard.getTileAt(capturedPiece.position));
                            }
                            //Remove captured pieces from the game
                            removeCapturedPieces();
                        }

                        //Move the active piece to the destination tile
                        activeGamePiece.moveTo(tile);
                        //Unoccupy the tile the active piece came from
                        gameBoard.getTileAt((int)(activeGamePiece.position.X), (int)(activeGamePiece.position.Y)).unOccupy();
                        //Remove all marking of tiles
                        gameBoard.clearMarkings();                        
                        actionDone = true;
                        break;
                    }
                }
            }
            return actionDone;
        }
    }
}
