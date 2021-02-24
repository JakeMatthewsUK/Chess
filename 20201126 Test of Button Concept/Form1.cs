using System;
using System.Collections.Generic;
using System.Windows.Forms;



namespace _20201126_Test_of_Button_Concept
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        bool playerOneTurn = true;                                  //playerOne = white
        bool isFirstClick = true;                                   //differentiate between selecting a piece to move and selecting a destination square    
        bool foundCheck = false;                                    //denote that the active player is in check
        int candidateMoves = 0;                                     //track the number of moves the active player could make (ignoring those that put himself into check)
        int movesThatCauseCheck = 0;                                //if (candidateMoves-movesThatCauseCheck)==0, the game is in CheckMate 
        int whiteKingRow = 7, whiteKingCol = 3;                       //track the position of the white king                   
        int blackKingRow = 0, blackKingCol = 3;                       //track the position of the black king   
        int moveCount = 0;                                          //to display the total number of moves made so far
        int timeInSeconds = 0;                                      //display the time taken so far
        int[] knightRowVector = { -2, -2, -1, -1, 1, 1, 2, 2 };     //iterating through these two arrays in step gives the column and row offsets in the 8 possible knight moves 
        int[] knightColumnVector = { -1, 1, -2, 2, -2, 2, -1, 1 };  //^

        PictureBox firstSelection = null;                           //a handle to store the picturebox a player clicked on first (the piece they want to move)
        PictureBox secondSelection = null;                          //a handle to store the picturebox a player clicked on second (the destination square)
        PictureBox copyOfFirstSelection = new PictureBox();         //a blank picturebox that will clone and store the details (background Image & Tag) of the first seleted picturebox
        PictureBox copyOfSecondSelection = new PictureBox();        //^ but for the second selected picturebox

        private void onClick(object sender, EventArgs e)
        {
            if (isFirstClick)       //player has clicked on a piece to select it for movement
            {
                firstSelection = sender as PictureBox;                                              //grab the clicked picturebox
                string pieceTag = (string)firstSelection.Tag;
                Console.WriteLine("first selection tag was " + firstSelection.Tag);
                if (playerOneTurn && pieceTag[0] == 'w' || !playerOneTurn && pieceTag[0] == 'b')    //if they chose one of their own pieces
                {
                    cloneFirstSelectionPictureBox();                                                //store the attributes of the first picturebox before changing it
                    firstSelection.BackColor = System.Drawing.Color.Red;                            //highlight the cell                                                               
                    scanForAvailableMoves(firstSelection);                                          //go though and find squares the piece can move to (and highlight them)
                    candidateMoves = 0;                                                             //reset this for future use
                    movesThatCauseCheck = 0;                                                        //^
                    isFirstClick = false;                                                           //note that we are moving on to the second click next time
                }
            }
            else                                                                                    //player has chosen a destination square to try to move a piece to
            {
                secondSelection = sender as PictureBox;                                             //grab the clicked picturebox
                isFirstClick = true;
                if (secondSelection.BackColor != System.Drawing.Color.Pink)                         //if player selects any unhighlighted (non reachable) square
                {
                    unHighlightMoves();                                                             //remove the highlighting of all cells 
                }
                else                                                                                //player selected a highlighted square they may be able to move to
                {
                    if ((string)firstSelection.Tag == "wKing")
                    {
                        whiteKingCol = gridTLP.GetColumn(secondSelection);                          //store the king location if this is the piece they have moved
                        whiteKingRow = gridTLP.GetRow(secondSelection);
                    }
                    if ((string)firstSelection.Tag == "bKing")
                    {
                        blackKingCol = gridTLP.GetColumn(secondSelection);
                        blackKingRow = gridTLP.GetRow(secondSelection);
                    }
                    cloneSecondSelectionPictureBox();                                   //store its attributes in copyOfSecondSelection
                    updatePictureBoxesAfterMove();                                      //unhighlight the selected piece and update picturebox attributes to reflect the move
                    movesThatCauseCheck = 0;                                            //reset this value to zero before testForCheck()
                    testForCheck();                                                     //this function increments movesThatCauseCheck by one if the current move will put the moving player in check

                    if (movesThatCauseCheck == 1)                                       //player put themself in check -> need to undo move
                    {
                        undoMove();                                                     //revert pictureboxes, restore the king position if it tried to move, unhighlight cells
                    }
                    else                                                                //successful move
                    {
                        playerOneTurn = !playerOneTurn;                                 //switch turn
                        movesThatCauseCheck = 0;                                        //reset before selectAllPieces()
                        candidateMoves = 0;                                             //^
                        selectAllPieces();                                              //goes through all of the possible moves of the active player (not the one who just moved)  
                                                                                        //...and finds the total number of candidate moves and the number that put themself in check
                        unHighlightMoves();
                        if (movesThatCauseCheck == candidateMoves)                      //the player who just moved has left his opponent no viable moves -> CheckMate       
                        {
                            MessageBox.Show("checkMate");
                            checkMateSequence();
                        }
                    }
                    updateDisplay();                                                    //reflect whose turn it is and the number of moves made
                }
            }
            Control endRowControl = checkEndRows();                                     //if a pawn has reached the opposite end of the board this returns that control, otherwise null
            if (endRowControl != null)
            {
                unHighlightMoves();
                switchtoPromotionMenu();   //pops up a menu where the played can choose a piece to promote their pawn into, updates the pieces and tests if the promotion causes checkmate
            }
            
        }

        public void cloneFirstSelectionPictureBox()
        {
            copyOfFirstSelection.BackColor = firstSelection.BackColor;     //store the attributes of the first picturebox before changing it                 
            copyOfFirstSelection.Image = firstSelection.Image;
            copyOfFirstSelection.Tag = firstSelection.Tag;
        }
        public void cloneSecondSelectionPictureBox()
        {
            copyOfSecondSelection.Tag = secondSelection.Tag;               //store the attributes of the second picturebox before changing it
            copyOfSecondSelection.Image = secondSelection.Image;
        }
        public void updatePictureBoxesAfterMove()
        {
            firstSelection.BackColor = copyOfFirstSelection.BackColor;          //revert first picturebox to original (unhighlighted) colour
            secondSelection.Tag = firstSelection.Tag;                           //give the first picturebox the attributes of the square it is moving to
            secondSelection.Image = firstSelection.Image;                       //^
            firstSelection.Tag = "empty";                                       //change the attributes of the original square to reflect that there is no longer a piece there
            firstSelection.Image = null;                                        //^
        }
        public void undoMove()
        {
            firstSelection.Tag = copyOfFirstSelection.Tag;                  //return the selected pictureboxes back to their original status
            firstSelection.Image = copyOfFirstSelection.Image;
            secondSelection.Tag = copyOfSecondSelection.Tag;
            secondSelection.Image = copyOfSecondSelection.Image;

            if (playerOneTurn && (string)firstSelection.Tag == "wKing")     //if a player moved their king, update the ints that store where it is located to reflect undoing the move
            {
                whiteKingCol = gridTLP.GetColumn(firstSelection);
                whiteKingRow = gridTLP.GetRow(firstSelection);
            }
            if (!playerOneTurn && (string)firstSelection.Tag == "bKing")    //^
            {
                blackKingCol = gridTLP.GetColumn(firstSelection);
                blackKingRow = gridTLP.GetRow(firstSelection);
            }
            unHighlightMoves();                                             //leaving the cells highlighted allows any piece to move there - need to prevent this
        }
        private void updateDisplay()
        {   //called after each turn
            movesLabel.Text = "Move number: " + moveCount;
            if (!playerOneTurn)
            {
                playerTurnButton.Text = "2";
                playerTurnButton.ForeColor = System.Drawing.Color.Black;
            }
            else
            {
                playerTurnButton.Text = "1";
                playerTurnButton.ForeColor = System.Drawing.Color.White;
            }
        }
        public void checkMateSequence()
        {

        }
        public class pictureBoxInformation
        {
            public int startRow;
            public int startCol;
            public string pieceTitle;
            public string pieceType;
            public string pieceColor;
            public pictureBoxInformation(int _startCol, int _startRow, string _pieceTitle, string _pieceType, string _pieceColor)
            {
                startCol = _startCol;
                startRow = _startRow;
                pieceTitle = _pieceTitle;
                pieceType = _pieceType;
                pieceColor = _pieceColor;
            }
        };
        private void selectAllPieces()
        {       //selects all of the players pieces and test how many moves they can make in total
            List<Control> playersRemainingPieces = new List<Control>();     //a list to store all of the controls that represent the current players pieces 
            foreach (Control c in gridTLP.Controls)
            {
                if (c is PictureBox)
                {
                    string thisPiece = (string)c.Tag;
                    if (thisPiece.Substring(0, 1) == "w")
                    {
                        if (playerOneTurn)                                  //only add it to the list if it begins with the correct letter (w for white, b for black)
                        {
                            playersRemainingPieces.Add((Control)c);
                        }
                    }
                    else if (thisPiece.Substring(0, 1) == "b")
                    {
                        if (!playerOneTurn)
                        {
                            playersRemainingPieces.Add((Control)c);
                        }
                    }
                }
            }
            candidateMoves = 0;                                             //reset these as they are updated in scanForAvailableMoves()
            movesThatCauseCheck = 0;                                        //^

            for (int i = 0; i < playersRemainingPieces.Count; i++)          //go thorough all of the players pieces in turn
            {
                scanForAvailableMoves(playersRemainingPieces[i]);           //count the number of candidateMoves and the number that put themself in check 
            }

            if (playerOneTurn)                                              //console output - this is only used to demonstrate and debug the program                                         
            {
                Console.WriteLine("white can make " + candidateMoves + ". Of these, " + movesThatCauseCheck + " cause check, leaving " + (candidateMoves - movesThatCauseCheck) + " possible moves");
            }
            else
            {
                Console.WriteLine("black can make " + candidateMoves + ". Of these, " + movesThatCauseCheck + " cause check, leaving " + (candidateMoves - movesThatCauseCheck) + " possible moves");
            }
        }
        private void scanForAvailableMoves(Control activePicBox)
        {
            //this funtion is called for 2 reasons:
            // 1) to highlight the available moves for a piece a played has selected
            // 2) to count the number of moves a piece can make, and the number that cause check 

            int startRow = gridTLP.GetRow(activePicBox);                        //get the relevant information about the picturebox and store it as a pictureBoxInformation instance
            int startCol = gridTLP.GetColumn(activePicBox);                     //...this allows for easier referencing in later steps
            string pieceTitle = (string)activePicBox.Tag;
            string pieceColor = pieceTitle.Substring(0, 1);
            string pieceType = pieceTitle.Substring(1, pieceTitle.Length - 1);
            pictureBoxInformation currentPiece = new pictureBoxInformation(startCol, startRow, pieceTitle, pieceType, pieceColor);
            findReachableSquares(currentPiece);                                 //use the newly created pictureBoxInformation to find which squares are reachable
        }
        private void findReachableSquares(pictureBoxInformation currentMove)
        {
            switch (currentMove.pieceType)  //the possible moves depend on the piece type
            {
                case "Pawn":                                        //the most awkward type
                    if (currentMove.pieceColor == "w")              //movement dependent on color (unlike other pieces)
                    {                                               //first test for vertical only moves
                        if (canWeMoveHere(0, -1, currentMove))      //canWeMoveHere highlights only the cells we can move to based on the supplied movement vector arguments
                        {                                           //It returns true if the move is viable, otherwise false
                            if (currentMove.startRow == 6)          //edge case where pawn is in start row and may be able to move 2 units
                            {
                                canWeMoveHere(0, -2, currentMove);
                            }
                        }
                        canWeMoveHere(-1, -1, currentMove);         //now test for diagonal attack moves
                        canWeMoveHere(1, -1, currentMove);          //^
                    }
                    else                                            //alternate situation where the tag of the active control begins with 'b' - it is a black piece
                    {
                        if (canWeMoveHere(0, 1, currentMove))
                        {
                            if (currentMove.startRow == 1)
                            {
                                canWeMoveHere(0, 2, currentMove);
                            }
                        }
                        canWeMoveHere(-1, 1, currentMove);
                        canWeMoveHere(1, 1, currentMove);
                    }
                    break;
                case "Knight":
                    for (int i = 0; i < 8; i++)                     //iterate through the global knightRowVector & knightColumnVector arrays to get the dy and dx values
                    {
                        canWeMoveHere(knightRowVector[i], knightColumnVector[i], currentMove);
                    }
                    break;
                case "Rook":
                    testOrthogonal(currentMove);                    //a subfuction that handles vertical only and horizontal only moves
                    break;
                case "Bishop":
                    testDiagonal(currentMove);                      //a subfuction that handles moves that are both vertical and horizontal
                    break;
                case "Queen":
                    testOrthogonal(currentMove);
                    testDiagonal(currentMove);
                    break;
                case "King":
                    testOrthogonal(currentMove);
                    testDiagonal(currentMove);
                    break;
            }
        }

        private void testOrthogonal(pictureBoxInformation currentMove)
        {
            int offset = 1;                                     //start off to the right of the active control and move right
            while (canWeMoveHere(offset, 0, currentMove))       //call the function repeatedly until we find a non reachable square (or the edge of the board)
            {
                offset++;
            }
            offset = -1;                                        //start the left of the active control and move left
            while (canWeMoveHere(offset, 0, currentMove))
            {
                offset--;
            }
            offset = 1;                                         //start below the active control and move down
            while (canWeMoveHere(0, offset, currentMove))
            {
                offset++;
            }
            offset = -1;                                        //start above the active control and move up
            while (canWeMoveHere(0, offset, currentMove))
            {
                offset--;
            }
        }
        private void testDiagonal(pictureBoxInformation currentMove)
        {
            int offset = 1;                                     //start below and to the right of the active control and move down and right
            while (canWeMoveHere(offset, offset, currentMove))
            {
                offset++;
            }
            offset = 1;                                         //start above and to the right of the active control and move up and right
            while (canWeMoveHere(offset, -offset, currentMove))
            {
                offset++;
            }
            offset = 1;                                         //start below and to the left of the active control and move down and left
            while (canWeMoveHere(-offset, offset, currentMove))
            {
                offset++;
            }
            offset = 1;                                         //start above and to the left of the active control and move up and left
            while (canWeMoveHere(-offset, -offset, currentMove))
            {
                offset++;
            }
        }
        private bool canWeMoveHere(int columnVector, int rowVector, pictureBoxInformation currentMove)
        {
            int col = currentMove.startCol + columnVector;      //the column containing the PictureBox we are testing
            int row = currentMove.startRow + rowVector;         //the row containing the PictureBox we are testing

            if (col >= 0 && col <= 7 && row >= 0 && row <= 7)                   //if we are still on the board (otherwise return false)
            {
                Control destination = gridTLP.GetControlFromPosition(col, row);                //grab the destination control
                string destinationTag = (string)destination.Tag;
                string destinationPieceColor = destinationTag.Substring(0, 1);                  //'b' for black, 'w' for white, 'e' for empty

                if (destinationPieceColor == "e")                                       //destination cell is empty
                {
                    if (!(currentMove.pieceType == "Pawn" && columnVector != 0))        //check we are not in the edge case where pawn cannot move diagonal into empty space
                    {
                        candidateMoves++;                                               //note that the move is viable (used when function if called to see if a player is in check)
                        destination.BackColor = System.Drawing.Color.Pink;              //highlight the cell (used when a player has chosen a piece to move)

                        Control startSquare = gridTLP.GetControlFromPosition(currentMove.startCol, currentMove.startRow);       //get a handle on the start square Control

                        string startSquareTitle = (string)startSquare.Tag;

                        if (startSquareTitle == "wKing")
                        {
                            whiteKingCol = col;                 //if the piece that was selected to move is a king, update its new position temporarily to the place it can reach
                            whiteKingRow = row;                 //..this allows us to test whether that move would put the player in check - it is put back afterwards
                        }
                        else if (startSquareTitle == "bKing")
                        {
                            blackKingCol = col;
                            blackKingRow = row;
                        }

                        string endSquareTitle = (string)destination.Tag;
                        destination.Tag = startSquare.Tag;              //temporarily update the tags as if the player has moved to test for check
                        startSquare.Tag = "empty";                      //...they will be reset after the testForCheck function

                        testForCheck();                   //test whether this move into an empty square will put self into check - if so, increment the movesThatCauseCheck function

                        startSquare.Tag = destination.Tag;              //the next few lines reset the board back to its state before the move          
                        destination.Tag = endSquareTitle;

                        if ((string)startSquare.Tag == "wKing")
                        {
                            whiteKingCol = currentMove.startCol;
                            whiteKingRow = currentMove.startRow;
                        }
                        else if ((string)startSquare.Tag == "bKing")
                        {
                            blackKingCol = currentMove.startCol;
                            blackKingRow = currentMove.startRow;
                        }


                        if (currentMove.pieceType == "King")
                        {
                            return false;           //edge case where King can only move one unit - return false to stop it trying to move further in the testOrthogonal & testDiagonal functions
                        }
                        return true;                //if the piece has the correct ability, we need to test if it can move further in this direction so return true
                    }
                }
                else if (destinationPieceColor != currentMove.pieceColor)           //case where the active piece can reach an opponent piece
                {
                    if (!(currentMove.pieceType == "Pawn" && columnVector == 0))    //exclude edge case where pawn cannot move forward to take a piece
                    {
                        candidateMoves++;                                           //note that the move is possible (though it may cause check)
                        destination.BackColor = System.Drawing.Color.Pink;          //highlight it as a possible move

                        Control startSquare = gridTLP.GetControlFromPosition(currentMove.startCol, currentMove.startRow);   //get a handle on the start square Control
                        string startSquareTitle = (string)startSquare.Tag;
                        if (startSquareTitle == "wKing")
                        {
                            whiteKingCol = col;                 //if the piece that was selected to move is a king, update its new position temporarily to the place it can reach
                            whiteKingRow = row;                 //..this allows us to test whether that move would put the player in check - it is put back afterwards
                        }
                        else if (startSquareTitle == "bKing")
                        {
                            blackKingCol = col;
                            blackKingRow = row;
                        }
                        string endSquareTitle = (string)destination.Tag;
                        destination.Tag = startSquare.Tag;              //temporarily update the tags as if the player has moved to test for check
                        startSquare.Tag = "empty";                      //...they will be reset after the testForCheck function
                        testForCheck();                                 //test whether this attacking move will put self into check - if so, increment the movesThatCauseCheck function
                        startSquare.Tag = destination.Tag;              //the next few lines reset the board back to its state before the move 
                        destination.Tag = endSquareTitle;
                        if ((string)startSquare.Tag == "wKing")
                        {
                            whiteKingCol = currentMove.startCol;
                            whiteKingRow = currentMove.startRow;
                        }
                        else if ((string)startSquare.Tag == "bKing")
                        {
                            blackKingCol = currentMove.startCol;
                            blackKingRow = currentMove.startRow;
                        }
                    }
                }
            }
            return false;
        }
        private void testForCheck()
        {   //this function is analogous to scanForAvailableMoves() - it tests if a move causes check and increments the movesThatCauseCheck int
            //it does this my starting at the king and testing if it could reach any opponent pieces by using a move that the opponent piece could make
            //it delegates to subfunctions that do this, in a similar way to scanForAvailableMoves()
            Control kingToCheck = gridTLP.GetControlFromPosition(whiteKingCol, whiteKingRow);

            if (!playerOneTurn)
            {
                kingToCheck = gridTLP.GetControlFromPosition(blackKingCol, blackKingRow);
            }
            int startRow = gridTLP.GetRow(kingToCheck);
            int startCol = gridTLP.GetColumn(kingToCheck);
            string pieceTitle = (string)kingToCheck.Tag;
            string pieceColor = pieceTitle.Substring(0, 1);
            string pieceType = pieceTitle.Substring(1, pieceTitle.Length - 1);
            pictureBoxInformation currentPiece = new pictureBoxInformation(startCol, startRow, pieceTitle, pieceType, pieceColor);
            checkSearch(currentPiece);
        }

        private void checkSearch(pictureBoxInformation currentMove)
        {
            //explore moving Orthogonally
            int offset = 1;             
            while (!foundCheck && exploreSquares(offset, 0, currentMove, "Ortho"))  //ortho denotes an orthogonal move
            {                       //the foundcheck bool is set to true when check is found - this prevents movesThatCause check from being incremented more than once on the same square
                offset++;           //...such as when the king would be put in check my more than one opponent piece
            }
            offset = -1;
            while (!foundCheck && exploreSquares(offset, 0, currentMove, "Ortho"))
            {
                offset--;
            }
            offset = 1;
            while (!foundCheck && exploreSquares(0, offset, currentMove, "Ortho"))
            {
                offset++;
            }
            offset = -1;
            while (!foundCheck && exploreSquares(0, offset, currentMove, "Ortho"))
            {
                offset--;
            }

            //explore moving diagonally

            offset = 1;
            while (!foundCheck && exploreSquares(offset, offset, currentMove, "Diag"))  //diag denotes an diagonal move
            {
                offset++;
            }
            offset = 1;
            while (!foundCheck && exploreSquares(offset, -offset, currentMove, "Diag"))
            {
                offset++;
            }
            offset = 1;
            while (!foundCheck && exploreSquares(-offset, offset, currentMove, "Diag"))
            {
                offset++;
            }
            offset = 1;
            while (!foundCheck && exploreSquares(-offset, -offset, currentMove, "Diag"))
            {
                offset++;
            }
            //explore knight moves

            for (int i = 0; i < 8; i++)
            {
                if (!foundCheck)
                {
                    exploreSquares(knightRowVector[i], knightColumnVector[i], currentMove, "Knight");   //knight denotes that we are testing a knight move
                }
                else { i = 8; }
            }

            //explore pawn moves
            if (!foundCheck)
            {
                if (currentMove.pieceColor == "w")
                {
                    exploreSquares(-1, -1, currentMove, "Pawn");                //pawn denotes that we are testing a knight move
                    exploreSquares(1, -1, currentMove, "Pawn");
                }
                else
                {
                    exploreSquares(-1, 1, currentMove, "Pawn");
                    exploreSquares(1, 1, currentMove, "Pawn");
                }
            }
            foundCheck = false;
        }

        private bool exploreSquares(int columnVector, int rowVector, pictureBoxInformation currentMove, string attackVulnerableTo)
        {

            int col = currentMove.startCol + columnVector;
            int row = currentMove.startRow + rowVector;

            if (col >= 0 && col <= 7 && row >= 0 && row <= 7)                   //if we are still on the board
            {
                Control destination = gridTLP.GetControlFromPosition(col, row);
                string destinationTag = (string)destination.Tag;
                string firstLetterOfDestinationTag = destinationTag.Substring(0, 1);
                string destinationPieceType = destinationTag.Substring(1, destinationTag.Length - 1);

                if (firstLetterOfDestinationTag == "e")             //empty cell
                {
                    return true;
                }
                else if (firstLetterOfDestinationTag != currentMove.pieceColor)
                {
                    //possibleMoves--;
                    if (attackVulnerableTo == "Ortho")
                    {
                        if (destinationPieceType == "Rook" || destinationPieceType == "Queen")
                        {
                            movesThatCauseCheck++;
                            foundCheck = true;
                            return false;
                        }
                        else if (destinationPieceType == "King" && columnVector <= 1 && columnVector >= -1 && rowVector <= 1 && rowVector >= -1)
                        {
                            movesThatCauseCheck++;
                            foundCheck = true;
                            return false;
                        }
                    }
                    if (attackVulnerableTo == "Diag")
                    {
                        if (destinationPieceType == "Bishop" || destinationPieceType == "Queen")
                        {
                            movesThatCauseCheck++;
                            foundCheck = true;
                            return false;
                        }
                        else if (destinationPieceType == "King" && columnVector <= 1 && columnVector >= -1 && rowVector <= 1 && rowVector >= -1)
                        {
                            movesThatCauseCheck++;
                            foundCheck = true;
                            return false;
                        }
                    }
                    if (attackVulnerableTo == "Knight")
                    {
                        if (destinationPieceType == "Knight")
                        {
                            movesThatCauseCheck++;
                            foundCheck = true;
                            return false;
                        }
                    }
                    if (attackVulnerableTo == "Pawn")
                    {
                        if (destinationPieceType == "Pawn")
                        {
                            movesThatCauseCheck++;
                            foundCheck = true;
                            return false;
                        }
                    }
                    return false;
                }
            }
            return false;
        }

        private void unHighlightMoves()
        {
            foreach (Control c in gridTLP.Controls)
            {
                if (c is PictureBox)
                {
                    if (c.BackColor == System.Drawing.Color.Pink || c.BackColor == System.Drawing.Color.Red)
                    {
                        if ((gridTLP.GetRow(c) % 2 == 0 && gridTLP.GetColumn(c) % 2 == 0) || (gridTLP.GetRow(c) % 2 == 1 && gridTLP.GetColumn(c) % 2 == 1))
                        {
                            c.BackColor = System.Drawing.Color.LightYellow;
                        }
                        else
                        {
                            c.BackColor = System.Drawing.Color.SandyBrown;
                        }
                    }
                }
            }
        }
        private Control checkEndRows()
        {

            int startCol = 0;
            bool exit = false;
            Control moveTest;
            while (startCol < 8 && !exit)
            {
                moveTest = gridTLP.GetControlFromPosition(startCol, 0);
                if ((string)moveTest.Tag == "wPawn")
                {
                    moveTest.BackColor = System.Drawing.Color.Green;
                    playerOneTurn = !playerOneTurn;
                    return moveTest;
                }
                startCol++;
            }
            startCol = 0;
            exit = false;
            while (startCol < 8 && !exit)
            {
                moveTest = gridTLP.GetControlFromPosition(startCol, 7);
                if ((string)moveTest.Tag == "bPawn")
                {
                    moveTest.BackColor = System.Drawing.Color.Green;
                    playerOneTurn = !playerOneTurn;
                    return moveTest;
                }
                startCol++;
            }
            return null;
        }
        private void switchtoPromotionMenu()
        {

            if (!playerOneTurn)
            {
                foreach (PictureBox c in tlp2.Controls)
                {
                    if ((string)c.Tag == "wQueen")
                    {
                        c.Image = Properties.Resources.bQueen;
                        c.Tag = "bQueen";
                    }
                    else if ((string)c.Tag == "wRook")
                    {
                        c.Image = Properties.Resources.bRook;
                        c.Tag = "bRook";
                    }
                    else if ((string)c.Tag == "wBishop")
                    {
                        c.Image = Properties.Resources.bBishop;
                        c.Tag = "bBishop";
                    }
                    else if ((string)c.Tag == "wKnight")
                    {
                        c.Image = Properties.Resources.bKnight;
                        c.Tag = "bKnight";
                    }
                }
            }
            else
            {
                foreach (PictureBox c in tlp2.Controls)
                {
                    if ((string)c.Tag == "bQueen")
                    {
                        c.Image = Properties.Resources.wQueen;
                        c.Tag = "wQueen";
                    }
                    else if ((string)c.Tag == "bRook")
                    {
                        c.Image = Properties.Resources.wRook;
                        c.Tag = "wRook";
                    }
                    else if ((string)c.Tag == "bBishop")
                    {
                        c.Image = Properties.Resources.wBishop;
                        c.Tag = "wBishop";
                    }
                    else if ((string)c.Tag == "bKnight")
                    {
                        c.Image = Properties.Resources.wKnight;
                        c.Tag = "wKnight";
                    }
                }
            }
            tlp2.Visible = true;
            tlp2.Enabled = true;
            piecePromotionLabel.Visible = true;
            gridTLP.Enabled = false;
            playerTurnButton.Visible = false;
            playerTurnLabel.Visible = false;
            movesLabel.Visible = false;
            gameTimeLabel.Visible = false;

        }
        private void onPromotionClick(object sender, EventArgs e)
        {
            PictureBox selection = sender as PictureBox;
            secondSelection.Tag = selection.Tag;
            secondSelection.Image = selection.Image;
            if ((gridTLP.GetRow(secondSelection) % 2 == 0 && gridTLP.GetColumn(secondSelection) % 2 == 0) || (gridTLP.GetRow(secondSelection) % 2 == 1 && gridTLP.GetColumn(secondSelection) % 2 == 1))
            {
                secondSelection.BackColor = System.Drawing.Color.LightYellow;
            }
            else
            {
                secondSelection.BackColor = System.Drawing.Color.SandyBrown;
            }
            tlp2.Visible = false;
            tlp2.Enabled = false;
            piecePromotionLabel.Visible = false;
            gridTLP.Enabled = true;
            playerTurnButton.Visible = true;
            playerTurnLabel.Visible = true;
            movesLabel.Visible = true;
            gameTimeLabel.Visible = true;

            playerOneTurn = !playerOneTurn;
            movesThatCauseCheck = 0;
            candidateMoves = 0;
            selectAllPieces();
            unHighlightMoves();
            if (movesThatCauseCheck == candidateMoves)
            {

                MessageBox.Show("checkMate");
            }
            movesThatCauseCheck = 0;
            candidateMoves = 0;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            timeInSeconds++;

            if (timeInSeconds < 60)
            {
                gameTimeLabel.Text = "Game time: " + timeInSeconds + "s";
            }
            else
            {
                gameTimeLabel.Text = "Game time: " + timeInSeconds / 60 + "m" + timeInSeconds % 60 + "s";
            }
        }
    }

}
