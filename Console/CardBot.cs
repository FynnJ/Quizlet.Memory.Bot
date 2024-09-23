using Newtonsoft.Json;
using OpenQA.Selenium;
using SeleniumUndetectedChromeDriver;

namespace Console;

public static class CardBot
{
    public static async Task Beat()
    {
        System.Console.WriteLine("Program start");

        var cardSolutions = JsonConvert.DeserializeObject<Cards>(File.ReadAllText("cards.json"));
        if (cardSolutions == null)
        {
            throw new Exception("Karteikarten.json not found");
        }

        var driver = UndetectedChromeDriver.Create(
            driverExecutablePath:
            await new ChromeDriverInstaller().Auto());

        driver.Navigate().GoToUrl("https://quizlet.com/latest");

        System.Console.WriteLine("Einloggen und Memory oeffnen");
        System.Console.ReadLine();

        var play = driver.FindElement(By.XPath("//*[text()='Spiel beginnen']"));
        play.Click();

        MatchCardsExceptionHelper(driver, cardSolutions);

        System.Console.ReadLine();
        driver.Quit();

        System.Console.WriteLine("Program end");
    }

    private static void MatchCardsExceptionHelper(UndetectedChromeDriver driver, Cards cardSolutions)
    {
        try
        {
            MatchCards(driver, cardSolutions);
        }
        catch (Exception exception)
        {
            System.Console.WriteLine(exception.Message);
            MatchCardsExceptionHelper(driver, cardSolutions);
        }
    }

    private static void MatchCards(UndetectedChromeDriver driver, Cards cardSolutions)
    {
        var amountOfCards = driver.FindElements(By.ClassName("tqy0hun")).Count;
        if (amountOfCards <= 0) return;

        var cardElementsFromSite = driver.FindElements(By.ClassName("tqy0hun"));
        var currentCard = cardElementsFromSite.First();

        currentCard.Click();

        var cardSolution = cardSolutions.Items
            .Single(cardSolution =>
                cardSolution.Text1 == currentCard.Text || cardSolution.Text2 == currentCard.Text);
        if (currentCard.Text == cardSolution.Text1)
        {
            var otherCard = cardElementsFromSite
                .Single(otherCard => otherCard.Text == cardSolution.Text2);

            otherCard.Click();
        }
        else if (currentCard.Text == cardSolution.Text2)
        {
            var otherCard = cardElementsFromSite
                .Single(otherCard => otherCard.Text == cardSolution.Text1);

            otherCard.Click();
        }

        MatchCards(driver, cardSolutions);
    }
}