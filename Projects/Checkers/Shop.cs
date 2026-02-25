namespace Checkers;

public class Shop
{
    public int gold { get; private set; }

    // It should be a dictionary! But its too late for me to realize it
    public (char buyKey, string useKey, string item, int cost, int currentNum)[] ItemList = new (char, string, string, int, int)[]
    {
        ('W', "Space", "a chance to Undo", 10, 0),
        ('A', "S",     "a Bomb",        20, 0),
    };
    public (string way, int earn)[] waysToEarnGold = new(string way, int earn)[]
    {
        ("You capture a token", 10),
        ("You turn a king", 10),
        ("Computer captures a token", 5),
        ("Computer turns a king", 5),
    };
    Dictionary<Enum, int> ChangeGoldDictionary; // GetGold and SpendGold are both Enum
    public Shop()
    {
        gold = 100; // when init, gold is 0
        ChangeGoldDictionary = new() // how many gold each ways get
        {
            {GetGold.PlayerCapture,     waysToEarnGold[0].earn},
            {GetGold.PlayerBecomeKing,  waysToEarnGold[1].earn},
            {GetGold.EnemyCapture,      waysToEarnGold[2].earn},
            {GetGold.EnemyBecomeKing,   waysToEarnGold[3].earn},
            {SpendGold.BuyUndoChance,   ItemList[0].cost},
            {SpendGold.BuyBomb,         ItemList[1].cost},
        };
    }

    // I was going to use Generics to solve it. But I then realize it is a Polymorphism question.
    public void ChangeGold(GetGold way)
    {
        if (ChangeGoldDictionary.TryGetValue(way, out int amount)) gold += amount;
    }
    bool ChangeGold(SpendGold way)
    {
        if (ChangeGoldDictionary.TryGetValue(way, out int amount)) 
        {
            if ( gold >= amount) 
            {
                gold -= amount;
                return true;
            }
        }
        return false;
    }

    public void BuyItem(ConsoleKey key)
    {
        switch(key)
        {
            case ConsoleKey.W: if(ChangeGold(SpendGold.BuyUndoChance)) ItemList[0].currentNum++; break; // if press w and have money, buy an undo chance
            case ConsoleKey.A: if(ChangeGold(SpendGold.BuyBomb))       ItemList[1].currentNum++; break;
            default: return;
        }
    }

    public void ApplyItemEffect(UseItem item, Game game)
    {
        // I had intended to organize all item use here but failed
        if(item is UseItem.Reset) game.HistoryRecorder?.Reset(game);
    }
    public void ApplyItemEffect(UseItem item, Game game, (int X, int Y)? target)
    {
        if (item is UseItem.Bomb && target.HasValue) UseBomb(game, target.Value); // this target can be null, so it has to use target.Value to get .X and .Y
    }
    
    private void UseBomb(Game game, (int x, int y) cursor)
    {
        // check if bomb is enough
        if (ItemList[1].currentNum <= 0) return;

        // check the location of cursor has an enemy's piece
        Piece? enemyPiece = game.Board[cursor.x, cursor.y];
        if (enemyPiece is null || enemyPiece.Color == Black) return;

        game.Board.Pieces.Remove(enemyPiece); // remove this piece
        ItemList[1].currentNum--; // consumne one item
        game.CheckForWinner(); // the player may win by removing this piece
    }
}

public enum GetGold // ways to get gold
{
    PlayerCapture,
    EnemyCapture,
    PlayerBecomeKing,
    EnemyBecomeKing,
}

public enum SpendGold // ways to spend gold
{
    BuyUndoChance,
    BuyBomb,
}

public enum UseItem
{
    Undo,
    Reset,
    Bomb,
}