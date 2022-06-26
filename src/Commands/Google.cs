using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        const string BASE_URL = "https://www.google.com/search";
        await cmdSock.DeferAsync();
        string usrQuery = (string)cmdSock.Data.Options.ElementAt(0).Value;
        try
        {
            #region Assemble URL

            FormUrlEncodedContent encodedContent = new(
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("q", usrQuery)
                }
            );

            StringBuilder assembler = new(BASE_URL);

            assembler
                .Append('?')
                .Append(await encodedContent.ReadAsStringAsync());

            #endregion Assemble URL



            ScrapeGoogle scraper = new(assembler.ToString());


            Task<List<ScrapedSearchResult>> scraperTask = scraper.GetResultsAsync();

            await scraperTask;

            List<EmbedFieldBuilder> fields = new();
            List<ScrapedSearchResult> results = await scraperTask;
            results.OrderBy(x => x.ItemPosition); // Order elements
            int addResults = 10;

            if (results.Count < 10) addResults = results.Count;

            for (int i = 0; i < addResults; i++)
            {
                fields.Add(new Discord.EmbedFieldBuilder() { Name = results[i].Title, Value = results[i].URL });
            }
            await cmdSock.FollowupAsync(embed: new EmbedBuilder() { Title = $"Searched \'{usrQuery}\' on google", Fields = fields, Footer = Extensions.GetTimeFooter() }.Build());
        }
        catch (NoResultsException ex)
        {
            Console.WriteLine(ex);
            await cmdSock.FollowupAsync(embed: new EmbedBuilder()
            {
                Title = "No search results :(",
                Description = $"No search results on google were found for query \'{usrQuery}\'",
                Footer = Extensions.GetTimeFooter()
            }.Build());

            return;

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

    }
}