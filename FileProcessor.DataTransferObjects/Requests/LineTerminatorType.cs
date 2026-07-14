using System;
using System.Text.Json.Serialization;

namespace FileProcessor.DataTransferObjects.Requests;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LineTerminatorType
{
    LineFeed = 1,

    CarriageReturnLineFeed = 2,

    CarriageReturn = 3
}
