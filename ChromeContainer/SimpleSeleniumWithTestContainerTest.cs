namespace ChromeContainer;

using SeleniumExtras.WaitHelpers;
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
        var nextPageElementSelector = By.Id("add-to-cart-sauce-labs-backpack");

        waiter.Until(ExpectedConditions.ElementIsVisible(nextPageElementSelector));
        remote.Quit();
    }
    
    [Theory]
    [InlineData("standard_user", "secret_sauce1")]
    [InlineData("abc", "secret_sauce")]
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

        remote.Quit();
    }
}