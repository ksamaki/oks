namespace Oks.Web.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
public sealed class OksSkipTransactionAttribute : Attribute
{
}