using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;



namespace _20201126_Test_of_Button_Concept
{

    public partial class Form1 : Form
    {
        int[] rowVector = { -2, -2, -1, -1, 1, 1, 2, 2 };
        int[] columnVector = { -1, 1, -2, 2, -2, 2, -1, 1 };
        public Form1()
        {
            InitializeComponent();
        }
        bool playerOneTurn = true;
        bool firstSelectionMade = false;
        bool checkingIfInCheck = false;
        bool foundCheck = false;
        int possibleMoves = 0;
        int movesThatCauseCheck = 0;
        //need to keep track of king positions to look for check
        int wKingRow = 7;
        int wKingCol = 3;
        int bKingRow = 0;
        int bKingCol = 3;
        int moveCount = 0;
        int timeInSeconds = 0;

        PictureBox firstSelection = null;
        PictureBox copyOfFirstSelection = new PictureBox();
        PictureBox secondSelection = null;
        PictureBox copyOfSecondSelection = new PictureBox();

        private void onClick(object sender, EventArgs e)
        {
            if (!firstSelectionMade)
            {
                unHighlightMoves();
                firstSelection = sender as PictureBox;
                string pieceType = (string)firstSelection.Tag;
                if (playerOneTurn && pieceType[0] == 'w' || !playerOneTurn && pieceType[0] == 'b')  //if they chose one of their pieces
                {
                    copyOfFirstSelection.BackColor = firstSelection.BackColor;
                    copyOfFirstSelection.Image = firstSelection.Image;
                    copyOfFirstSelection.Tag = firstSelection.Tag;
                    firstSelection.BackColor = System.Drawing.Color.Red;    //highlight cell
                    disableControls();
                    extractPictureBoxInformation(firstSelection);
                    possibleMoves = 0;
                    movesThatCauseCheck = 0;
                    //focusOnSelection(firstSelection);
                    firstSelectionMade = true;
                }
            }
            else
            {
                secondSelection = sender as PictureBox;
                firstSelectionMade = false;
                //if player selects any non movable space
                if (secondSelection.BackColor != System.Drawing.Color.Pink)
                {
                    unHighlightMoves();
                }
                else
                {
                    //move king location if this is the piece they have selected
                    if ((string)firstSelection.Tag == "wKing")
                    {
                        wKingCol = gridTLP.GetColumn(secondSelection);
                        wKingRow = gridTLP.GetRow(secondSelection);
                    }
                    if ((string)firstSelection.Tag == "bKing")
                    {
                        bKingCol = gridTLP.GetColumn(secondSelection);
                        bKingRow = gridTLP.GetRow(secondSelection);
                    }
                    //copy second to clone
                    copyOfSecondSelection.Tag = secondSelection.Tag;
                    copyOfSecondSelection.Image = secondSelection.Image;
                    //revert first to original colour
                    firstSelection.BackColor = copyOfFirstSelection.BackColor;
                    //move second on top of first
                    secondSelection.Tag = firstSelection.Tag;
                    secondSelection.Image = firstSelection.Image;
                    firstSelection.Tag = "empty";
                    firstSelection.Image = null;
                    possibleMoves = 0;
                    movesThatCauseCheck = 0;
                    testForCheck();
                    

                    if (movesThatCauseCheck == 1)
                    {
                        Console.WriteLine("white king positions: " + wKingRow + "," + wKingCol);
                        Console.WriteLine("black king positions: " + bKingRow + "," + bKingCol);
     
                        firstSelection.Tag = copyOfFirstSelection.Tag;
                        firstSelection.Image = copyOfFirstSelection.Image;
                        secondSelection.Tag = copyOfSecondSelection.Tag;
                        secondSelection.Image = copyOfSecondSelection.Image;

                        if (playerOneTurn && (string)firstSelection.Tag == "wKing")
                        {
                            wKingCol = gridTLP.GetColumn(firstSelection);
                            wKingRow = gridTLP.GetRow(firstSelection);
                        }
                        if (!playerOneTurn && (string)firstSelection.Tag == "bKing")
                        {
                            bKingCol = gridTLP.GetColumn(firstSelection);
                            bKingRow = gridTLP.GetRow(firstSelection);
                        }
                        unHighlightMoves();
                    }
                    else
                    {
                        playerOneTurn = !playerOneTurn;
                        movesThatCauseCheck = 0;
                        possibleMoves = 0;
                        selectAllPieces();
                        if (movesThatCauseCheck == possibleMoves)
                        {
                            unHighlightMoves();
                            MessageBox.Show("checkMate");
                        }
                        disableControls();
                        unHighlightMoves();
                        moveCount++;
                        movesLabel.Text = "Move number: " + moveCount;
                    }
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
            }
            Control endRowControl = checkEndRows();
            if (endRowControl != null)
            {
                unHighlightMoves();
                switchtoPromotionMenu();
            }
        }

        public class pieceMoveInformation
        {
            public int startRow;
            public int startCol;
            public string pieceTitle;
            public string pieceType;
            public string pieceColor;
            public pieceMoveInformation(int _startCol, int _startRow, string _pieceTitle, string _pieceType, string _pieceColor)
            {
                startCol = _startCol;
                startRow = _startRow;
                pieceTitle = _pieceTitle;
                pieceType = _pieceType;
                pieceColor = _pieceColor;
            }
        };
        private void selectAllPieces()
        {
            List<Control> playersRemainingPieces = new List<Control>();

            foreach (Control c in gridTLP.Controls)
            {
                if (c is PictureBox)
                {
                    string thisPiece = (string)c.Tag;
                    if (thisPiece.Substring(0, 1) == "w")
                    {
                        if (playerOneTurn)
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
            int piecesFound = 0;
            possibleMoves = 0;
            movesThatCauseCheck = 0;
            for (int i = 0; i < playersRemainingPieces.Count; i++)
            {
                extractPictureBoxInformation(playersRemainingPieces[i]);
                piecesFound++;
            }
            if (playerOneTurn)
            {
                Console.WriteLine("white can make " + possibleMoves + ". Of these, " + movesThatCauseCheck + " cause check, leaving " + (possibleMoves - movesThatCauseCheck) + " viable moves");
            }
            else
            {
                Console.WriteLine("black can make " + possibleMoves + ". Of these, " + movesThatCauseCheck + " cause check, leaving " + (possibleMoves - movesThatCauseCheck) + " viable moves");
            }
        }
        private void extractPictureBoxInformation(Control activePicBox)
        {
            int startRow = gridTLP.GetRow(activePicBox);
            int startCol = gridTLP.GetColumn(activePicBox);
            string pieceTitle = (string)activePicBox.Tag;
            string pieceColor = pieceTitle.Substring(0, 1);
            string pieceType = pieceTitle.Substring(1, pieceTitle.Length - 1);
            pieceMoveInformation currentPiece = new pieceMoveInformation(startCol, startRow, pieceTitle, pieceType, pieceColor);
            findReachableSquares(currentPiece);
        }
        private void findReachableSquares(pieceMoveInformation currentMove)
        {
            switch (currentMove.pieceType)
            {
                case "Pawn":
                    if (currentMove.pieceColor == "w")
                    {
                        if (canWeKeepMoving(0, -1, currentMove))
                        {

                            if (currentMove.startRow == 6)
                            {
                                canWeKeepMoving(0, -2, currentMove);
                            }
                        }
                        canWeKeepMoving(-1, -1, currentMove);
                        canWeKeepMoving(1, -1, currentMove);
                    }
                    else
                    {
                        if (canWeKeepMoving(0, 1, currentMove))
                        {
                            if (currentMove.startRow == 1)
                            {
                                canWeKeepMoving(0, 2, currentMove);
                            }
                        }
                        canWeKeepMoving(-1, 1, currentMove);
                        canWeKeepMoving(1, 1, currentMove);
                    }
                    break;
                case "Knight":
                    for (int i = 0; i < 8; i++)
                    {
                        canWeKeepMoving(rowVector[i], columnVector[i], currentMove);
                    }
                    break;
                case "Rook":
                    testOrthogonal(currentMove);
                    break;
                case "Bishop":
                    testDiagonal(currentMove);
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

        private void testOrthogonal(pieceMoveInformation currentMove)
        {
            int offset = 1;
            while (canWeKeepMoving(offset, 0, currentMove))
            {
                offset++;
            }
            offset = -1;
            while (canWeKeepMoving(offset, 0, currentMove))
            {
                offset--;
            }
            offset = 1;
            while (canWeKeepMoving(0, offset, currentMove))
            {
                offset++;
            }
            offset = -1;
            while (canWeKeepMoving(0, offset, currentMove))
            {
                offset--;
            }
        }
        private void testDiagonal(pieceMoveInformation currentMove)
        {
            int offset = 1;
            while (canWeKeepMoving(offset, offset, currentMove))
            {
                offset++;
            }
            offset = 1;
            while (canWeKeepMoving(offset, -offset, currentMove))
            {
                offset++;
            }
            offset = 1;
            while (canWeKeepMoving(-offset, offset, currentMove))
            {
                offset++;
            }
            offset = 1;
            while (canWeKeepMoving(-offset, -offset, currentMove))
            {
                offset++;
            }
        }
        private bool canWeKeepMoving(int columnVector, int rowVector, pieceMoveInformation currentMove)
        {
            int col = currentMove.startCol + columnVector;
            int row = currentMove.startRow + rowVector;

            if (col >= 0 && col <= 7 && row >= 0 && row <= 7)
            {
                Control moveTest = gridTLP.GetControlFromPosition(col, row);
                string destinationTag = (string)moveTest.Tag;
                string firstLetterOfDestinationTag = destinationTag.Substring(0, 1);

                if (firstLetterOfDestinationTag == "e")             //empty cell
                {
                    if (!(currentMove.pieceType == "Pawn" && columnVector != 0))    //edge case where pawn cannot move diagonal into empty space
                    {
                        possibleMoves++;
                        moveTest.BackColor = System.Drawing.Color.Pink;
                        Control startSquare = gridTLP.GetControlFromPosition(currentMove.startCol, currentMove.startRow);
                        string startSquareTitle = (string)startSquare.Tag;
                        if (startSquareTitle == "wKing")
                        {
                            wKingCol = col;
                            wKingRow = row;
                        }
                        else if (startSquareTitle == "bKing")
                        {
                            bKingCol = col;
                            bKingRow = row;
                        }

                        string endSquareTitle = (string)moveTest.Tag;
                        moveTest.Tag = startSquare.Tag;
                        startSquare.Tag = "empty";
                        testForCheck();
                        startSquare.Tag = moveTest.Tag;
                        moveTest.Tag = endSquareTitle;
                        if ((string)startSquare.Tag == "wKing")
                        {
                            wKingCol = currentMove.startCol;
                            wKingRow = currentMove.startRow;
                        }
                        else if ((string)startSquare.Tag == "bKing")
                        {
                            bKingCol = currentMove.startCol;
                            bKingRow = currentMove.startRow;
                        }
                        if (currentMove.pieceType == "King")
                        {
                            return false;           //edge case where King can only move one unit
                        }
                        return true;                //signals can may be able to move further in this direction if the piece has that ability
                    }
                }
                else if (firstLetterOfDestinationTag != currentMove.pieceColor)
                {
                    if ((currentMove.pieceType == "Pawn" && columnVector == 0))    //edge case where pawn cannot move forward to take piece
                    {

                        return false;                //signals can may be able to move further in this direction if the piece has that ability
                    }
                    else
                    {
                        possibleMoves++;
                        moveTest.BackColor = System.Drawing.Color.Pink;
                        Control startSquare = gridTLP.GetControlFromPosition(currentMove.startCol, currentMove.startRow);

                        string startSquareTitle = (string)startSquare.Tag;
                        if (startSquareTitle == "wKing")
                        {
                            wKingCol = col;
                            wKingRow = row;
                        }
                        else if (startSquareTitle == "bKing")
                        {
                            bKingCol = col;
                            bKingRow = row;
                        }
                        string endSquareTitle = (string)moveTest.Tag;
                        moveTest.Tag = startSquare.Tag;
                        startSquare.Tag = "empty";
                        testForCheck();
                        startSquare.Tag = moveTest.Tag;
                        moveTest.Tag = endSquareTitle;
                        if ((string)startSquare.Tag == "wKing")
                        {
                            wKingCol = currentMove.startCol;
                            wKingRow = currentMove.startRow;
                        }
                        else if ((string)startSquare.Tag == "bKing")
                        {
                            bKingCol = currentMove.startCol;
                            bKingRow = currentMove.startRow;
                        }
                    }
                }
            }
            return false;
        }

        private void testForCheck()
        {
            Control kingToCheck = gridTLP.GetControlFromPosition(wKingCol, wKingRow);

            if (!playerOneTurn)
            {
                kingToCheck = gridTLP.GetControlFromPosition(bKingCol, bKingRow);
            }
            int startRow = gridTLP.GetRow(kingToCheck);
            int startCol = gridTLP.GetColumn(kingToCheck);
            string pieceTitle = (string)kingToCheck.Tag;
            string pieceColor = pieceTitle.Substring(0, 1);
            string pieceType = pieceTitle.Substring(1, pieceTitle.Length - 1);
            pieceMoveInformation currentPiece = new pieceMoveInformation(startCol, startRow, pieceTitle, pieceType, pieceColor);
            checkSearch(currentPiece);
        }

        private void checkSearch(pieceMoveInformation currentMove)
        {
            //explore moving Orthogonally
            int offset = 1;
            while (!foundCheck && exploreSquares(offset, 0, currentMove, "Ortho"))
            {
                offset++;
            }
            offset = -1;
            while (!foundCheck &&  exploreSquares(offset, 0, currentMove, "Ortho"))
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
            while (!foundCheck && exploreSquares(offset, offset, currentMove, "Diag")) 
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
                    exploreSquares(rowVector[i], columnVector[i], currentMove, "Knight");
                }
                else { i = 8; }
            }

            //explore pawn moves
            if (!foundCheck)
            {
                if (currentMove.pieceColor == "w")
                {
                    exploreSquares(-1, -1, currentMove, "Pawn");
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

        private bool exploreSquares(int columnVector, int rowVector, pieceMoveInformation currentMove, string attackVulnerableTo)
        {

            int col = currentMove.startCol + columnVector;
            int row = currentMove.startRow + rowVector;

            if (col >= 0 && col <= 7 && row >= 0 && row <= 7)
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

        private void disableControls()
        {
            foreach (Control c in gridTLP.Controls)
            {
                if (c is PictureBox)
                {
                    if ((string)c.Tag == "empty")
                    {
                        c.Enabled = true;
                    }
                }
            }
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
            possibleMoves = 0;
            selectAllPieces();
            unHighlightMoves();
            if (movesThatCauseCheck == possibleMoves)
            {

                MessageBox.Show("checkMate");
            }
            movesThatCauseCheck = 0;
            possibleMoves = 0;
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
