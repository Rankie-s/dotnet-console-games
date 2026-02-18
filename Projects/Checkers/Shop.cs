namespace Checkers;

public class Shop
{
    public int gold { get; private set; }

    // It should be a dictionary! But its too late for me to realize it
    public (char key, string item, int cost, int currentNum)[] ItemList = new (char, string, int, int)[]
    {
        ('w', "A chance to Undo", 10, 0)
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
            {GetGold.PlayerCapture, waysToEarnGold[0].earn},
            {GetGold.PlayerBecomeKing, waysToEarnGold[1].earn},
            {GetGold.EnemyCapture, waysToEarnGold[2].earn},
            {GetGold.EnemyBecomeKing, waysToEarnGold[3].earn},
            {SpendGold.BuyUndoChance, ItemList[0].cost},
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
            default: return;
        }
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
}