namespace FileProcessor.Bootstrapper;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using BusinessLogic.FileFormatHandlers;
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
                                                                             if (fileFormatHandlerName == "SafaricomFileFormatHandler")
                                                                                 return new SafaricomFileFormatHandler();
                                                                             if (fileFormatHandlerName == "VoucherFileFormatHandler")
                                                                                 return new VoucherFileFormatHandler();

                                                                             return null;
                                                                         });
    }

    #endregion
}
