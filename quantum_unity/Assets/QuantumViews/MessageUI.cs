using Quantum;
using TMPro;
using UnityEngine;

public class MessageUI : QuantumCallbacks
{
    public TMP_Text Messages;
    public TMP_Text Score;

    public void Start()
    {
        QuantumEvent.Subscribe<EventDeliveredSupply>(this, HandleDelivery);
        QuantumEvent.Subscribe<EventPickedSupply>(this, HandlePickup);
        QuantumEvent.Subscribe<EventScoreDisconnect>(this, HandleScoreDisonnect);
        QuantumEvent.Subscribe<EventScoreReconnect>(this, HandleScoreReconnect);
    }

    public override void OnGameStart(QuantumGame game)
    {
        UpdateScore(game);
    }

    public override void OnGameResync(QuantumGame game)
    {
        // late join init UI
        UpdateScore(game);
    }

    public void HandleDelivery(EventDeliveredSupply e)
    {
        Messages.text = "Great! Now pick and bring another.";
    }
    
    public void HandlePickup(EventPickedSupply e)
    {
        Messages.text = "Deliver to the cottage, be careful.";
    }
    
    public void HandleScoreReconnect(EventScoreReconnect e)
    {
        Messages.text = "Reconnect team scored!";
        UpdateScore(e.Game);
    }
    
    public void HandleScoreDisonnect(EventScoreDisconnect e)
    {
        Messages.text = "Disconnect team scored!";
        UpdateScore(e.Game);
    }

    private void UpdateScore(QuantumGame game)
    {
        var frame = game.Frames.Verified;
        var score = frame.GetSingleton<Score>();
        Score.text = score.TeamReconnect + " x " + score.TeamDisconnect;
    }
}
