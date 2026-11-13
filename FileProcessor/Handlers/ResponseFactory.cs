using System;
using TransactionProcessing.UnionResults;

namespace FileProcessor.Handlers;

internal static class ResponseFactory
{
    public static Result<T> FromResult<T>(Result<T> result) => result;

    public static Result FromResult(Result result) => result;

    public static Result<object?> FromResult<T>(Result<T> result, Func<T, object?> map)
        => result.Map(map);
}
