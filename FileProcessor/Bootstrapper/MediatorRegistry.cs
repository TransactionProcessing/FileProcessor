namespace FileProcessor.Bootstrapper;

using System;
using BusinessLogic.RequestHandlers;
using BusinessLogic.Requests;
using Lamar;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 
/// </summary>
/// <seealso cref="Lamar.ServiceRegistry" />
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
        this.AddTransient<ServiceFactory>(context => { return t => context.GetService(t); });

        this.AddSingleton<IRequestHandler<UploadFileRequest, Guid>, FileRequestHandler>();
        this.AddSingleton<IRequestHandler<ProcessUploadedFileRequest, Unit>, FileRequestHandler>();
        this.AddSingleton<IRequestHandler<SafaricomTopupRequest, Unit>, FileRequestHandler>();
        this.AddSingleton<IRequestHandler<VoucherRequest, Unit>, FileRequestHandler>();
        this.AddSingleton<IRequestHandler<ProcessTransactionForFileLineRequest, Unit>, FileRequestHandler>();
    }

    #endregion
}