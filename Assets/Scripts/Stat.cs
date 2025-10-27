using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Stats {
    public enum ModifierType {
        /// <summary>
        /// This is a flat amount that is added to the base value. This is very powerful.
        /// </summary>
        [Tooltip("This is a flat amount that is added to the base value. This is very powerful.")]
        Flat = 100,

        /// <summary>
        /// This is the most common type of modifier. It takes the base value and multiplies it by the modifier value.
        /// </summary>
        [Tooltip(
            "This is the most common type of modifier. It takes the base value and multiplies it by the modifier value.")]
        PercentMultiply = 200,

        /// <summary>
        /// This is a percentage added after everything else. It is exponential so use carefully.
        /// </summary>
        [Tooltip("This is a percentage added after everything else. It is exponential so use carefully.")]
        PercentAdd = 300,
    }

    public class Modifier {
        public readonly float Value;
        public readonly ModifierType Type;
        public readonly int Order;
        public readonly object Source;
        public readonly bool IsTimed;
        public float Duration { get; private set; }
        public readonly float MaxDuration;

        /// <summary>
        /// A modification to the value of a CharacterStat.
        /// </summary>
        /// <param name="value">Amount the stat is changed</param>
        /// <param name="type">How the value is applied to the base</param>
        /// <param name="order">Its priority in the stat calculation</param>
        /// <param name="source">The thing that applied the stat change</param>
        /// <param name="duration">How long the buff will last (in seconds)</param>
        /// <param name="maxDuration">Maximum amount of time the duration cam be</param>
        public Modifier(float value, ModifierType type, object source, float duration = -1f, float maxDuration = -1f) {
            Value = value;
            Type = type;
            Order = (int)type;
            Source = source;
            if (duration > 0) {
                IsTimed = true;
                Duration = duration;
                if (maxDuration > 0) {
                    MaxDuration = maxDuration;
                }
                else {
                    MaxDuration = duration;
                }
            }
        }

        public bool UpdateDuration(float deltaTime) {
            if (Duration > 0) {
                Duration -= deltaTime;
                return Duration <= 0;
            }

            return false;
        }

        public float AddDuration(float duration) {
            Duration += duration;
            if (Duration > MaxDuration) {
                Duration = MaxDuration;
            }

            return Duration;
        }

        public float SetDuration(float duration) {
            Duration = duration;
            if (Duration > MaxDuration) {
                Duration = MaxDuration;
            }

            return Duration;
        }
    }

    [Serializable]
    public class Stat {
        public float BaseValue;

        public float Value {
            get {
                if (isDirty || BaseValue != lastBaseValue) {
                    lastBaseValue = BaseValue;
                    value = CalculateFinalValue();
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

        //Create a new stat with no base value
        public Stat() {
            modifiers = new List<Modifier>();
            Modifiers = modifiers.AsReadOnly();
        }

        //Create a new stat with a base value
        public Stat(float baseValue) : this() {
            BaseValue = baseValue;
        }
        
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
        
        /// <summary>
        /// If the Stat has any timed modifiers, this will update their duration.
        /// </summary>
        public void UpdateTimers() {
            for (int i = modifiers.Count - 1; i >= 0; i--) {
                if (modifiers[i].IsTimed && modifiers[i].UpdateDuration(Time.deltaTime)) {
                    RemoveModifier(Modifiers[i]);
                }
            }
        }

        
        public void AddModifier(Modifier mod) {
            isDirty = true;
            modifiers.Add(mod);
            modifiers.Sort(CompareModifierOrder);
        }

        
        public bool RemoveModifier(Modifier mod) {
            if (modifiers.Remove(mod)) {
                isDirty = true;
                return true;
            }

            return false;
        }

        public bool RemoveAllModifiersFromSource(object source) {
            bool didRemove = false;

            // Loops through the list backwards to avoid index issues once a modifier is removed
            for (int i = modifiers.Count - 1; i >= 0; i--) {
                if (modifiers[i].Source == source) {
                    didRemove = true;
                    isDirty = true;
                    modifiers.RemoveAt(i);
                }
            }

            return didRemove;
        }

        
        public void AddDuration(Modifier mod, float duration) {
            int index = modifiers.IndexOf(mod);
            if (index != -1) {
                modifiers[index].AddDuration(duration);
            }
        }
        
        public void AddDuration(object source, float duration) {
            for (int i = 0; i < modifiers.Count; i++) {
                if (modifiers[i].Source == source) {
                    modifiers[i].AddDuration(duration);
                }
            }
        }
        
        
        public void SetDuration(Modifier mod, float duration) {
            int index = modifiers.IndexOf(mod);
            if (index != -1) {
                modifiers[index].SetDuration(duration);
            }
        }
        
        public void SetDuration(object source, float duration) {
            for (int i = 0; i < modifiers.Count; i++) {
                if (modifiers[i].Source == source) {
                    modifiers[i].SetDuration(duration);
                }
            }
        }

        
        
        protected float CalculateFinalValue() {
            float finalValue = BaseValue;
            float sumPercentAdd = 0;

            for (int i = 0; i < modifiers.Count; i++) {
                Modifier mod = modifiers[i];

                switch (mod.Type) {
                    case ModifierType.Flat:

                        finalValue += mod.Value;
                        break;
                    case ModifierType.PercentAdd:

                        sumPercentAdd += mod.Value;

                        // If we're at the end of the list OR the next modifer is a not a PercentAdd, then apply the final value
                        if (i + 1 >= modifiers.Count || modifiers[i + 1].Type != ModifierType.PercentAdd) {
                            finalValue *= 1 + sumPercentAdd;
                            sumPercentAdd = 0;
                        }

                        break;
                    case ModifierType.PercentMultiply:

                        finalValue *= 1 + mod.Value;
                        break;
                    default:
                        Debug.LogWarning("Modifier Type Is Not Handled");
                        break;
                }
            }

            return (float)Math.Round(finalValue, 4);
        }
        
        protected int CompareModifierOrder(Modifier a, Modifier b) {
            if (a.Order < b.Order)
                return -1;
            else if (a.Order > b.Order)
                return 1;
            return 0;
        }
    }
}