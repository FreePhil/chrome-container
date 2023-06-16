using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;

namespace ChromeContainer;

public class SeleniumFixture: IAsyncLifetime
{
    public INetwork Network { get; set; }
    public IContainer Hub { get; set; }
    public IContainer Browser { get; set; }
    public IContainer Recorder { get; set; }
    
    public async Task InitializeAsync()
    {
        Network = new NetworkBuilder()
            .WithName("seleniumnetwork")
            .Build();
        Hub = new ContainerBuilder()
            .WithImage("selenium/hub:latest")
            .WithName("selenium_hub")
            .WithPortBinding(4442, 4442)
            .WithPortBinding(4443, 4443)
            .WithPortBinding(4444, 4444)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(4444))
            .WithNetwork(Network)
            .Build();

        var env = new Dictionary<string, string>
        {
            { "SE_EVENT_BUS_HOST", "selenium_hub" },
            { "SE_EVENT_BUS_PUBLISH_PORT", "4442" },
            { "SE_EVENT_BUS_SUBSCRIBE_PORT", "4443" }
        };

        Browser = new ContainerBuilder()
            .WithImage("selenium/node-chrome:latest")
            .WithName("selenium_chrome")
            .WithEnvironment(env)
            .WithNetwork(Network)
            .Build();
        
        var recorderEnv = new Dictionary<string, string>
        {
            { "DISPLAY_CONTAINER_NAME", "selenium_chrome" },
            { "FILE_NAME", "chrome.mp4" }
        };
        Recorder = new ContainerBuilder()
            .WithImage("selenium/video:latest")
            .WithName("selenium_video")
            .WithEnvironment(recorderEnv)
            .WithBindMount("C:\\Users\\Phil", "/videos")
            .WithNetwork(Network)
            .Build();
            

        await Network.CreateAsync();
        await Hub.StartAsync();
        await Browser.StartAsync();
        await Recorder.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await Recorder.StopAsync();
        await Browser.StopAsync();
        await Hub.StopAsync();
        await Network.DeleteAsync();
    }
}