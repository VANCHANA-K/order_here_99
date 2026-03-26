using System.Security.Cryptography;
using System.Text;

namespace QrFoodOrdering.Application.Common.Idempotency;

public static class IdempotencyRequestHasher
{
    public static string Compute(string canonicalValue)
    {
        var bytes = Encoding.UTF8.GetBytes(canonicalValue);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
