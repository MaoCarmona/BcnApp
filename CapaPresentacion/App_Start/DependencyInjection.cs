using DotNetBcnModule.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;
using NetBcnModule.Services;
using NetBcnModule.Services.Queries;
using NetBcnModule.Services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DotNetBcnModule.Presentation.App_Start
{
    public static class DependencyInjection
    {
        private static IServiceProvider _serviceProvider;

        public static void Configure()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            
            _serviceProvider = services.BuildServiceProvider();
            
            // Set up MVC dependency resolver
            DependencyResolver.SetResolver(new CustomDependencyResolver(_serviceProvider));
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ILoggingService, LoggingService>();
            
            // Register DatabaseService for BCN database specifically
            services.AddScoped<IDatabaseService>(provider => 
            {
                var loggingService = provider.GetService<ILoggingService>();
                return new DatabaseService(DatabaseTarget.DBBCN, loggingService);
            });
            
            services.AddScoped<IDataProcessingService, DataProcessingService>();
            services.AddScoped<IAresWebService, AresWebService>();
            services.AddScoped<IQueriesService, QueriesService>();

            // Register specialized integration services
            services.AddScoped<IOperationalIntegrationService, OperationalIntegrationService>();
            services.AddScoped<IConsolidatedIntegrationService, ConsolidatedIntegrationService>();
            services.AddScoped<IBalanceIntegrationService, BalanceIntegrationService>();

            services.AddScoped<IIntegrationService, IntegrationService>();
            services.AddScoped<IBcnModuleService, BcnModuleService>();
        }

        public static IServiceProvider GetServiceProvider()
        {
            return _serviceProvider;
        }
    }

    public class CustomDependencyResolver : IDependencyResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public CustomDependencyResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _serviceProvider.GetServices(serviceType);
        }
    }
}