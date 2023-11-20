using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;

public static class Utils
{
    public static bool IsOneOf<T>(this T self, params T[] other)
    {
        return other.Contains(self);
    }

    public static bool IsNullOrEmpty(this string self)
    {
        return self == null || self == string.Empty;
    }
}
