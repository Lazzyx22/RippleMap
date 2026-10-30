using System.Text.RegularExpressions;

namespace RippleMap.Tests.Domain;

public static class ResourceIdRules
{
    public static bool IsValid(String? Id)
    {
        return Id is not null
            && Regex.IsMatch(Id, @"^[a-zA-Z0-9_-]+$");
    }
}
