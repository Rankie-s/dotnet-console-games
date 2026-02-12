namespace Checkers;

public class Game
{
	private const int PiecesPerColor = 12;

	public PieceColor Turn { get; private set; }
	public Board Board { get; }
	public PieceColor? Winner { get; private set; } // when null, no one wins yet
	public List<Player> Players { get; }

	public Game(int humanPlayerCount)
	{
		// the input step guarantees it can't be <0 or >2. But this is needed for some reason?
		if (humanPlayerCount < 0 || 2 < humanPlayerCount) throw new ArgumentOutOfRangeException(nameof(humanPlayerCount));
		Board = new Board();
		Players = new()
		{
			new Player(humanPlayerCount >= 1, Black), // if 0 player, this will be (false, Black). if 1 => (true, Black)
			new Player(humanPlayerCount >= 2, White),
		};
		Turn = Black; // when init, black goes first
		Winner = null;
	}

	public void PerformMove(Move move)
	{
		// the loc of the piece that needs to be moved is moved here
		// it doesn't need to worry about out of boundary because the curser cannot be out
		(move.PieceToMove.X, move.PieceToMove.Y) = move.To;
		if ((move.PieceToMove.Color is Black && move.To.Y is 7) ||
			(move.PieceToMove.Color is White && move.To.Y is 0))
		{
			move.PieceToMove.Promoted = true; // get to the bottom, PROMOT!
		}
		if (move.PieceToCapture is not null) // the destination has an enemy piece
		{
			Board.Pieces.Remove(move.PieceToCapture); // remove the piece from the list
		}
		if (move.PieceToCapture is not null && // if it captures a piece and has another piece to capture, it is an Aggressor
			Board.GetPossibleMoves(move.PieceToMove).Any(m => m.PieceToCapture is not null))
		{
			Board.Aggressor = move.PieceToMove; // when it is an Aggressor, the turn remains the same.
		}
		else // else, change turn
		{
			Board.Aggressor = null;
			Turn = Turn is Black ? White : Black;
		}
		CheckForWinner();
	}

	public void CheckForWinner() // all pieces left are from one side, this side wins
	{
		if (!Board.Pieces.Any(piece => piece.Color is Black))
		{
			Winner = White;
		}
		if (!Board.Pieces.Any(piece => piece.Color is White))
		{
			Winner = Black;
		}
		if (Winner is null && Board.GetPossibleMoves(Turn).Count is 0)
		{
			Winner = Turn is Black ? White : Black;
		}
	}

	public int TakenCount(PieceColor colour) =>
		PiecesPerColor - Board.Pieces.Count(piece => piece.Color == colour);
}
