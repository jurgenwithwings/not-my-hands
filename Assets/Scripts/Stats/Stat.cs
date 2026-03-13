using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Stats {
    public enum ModifierType {
        /// <summary>
        /// This is a flat amount that is added to the base value. This is very powerful.
        /// </summary>
        [Tooltip("This is a flat amount that is added to the base value. This is very powerful. (eg. +10)")]
        BaseAdd = 100,

        /// <summary>
        /// Most common/general modifier. This pools up one big percentage that is multiplied to the base.
        /// </summary>
        [Tooltip("Most common/general modifier. This pools up one big percentage that is multiplied to the base. (eg +30% = 0.3, -50% = -0.5)")]
        Additive = 200,
        
        /// <summary>
        /// This is a compounding percentage that exponentially increases the base value. Strong effect usually combined with small numbers.
        /// </summary>
        [Tooltip("This is a compounding percentage that exponentially increases the base value. Strong effect usually combined with small numbers. (eg +30% = x1.3, -50% = x0.5)")]
        Multiplicative = 300,
        
        /// <summary>
        /// This pools up one big percentage that is multiplied to the modified value.
        /// </summary>
        [Tooltip("This pools up one big percentage that is multiplied to the modified value. (eg +30% = 0.3, -50% = -0.5)")]
        FinalAdditive = 400,
        
        /// <summary>
        /// This is a compounding multiplier applied to the result of all other modifiers. This is extremely potent and should be used sparingly.
        /// </summary>
        [Tooltip("This is a compounding multiplier applied to the result of all other modifiers. This is extremely potent and should be used sparingly. (eg +30% = x1.3, -50% = x0.5)")]
        FinalMultiplicative = 500,
        
        /// <summary>
        /// This is a final flat value added at the very end after all other types.
        /// </summary>
        [Tooltip("This is a final flat value added at the very end after all other types. (eg. +10)")]
        FinalFlat = 600,
    }

    public struct Modifier : IEquatable<Modifier> {
        public readonly float Value;
        public readonly ModifierType Type;
        public readonly int Order;
        public readonly object Source;

        /// <summary>
        /// A modification to the value of a CharacterStat.
        /// </summary>
        /// <param name="value">Amount the stat is changed</param>
        /// <param name="type">How the value is applied to the base</param>
        /// <param name="source">The thing that applied the stat change</param>
        public Modifier(float value, ModifierType type, object source) {
            Value = value;
            Type = type;
            Order = (int)type;
            Source = source;
        }

        #region IEquality
        public bool Equals(Modifier other) {
            return Value.Equals(other.Value) && Type == other.Type && Order == other.Order && Equals(Source, other.Source);
        }

        public override bool Equals(object obj) {
            return obj is Modifier other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(Value, (int)Type, Order, Source);
        }
        #endregion
    }

    [Serializable] public class Stat {
        public float BaseValue;

        public float Value {
            get {
                if (isDirty || BaseValue != lastBaseValue) {
                    lastBaseValue = BaseValue;
                    value = CalculateFinalValue(BaseValue, modifiers.ToArray());
                    isDirty = false;
                }

                return value;
            }
        }
        
        public int IntValue => Mathf.FloorToInt(Value);

        protected bool isDirty = true;
        protected float value;
        protected float lastBaseValue = float.MinValue;

        protected readonly List<Modifier> modifiers;
        public readonly ReadOnlyCollection<Modifier> Modifiers;
        
        public Action<Stat> OnValueChanged;

        //Create a new stat with no base value
        public Stat() {
            modifiers = new List<Modifier>();
            Modifiers = modifiers.AsReadOnly();
        }

        //Create a new stat with a base value
        public Stat(float baseValue) : this() {
            BaseValue = baseValue;
        }
        
        #region Operators
        //Create a stat from a float implicitly
        public static implicit operator Stat(float value) {
            return new Stat(value);
        }
        
        //Get the float value of a stat implicitly
        public static implicit operator float(Stat stat) {
            return stat.Value;
        }

        //Get the int value of a stat implicitly
        public static implicit operator int(Stat stat) {
            return stat.IntValue;
        }
        #endregion

        public void SetBaseValue(float baseValue) {
            BaseValue = baseValue;
            isDirty = true;
            OnValueChanged?.Invoke(this);
        }
        
        public void AddModifier(Modifier mod) {
            modifiers.Add(mod);
            modifiers.Sort(CompareModifierOrder);
            isDirty = true;
            OnValueChanged?.Invoke(this);
        }
        
        public bool RemoveModifier(Modifier mod) {
            if (modifiers.Remove(mod)) {
                isDirty = true;
                OnValueChanged?.Invoke(this);
                return true;
            }

            return false;
        }

        public bool RemoveAllModifiersFromSource(object source) {
            bool didRemove = false;

            // Loops through the list backwards to avoid index issues once a modifier is removed
            for (int i = modifiers.Count - 1; i >= 0; i--) {
                if (modifiers[i].Source == source) {
                    modifiers.RemoveAt(i);
                    didRemove = true;
                    isDirty = true;
                    OnValueChanged?.Invoke(this);
                }
            }

            return didRemove;
        }
        
        protected int CompareModifierOrder(Modifier a, Modifier b) {
            if (a.Order < b.Order)
                return -1;
            else if (a.Order > b.Order)
                return 1;
            return 0;
        }
        
        public static float CalculateFinalValue(float baseValue, Modifier[] modifiers) {
            float baseAdd = 0;
            float additive = 0;
            float multiplicative = 1;
            float finalAdditive = 0;
            float finalMultiplicative = 1;
            float finalFlat = 0;

            foreach (Modifier mod in modifiers) {
                switch (mod.Type) {
                    case ModifierType.BaseAdd:
                        baseAdd += mod.Value;
                        break;
                    
                    case ModifierType.Additive:
                        additive += mod.Value;
                        break;
                    
                    case ModifierType.Multiplicative:
                        multiplicative *= mod.Value;
                        break;
                    
                    case ModifierType.FinalAdditive:
                        finalAdditive += mod.Value;
                        break;
                    
                    case ModifierType.FinalMultiplicative:
                        finalMultiplicative *= mod.Value;
                        break;

                    case ModifierType.FinalFlat:
                        finalFlat += mod.Value;
                        break;
                        
                    default:
                        Debug.LogWarning("Modifier Type Is Not Handled");
                        break;
                }
            }
            
            // Base Value
            float newBase = baseValue + baseAdd;
            
            // Modify Base Value
            float modified = newBase * (1 + additive);
            modified += ((newBase * multiplicative) - newBase);
            
            // Apply Final Modifiers to Modified Value
            float final = modified * (1 + finalAdditive);
            final +=  ((modified * finalMultiplicative) - modified);
            
            // Add Final Flat Amount
            final += finalFlat;
            
            return (float)Math.Round(final, 4);
        }
    }

    public static class StatExtensions {
        
    }
}
