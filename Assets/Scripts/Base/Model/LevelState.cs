using System.Collections.Generic;

public class LevelState : Observable
{
    public int score;
    public int levelNumber;
    public int[] starValues;
    public int remainingBubbles;
    public BubbleQueue bubbleQueue;
    public Dictionary<BubbleType, int> typeTotals = new Dictionary<BubbleType, int>();

    public LevelState()
    {
        // TODO - Come back and config this
        bubbleQueue = new WeightedBubbleQueue(this);
    }

    public void UpdateTypeTotals(BubbleType type, int delta)
    {
        typeTotals[type] = typeTotals.ContainsKey(type) ? typeTotals[type] + delta : delta;
        NotifyListeners();
    }
}
