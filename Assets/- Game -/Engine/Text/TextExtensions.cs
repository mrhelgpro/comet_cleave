using UnityEngine;

public static class TextExtensions
{
    public static string ToColor(this string text, Color color)
    {
        return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
    }
    
    public static string ToBold(this string text)
    {
        return $"<b>{text}</b>";
    }
    public static string ToItalic(this string text)
    {
        return $"<i>{text}</i>";
    }

    public static string ToNoBreak(this string text)
    {
        return $"<nobr>{text}</nobr>";
    }
}
