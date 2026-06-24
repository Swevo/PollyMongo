// <copyright file="PollyMongoServiceCollectionExtensionsTests.cs" company="Justin Bannister">
// Copyright (c) Justin Bannister. All rights reserved.
// </copyright>

namespace PollyMongo.Tests;

public class PollyMongoServiceCollectionExtensionsTests
{
    [Fact]
    public void AddPollyMongo_WithBuilder_RegistersResiliencePipelineSingleton()
    {
        var services = new ServiceCollection();
        services.AddPollyMongo(pipeline => pipeline.AddTimeout(TimeSpan.FromSeconds(5)));

        var provider = services.BuildServiceProvider();
        var pipeline = provider.GetService<ResiliencePipeline>();

        Assert.NotNull(pipeline);
    }

    [Fact]
    public void AddPollyMongo_WithPrebuiltPipeline_RegistersResiliencePipelineSingleton()
    {
        var services = new ServiceCollection();
        var prebuilt = new ResiliencePipelineBuilder()
            .AddTimeout(TimeSpan.FromSeconds(5))
            .Build();

        services.AddPollyMongo(prebuilt);

        var provider = services.BuildServiceProvider();
        var pipeline = provider.GetService<ResiliencePipeline>();

        Assert.Same(prebuilt, pipeline);
    }

    [Fact]
    public void AddPollyMongo_NullServices_ThrowsArgumentNullException()
    {
        IServiceCollection? services = null;
        Assert.Throws<ArgumentNullException>(() => services!.AddPollyMongo(_ => { }));
    }

    [Fact]
    public void AddPollyMongo_NullConfigure_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();
        Action<ResiliencePipelineBuilder>? configure = null;
        Assert.Throws<ArgumentNullException>(() => services.AddPollyMongo(configure!));
    }
}
