namespace Oks.Web.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public sealed class OksSkipRateLimitAttribute : Attribute
{
}