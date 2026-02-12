namespace Checkers;

public class Piece // create a class of piece with its loc, color and if Promoted
{
	public int X { get; set; }

	public int Y { get; set; }

	public string NotationPosition // when set, piece pos like A2 will be transfered to (0, 1)
	{
		get => Board.ToPositionNotationString(X, Y); // get a string like A2 or D6, and restore them in X and Y
		set => (X, Y) = Board.ParsePositionNotation(value); // read X and Y and set them to a string
	}

	public PieceColor Color { get; init; } // init only, readonly after.

	public bool Promoted { get; set; }
}
