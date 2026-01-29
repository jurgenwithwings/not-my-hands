using System;
using UnityEngine;

public class EntityEventManager : MonoBehaviour, IStatboard
{
    public Statboard statboard { get; set; }
    
    /// <summary>
    /// Called when this entity takes damage.
    /// <para><c>DamageTaken</c> - The damage that was taken</para>
    /// </summary>
    public Action<DamageInfo> OnDamageTaken;
    
    /// <summary>
    /// Called when this entity heals.
    /// <para><c>healAmount</c> - The amount of health healed</para>
    /// </summary>
    public Action<float> OnHealed;

    /// <summary>
    /// Called when a Health component has received damage from this entity.
    /// <para><c>Damage Taken</c> - The Damage that was received</para>
    /// <para><c>Victim</c> - Victim's statboard</para>
    /// </summary>
    public Action<DamageInfo, Statboard> OnReceivedYourDamage;
    
    /// <summary>
    /// Called before damage is sent from this entity to another.
    /// <para><c>DamageInfo</c> - DamageInfo to be modified</para>
    /// <para><c>Victim</c> - Victim's statboard</para>
    /// <para><c>Attacker</c> - Attacker's statboard</para>
    /// </summary>
    //public Action<DamageInfo, Statboard, Statboard> OnPreSendDamage;
    public PreSendDamage OnPreSendDamage;
    public delegate void PreSendDamage(ref DamageInfo damageInfo, Statboard victim, Statboard self);
    
    /// <summary>
    /// Called before damage is applied to this entity.
    /// <para><c>DamageInfo</c> - DamageInfo to be modified</para>
    /// <para><c>Victim</c> - Victim's statboard</para>
    /// </summary>
    //public Action<DamageInfo, Statboard, Statboard> OnPreSendDamage;
    public PreApplyDamage OnPreApplyDamage;
    public delegate void PreApplyDamage(ref DamageInfo damageInfo, Statboard victim);
    
    /// <summary>
    /// Called when health is changes but not through taking damage.
    /// <para><c>CurrentHealth</c> - The new current amount of health.</para>
    /// <para><c>MaxHealth</c> - The new maximum health.</para>
    /// </summary>
    public Action<float, float> OnHealthChanged;

    /// <summary>
    /// Called when mana is successfully changed. Providing the amount the mana was changed by.
    /// <para><c>ManaSpent</c> - The amount of mana spent or gained. Negative if spent.</para>
    /// </summary>
    public Action<float> OnManaChanged;

    /// <summary>
    /// Called when a new Organ is picked up by this entity.
    /// <para><c>New Organ</c> - The Organ that the entity has picked up.</para>
    /// <para><c>Old Organ</c> - The Organ being dropped for the new one.</para>
    /// </summary>
    public Action<OrganData, OrganData> OnOrganChanged;
    
    /// <summary>
    /// Called when a new Relic is picked up by this entity.
    /// <para><c>New Relic</c> - The Relic that the entity has picked up.</para>
    /// </summary>
    public Action<RelicData> OnRelicAdded;
    
    /// <summary>
    /// Called when a new Limb is picked up by this entity.
    /// <para><c>New Limb</c> - The Limb that the entity has picked up.</para>
    /// <para><c>LimbSide</c> - The side that the new Limb occupies.</para>
    /// <para><c>Old Limb</c> - The Limb being dropped for the new one.</para>
    /// </summary>
    public Action<LimbData, LimbSide, LimbData> OnLimbChanged;

    /// <summary>
    /// Called when this entity kills another.
    /// <para><c>Victim</c> - The entity that was killed.</para>
    /// </summary>
    public Action<Statboard> OnKilledTarget;
}
