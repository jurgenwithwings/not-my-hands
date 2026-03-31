using System;
using System.Collections.Generic;
using UnityEngine;

public class RelicManager : MonoBehaviour, IStatboard
{
    public Statboard statboard { get; set; }

    public List<Relic> relics { get; private set; } = new();
    
    public void AddRelic(RelicData data, int amount = 1) {
        if (GetRelicFromList(data, out Relic relic)) {
            relic.AddStack(amount);
            statboard.eventManager?.OnRelicAdded?.Invoke(relic.data);
        }
        else {
            Relic newRelic = Activator.CreateInstance(data.Type()) as Relic;
            if (newRelic != null) {
                newRelic.Initialise(this, data);
                newRelic.AddStack(amount);
                relics.Add(newRelic);
                statboard.eventManager?.OnRelicAdded?.Invoke(newRelic.data);
            }
        }
    }

    public bool RemoveRelic(RelicData data) {
        if (GetRelicFromList(data, out Relic relic)) {
            relic.Remove();
            relics.Remove(relic);
            statboard.eventManager?.OnRelicRemoved?.Invoke(relic.data);
            return true;
        }
        return false;
    }

    private void Update() {
        for (int i = relics.Count - 1; i >= 0; i--) {
            relics[i].Update();
        }
    }

    private bool GetRelicFromList(RelicData dataType, out Relic relic) {
        relic = relics.Find(r => r.data == dataType);
        return relic != null;
    }
}
