using System;
using Stats;
using UnityEngine;

[Serializable]
public abstract class Relic {
    protected RelicManager manager;
    public RelicData data { get; private set; }
    
    protected Statboard stats => manager.statboard;

    public int stacks { get; private set; }
    
    public virtual void Initialise(RelicManager relicManager, RelicData relicData) {
        manager = relicManager;
        data = relicData;
    }

    public virtual void AddStack(int amount) {
        stacks += amount;
    }

    public virtual void RemoveStack() {
        stacks--;
        if (stacks <= 0) {
            manager.RemoveRelic(data);
        }
    }

    public virtual void Remove() {
        
    }

    public virtual void Tick() {
        
    }
}

[Serializable] public class WovenEye : Relic {
    private string source;

    public override void AddStack(int amount) {
        base.AddStack(amount);
        ReplaceModifier();
        //Debug.Log("Added Relic");
    }

    public override void RemoveStack() {
        base.RemoveStack();
        if (stacks > 0) {
            ReplaceModifier();
        }
    }

    private void ReplaceModifier() {
        stats.moveSpeed.RemoveAllModifiersFromSource(source);
        PlayerHUDEvents.DebugText($"New Mod Amount: {0.05f * stacks} | Relic Count: {manager.relics.Count}", 3, "Relic");
        stats.moveSpeed.AddModifier(new Modifier(0.05f * stacks, ModifierType.Multiply, source));
    }

    public override void Remove() {
        base.Remove();
        stats.moveSpeed.RemoveAllModifiersFromSource(source);
    }
}
