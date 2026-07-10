namespace FileProcessor.DataTransferObjects.Requests;

public class UpdateFileProfileRequest
{
    public string Name { get; set; }

    public string ListeningDirectory { get; set; }

    public string RequestType { get; set; }

    public string OperatorName { get; set; }

    public string LineTerminator { get; set; }

    public string FileFormatHandler { get; set; }
}
