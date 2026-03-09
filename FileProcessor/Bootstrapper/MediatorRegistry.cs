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
        this.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(FileRequestHandler).Assembly));
    }

    #endregion
}