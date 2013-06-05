//CheckersGame.cs
//Written by Wei Wei Huang

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Main driver for a checkers game. This class handles the creation of new checker games,
    /// any game inputs for the checkers game, drawing of the checkers game, and the player-ai logic.
    /// </summary>
    class CheckersGame
    {
        //max value to be used to find beta
        private const int MAX_INT = int.MaxValue;
        //min value to be used to find alpha
        private const int MIN_INT = int.MinValue;
        //cuttoff set for the alpha-beta algorithm
        private const int cutoff = 12, aiTimeDelay = 1;
        private int iterativeDepth;
        //textures used in the game
        private Texture2D whitePieceTexture, blackPieceTexture, activePieceTexture,
                            activePieceTexture2, checkerGameBoardTexture, moveTileTexture,
                            jumpTileTexture, whiteButtonTexture, blackButtonTexture;
        //the current state of the game
        private CheckersGameState currentState;
        //fonts used in the game
        private SpriteFont font, textFont;
        //boolean for when a mouse event occurs
        private bool isMousePressed, didAiMove;
        //GameMove to keeptrack of the best move in alpha-beta 
        private GameMove bestMove;
        //Datetime to keep track of when alpha-beta starts
        private DateTime startTime, endTime, aiMoveStart;
        //Rectangle areas for the two buttons
        private Rectangle blackButton, whiteButton;
        //int values to keep track of the depth, nodes, maxpruned, and minpruned
        private int maxDepth, nodes, maxPruned, minPruned;
        //Thread used to run ai in the background
        Thread aiThread;

        /// <summary>
        /// Function to load all the content necessary for the game
        /// In this case, the textures and fonts
        /// </summary>
        /// <param name="content"></param>
        public void Load(ContentManager content)
        {
            font = content.Load<SpriteFont>(@"Fonts\gameFont");
            textFont = content.Load<SpriteFont>(@"Fonts\TextFont");
            whitePieceTexture = content.Load<Texture2D>(@"Sprites\WhitePiece");
            blackPieceTexture = content.Load<Texture2D>(@"Sprites\BlackPiece");
            activePieceTexture = content.Load<Texture2D>(@"Sprites\ActivePiece");
            activePieceTexture2 = content.Load<Texture2D>(@"Sprites\ActivePiece2");
            checkerGameBoardTexture = content.Load<Texture2D>(@"Sprites\CheckerBoard");
            moveTileTexture = content.Load<Texture2D>(@"Sprites\HighlightMoveTile");
            jumpTileTexture = content.Load<Texture2D>(@"Sprites\HighlightJumpTile");
            whiteButtonTexture = content.Load<Texture2D>(@"Sprites\WhiteButton");
            blackButtonTexture = content.Load<Texture2D>(@"Sprites\BlackButton");
        }

        /// <summary>
        /// Function called to initialize a new checkers game
        /// </summary>
        public void initComponent()
        {
            isMousePressed = false;
            didAiMove = false;
            currentState = new CheckersGameState(PieceColor.None);
            blackButton = new Rectangle(25, 640, 150, 40);
            whiteButton = new Rectangle(200, 640, 150, 40);
        }

        /// <summary>
        /// Update function that is called periodically by the game engine
        /// </summary>
        /// <param name="main">Main driver for the game</param>
        /// <param name="gameTime">Gametime recieved by the main</param>
        public void Update(CheckersMain main, GameTime gameTime)
        {
            //Below handles the animated marking of the active game piece
            if (currentState.activeGamePiece != null)
            {
                if (currentState.activeGamePiece.rotation - CheckersPiece.ROTATION_SPEED * MathHelper.TwoPi < 0)
                    currentState.activeGamePiece.rotation -= CheckersPiece.ROTATION_SPEED * MathHelper.TwoPi + MathHelper.TwoPi;
                else
                    currentState.activeGamePiece.rotation -= CheckersPiece.ROTATION_SPEED * MathHelper.TwoPi;
            }
            //Below sets the first player in the game to black when a game is started
            if (currentState.turnColor == PieceColor.None && currentState.blackGamePieces.Count > 0)
            {
                currentState.changePlayerTurn(PieceColor.Black);
            }
            //Below takes care of running alpha-beta when it is the computer's turn,
            //and doing the best move selected by the algorithm
            if (currentState.turnColor == currentState.aiColor && currentState.winner == PieceColor.None)
            {
                if (aiThread == null)
                {
                    aiThread = new Thread(doAiMove);
                    aiThread.Start();
                }
            }
            //If this case is reached, it means that it was the player's turn, and any mouse events should be processed
            else
            {
                int mouseX = Mouse.GetState().X;
                int mouseY = Mouse.GetState().Y;
                //Handle mouse events for the game only if it's not minimized
                if (Mouse.GetState().LeftButton == ButtonState.Pressed && isMousePressed == false && main.IsActive)
                {
                    isMousePressed = true;
                    handleMouseAction(mouseX, mouseY);
                }
                //Handle mouse events for the game only if it's not minimized
                if (Mouse.GetState().LeftButton == ButtonState.Released && main.IsActive)
                {
                    isMousePressed = false;
                }
            }
            //If the computer finishes (thread is sleeping) join the two threads and set the thread back to null
            if (aiThread != null)
            {
                if(aiThread.IsAlive == false)
                {
                    aiThread.Join();
                    aiThread = null;
                }
            }
        }

        /// <summary>
        /// Function that handles all drawings in the game
        /// This function is called everytime after an update
        /// </summary>
        /// <param name="spriteBatch">Spritebatch initialized by main</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            //Draw the board
            currentState.gameBoard.Draw(spriteBatch, checkerGameBoardTexture);

            //Draw all white pieces
            foreach (CheckersPiece piece in currentState.whiteGamePieces)
            {
                if (piece.getCaptureStatus() == false)
                    piece.Draw(spriteBatch, whitePieceTexture);
            }
            //Draw all black pieces
            foreach (CheckersPiece piece in currentState.blackGamePieces)
            {
                if (piece.getCaptureStatus() == false)
                    piece.Draw(spriteBatch, blackPieceTexture);
            }
            //Draw the active game piece marking
            CheckersPiece active = currentState.activeGamePiece;
            if (active != null)
            {
                if (active.getColor() == PieceColor.White)
                    currentState.activeGamePiece.Draw(spriteBatch, activePieceTexture2, true);
                else
                    currentState.activeGamePiece.Draw(spriteBatch, activePieceTexture, true);
            }
            //Draw any marked tiles
            BoardTile[,] tiles = currentState.gameBoard.getTiles();
            for (int x = 0; x < CheckersBoard.MAX_HORIZONTAL_TILES; x++)
            {
                for (int y = 0; y < CheckersBoard.MAX_VERTICAL_TILES; y++)
                {
                    if (tiles[x, y].getStatus() == TileStatus.MOVE)
                    {
                        tiles[x, y].Draw(spriteBatch, moveTileTexture);
                    }
                    else if (tiles[x, y].getStatus() == TileStatus.JUMP)
                    {
                        tiles[x, y].Draw(spriteBatch, jumpTileTexture);
                    }
                }
            }
            //Draw the text and buttons for the UI
            spriteBatch.DrawString(textFont, "Select a color to start a new game", new Vector2(10, 610), Color.Bisque);
            spriteBatch.Draw(blackButtonTexture, blackButton, Color.White);
            spriteBatch.Draw(whiteButtonTexture, whiteButton, Color.White);
            //Below displays the player's turn, or the winner if there exists
            if (currentState.winner == PieceColor.None && currentState.turnColor != PieceColor.None)
            {
                String message = currentState.turnColor + " turn";
                spriteBatch.DrawString(font, message, new Vector2(400, 630), Color.Bisque);
            }
            else
            {
                if (currentState.winner == PieceColor.Black)
                {
                    String message = "Black Wins!";
                    spriteBatch.DrawString(font, message, new Vector2(385, 630), Color.Red);
                }
                if (currentState.winner == PieceColor.White)
                {
                    String message = "White Wins!";
                    spriteBatch.DrawString(font, message, new Vector2(385, 630), Color.Blue);
                }
            }
            spriteBatch.End();
        }

        /// <summary>
        /// Function that handles a mouse click action
        /// </summary>
        /// <param name="mouseX">X position of the mouse</param>
        /// <param name="mouseY">Y position of the mouse</param>
        private void handleMouseAction(int mouseX, int mouseY)
        {
            //Make a rectangle for the mouse click size of 1 pixel
            Rectangle mouseClick = new Rectangle(mouseX, mouseY, 1, 1);
            //Below handles the event of when the black button is pressed
            if (mouseClick.Intersects(blackButton))
            {
                newGame(PieceColor.Black);
            }
            //Below handles the event of when the white button is pressed
            else if (mouseClick.Intersects(whiteButton))
            {
                newGame(PieceColor.White);
            }
            //Below handles any clicking of checkers pieces or board tiles
            else if (currentState.winner == PieceColor.None && currentState.turnColor != PieceColor.None)
            {
                bool clickablePlace = false;
                foreach (CheckersPiece gamePiece in currentState.moveablePieces)
                {
                    if (gamePiece.intersects(mouseClick) == true)
                    {
                        clickablePlace = true;
                        currentState.doGamePieceAction(gamePiece);
                        break;
                    }
                }
                BoardTile[,] tiles = currentState.gameBoard.getTiles();
                for (int x = 0; x < CheckersBoard.MAX_HORIZONTAL_TILES; x++)
                {
                    for (int y = 0; y < CheckersBoard.MAX_VERTICAL_TILES; y++)
                    {
                        if (tiles[x, y].intersects(mouseClick) == true)
                        {
                            clickablePlace = true;
                            if (currentState.doTileAction(tiles[x, y]) == true)
                            {
                                currentState.activeGamePiece.Update(currentState);
                                break;
                            }
                        }
                    }
                    if (clickablePlace == true)
                    {
                        break;
                    }
                }
                if (clickablePlace == false)
                {
                    currentState.gameBoard.clearMarkings();
                }
            }
        }

        /// <summary>
        /// Function that starts a new game for the player given the player color
        /// </summary>
        /// <param name="playerColor">Color of the player</param>
        public void newGame(PieceColor playerColor)
        {
            Console.WriteLine("========================================================"
                              + "\n                         NEW GAME                       "
                              + "\n========================================================");
            iterativeDepth = 0;
            currentState = new CheckersGameState(playerColor);
        }

        /// <summary>
        /// Function that is called whenever it is the turn of the AI to do its move
        /// </summary>
        public void doAiMove()
        {
            if (didAiMove == false)
            {
                bool onlyOneMove = false;
                //Check if theres only 1 moveable piece
                if (currentState.moveablePieces.Count == 1)
                {
                    //If there only 1 moveable piece and only 1 available action, set best move to that move
                    if (currentState.moveablePieces.First().getPossibleMoves().Count == 1)
                    {
                        bestMove = currentState.moveablePieces.First().getPossibleMoves().First();
                        onlyOneMove = true;
                        Console.WriteLine("Only one move was available - Alpha-beta wasn't required"
                                        + "\n--------------------------------------------------------");
                    }
                }
                //If there is more than one move run algorithm
                if (onlyOneMove == false)
                {
                    int bestMoveValue = AlphaBetaSearch(new CheckersGameState(currentState));

                    //if the algorithm determines that the AI will win or lose regardless of which move,
                    //then make the ai do the first possible move it can
                    if (bestMoveValue == MIN_INT || bestMoveValue == MAX_INT)
                    {
                        bestMove = currentState.moveablePieces.First().getPossibleMoves()[0];
                    }
                    //if algorithm took 50 or more seconds to run, it has reach time cutoff
                    if ((endTime - startTime).TotalSeconds >= 55)
                    {
                        Console.Write("Tree reached time cutoff of 55 seconds"
                            + "\n--Max Depth: " + maxDepth
                            + "\n--Cut Off Depth: " + (cutoff + iterativeDepth));

                        //Reached time cutoff - reduce the cutoff by 1/4
                        float actualCutoff = 0.75f * (cutoff + iterativeDepth);
                        iterativeDepth = (((int)actualCutoff - cutoff) > 0) ? (int)actualCutoff - cutoff : 0;
                    }
                    //Else if maxDepth is equal to the cutoff depth, that means the tree reached cutoff level
                    else if (maxDepth == cutoff + iterativeDepth)
                    {
                        Console.Write("Tree reached cut off"
                            + "\n--Cut Off Depth: " + (cutoff + iterativeDepth));

                        //Since alpha-beta was able to finish in reasonable time (< 50s)
                        // and the cutoff was reached, increased by 1 + (1/4)number of turns passed 
                        iterativeDepth = 1 + (int)(currentState.numTurnsPassed * 0.25);
                    }
                    //else the tree completed before reaching cutoff
                    else
                    {
                        Console.Write("Tree completed before reaching cut off"
                            + "\n--Max Depth: " + maxDepth
                            + "\n--Cut Off Depth: " + (cutoff + iterativeDepth));
                    }
                    Console.WriteLine("\n--Nodes Generated: " + nodes
                                    + "\n--# times pruning occured in MAX-VALUE: " + maxPruned
                                    + "\n--# times pruning occured in MIN-VALUE: " + minPruned
                                    + "\nFound Move in: " + (endTime - startTime).TotalSeconds
                                    + "seconds\n--------------------------------------------------------");

                    //AI is completed sleep the thread
                    Thread.Sleep(1000);
                }

                //Do the actual move
                CheckersPiece movePiece = bestMove.movePiece;
                currentState.doGamePieceAction(currentState.getPiece(movePiece.getColor(), movePiece.position));
                //Set now to when the delay for the AI to actually do the move to now
                didAiMove = true;
                aiMoveStart = DateTime.Now;
            }
            else
            {
                //If the delay for an AI to do the move has been reached do the move
                if ((DateTime.Now - aiMoveStart).TotalSeconds > aiTimeDelay)
                {
                    if (currentState.doTileAction(currentState.gameBoard.getTileAt(bestMove.destinationPosition)) == true)
                    {
                        currentState.activeGamePiece.Update(currentState);
                        didAiMove = false;
                    }
                }
            }
        }

        /// <summary>
        /// Main alpha-beta search algorithm
        /// </summary>
        /// <param name="state">The current state of the game</param>
        /// <returns>The value for the best next move</returns>
        public int AlphaBetaSearch(CheckersGameState state)
        {
            //Initialize all the variables to be used in the algorithm
            int alpha = MIN_INT;
            int beta = MAX_INT;
            nodes = 0;
            maxDepth = 0;
            maxPruned = 0;
            minPruned = 0;
            startTime = DateTime.Now;

            int value = MaxValue(state, alpha, beta, 0);            
            endTime = DateTime.Now;
            return value;
        }

        /// <summary>
        /// Max value search function
        /// </summary>
        /// <param name="state">The current state of the game</param>
        /// <param name="alpha">highest alpha found</param>
        /// <param name="beta">lowest beta found</param>
        /// <param name="depth">depth current state is at</param>
        /// <returns>The best max value</returns>
        public int MaxValue(CheckersGameState state, int alpha, int beta, int depth)
        {
            //increase depth
            depth++;
            maxDepth = (depth > maxDepth) ? depth : maxDepth;
            //Evaluate the state
            int utilityValue = Evaluate(state); 
            //If the state is terminal or if the cuttoff is reached, return the value
            if (utilityValue == MAX_INT || utilityValue == MIN_INT 
                || depth == cutoff + iterativeDepth || (DateTime.Now - startTime).TotalSeconds > 55)
            {
                return utilityValue;
            }
            //integer value used to determine alpha
            int value = MIN_INT;
            //A temporary gamestate to prevent anything in the actual game from changing
            CheckersGameState tempState;

            //The loop below finds every possible action from this state,
            //then using the temporary state, it will do the action,
            //and pass that state into the minValue function to find the largest possible value for alpha
            foreach (CheckersPiece piece in state.moveablePieces)
            {
                state.doGamePieceAction(piece);
                foreach (GameMove action in state.activeGamePiece.getPossibleMoves())
                {
                    tempState = new CheckersGameState(state);
                    nodes++;
                    if (tempState.doTileAction(tempState.gameBoard.getTileAt(action.destinationPosition)) == true)
                    {
                        tempState.activeGamePiece.Update(tempState);
                    }
                    value = Math.Max(value, MinValue(tempState, alpha, beta, depth));
                                        
                    //if alpha becomes greater than or equal to beta prune the tree
                    if (value >= beta)
                    {
                        maxPruned++;
                        return value;
                    }

                    if (alpha < value)
                    {
                        alpha = value;
                        //if we are in the actual state of the game, that means we found a new best move
                        if (depth == 1)
                        {
                            bestMove = action;
                        }
                    }
                }
            }            

            return value;
        }

        /// <summary>
        /// Min value search function
        /// </summary>
        /// <param name="state">The current state of the game</param>
        /// <param name="alpha">highest alpha found</param>
        /// <param name="beta">lowest beta found</param>
        /// <param name="depth">depth current state is at</param>
        /// <returns>The best min value</returns>
        public int MinValue(CheckersGameState state, int alpha, int beta, int depth)
        {
            //increase depth
            depth++;
            maxDepth = (depth > maxDepth) ? depth : maxDepth;
            //Evaluate the state
            int utilityValue = Evaluate(state);
            //If the state is terminal or if the cuttoff is reached, return the value
            if (utilityValue == MAX_INT || utilityValue == MIN_INT 
                || depth == cutoff + iterativeDepth || (DateTime.Now - startTime).TotalSeconds > 55)
            {
                return utilityValue;
            }
            //integer value used to determine alpha
            int value = MAX_INT;
            //A temporary gamestate to prevent anything in the actual game from changing
            CheckersGameState tempState;

            //The loop below finds every possible action from this state,
            //then using the temporary state, it will do the action,
            //and pass that state into the maxValue function to find the smallest possible value for beta
            foreach(CheckersPiece piece in state.moveablePieces)
            {
                state.doGamePieceAction(piece);
                foreach (GameMove action in state.activeGamePiece.getPossibleMoves())
                {
                    //Do the action
                    tempState = new CheckersGameState(state);
                    nodes++;
                    if (tempState.doTileAction(tempState.gameBoard.getTileAt(action.destinationPosition)) == true)
                    {
                        tempState.activeGamePiece.Update(tempState);
                    }
                    value = Math.Min(value, MaxValue(tempState, alpha, beta, depth));

                    //if beta becomes less than or equal to alpha prune the tree
                    if (value <= alpha)
                    {
                        minPruned++;
                        return value;
                    }
                    beta = Math.Min(beta, value);
                }
            }            

            return value;
        }

        /// <summary>
        /// Function to evaluate the value of the state (Evaluation function) 
        /// </summary>
        /// <param name="state">The state to be evaluated</param>
        /// <returns>The value of the state</returns>
        public int Evaluate(CheckersGameState state)
        {
            PieceColor winner = state.winner;
            //white is winner, if ai is white, then return max_int else return min_int
            if (winner == PieceColor.White) 
            {
                return (state.aiColor == PieceColor.White) ? MAX_INT : MIN_INT;
            }
            //Find out that white is winner, if ai is white, then return max_int else return min_int
            else if (winner == PieceColor.Black)
            {
                return (state.aiColor == PieceColor.White) ? MIN_INT : MAX_INT;
            }
            //return the custom designed evaluation function
            //The states are evaluated by the difference in pieces left weighted by the number of turns that has occured
            //Every 4 turns the weight of difference in pieces goes up
            else
            {
                if (state.aiColor == PieceColor.White)
                {
                    return (int)(state.whiteGamePieces.Count - state.blackGamePieces.Count) * (int)(1 + state.numTurnsPassed * 0.25);
                }
                else
                {
                    return (int)(state.blackGamePieces.Count - state.whiteGamePieces.Count) * (int)(1 + state.numTurnsPassed * 0.25);
                }
            }
        }
    }
}
