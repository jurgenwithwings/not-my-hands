using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class SectionAttribute : PropertyAttribute
{
    public readonly string Name;
    public SectionAttribute(string name)
    {
        Name = name;
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class SectionEndAttribute : PropertyAttribute
{
    public SectionEndAttribute() { }
}
