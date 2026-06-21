using Microsoft.Extensions.DependencyInjection;

namespace Rat.Domain.Infrastructure
{
    /// <summary>
    /// Interface to define application start
    /// </summary>
    public partial interface IAppStart
    {
        /// <summary>
        /// Service dependency injection configuration
        /// </summary>
        /// <param name="services">collection of services</param>
        void ConfigureServices(IServiceCollection services);

        /// <summary>
        /// Instance order
        /// </summary>
        int Order { get; }
    }
}
