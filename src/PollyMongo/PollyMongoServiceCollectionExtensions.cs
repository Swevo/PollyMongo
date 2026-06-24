// <copyright file="PollyMongoServiceCollectionExtensions.cs" company="Justin Bannister">
// Copyright (c) Justin Bannister. All rights reserved.
// </copyright>

namespace PollyMongo;

/// <summary>
/// Extension methods for registering a shared <see cref="ResiliencePipeline"/> with the
/// Microsoft dependency-injection container for use with MongoDB.
/// </summary>
public static class PollyMongoServiceCollectionExtensions
{
    /// <summary>
    /// Registers a <see cref="ResiliencePipeline"/> singleton built from <paramref name="configure"/>,
    /// which can then be injected alongside <see cref="IMongoCollection{T}"/> and used via
    /// <see cref="PollyMongoExtensions.WithPolly{T}"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">
    /// A delegate that configures the <see cref="ResiliencePipelineBuilder"/>
    /// (e.g. adds retry, timeout, circuit-breaker strategies).
    /// </param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddPollyMongo(
        this IServiceCollection services,
        Action<ResiliencePipelineBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new ResiliencePipelineBuilder();
        configure(builder);
        return services.AddPollyMongo(builder.Build());
    }

    /// <summary>
    /// Registers a pre-built <see cref="ResiliencePipeline"/> singleton that can be injected
    /// alongside <see cref="IMongoCollection{T}"/> and used via
    /// <see cref="PollyMongoExtensions.WithPolly{T}"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="pipeline">A fully configured <see cref="ResiliencePipeline"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddPollyMongo(
        this IServiceCollection services,
        ResiliencePipeline pipeline)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(pipeline);

        services.AddSingleton(pipeline);
        return services;
    }
}
