using System.Collections.Generic;
using FileProcessor.Models;
using SimpleResults;

namespace FileProcessor.Bootstrapper;

using System;
using System.Diagnostics.CodeAnalysis;
using BusinessLogic.RequestHandlers;
using BusinessLogic.Requests;
using Lamar;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

[ExcludeFromCodeCoverage]
public class MediatorRegistry : ServiceRegistry
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="MediatorRegistry"/> class.
    /// </summary>
    public MediatorRegistry()
    {
        this.AddSingleton<IMediator, Mediator>();
        // request & notification handlers

        this.AddSingleton<IRequestHandler<FileCommands.ProcessTransactionForFileLineCommand, Result>, FileRequestHandler>();
        this.AddSingleton<IRequestHandler<FileCommands.ProcessUploadedFileCommand, Result>, FileRequestHandler>();
        this.AddSingleton<IRequestHandler<FileCommands.UploadFileCommand, Result<Guid>>, FileRequestHandler>();
        
        this.AddSingleton<IRequestHandler<FileQueries.GetFileQuery, Result<FileDetails>>, FileRequestHandler>();
        this.AddSingleton<IRequestHandler<FileQueries.GetImportLogQuery, Result<Models.FileImportLog>>, FileRequestHandler>();
        this.AddSingleton<IRequestHandler<FileQueries.GetImportLogsQuery, Result<List<Models.FileImportLog>>>, FileRequestHandler>();
    }

    #endregion
}