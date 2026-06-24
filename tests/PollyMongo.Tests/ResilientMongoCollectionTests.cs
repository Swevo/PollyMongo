// <copyright file="ResilientMongoCollectionTests.cs" company="Justin Bannister">
// Copyright (c) Justin Bannister. All rights reserved.
// </copyright>

namespace PollyMongo.Tests;

public class ResilientMongoCollectionTests
{
    private readonly Mock<IMongoCollection<TestDocument>> _collection = new();
    private readonly ResiliencePipeline _pipeline = ResiliencePipeline.Empty;

    [Fact]
    public void WithPolly_NullCollection_ThrowsArgumentNullException()
    {
        IMongoCollection<TestDocument>? collection = null;
        Assert.Throws<ArgumentNullException>(() => collection!.WithPolly(_pipeline));
    }

    [Fact]
    public void WithPolly_NullPipeline_ThrowsArgumentNullException()
    {
        ResiliencePipeline? pipeline = null;
        Assert.Throws<ArgumentNullException>(() => _collection.Object.WithPolly(pipeline!));
    }

    [Fact]
    public void WithPolly_ValidArguments_ReturnsResilientMongoCollection()
    {
        var result = _collection.Object.WithPolly(_pipeline);
        Assert.NotNull(result);
        Assert.IsType<ResilientMongoCollection<TestDocument>>(result);
    }

    public record TestDocument(string Id, string Name);
}
