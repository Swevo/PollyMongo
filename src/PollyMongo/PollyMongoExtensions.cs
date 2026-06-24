// <copyright file="PollyMongoExtensions.cs" company="Justin Bannister">
// Copyright (c) Justin Bannister. All rights reserved.
// </copyright>

namespace PollyMongo;

/// <summary>
/// Extension methods for wrapping an <see cref="IMongoCollection{T}"/> with a Polly v8 resilience pipeline.
/// </summary>
public static class PollyMongoExtensions
{
    /// <summary>
    /// Wraps <paramref name="collection"/> in a <see cref="ResilientMongoCollection{T}"/> that
    /// executes every query and write inside the supplied <paramref name="pipeline"/>.
    /// </summary>
    /// <typeparam name="T">The document type.</typeparam>
    /// <param name="collection">The underlying MongoDB collection.</param>
    /// <param name="pipeline">The Polly v8 resilience pipeline to apply to every operation.</param>
    /// <returns>A <see cref="ResilientMongoCollection{T}"/> backed by <paramref name="collection"/>.</returns>
    public static ResilientMongoCollection<T> WithPolly<T>(
        this IMongoCollection<T> collection,
        ResiliencePipeline pipeline)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(pipeline);

        return new ResilientMongoCollection<T>(collection, pipeline);
    }
}
