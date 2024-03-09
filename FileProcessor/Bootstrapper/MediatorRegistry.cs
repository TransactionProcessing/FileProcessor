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

        this.AddSingleton<IRequestHandler<UploadFileRequest, Guid>, FileRequestHandler>();
        this.AddSingleton<IRequestHandler<ProcessUploadedFileRequest>, FileRequestHandler>();
        //this.AddSingleton<IRequestHandler<SafaricomTopupRequest>, FileRequestHandler>();
        //this.AddSingleton<IRequestHandler<VoucherRequest>, FileRequestHandler>();
        this.AddSingleton<IRequestHandler<ProcessTransactionForFileLineRequest>, FileRequestHandler>();
    }

    #endregion
}