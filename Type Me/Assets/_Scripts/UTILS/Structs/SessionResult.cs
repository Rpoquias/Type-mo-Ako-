using System;

[Serializable]
public struct SessionResult
{
    public int score;
    public float timeLasted;
    public int highScore;
    public float bestTime;

    public SessionResult(int score, float timeLasted, int highScore, float bestTime)
    {
        this.score = score;
        this.timeLasted = timeLasted;
        this.highScore = highScore;
        this.bestTime = bestTime;
    }
}
