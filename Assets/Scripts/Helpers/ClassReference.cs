using System;
using UnityEngine;

[Serializable]
public class ClassReference<T> where T : class
{
    [SerializeField]
    private string className;

    public Type Type
    {
        get
        {
            if (string.IsNullOrEmpty(className)) return null;
            return Type.GetType(className);
        }
        set
        {
            className = value?.AssemblyQualifiedName;
        }
    }

    public bool IsValid => Type != null && typeof(T).IsAssignableFrom(Type);
    
    public T CreateInstance()
    {
        if (Type == null)
            return null;

        if (!typeof(T).IsAssignableFrom(Type))
        {
            Debug.LogError($"[ClassReference] Type '{Type.FullName}' is not assignable to '{typeof(T).FullName}'");
            return null;
        }

        return Activator.CreateInstance(Type) as T;
    }

    public ClassReference(Type type) {
        Type = type;
    }

    public static implicit operator ClassReference<T>(Type type)
    {
        if (type == null)
            return null;

        if (!typeof(T).IsAssignableFrom(type))
        {
            Debug.LogError($"[ClassReference] Cannot assign type '{type.FullName}' to '{typeof(T).FullName}'");
            return null;
        }

        return new ClassReference<T>(type);
    }

    // Convert from a ClassReference<T> to a System.Type
    public static implicit operator Type(ClassReference<T> classRef)
    {
        return classRef?.Type;
    }
}