using System.Linq;

public static class Utils
{
    public static bool isOneOf<T>(this T self, params T[] other)
    {
        return other.Contains(self);
    }
}
