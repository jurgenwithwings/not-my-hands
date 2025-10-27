using System;
using Stats;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.Serialization;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Get Statboard Value", story: "Get [Stat] from [Statboard] and assign to [Variable]", category: "Action", id: "13ff72b21f554bfdf3ffb9b94060f962")]
public partial class GetStatboardValueAction : Action
{
    [SerializeReference] public BlackboardVariable<Statboard.VariableType> Stat;
    [SerializeReference] public BlackboardVariable<Statboard> Statboard;
    [SerializeReference] public BlackboardVariable<float> Variable;

    protected override Status OnStart() {
        Stat stat = Statboard.Value.GetStatByEnum(Stat.Value);
        if (stat != null) {
            Variable.Value = stat.Value;
        }
        return Status.Running;
    }
}

