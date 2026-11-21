#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace TransactionProcessing.UnionResults;

internal static class ResultHelpers
{
    internal static ErrorList CreateFailure(Result result) => CreateFailure(result.GetErrors());

    internal static ErrorList CreateFailure<T>(Result<T> result) => CreateFailure(result.GetErrors());

    internal static ErrorList CreateFailure(IEnumerable<string> messages) =>
        new(messages.Select(message => new Error(ErrorType.Failure, message)));

    internal static T GetValueOrThrow<T>(this Result<T> result) =>
        result.Value is Success<T> success
            ? success.Value
            : throw new InvalidOperationException(result.GetErrorMessage());

    private static ErrorList CreateFailure(IReadOnlyList<Error> errors) => errors switch
    {
        ErrorList list => list,
        { Count: > 0 } => new ErrorList(errors),
        _ => throw new InvalidOperationException("Only a failed result can be propagated as a failure.")
    };
}
