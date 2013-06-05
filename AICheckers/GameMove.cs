//CheckersMove.cs
//Written by Wei Wei Huang

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace AICheckers
{
    /// <summary>
    /// Struct used to represent a game move
    /// </summary>
    struct GameMove
    {
        //The checkers piece that is moving
        public CheckersPiece movePiece;
        //Enemy checker pieces that were captured in with this move
        public List<CheckersPiece> capturedPieces;
        //Destination x,y coordinate stored in a 2D vector for this move
        public Vector2 destinationPosition;
        //Original x,y coordinate of the moving piece stored in a 2D vector
        public Vector2 originalPosition;

        /// <summary>
        /// Constructor for a Game Move
        /// </summary>
        /// <param name="movePiece">The moving piece</param>
        /// <param name="destination">The destination position of the moving piece</param>
        public GameMove(CheckersPiece movePiece, Vector2 destination)
        {
            this.movePiece = movePiece;
            capturedPieces = new List<CheckersPiece>();
            destinationPosition = new Vector2(destination.X, destination.Y);
            originalPosition = new Vector2(movePiece.position.X, movePiece.position.Y);
        }

        /// <summary>
        /// Copy constructor for a game move.
        /// This constructor is used for creating temporary game states.
        /// </summary>
        /// <param name="move">The move to copy</param>
        public GameMove(GameMove move)
        {
            movePiece = move.movePiece;
            capturedPieces = new List<CheckersPiece>();
            foreach (CheckersPiece piece in move.capturedPieces)
            {
                capturedPieces.Add(new CheckersPiece(piece));
            }
            destinationPosition = new Vector2(move.destinationPosition.X, move.destinationPosition.Y);
            originalPosition = new Vector2(move.originalPosition.X, move.originalPosition.Y);
        }
    }
}
