using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace FileProcessor.DataTransferObjects.Requests;

/// <summary>Line ending values accepted by file profile requests.</summary>
[Description("Line ending values accepted by file profile requests.")]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LineTerminatorType
{
    /// <summary>Line feed (LF, `\n`).</summary>
    [Description("Line feed (LF, `\\n`).")]
    LineFeed = 1,

    /// <summary>Carriage return followed by line feed (CRLF, `\r\n`).</summary>
    [Description("Carriage return followed by line feed (CRLF, `\\r\\n`).")]
    CarriageReturnLineFeed = 2,

    /// <summary>Carriage return (CR, `\r`).</summary>
    [Description("Carriage return (CR, `\\r`).")]
    CarriageReturn = 3
}
