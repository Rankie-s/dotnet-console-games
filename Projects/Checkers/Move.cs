namespace Checkers;

public class Move
{
	public Piece PieceToMove { get; set; } // get the piece that will move

	public (int X, int Y) To { get; set; } // get the loc where the piece will move TO
	// the name of (int X, int Y) is "To"

	// it cannot be like this. Because this Move is also used to see the posibilties of move.
	/*
	private (int X, int Y) _to;
	public (int X, int Y) To
	{
		get => _to;
		set 
		{
			_to = value;
			(PieceToMove.X, PieceToMove.Y) = _to; // or set => (PieceToMove.X, PieceToMove.Y) = _to = value
		} 
	}
	*/

	public Piece? PieceToCapture { get; set; } // get the piece that the last piece captures. 
	// May be a null if that piece moves to an empty space

	public Move(Piece pieceToMove, (int X, int Y) to, Piece? pieceToCapture = null)
	{
		PieceToMove = pieceToMove;
		To = to;
		PieceToCapture = pieceToCapture;
	}
}
