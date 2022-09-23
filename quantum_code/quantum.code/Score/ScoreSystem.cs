namespace Quantum
{
  public unsafe class ScoreSystem : SystemSignalsOnly, ISignalScoreReconnect, ISignalScoreDisconnect
  {
    public void ScoreReconnect(Frame f)
    {
      if (f.Unsafe.TryGetPointerSingleton<Score>(out var score))
      {
        score->TeamReconnect++;
        f.Events.ScoreReconnect(default);
      }
    }

    public void ScoreDisconnect(Frame f)
    {
      if (f.Unsafe.TryGetPointerSingleton<Score>(out var score))
      {
        score->TeamDisconnect++;
        f.Events.ScoreDisconnect(default);
      }
    }
  }
}