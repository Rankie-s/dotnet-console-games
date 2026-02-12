namespace Checkers;

public class Player
{
	public bool IsHuman { get; } // can only be init in the constructor
	// It is better to be a property instead of a field if it is public
	public PieceColor Color { get; }

	public Player(bool isHuman, PieceColor color)
	{
		IsHuman = isHuman;
		Color = color;
	}
}
