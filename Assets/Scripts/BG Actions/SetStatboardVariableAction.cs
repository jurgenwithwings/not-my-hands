using System;
using Stats;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.Serialization;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set Statboard Variable", story: "Set [Stat] to [Value] on [Statboard]", category: "Action", id: "e8908b3bc502dcaa902bf9340fbe1aa8")]
public partial class SetStatboardVariableAction : Action
{
    [SerializeReference] public BlackboardVariable<Statboard.VariableType> Stat;
    [SerializeReference] public BlackboardVariable<float> Value;
    [SerializeReference] public BlackboardVariable<Statboard> Statboard;

    protected override Status OnStart() {
        Stat stat = Statboard.Value.GetStatByEnum(Stat.Value);
        if (stat != null) {
            stat.SetBaseValue(Value.Value);
        }
        return Status.Success;
    }
}