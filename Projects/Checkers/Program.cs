using System.ComponentModel;

Exception? exception = null;

Encoding encoding = Console.OutputEncoding;

try
{
	Console.OutputEncoding = Encoding.UTF8;
	Game game = ShowIntroScreenAndGetOption();
	Console.Clear();
	RunGameLoop(game);
	RenderGameState(game, promptPressKey: true); // the same as RenderGameState(game, true) but much clearer
	Console.ReadKey(true);
}
catch (Exception e)
{
	exception = e;
	throw;
}
finally
{
	Console.OutputEncoding = encoding;
	Console.CursorVisible = true;
	Console.Clear();
	Console.WriteLine(exception?.ToString() ?? "Checkers was closed.");
}

// choose num of players
Game ShowIntroScreenAndGetOption()
{
	Console.Clear();
	Console.WriteLine();
	Console.WriteLine("  Checkers");
	Console.WriteLine();
	Console.WriteLine("  Checkers is played on an 8x8 board between two sides commonly known as black");
	Console.WriteLine("  and white. The objective is simple - capture all your opponent's pieces. An");
	Console.WriteLine("  alternative way to win is to trap your opponent so that they have no valid");
	Console.WriteLine("  moves left.");
	Console.WriteLine();
	Console.WriteLine("  Black starts first and players take it in turns to move their pieces forward");
	Console.WriteLine("  across the board diagonally. Should a piece reach the other side of the board");
	Console.WriteLine("  the piece becomes a king and can then move diagonally backwards as well as");
	Console.WriteLine("  forwards.");
	Console.WriteLine();
	Console.WriteLine("  Pieces are captured by jumping over them diagonally. More than one enemy piece");
	Console.WriteLine("  can be captured in the same turn by the same piece. If you can capture a piece");
	Console.WriteLine("  you must capture a piece.");
	Console.WriteLine();
	Console.WriteLine("  Moves are selected with the arrow keys. Use the [enter] button to select the");
	Console.WriteLine("  from and to squares. Invalid moves are ignored.");
	Console.WriteLine();
	Console.WriteLine("  Press a number key to choose number of human players:");
	Console.WriteLine("    [0] Black (computer) vs White (computer)");
	Console.WriteLine("    [1] Black (human) vs White (computer)");
	Console.Write("    [2] Black (human) vs White (human)");

	int? humanPlayerCount = null;
	while (humanPlayerCount is null) // check keyPress. If press a valid num, humanPlayerCount will not be a null any more
	{
		Console.CursorVisible = false;
		switch (Console.ReadKey(true).Key)
		{
			case ConsoleKey.D0 or ConsoleKey.NumPad0: humanPlayerCount = 0; break;
			case ConsoleKey.D1 or ConsoleKey.NumPad1: humanPlayerCount = 1; break;
			case ConsoleKey.D2 or ConsoleKey.NumPad2: humanPlayerCount = 2; break;
		}
	}
	return new Game(humanPlayerCount.Value);
}

