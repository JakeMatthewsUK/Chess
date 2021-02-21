using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        bool checkingForCheckMate = false;
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

        List<Control> hasBlackInCheck = new List<Control>();
        List<Control> hasWhiteInCheck = new List<Control>();

        private void onClick(object sender, EventArgs e)
        {
            if (!firstSelectionMade)
            {
                firstSelection = sender as PictureBox;
                string pieceType = (string)firstSelection.Tag;
                if (playerOneTurn && pieceType[0] == 'w' || !playerOneTurn && pieceType[0] == 'b')  //if they chose one of their pieces
                {
                    copyOfFirstSelection.BackColor = firstSelection.BackColor;
                    copyOfFirstSelection.Image = firstSelection.Image;
                    copyOfFirstSelection.Tag = firstSelection.Tag;
                    firstSelection.BackColor = System.Drawing.Color.Red;    //highlight cell
                    disableControls();
                    focusOnSelection(firstSelection);
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

                    playerOneTurn = !playerOneTurn;

                    if (checkForCheck())
                    {
                        List<System.Drawing.Color> colourList = new List<Color>();

                        if (hasBlackInCheck.Count > 0)
                        {
                            for (int i = 0; i < hasBlackInCheck.Count; i++)
                            {
                                colourList.Add(hasBlackInCheck[i].BackColor);
                                hasBlackInCheck[i].BackColor = System.Drawing.Color.Purple;
                            }
                            MessageBox.Show("Check");

                            for (int i = 0; i < hasBlackInCheck.Count; i++)
                            {
                                hasBlackInCheck[i].BackColor = colourList[i];
                            }
                            colourList.Clear();
                        }
                        if (hasWhiteInCheck.Count > 0)
                        {
                            for (int i = 0; i < hasWhiteInCheck.Count; i++)
                            {
                                colourList.Add(hasWhiteInCheck[i].BackColor);
                                hasWhiteInCheck[i].BackColor = System.Drawing.Color.Purple;
                            }
                            MessageBox.Show("Check");

                            for (int i = 0; i < hasWhiteInCheck.Count; i++)
                            {
                                hasWhiteInCheck[i].BackColor = colourList[i];
                            }
                            colourList.Clear();
                        }

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
                        playerOneTurn = !playerOneTurn;
                   

                
                        //someFunction();
                        hasBlackInCheck.Clear();
                        hasWhiteInCheck.Clear();

                    }

                    disableControls();
                    unHighlightMoves();

                    moveCount++;
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
            }
            Control endRowControl = checkEndRows();

            if (endRowControl != null) { switchtoPromotionMenu(); }


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
        public class pieceMoveInformation
        {
            public int startRow;
            public int startCol;
            public int possibleMoves = 0;
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
        private void focusOnSelection(PictureBox activePicBox)
        {
            int startRow = gridTLP.GetRow(activePicBox);
            int startCol = gridTLP.GetColumn(activePicBox);
            string pieceTitle = (string)activePicBox.Tag;
            string pieceColor = pieceTitle.Substring(0, 1);
            string pieceType = pieceTitle.Substring(1, pieceTitle.Length - 1);
            pieceMoveInformation currentPiece = new pieceMoveInformation(startCol, startRow, pieceTitle, pieceType, pieceColor);
            currentPiece.possibleMoves = 0;
            exploreMoves(currentPiece);
        }
        private void exploreMoves(pieceMoveInformation currentMove)
        {
            switch (currentMove.pieceType)
            {
                case "Pawn":
                    if (currentMove.pieceColor == "w")
                    {
                        if (testAndPaint(0, -1, currentMove))
                        {

                            if (currentMove.startRow == 6)
                            {
                                testAndPaint(0, -2, currentMove);
                            }
                        }
                        testAndPaint(-1, -1, currentMove);
                        testAndPaint(1, -1, currentMove);
                    }
                    else
                    {
                        if (testAndPaint(0, 1, currentMove))
                        {
                            if (currentMove.startRow == 1)
                            {
                                testAndPaint(0, 2, currentMove);
                            }
                        }
                        testAndPaint(-1, 1, currentMove);
                        testAndPaint(1, 1, currentMove);
                    }
                    break;
                case "Knight":
                    for (int i = 0; i < 8; i++)
                    {
                        testAndPaint(rowVector[i], columnVector[i], currentMove);
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
            while (testAndPaint(offset, 0, currentMove))
            {
                offset++;
            }
            offset = -1;
            while (testAndPaint(offset, 0, currentMove))
            {
                offset--;
            }
            offset = 1;
            while (testAndPaint(0, offset, currentMove))
            {
                offset++;
            }
            offset = -1;
            while (testAndPaint(0, offset, currentMove))
            {
                offset--;
            }
        }
        private void testDiagonal(pieceMoveInformation currentMove)
        {
            int offset = 1;
            while (testAndPaint(offset, offset, currentMove))
            {
                offset++;
            }
            offset = 1;
            while (testAndPaint(offset, -offset, currentMove))
            {
                offset++;
            }
            offset = 1;
            while (testAndPaint(-offset, offset, currentMove))
            {
                offset++;
            }
            offset = 1;
            while (testAndPaint(-offset, -offset, currentMove))
            {
                offset++;
            }
        }

        private bool findPiecesThatCauseCheck(int columnVector, int rowVector, pieceMoveInformation currentMove)
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
                        if (currentMove.pieceType == "King")
                        {
                            return false;           //edge case where King can only move one unit
                        }
                        return true;                //signals can may be able to move further in this direction if the piece has that ability
                    }
                }
                else if (firstLetterOfDestinationTag != currentMove.pieceColor)
                {
                    if (!(currentMove.pieceType == "Pawn" && columnVector == 0))    //edge case where pawn cannot move forward to take piece
                    {
                        string destinationPieceType = destinationTag.Substring(1, destinationTag.Length - 1);
                        if (destinationPieceType == "King")
                        {
                            if (currentMove.pieceColor == "b")
                            {
                                if (!playerOneTurn)
                                {
                                    hasWhiteInCheck.Add(gridTLP.GetControlFromPosition(currentMove.startCol, currentMove.startRow));
                                }
                            }
                            else
                            {
                                if (playerOneTurn)
                                {
                                    hasBlackInCheck.Add(gridTLP.GetControlFromPosition(currentMove.startCol, currentMove.startRow));
                                }
                            }
                        }
                        return false;                //signals can may be able to move further in this direction if the piece has that ability
                    }
                }


            }
            return false;
        }


        private bool testAndPaint(int columnVector, int rowVector, pieceMoveInformation currentMove)
        {           //paints cells we can move to, and returns true if a move if further moves in that direction may be possible


            if (checkingIfInCheck)
            {
                return findPiecesThatCauseCheck(columnVector, rowVector, currentMove);
            }


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
                        moveTest.BackColor = System.Drawing.Color.Pink;
                        if (currentMove.pieceType == "King")
                        {
                            return false;           //edge case where King can only move one unit
                        }
                        return true;                //signals can may be able to move further in this direction if the piece has that ability
                    }
                }
                else if (firstLetterOfDestinationTag != currentMove.pieceColor)
                {

                    if (!(currentMove.pieceType == "Pawn" && columnVector == 0))    //edge case where pawn cannot move forward to take piece
                    {
                        moveTest.BackColor = System.Drawing.Color.Pink;
                        return false;                //signals can may be able to move further in this direction if the piece has that ability
                    }
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
            playerOneTurn = !playerOneTurn;
            movesLabel.Visible = true;
            gameTimeLabel.Visible = true;

        }

        private bool checkForCheck()
        {
            checkingIfInCheck = true;

            foreach (Control c in gridTLP.Controls)
            {
                if (c is PictureBox)
                {
                    if ((string)c.Tag != "empty")
                    {
                        focusOnSelection((PictureBox)c);
                    }
                }
            }
            checkingIfInCheck = false;
            if ((playerOneTurn && hasBlackInCheck.Count > 0) || (!playerOneTurn && hasWhiteInCheck.Count > 0))
            {
                return true;
            }
            return false;
        }

        bool playerCanEscape()
        {
                foreach (Control c in gridTLP.Controls)
                {
                    focusOnSelection((PictureBox)c);
                }
            return false;

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
