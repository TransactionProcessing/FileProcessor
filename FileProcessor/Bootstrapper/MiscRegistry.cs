namespace FileProcessor.Bootstrapper;

using BusinessLogic.Common;
using BusinessLogic.Services;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class MiscRegistry : ServiceRegistry
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="MiscRegistry"/> class.
    /// </summary>
    public MiscRegistry()
    {
        this.AddSingleton<IModelFactory, ModelFactory>();
        this.AddSingleton<Common.IModelFactory, Common.ModelFactory>();
        this.AddSingleton<IFileProcessorDomainService, FileProcessorDomainService>();
    }

    #endregion
}