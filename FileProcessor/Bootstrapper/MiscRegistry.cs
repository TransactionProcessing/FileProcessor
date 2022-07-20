namespace FileProcessor.Bootstrapper;

using BusinessLogic.Common;
using Lamar;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 
/// </summary>
/// <seealso cref="Lamar.ServiceRegistry" />
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
    }

    #endregion
}