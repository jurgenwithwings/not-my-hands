using System;
using System.Collections.Generic;
using UnityEngine;

public class RelicManager : MonoBehaviour, IStatboard
{
    public Statboard statboard { get; set; }

    public List<Relic> relics { get; private set; } = new();
    
    public void AddRelic(ClassReference<Relic> type, RelicData data, int amount = 1) {
        if (GetRelicFromList(type, out Relic relic)) {
            relic.AddStack(amount);
        }
        else {
            relic = type.CreateInstance();
            if (relic != null) {
                relic.Initialise(this, data);
                relic.AddStack(amount);
                relics.Add(relic);
            }
        }
    }

    public void RemoveRelic(ClassReference<Relic> type, int amountToRemove = 0) {
        if (GetRelicFromList(type, out Relic relic)) {
            relic.Remove();
            relics.Remove(relic);
        }
    }

    private void Update() {
        for (int i = relics.Count - 1; i >= 0; i--) {
            relics[i].Tick();
        }
    }

    private bool GetRelicFromList(ClassReference<Relic> type, out Relic relic) {
        relic = relics.Find(e => e.GetType() == type);
        return relic != null;
    }
}
