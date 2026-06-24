// <copyright file="ResilientMongoCollection.cs" company="Justin Bannister">
// Copyright (c) Justin Bannister. All rights reserved.
// </copyright>

namespace PollyMongo;

/// <summary>
/// A MongoDB.Driver decorator that executes every query and write operation inside a Polly v8
/// <see cref="ResiliencePipeline"/>. Create one via
/// <see cref="PollyMongoExtensions.WithPolly{T}(IMongoCollection{T}, ResiliencePipeline)"/>.
/// </summary>
/// <typeparam name="T">The document type.</typeparam>
public sealed class ResilientMongoCollection<T>(IMongoCollection<T> inner, ResiliencePipeline pipeline)
{
    /// <summary>
    /// Finds documents matching <paramref name="filter"/> and returns them as a list.
    /// </summary>
    public Task<List<T>> FindAsync(
        FilterDefinition<T> filter,
        FindOptions<T>? options = null,
        CancellationToken cancellationToken = default) =>
        pipeline.ExecuteAsync(
            ct => new ValueTask<List<T>>(
                inner.Find(filter, new FindOptions { BatchSize = options?.BatchSize })
                     .ToListAsync(ct)),
            cancellationToken).AsTask();

    /// <summary>
    /// Returns the first document matching <paramref name="filter"/>, or <c>default</c> if none found.
    /// </summary>
    public Task<T?> FindOneAsync(
        FilterDefinition<T> filter,
        FindOptions<T>? options = null,
        CancellationToken cancellationToken = default) =>
        pipeline.ExecuteAsync(
            ct => new ValueTask<T?>(
                inner.Find(filter).FirstOrDefaultAsync(ct)!),
            cancellationToken).AsTask();

    /// <summary>
    /// Inserts a single document into the collection.
    /// </summary>
    public Task InsertOneAsync(
        T document,
        InsertOneOptions? options = null,
        CancellationToken cancellationToken = default) =>
        pipeline.ExecuteAsync(
            ct => new ValueTask(inner.InsertOneAsync(document, options, ct)),
            cancellationToken).AsTask();

    /// <summary>
    /// Inserts multiple documents into the collection.
    /// </summary>
    public Task InsertManyAsync(
        IEnumerable<T> documents,
        InsertManyOptions? options = null,
        CancellationToken cancellationToken = default) =>
        pipeline.ExecuteAsync(
            ct => new ValueTask(inner.InsertManyAsync(documents, options, ct)),
            cancellationToken).AsTask();

    /// <summary>
    /// Updates the first document matching <paramref name="filter"/>.
    /// </summary>
    public Task<UpdateResult> UpdateOneAsync(
        FilterDefinition<T> filter,
        UpdateDefinition<T> update,
        UpdateOptions? options = null,
        CancellationToken cancellationToken = default) =>
        pipeline.ExecuteAsync(
            ct => new ValueTask<UpdateResult>(
                inner.UpdateOneAsync(filter, update, options, ct)),
            cancellationToken).AsTask();

    /// <summary>
    /// Updates all documents matching <paramref name="filter"/>.
    /// </summary>
    public Task<UpdateResult> UpdateManyAsync(
        FilterDefinition<T> filter,
        UpdateDefinition<T> update,
        UpdateOptions? options = null,
        CancellationToken cancellationToken = default) =>
        pipeline.ExecuteAsync(
            ct => new ValueTask<UpdateResult>(
                inner.UpdateManyAsync(filter, update, options, ct)),
            cancellationToken).AsTask();

    /// <summary>
    /// Replaces the first document matching <paramref name="filter"/> with <paramref name="replacement"/>.
    /// </summary>
    public Task<ReplaceOneResult> ReplaceOneAsync(
        FilterDefinition<T> filter,
        T replacement,
        ReplaceOptions? options = null,
        CancellationToken cancellationToken = default) =>
        pipeline.ExecuteAsync(
            ct => new ValueTask<ReplaceOneResult>(
                inner.ReplaceOneAsync(filter, replacement, options, ct)),
            cancellationToken).AsTask();

    /// <summary>
    /// Deletes the first document matching <paramref name="filter"/>.
    /// </summary>
    public Task<DeleteResult> DeleteOneAsync(
        FilterDefinition<T> filter,
        CancellationToken cancellationToken = default) =>
        pipeline.ExecuteAsync(
            ct => new ValueTask<DeleteResult>(
                inner.DeleteOneAsync(filter, ct)),
            cancellationToken).AsTask();

    /// <summary>
    /// Deletes all documents matching <paramref name="filter"/>.
    /// </summary>
    public Task<DeleteResult> DeleteManyAsync(
        FilterDefinition<T> filter,
        CancellationToken cancellationToken = default) =>
        pipeline.ExecuteAsync(
            ct => new ValueTask<DeleteResult>(
                inner.DeleteManyAsync(filter, ct)),
            cancellationToken).AsTask();

    /// <summary>
    /// Returns the number of documents matching <paramref name="filter"/>.
    /// </summary>
    public Task<long> CountDocumentsAsync(
        FilterDefinition<T> filter,
        CountOptions? options = null,
        CancellationToken cancellationToken = default) =>
        pipeline.ExecuteAsync(
            ct => new ValueTask<long>(
                inner.CountDocumentsAsync(filter, options, ct)),
            cancellationToken).AsTask();

    /// <summary>
    /// Finds a single document matching <paramref name="filter"/>, applies <paramref name="update"/>,
    /// and returns the document (before or after the update, depending on <paramref name="options"/>).
    /// </summary>
    public Task<T?> FindOneAndUpdateAsync(
        FilterDefinition<T> filter,
        UpdateDefinition<T> update,
        FindOneAndUpdateOptions<T>? options = null,
        CancellationToken cancellationToken = default) =>
        pipeline.ExecuteAsync(
            ct => new ValueTask<T?>(
                inner.FindOneAndUpdateAsync(filter, update, options, ct)!),
            cancellationToken).AsTask();

    /// <summary>
    /// Finds a single document matching <paramref name="filter"/>, deletes it, and returns it.
    /// </summary>
    public Task<T?> FindOneAndDeleteAsync(
        FilterDefinition<T> filter,
        FindOneAndDeleteOptions<T>? options = null,
        CancellationToken cancellationToken = default) =>
        pipeline.ExecuteAsync(
            ct => new ValueTask<T?>(
                inner.FindOneAndDeleteAsync(filter, options, ct)!),
            cancellationToken).AsTask();

    /// <summary>
    /// Finds a single document matching <paramref name="filter"/>, replaces it with
    /// <paramref name="replacement"/>, and returns the document (before or after, depending on
    /// <paramref name="options"/>).
    /// </summary>
    public Task<T?> FindOneAndReplaceAsync(
        FilterDefinition<T> filter,
        T replacement,
        FindOneAndReplaceOptions<T>? options = null,
        CancellationToken cancellationToken = default) =>
        pipeline.ExecuteAsync(
            ct => new ValueTask<T?>(
                inner.FindOneAndReplaceAsync(filter, replacement, options, ct)!),
            cancellationToken).AsTask();
}
