using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools.V113.Page;
using OpenQA.Selenium.DevTools.V114.Runtime;
using OpenQA.Selenium.Remote;

namespace ChromeContainer;

using DotNet.Testcontainers;
using DotNet.Testcontainers.Builders;
using Xunit;

public class UnitTest1
{
    public UnitTest1()
    {
    }
    
    [Fact]
    public async Task Test1()
    {
        var gridNetwork = new NetworkBuilder()
            .WithName("seleniumnetwork")
            .Build();
        var hub = new ContainerBuilder()
            .WithImage("selenium/hub:latest")
            .WithName("selenium_hub")
            .WithPortBinding(4442, 4442)
            .WithPortBinding(4443, 4443)
            .WithPortBinding(4444, 4444)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(4444))
            .WithNetwork(gridNetwork)
            .Build();

        var env = new Dictionary<string, string>
        {
            { "SE_EVENT_BUS_HOST", "selenium_hub" },
            { "SE_EVENT_BUS_PUBLISH_PORT", "4442" },
            { "SE_EVENT_BUS_SUBSCRIBE_PORT", "4443" }
        };

        var seleniumContainer = new ContainerBuilder()
            .WithImage("selenium/node-chrome:latest")
            .WithName("selenium_chrome")
            .WithEnvironment(env)
            .WithNetwork(gridNetwork)
            .Build();
        
        var videoEnv = new Dictionary<string, string>
        {
            { "DISPLAY_CONTAINER_NAME", "selenium_chrome" },
            { "FILE_NAME", "chrome.mp4" }
        };
        var seleniumVideo = new ContainerBuilder()
            .WithImage("selenium/video:latest")
            .WithName("selenium_video")
            .WithEnvironment(videoEnv)
            .WithNetwork(gridNetwork)
            .Build();
            

        await gridNetwork.CreateAsync();
        await hub.StartAsync();
        await seleniumContainer.StartAsync();
        await seleniumVideo.StartAsync();

        var remote = new RemoteWebDriver(new Uri("http://localhost:4444"), new ChromeOptions());
        remote.Navigate().GoToUrl("https://magento.softwaretestingboard.com/");
    }
}