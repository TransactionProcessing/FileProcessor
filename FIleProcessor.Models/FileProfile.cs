namespace FIleProcessor.Models
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IEquatable{FIleProcessor.Models.FileProfile}" />
    public record FileProfile
    {
        /// <summary>
        /// Gets or sets the file profile identifier.
        /// </summary>
        /// <value>
        /// The file profile identifier.
        /// </value>
        public Guid FileProfileId { get; init; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public String Name { get; init; }
        /// <summary>
        /// Gets or sets the name of the operator.
        /// </summary>
        /// <value>
        /// The name of the operator.
        /// </value>
        public String OperatorName { get; init; }

        /// <summary>
        /// Gets or sets the listening directory.
        /// </summary>
        /// <value>
        /// The listening directory.
        /// </value>
        public String ListeningDirectory { get; init; }
        /// <summary>
        /// Gets or sets the processed directory.
        /// </summary>
        /// <value>
        /// The processed directory.
        /// </value>
        public String ProcessedDirectory { get; init; }
        /// <summary>
        /// Gets or sets the failed directory.
        /// </summary>
        /// <value>
        /// The failed directory.
        /// </value>
        public String FailedDirectory { get; init; }

        /// <summary>
        /// Gets or sets the file format handler.
        /// </summary>
        /// <value>
        /// The file format handler.
        /// </value>
        public String FileFormatHandler { get; init; }
        /// <summary>
        /// Gets or sets the type of the request.
        /// </summary>
        /// <value>
        /// The type of the request.
        /// </value>
        public String RequestType { get; init; }

        /// <summary>
        /// Gets or sets the line terminator.
        /// </summary>
        /// <value>
        /// The line terminator.
        /// </value>
        public String LineTerminator { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileProfile" /> class.
        /// </summary>
        /// <param name="fileProfileId">The file profile identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="listeningDirectory">The listening directory.</param>
        /// <param name="requestType">Type of the request.</param>
        /// <param name="operatorName">Name of the operator.</param>
        /// <param name="lineTerminator">The line terminator.</param>
        public FileProfile(Guid fileProfileId, String name, String listeningDirectory, String requestType, String operatorName, String lineTerminator, String fileFormatHandler)
        {
            this.FileProfileId = fileProfileId;
            this.Name = name;
            this.ListeningDirectory = listeningDirectory;
            this.ProcessedDirectory = $"{listeningDirectory}/processed";
            this.FailedDirectory = $"{listeningDirectory}/failed";
            this.RequestType = requestType;
            this.OperatorName = operatorName;
            this.LineTerminator = lineTerminator;
            this.FileFormatHandler = fileFormatHandler;
        }
    }
}