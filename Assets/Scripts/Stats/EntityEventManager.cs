using System;
using UnityEngine;

public class EntityEventManager : MonoBehaviour, IStatboard
{
    public Statboard statboard { get; set; }
    
    /// <summary>
    /// Called when this entity takes damage.
    /// <para><c>DamageInfo</c> - The damage that was taken</para>
    /// </summary>
    public Action<DamageInfo> OnDamageTaken;

    /// <summary>
    /// Called when a Health component has received damage from this entity.
    /// <para><c>DamageInfo</c> - The Damage that was received</para>
    /// <para><c>Statboard</c> - Victim's statboard</para>
    /// </summary>
    public Action<DamageInfo, Statboard> OnReceivedYourDamage;
    
    /// <summary>
    /// Called before damage is sent from this entity to another.
    /// <para><c>DamageInfo</c> - DamageInfo to be modified</para>
    /// <para><c>Statboard</c> - Victim's statboard</para>
    /// <para><c>Statboard</c> - Attacker's statboard</para>
    /// </summary>
    public Action<DamageInfo, Statboard, Statboard> OnPreSendDamage;

    /// <summary>
    /// Called when mana is successfully changed. Providing the amount the mana was changed by.
    /// <para><c>ManaSpent</c> - The amount of mana spent or gained. Negative if spent.</para>
    /// </summary>
    public Action<float> OnManaChanged;
}