void RunGameLoop(Game game)
{
	// if(game.HistoryRecorder is not null) game.HistoryRecorder.RecordBoard(game.Board); // record the board when start
	// game.HistoryRecorder?.RecordBoard(game.Board);
	// It doesn't need to do a record here, but I keep it because it showcases a better way to judge null.

	game.HistoryRecorder?.RecordBoard(game.Board); // record the board at the beginning
	while (game.Winner is null) // no one wins, continue the loop
	{
		Player currentPlayer = game.Players.First(player => player.Color == game.Turn);
		if (currentPlayer.IsHuman) // human move
		{
			while (game.Turn == currentPlayer.Color)
			{
				
				(int X, int Y)? selectionStart = null;
				(int X, int Y)? from = game.Board.Aggressor is not null ? (game.Board.Aggressor.X, game.Board.Aggressor.Y) : null;
				UseItem? item = null; // which item to use? default is null
				List<Move> moves = game.Board.GetPossibleMoves(game.Turn);
				// if there is a piece that must move(see Move.cs), you can only move it first.
				// bug: if there are two, the reminder has some bug
				if (moves.Select(move => move.PieceToMove).Distinct().Count() is 1) // don't want to learn LINQ for now
				{
					// select the piece for you
					Move must = moves.First();
					from = (must.PieceToMove.X, must.PieceToMove.Y);
					selectionStart = must.To;
				}
				while (from is null) // select the piece first
				{
					(item, from) = HumanMoveSelection(game);
					selectionStart = from;
					if(item is UseItem.Bomb)
					{
						game.GameShop?.ApplyItemEffect(UseItem.Bomb, game, from);
						from = null;
						item = null;
					}
					if(item is UseItem.Undo && game.HistoryRecorder is not null) // if choose undo, undo
					{
						game.HistoryRecorder.Undo(game);
						from = null;
						item = null;
					}
				}
				(item,(int X, int Y)? to) = HumanMoveSelection(game, selectionStart: selectionStart, from: from); // move the selected piece
				Piece? piece = null;
				piece = game.Board[from.Value.X, from.Value.Y]; // get the chosen piece

				if(item is UseItem.Undo && game.HistoryRecorder is not null) // if choose undo after choosing and before placing the piece, undo
				{
					game.HistoryRecorder.Undo(game);
					from = null;
					to = null;
					item = null;
				}

				if (piece is null || piece.Color != game.Turn) // no piece or wrong color: get back
				{
					from = null;
					to = null;
				}
				if (from is not null && to is not null) // has chosen piece and valid destination?
				{
					Move? move = game.Board.ValidateMove(game.Turn, from.Value, to.Value);
					if (move is not null && // can move?
						(game.Board.Aggressor is null || move.PieceToMove == game.Board.Aggressor))
					{
						game.PerformMove(move); // MOVE!
					}
				}
			}
		}
		else // computer move
		{
			List<Move> moves = game.Board.GetPossibleMoves(game.Turn);
			List<Move> captures = moves.Where(move => move.PieceToCapture is not null).ToList();
			if (captures.Count > 0) // can capture? if can, capture first
			{
				game.PerformMove(captures[Random.Shared.Next(captures.Count)]);
			}
			// can get closer to the king? if can, get closer
			else if(!game.Board.Pieces.Any(piece => piece.Color == game.Turn && !piece.Promoted))
			{
				var (a, b) = game.Board.GetClosestRivalPieces(game.Turn);
				Move? priorityMove = moves.FirstOrDefault(move => move.PieceToMove == a && Board.IsTowards(move, b));
				game.PerformMove(priorityMove ?? moves[Random.Shared.Next(moves.Count)]);
			}
			else // randomly move
			{
				game.PerformMove(moves[Random.Shared.Next(moves.Count)]);
			}
			game.HistoryRecorder?.RecordBoard(game.Board); // record the board before the player moves (after the computer move)
			// to avoid invaild move by player
		}

		RenderGameState(game, playerMoved: currentPlayer, promptPressKey: true);
		Console.ReadKey(true);
	}
}

