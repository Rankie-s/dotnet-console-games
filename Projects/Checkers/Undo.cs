namespace Checkers;

// known bug: if continue undo, 

public class UndoManager
{
    public List<List<Piece>> History {get; set;} // restore the history of the board every time the player's round begins (including first turn).

    public UndoManager()
    {
        History = new();
    }

    // method1: add the board to history list 
    // trigger: every time the player's turn begins
    public void RecordBoard(Board board)
    {
        List<Piece> pieceOnBoard = new List<Piece>(); // restore all pieces on board
        // can't just add the whole list to the history list. it will add a copy of ref.

        // now I'm confused: this p in foreach() is the copy of value or ref? I believe it's value, but I won't take risk, so I create a new Piece()
        foreach (Piece p in board.Pieces)
        {
            var piece = new Piece()
            {
                X = p.X, 
                Y = p.Y, 
                Color = p.Color, 
                Promoted = p.Promoted
            };
            pieceOnBoard.Add(piece); // add all pieces to the pieceOnBoard
        }

        History.Add(pieceOnBoard); // then add this board to the history list
    }

    // method2: every time the Undo is called, delete the last value in history list and render the new last one
    // trigger: when the player press the certain button
    public void Undo(Game game)
    {
        if(History.Count <= 1) return; // can't undo the first step
        if(game.GameShop!.ItemList[0].currentNum <= 0) return;

        // remove the last history count
        History.RemoveAt(History.Count - 1);

        // get the new last data.
        List<Piece> lastSavedState = History[History.Count - 1];

        // clear all pieces on board and create a new board with pervious pieces.
        game.Board.Pieces.Clear();
        foreach (Piece p in lastSavedState)
        {
            // again, I don't want to take risk when handling the copy of value and ref.
            Piece piece = new()
            {
                X = p.X,
                Y = p.Y,
                Color = p.Color,
                Promoted = p.Promoted
            };
            game.Board.Pieces.Add(piece); // create the new pieces on board.
        }

        game.GameShop!.ItemList[0].currentNum--; // Sub one Undo chance from itemlist
        // Aggressor should be cleaned
        // Bug: I can't read the last Aggressor now. Maybe I should add the aggressor data to history list?
        game.Board.Aggressor = null;

    }
    public void Reset(Game game)
    {
        // no reset at the start
        if (History.Count == 0) return;

        List<Piece> initState = History[0]; // init state of the board

        // clear all history and leave init only. delete all then add the first should be quicker
        History.Clear();
        History.Add(initState);

        // reset the board like  Undo
        game.Board.Pieces.Clear();
        foreach (Piece p in initState)
        {
            Piece piece = new()
            {
                X = p.X,
                Y = p.Y,
                Color = p.Color,
                Promoted = p.Promoted
            };
            game.Board.Pieces.Add(piece);
        }

        game.Board.Aggressor = null;
    }
}