using System.Security.Cryptography;
using System.Text;
using Oks.Authentication.Abstractions.Contracts;

namespace Oks.Authentication.EntityFrameworkCore.Services;

public sealed class Sha256SecretHasher : ISecretHasher
{
    public string Hash(string rawSecret)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawSecret));
        return Convert.ToHexString(bytes);
    }

    public bool Verify(string rawSecret, string hashedSecret)
        => string.Equals(Hash(rawSecret), hashedSecret, StringComparison.OrdinalIgnoreCase);
}
