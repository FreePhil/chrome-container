using FluentAssertions;
using SeleniumExtras.WaitHelpers;

namespace ChromeContainer;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using Xunit;

public class SimpleSeleniumWithTestContainerTest: IClassFixture<SeleniumFixture>
{
    private SeleniumFixture fixture;
    
    public SimpleSeleniumWithTestContainerTest(SeleniumFixture fixture)
    {
        this.fixture = fixture;
    }
    
    [Theory]
    [InlineData("standard_user", "secret_sauce")]
    [InlineData("problem_user", "secret_sauce")]
    [InlineData("performance_glitch_user", "secret_sauce")]
    public void TestLoginSuccess(string username, string password)
    {
        var remote = new RemoteWebDriver(new Uri("http://localhost:4444/wd/hub"),  new ChromeOptions());
        remote.Navigate().GoToUrl("https://www.saucedemo.com/");
        remote.Manage().Window.Maximize();
        var inputUsername = remote.FindElementById("user-name");
        var inputPassword = remote.FindElementById("password");

        var buttonLogin = remote.FindElementById("login-button");

        inputUsername.SendKeys(username);
        inputPassword.SendKeys(password);

        buttonLogin.Click();
        
        var waiter = new WebDriverWait(remote, TimeSpan.FromSeconds(1));
        By nextPageElementSelector = By.CssSelector("div.inventory_item");

        waiter.Until(ExpectedConditions.ElementIsVisible(nextPageElementSelector));
        
        Screenshot screenshot = ((ITakesScreenshot)remote).GetScreenshot();
        string screenshotPath = $"c:\\Users\\Phil\\Desktop\\TestLoginSuccess-{username}.png";
        screenshot.SaveAsFile(screenshotPath, ScreenshotImageFormat.Png);


        var items = remote.FindElementsByCssSelector("div.inventory_item");
        var firstImage = items[0].FindElement(By.CssSelector("img"));
        string filename = firstImage.GetAttribute("src");
        
        remote.Quit();
        
        items.Should().HaveCount(6);
        filename.Should().Contain("sauce-backpack");
    }
    
    [Theory]
    [InlineData("standard_user", "secret_sauce1")]
    [InlineData("abc", "secret_sauce")]
    [InlineData("a", "b")]
    public void TestLoginFailure(string username, string password)
    {
        var remote = new RemoteWebDriver(new Uri("http://localhost:4444/wd/hub"),  new ChromeOptions());
        remote.Navigate().GoToUrl("https://www.saucedemo.com/");
        remote.Manage().Window.Maximize();
        var inputUsername = remote.FindElementById("user-name");
        var inputPassword = remote.FindElementById("password");

        var buttonLogin = remote.FindElementById("login-button");

        inputUsername.SendKeys(username);
        inputPassword.SendKeys(password);
        
        buttonLogin.Click();
        
        var waiter = new WebDriverWait(remote, TimeSpan.FromSeconds(1));
        By errorMessageElement = By.CssSelector("div.error-message-container.error");

        waiter.Until(ExpectedConditions.ElementIsVisible(errorMessageElement));
        
        Screenshot screenshot = ((ITakesScreenshot)remote).GetScreenshot();
        string screenshotPath = $"c:\\Users\\Phil\\Desktop\\TestLoginFailure-{username}.png";
        screenshot.SaveAsFile(screenshotPath, ScreenshotImageFormat.Png);

        remote.Quit();
    }
}