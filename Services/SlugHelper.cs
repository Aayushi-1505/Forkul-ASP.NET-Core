using System.Text.RegularExpressions;

namespace Forkful.Services;

public static class SlugHelper
{
    public static string From(string value)
    {
        var slug = Regex.Replace(value.Trim().ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');
        return string.IsNullOrEmpty(slug) ? "dish" : slug;
    }
}
