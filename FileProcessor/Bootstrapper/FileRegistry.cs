namespace FileProcessor.Bootstrapper;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using BusinessLogic.FileFormatHandlers;
using FIleProcessor.Models;
using Lamar;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 
/// </summary>
/// <seealso cref="Lamar.ServiceRegistry" />
public class FileRegistry : ServiceRegistry
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="FileRegistry"/> class.
    /// </summary>
    public FileRegistry()
    {
        IEnumerable<FileProfile> fileProfiles = Startup.Configuration.GetSection("AppSettings:FileProfiles").GetChildren().ToList().Select(x => new
            {
                Name = x.GetValue<String>("Name"),
                FileProfileId = x.GetValue<Guid>("Id"),
                RequestType = x.GetValue<String>("RequestType"),
                ListeningDirectory = x.GetValue<String>("ListeningDirectory"),
                OperatorName = x.GetValue<String>("OperatorName"),
                LineTerminator = x.GetValue<String>("LineTerminator"),
                FileFormatHandler = x.GetValue<String>("FileFormatHandler")
            }).Select(f =>
                      {
                          return new FileProfile(f.FileProfileId,
                                                 f.Name,
                                                 f.ListeningDirectory,
                                                 f.RequestType,
                                                 f.OperatorName,
                                                 f.LineTerminator,
                                                 f.FileFormatHandler);
                      });
        this.AddSingleton(fileProfiles.ToList());
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