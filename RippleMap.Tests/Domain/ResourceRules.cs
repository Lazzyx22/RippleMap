using System.Text.RegularExpressions;

namespace RippleMap.Tests.Domain;

public static class ResourceIdRules
{
    public static bool IsValid(String? Id)
    {
        return Id is not null
            && Regex.IsMatch(Id, @"\A[a-zA-Z][a-zA-Z0-9_-]{0,63}\z");
    }
}
