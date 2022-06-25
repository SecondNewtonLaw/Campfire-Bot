using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using WebsiteScraper;

namespace CampfireBot;

internal partial class Commands
{
    public static async Task GetGoogleResults(SocketSlashCommand cmdSock)
    {
        await cmdSock.DeferAsync();
        string usrQuery = (string)cmdSock.Data.Options.ElementAt(0).Value;

        ScrapeGoogle scraper = new(usrQuery);

        Task<List<ScrapedSearchResult>> scraperTask = scraper.GetResultsAsync();

        await scraperTask;

        if (scraperTask.IsFaulted)
        {
            if (scraperTask.Exception?.GetBaseException().GetType() == typeof(WebsiteScraper.NoResultsException))
            {
                await cmdSock.FollowupAsync(embed: new EmbedBuilder()
                {
                    Title = "No search results :(",
                    Description = $"No search results on google were found for query \'{usrQuery}\'",
                    Footer = Extensions.GetTimeFooter()
                }.Build());
                return;
            }
            else
            {
                Console.WriteLine($"Exception occurred -> {scraperTask.Exception}");
            }
        }
        else
        {
            List<EmbedFieldBuilder> fields = new();
            List<ScrapedSearchResult> results = await scraperTask;
            results.OrderBy(x => x.ItemPosition); // Order elements

            for (int i = 0; i < 10; i++)
            {
                fields.Add(new Discord.EmbedFieldBuilder() { Name = results[i].Title, Value = results[i].URL });
            }
            await cmdSock.FollowupAsync(embed: new EmbedBuilder() { Title = "Searched \'{usrQuery}\' on google", Fields = fields }.Build());
        }
    }
}