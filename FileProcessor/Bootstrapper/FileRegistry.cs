namespace FileProcessor.Bootstrapper;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using BusinessLogic.FileFormatHandlers;
using FileProcessor.FileProfileAggregate;
using Lamar;
using Microsoft.Extensions.DependencyInjection;

[ExcludeFromCodeCoverage]
public class FileRegistry : ServiceRegistry
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="FileRegistry"/> class.
    /// </summary>
    public FileRegistry()
    {
        this.AddSingleton<IFileSystem, FileSystem>();
        this.AddSingleton<Func<String, IFileFormatHandler>>(container => fileFormatHandlerName =>
                                                                         {
                                                                             if (fileFormatHandlerName == FileProfileConfiguration.SafaricomFileFormatHandler)
                                                                                 return new SafaricomFileFormatHandler();
                                                                             if (fileFormatHandlerName == FileProfileConfiguration.VoucherFileFormatHandler)
                                                                                 return new VoucherFileFormatHandler();

                                                                             return null;
                                                                         });
    }

    #endregion
}
