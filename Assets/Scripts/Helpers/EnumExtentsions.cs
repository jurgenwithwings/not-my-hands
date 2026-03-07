using System;

public static class EnumExtensions {
    public static int Index<T>(this T value) where T : struct, Enum {
        return Convert.ToInt32(value);
    }
}