// render the gameboard
void RenderGameState(Game game, Player? playerMoved = null, (int X, int Y)? selection = null, (int X, int Y)? from = null, bool promptPressKey = false)
{
	const char BlackPiece = '○';
	const char BlackKing  = '☺';
	const char WhitePiece = '◙';
	const char WhiteKing  = '☻';
	const char Vacant     = '·';

	Console.CursorVisible = false;
	Console.SetCursorPosition(0, 0);
	StringBuilder sb = new();
	sb.AppendLine();
	sb.AppendLine("  Checkers");
	sb.AppendLine();
	sb.AppendLine($"    ╔═══════════════════╗");
	sb.AppendLine($"  8 ║  {B(0, 7)} {B(1, 7)} {B(2, 7)} {B(3, 7)} {B(4, 7)} {B(5, 7)} {B(6, 7)} {B(7, 7)}  ║ {BlackPiece} = Black");
	sb.AppendLine($"  7 ║  {B(0, 6)} {B(1, 6)} {B(2, 6)} {B(3, 6)} {B(4, 6)} {B(5, 6)} {B(6, 6)} {B(7, 6)}  ║ {BlackKing} = Black King");
	sb.AppendLine($"  6 ║  {B(0, 5)} {B(1, 5)} {B(2, 5)} {B(3, 5)} {B(4, 5)} {B(5, 5)} {B(6, 5)} {B(7, 5)}  ║ {WhitePiece} = White");
	sb.AppendLine($"  5 ║  {B(0, 4)} {B(1, 4)} {B(2, 4)} {B(3, 4)} {B(4, 4)} {B(5, 4)} {B(6, 4)} {B(7, 4)}  ║ {WhiteKing} = White King");
	sb.AppendLine($"  4 ║  {B(0, 3)} {B(1, 3)} {B(2, 3)} {B(3, 3)} {B(4, 3)} {B(5, 3)} {B(6, 3)} {B(7, 3)}  ║");
	sb.AppendLine($"  3 ║  {B(0, 2)} {B(1, 2)} {B(2, 2)} {B(3, 2)} {B(4, 2)} {B(5, 2)} {B(6, 2)} {B(7, 2)}  ║ Taken:");
	sb.AppendLine($"  2 ║  {B(0, 1)} {B(1, 1)} {B(2, 1)} {B(3, 1)} {B(4, 1)} {B(5, 1)} {B(6, 1)} {B(7, 1)}  ║ {game.TakenCount(White),2} x {WhitePiece}");
	sb.AppendLine($"  1 ║  {B(0, 0)} {B(1, 0)} {B(2, 0)} {B(3, 0)} {B(4, 0)} {B(5, 0)} {B(6, 0)} {B(7, 0)}  ║ {game.TakenCount(Black),2} x {BlackPiece}");
	sb.AppendLine($"    ╚═══════════════════╝");
	sb.AppendLine($"       A B C D E F G H");
	sb.AppendLine($"       Press 'Q' to see hint.");
	if(game.GameShop is not null) // print shop in pve mode
	{
		(char buyKey, string useKey, string itemName, int cost, int currentNum)[] items = game.GameShop.ItemList;
		string GoldLeft = game.GameShop.gold.ToString();
		for(int i= 0; i < items.Length; i++) // show what you can buy (shop)
		{
			(char buyKey, string useKey, string itemName, int cost, int currentNum) item = items[i];
			sb.AppendLine($"     press {item.buyKey} to spend {item.cost} gold to buy {item.itemName}.");
		}
		sb.AppendLine($"     You have {GoldLeft} gold left");
		sb.AppendLine($"     Items you have now:");
		for(int i= 0; i < items.Length; i++) // show items you have now
		{
			(char buyKey, string useKey, string itemName, int cost, int currentNum) item = items[i];
			sb.AppendLine($"         You have {item.currentNum} of {item.itemName} now. Press {item.useKey} to use it");
		}

		sb.AppendLine($"     Ways to earn gold:");
		foreach((string way, int earn) earn in game.GameShop.waysToEarnGold) // show how you get gold
			{
				sb.AppendLine($"         {earn.way} : {earn.earn} gold");
			}
	}
	sb.AppendLine();
	if (selection is not null)
	{
		sb.Replace(" $ ", $"[{ToChar(game.Board[selection.Value.X, selection.Value.Y])}]");
	}
	if (from is not null)
	{
		char fromChar = ToChar(game.Board[from.Value.X, from.Value.Y]);
		sb.Replace(" @ ", $"<{fromChar}>");
		sb.Replace("@ ",  $"{fromChar}>");
		sb.Replace(" @",  $"<{fromChar}");
	}
	PieceColor? wc = game.Winner;
	PieceColor? mc = playerMoved?.Color;
	PieceColor? tc = game.Turn;
	// Note: these strings need to match in length
	// so they overwrite each other.
	string w = $"  *** {wc} wins ***";
	string m = $"  {mc} moved       ";
	string t = $"  {tc}'s turn      ";
	sb.AppendLine(
		game.Winner is not null ? w :
		playerMoved is not null ? m :
		t);
	string p = "  Press any key to continue...";
	string s = "                              ";
	sb.AppendLine(promptPressKey ? p : s);
	Console.Write(sb);

	char B(int x, int y) =>
		(x, y) == selection ? '$' :
		(x, y) == from ? '@' :
		ToChar(game.Board[x, y]);

	static char ToChar(Piece? piece) =>
		piece is null ? Vacant :
		(piece.Color, piece.Promoted) switch
		{
			(Black, false) => BlackPiece,
			(Black, true)  => BlackKing,
			(White, false) => WhitePiece,
			(White, true)  => WhiteKing,
			_ => throw new NotImplementedException(),
		};
}

