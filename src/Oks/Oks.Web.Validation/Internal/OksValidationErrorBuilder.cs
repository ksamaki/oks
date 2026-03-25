using FluentValidation.Results;

namespace Oks.Web.Validation.Internal;

internal static class OksValidationErrorBuilder
{
    public static Dictionary<string, string[]> BuildDictionary(IEnumerable<ValidationFailure> failures)
        => failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(f => f.ErrorMessage).ToArray());
}
