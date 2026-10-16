namespace FileProcessor.BusinessLogic.FileFormatHandlers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public abstract record FileValidationError(String Code, String Reason);

public record FileLevelValidationError(String Code, String Reason)
    : FileValidationError(Code, Reason);

public record RowValidationError(Int32 LineNumber,
                                 String Field,
                                 String Code,
                                 String Reason)
    : FileValidationError(Code, Reason);

public record FileValidationResult(Guid FileId,
                                   Boolean IsValid,
                                   IReadOnlyList<FileValidationError> Errors);

internal static class FileFormatValidation
{
    public static IReadOnlyList<String> ReadLines(TextReader fileReader, String lineTerminator)
    {
        return fileReader.ReadToEnd().Split(lineTerminator);
    }

    public static FileValidationResult Validate(IReadOnlyList<String> fileLines,
                                                Func<String, Boolean> isIgnored,
                                                Func<String, Int32> expectedFieldCount,
                                                Func<String, String> parseDetailError,
                                                Boolean validateTrailerDetailCount)
    {
        List<(Int32 Number, String Value)> lines = fileLines
            .Select((line, index) => (Number: index + 1, Value: line?.Trim() ?? String.Empty))
            .Where(line => String.IsNullOrWhiteSpace(line.Value) == false)
            .ToList();

        if (lines.Count == 0)
            return new FileValidationResult(Guid.Empty,
                                            false,
                                            new[] { new FileLevelValidationError("EmptyFile", "The file contains no non-empty lines") });

        List<(Int32 Number, String Value)> detailLines = lines.Where(line => isIgnored(line.Value) == false).ToList();
        List<FileValidationError> errors = ValidateStructure(lines);
        errors.AddRange(ValidateDetailLines(detailLines, expectedFieldCount, parseDetailError));
        errors.AddRange(ValidateTrailerDetailCount(lines, detailLines.Count, validateTrailerDetailCount));

        return new FileValidationResult(Guid.Empty, errors.Count == 0, errors);
    }

    private static List<FileValidationError> ValidateStructure(IReadOnlyList<(Int32 Number, String Value)> lines)
    {
        List<FileValidationError> errors = new();
        Boolean firstLineIsHeader = lines[0].Value.StartsWith("H", StringComparison.Ordinal);
        Boolean lastLineIsTrailer = lines[^1].Value.StartsWith("T", StringComparison.Ordinal);

        if (firstLineIsHeader == false)
            errors.Add(new FileLevelValidationError("MissingHeader", "The first non-empty line must be a header"));

        if (lines.Any(line => line.Value.StartsWith("H", StringComparison.Ordinal)) && firstLineIsHeader == false)
            errors.Add(new FileLevelValidationError("HeaderNotFirst", "The header must be the first non-empty line"));

        if (lastLineIsTrailer == false)
            errors.Add(new FileLevelValidationError("MissingTrailer", "The last non-empty line must be a trailer"));

        if (lines.Any(line => line.Value.StartsWith("T", StringComparison.Ordinal)) && lastLineIsTrailer == false)
            errors.Add(new FileLevelValidationError("TrailerNotLast", "The trailer must be the last non-empty line"));

        if (lines.Count(line => line.Value.StartsWith("H", StringComparison.Ordinal)) != 1)
            errors.Add(new FileLevelValidationError("InvalidHeaderCount", "The file must contain exactly one header"));

        if (lines.Count(line => line.Value.StartsWith("T", StringComparison.Ordinal)) != 1)
            errors.Add(new FileLevelValidationError("InvalidTrailerCount", "The file must contain exactly one trailer"));

        return errors;
    }

    private static List<FileValidationError> ValidateDetailLines(IReadOnlyList<(Int32 Number, String Value)> detailLines,
                                                                 Func<String, Int32> expectedFieldCount,
                                                                 Func<String, String> parseDetailError)
    {
        List<FileValidationError> errors = new();
        if (detailLines.Count == 0)
            errors.Add(new FileLevelValidationError("NoDetailRows", "The file contains no detail rows"));

        foreach ((Int32 number, String value) detailLine in detailLines)
        {
            String[] fields = detailLine.value.Split(',');
            Int32 expectedCount = expectedFieldCount(detailLine.value);
            if (fields.Length != expectedCount)
            {
                errors.Add(new RowValidationError(detailLine.number,
                                                  "Record",
                                                  "InvalidFieldCount",
                                                  $"Expected {expectedCount} fields but found {fields.Length}"));
                continue;
            }

            String parseError = parseDetailError(detailLine.value);
            if (String.IsNullOrWhiteSpace(parseError) == false)
                errors.Add(new RowValidationError(detailLine.number, "Record", "InvalidDetail", parseError));
        }

        return errors;
    }

    private static List<FileValidationError> ValidateTrailerDetailCount(IReadOnlyList<(Int32 Number, String Value)> lines,
                                                                         Int32 detailLineCount,
                                                                         Boolean validateTrailerDetailCount)
    {
        List<FileValidationError> errors = new();
        if (validateTrailerDetailCount == false || lines[^1].Value.StartsWith("T", StringComparison.Ordinal) == false)
            return errors;

        String[] trailerFields = lines[^1].Value.Split(',');
        if (trailerFields.Length > 1 && Int32.TryParse(trailerFields[1].Trim(), out Int32 expectedCount))
        {
            if (expectedCount != detailLineCount)
                errors.Add(new FileLevelValidationError("TrailerDetailCountMismatch",
                                                        $"Trailer specifies {expectedCount} detail rows but found {detailLineCount}"));
        }
        else
        {
            errors.Add(new RowValidationError(lines[^1].Number,
                                              "DetailCount",
                                              "InvalidTrailer",
                                              "Trailer detail count is not a valid integer"));
        }

        return errors;
    }
}
