using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;

namespace ChromeContainer;

// ReSharper disable once ClassNeverInstantiated.Global
public class SeleniumFixture: IAsyncLifetime
{
    INetwork network;
    IContainer hub;
    IContainer browser;
    IContainer recorder;
    
    public async Task InitializeAsync()
    {
        // create docker network for containers
        //
        network = new NetworkBuilder()
            .WithName("seleniumnetwork")
            .Build();
        
        // create a hub for containers to attach
        //
        hub = new ContainerBuilder()
            .WithImage("selenium/hub:latest")
            .WithName("selenium_hub")
            .WithPortBinding(4442, 4442)
            .WithPortBinding(4443, 4443)
            .WithPortBinding(4444, 4444)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(4444))
            .WithNetwork(network)
            .Build();

        // use chrome env to create chrome container
        //
        var chromeEnv = new Dictionary<string, string>
        {
            { "SE_EVENT_BUS_HOST", "selenium_hub" },
            { "SE_EVENT_BUS_PUBLISH_PORT", "4442" },
            { "SE_EVENT_BUS_SUBSCRIBE_PORT", "4443" }
        };

        browser = new ContainerBuilder()
            .WithImage("selenium/node-chrome:latest")
            .WithName("selenium_chrome")
            .WithEnvironment(chromeEnv)
            .WithNetwork(network)
            .Build();
        
        // use recorder env to create recorder container
        //
        var recorderEnv = new Dictionary<string, string>
        {
            { "DISPLAY_CONTAINER_NAME", "selenium_chrome" },
            { "FILE_NAME", "chrome.mp4" }
        };
        recorder = new ContainerBuilder()
            .WithImage("selenium/video:latest")
            .WithName("selenium_video")
            .WithEnvironment(recorderEnv)
            .WithBindMount("/Users/phil/desktop/videos", "/videos")
            .WithNetwork(network)
            .Build();
            

        await network.CreateAsync();
        await hub.StartAsync();
        await browser.StartAsync();
        await recorder.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await recorder.StopAsync();
        await browser.StopAsync();
        await hub.StopAsync();
        await network.DeleteAsync();
    }
}