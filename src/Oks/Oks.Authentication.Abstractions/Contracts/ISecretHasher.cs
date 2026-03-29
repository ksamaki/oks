namespace Oks.Authentication.Abstractions.Contracts;

public interface ISecretHasher
{
    string Hash(string rawSecret);
    bool Verify(string rawSecret, string hashedSecret);
}
