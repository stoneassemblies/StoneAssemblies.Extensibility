﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyExtensions.cs" company="Stone Assemblies">
// Copyright © 2021 - 2021 Stone Assemblies. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace StoneAssemblies.Extensibility
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using NuGet.Packaging;

    using Serilog;

    /// <summary>
    ///     The Assembly extensions.
    /// </summary>
    public static class AssemblyExtensions
    {
        /// <summary>
        /// The create logger method info.
        /// </summary>
        private static readonly MethodInfo CreateLoggerMethodInfo = typeof(LoggerFactoryExtensions).GetMethods().First(info => info.Name == nameof(LoggerFactoryExtensions.CreateLogger) && info.GetGenericArguments().Length == 1);

        /// <summary>
        /// The logger factory.
        /// </summary>
        private static readonly ILoggerFactory LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddSerilog());

        /// <summary>
        /// The assembly cache.
        /// </summary>
        private static readonly ConcurrentDictionary<string, Assembly> AssemblyCache = new ConcurrentDictionary<string, Assembly>();

        /// <summary>
        /// The initialize extension.
        /// </summary>
        /// <param name="assembly">
        /// The assembly.
        /// </param>
        /// <param name="serviceCollection">
        /// The service collection.
        /// </param>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <param name="extensionManager">
        /// The extension manager.
        /// </param>
        /// <returns>
        /// The startup <see cref="object"/>.
        /// </returns>
        public static object InitializeExtension(
            this Assembly assembly,
            IServiceCollection serviceCollection,
            IConfiguration configuration,
            IExtensionManager extensionManager)
        {
            var startup = assembly.CreateStartup(configuration, extensionManager);
            if (startup != null)
            {
                ConfigureServices(startup, serviceCollection);
            }

            return startup;
        }

        /// <summary>
        /// Enum referenced assemblies.
        /// </summary>
        /// <param name="assembly">
        /// The assembly.
        /// </param>
        /// <param name="cache">
        /// The cache.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{Assembly}"/>.
        /// </returns>
        public static IEnumerable<Assembly> EnumReferences(this Assembly assembly, HashSet<string> cache = null)
        {
            cache ??= new HashSet<string>();

            if (cache.Contains(assembly.GetName().Name))
            {
                yield break;
            }

            foreach (var referencedAssemblyName in assembly.GetReferencedAssemblies())
            {
                var assemblyName = referencedAssemblyName.Name;
                var referencedAssembly = AssemblyCache.GetOrAdd(
                    assemblyName,
                    s => AppDomain.CurrentDomain.GetAssemblies()
                             .FirstOrDefault(a => a.GetName().Name == assemblyName)
                         ?? Assembly.Load(referencedAssemblyName));

                if (referencedAssembly is not null && !cache.Contains(assemblyName))
                {
                    cache.Add(assemblyName);
                    yield return referencedAssembly;

                    foreach (var reference in referencedAssembly.EnumReferences(cache))
                    {
                        yield return reference;
                    }
                }
            }
        }

        /// <summary>
        /// Enum referenced assemblies.
        /// </summary>
        /// <param name="assemblies">
        /// The assembly.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{Assembly}"/>.
        /// </returns>
        public static IEnumerable<Assembly> EnumReferences(this IEnumerable<Assembly> assemblies)
        {
            var cache = new HashSet<string>();
            return assemblies.SelectMany(a => a.EnumReferences(cache));
        }

        /// <summary>
        ///     The configure services.
        /// </summary>
        /// <param name="startup">
        ///     The startup.
        /// </param>
        /// <param name="serviceCollection">
        ///     The service collection.
        /// </param>
        private static void ConfigureServices(object startup, IServiceCollection serviceCollection)
        {
            var configureServiceMethod = startup.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(info => info.Name == "ConfigureServices");

            if (configureServiceMethod != null && configureServiceMethod.GetParameters().Length == 1
                                               && typeof(IServiceCollection).IsAssignableFrom(
                                                   configureServiceMethod.GetParameters()[0].ParameterType))
            {
                try
                {
                    configureServiceMethod.Invoke(startup, new object[] { serviceCollection });
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error configuring plugins services");
                }
            }
        }

        /// <summary>
        ///     Creates the startup .
        /// </summary>
        /// <param name="assembly">
        ///     The assembly.
        /// </param>
        /// <param name="configuration">
        ///     The configuration.
        /// </param>
        /// <param name="extensionManager">
        ///     The extension manager.
        /// </param>
        /// <returns>
        ///     The <see cref="object" />.
        /// </returns>
        private static object CreateStartup(
            this Assembly assembly,
            IConfiguration configuration,
            IExtensionManager extensionManager)
        {
            object startup = null;
            var startupType = assembly.GetTypes().FirstOrDefault(type => type.Name == "Startup");
            if (startupType != null)
            {
                object[] availableParameters = { configuration, extensionManager, CreateLoggerMethodInfo.MakeGenericMethod(startupType).Invoke(typeof(LoggerFactoryExtensions), new object[] { LoggerFactory }) };
                foreach (var constructorInfo in startupType.GetConstructors())
                {
                    var parameters = new List<object>();
                    foreach (var parameterInfo in constructorInfo.GetParameters())
                    {
                        var parameter =
                            availableParameters.FirstOrDefault(o => parameterInfo.ParameterType.IsInstanceOfType(o));
                        parameters.Add(parameter);
                    }

                    if (parameters.Count == constructorInfo.GetParameters().Length)
                    {
                        startup = Activator.CreateInstance(startupType, parameters.ToArray());
                        break;
                    }
                }
            }

            return startup;
        }
    }
}