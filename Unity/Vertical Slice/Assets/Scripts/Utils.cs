using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Utils
{
    public static bool isOneOf<T>(this T self, params T[] other)
    {
        return other.Contains(self);
    }
}