// it needs to return which item to use
// written later: it doesn't anymore, but I'm too confused to modify it now
(UseItem? item, (int X, int Y)?) HumanMoveSelection(Game game, (int X, int y)? selectionStart = null, (int X, int Y)? from = null)
{
    (int X, int Y) selection = selectionStart ?? (3, 3);
    Move? hint = null; 
    while (true)
    {
        // if press Q, render hint instead
        (int X, int Y) renderSelection = hint is not null ? hint.To : selection;
        (int X, int Y)? renderFrom = hint is not null ? (hint.PieceToMove.X, hint.PieceToMove.Y) : from;

        RenderGameState(game, selection: renderSelection, from: renderFrom);

        ConsoleKey key = Console.ReadKey(true).Key;
		if (key is ConsoleKey.UpArrow or ConsoleKey.DownArrow or ConsoleKey.LeftArrow or ConsoleKey.RightArrow)
        {
            hint = null; // clear hint if the player tries to move
        }
        switch (key)
        {
            case ConsoleKey.DownArrow:  selection.Y = Math.Max(0, selection.Y - 1); break;
            case ConsoleKey.UpArrow:    selection.Y = Math.Min(7, selection.Y + 1); break;
            case ConsoleKey.LeftArrow:  selection.X = Math.Max(0, selection.X - 1); break;
            case ConsoleKey.RightArrow: selection.X = Math.Min(7, selection.X + 1); break;
            case ConsoleKey.Enter:      return (null, selection); // choose to move
            case ConsoleKey.Spacebar:   return (UseItem.Undo, null); // use undo
            case ConsoleKey.R:          game.GameShop?.ApplyItemEffect(UseItem.Reset, game); break; // restart
            case ConsoleKey.Escape:     return (null, null); // quit the game
			case ConsoleKey.S:          return (UseItem.Bomb, selection); // use bomb
            case ConsoleKey.Q:
			// render hint if no hint. clear hint if press Q again
                if (hint is null) hint = MoveHint(game, game.Turn);
                else hint = null;
                break;

            default: game.GameShop?.BuyItem(key); break;
        }
    }
}

// return the location of possible player move (from and to) as a hint for player
// use the same logic as computer move
Move? MoveHint(Game game, PieceColor color)
{
	List<Move> moves = game.Board.GetPossibleMoves(color);
	if (moves.Count == 0) return null;
	List<Move> captures = moves.Where(move => move.PieceToCapture is not null).ToList();
	if (captures.Count > 0)
	{
		return captures[Random.Shared.Next(captures.Count)];
	}
	else if(!game.Board.Pieces.Any(piece => piece.Color == color && !piece.Promoted))
	{
		var (a, b) = game.Board.GetClosestRivalPieces(color);
		Move? priorityMove = moves.FirstOrDefault(move => move.PieceToMove == a && Board.IsTowards(move, b));
		return priorityMove ?? moves[Random.Shared.Next(moves.Count)];
	}
	else
	{
		return moves[Random.Shared.Next(moves.Count)];
	}
}