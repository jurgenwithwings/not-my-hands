using System;

public static class EnumExtensions {
    public static int ToInt<T>(this T value) where T : struct, Enum {
        return Convert.ToInt32(value);
    }
}
