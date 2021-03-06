using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace WebsiteScraper;
public class ScrapeXDADevelopers : IScrape
{
    private const string _xPathToLinks = "//ol[@class='block-body']/li/div/div/h3/a";
    private static readonly HtmlWeb _htmlWeb = new HtmlWeb()
    {
        UserAgent = Utilities.GetRandomUserAgent()
    };
    private const string _site = "https://forum.xda-developers.com/";

    private const string _searchUrl = "https://forum.xda-developers.com/search/00000000/";
    private readonly HtmlDocument _targetDoc;
    private readonly string _query;
    /// <summary>
    /// Try to scrape results of the HTML in the specified URL
    /// </summary>
    /// <param name="keywords">The URL of the HTML to try to scrape results from</param>
    public ScrapeXDADevelopers(string keywords, XDASearchOrder searchOrder)
    {
        var url = new StringBuilder();
        _query = keywords;
        string relevance;

        if (searchOrder == XDASearchOrder.Date) relevance = "date";
        else relevance = "relevance";

        FormUrlEncodedContent encodedContent = new(
            new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("q", keywords),
                new KeyValuePair<string, string>("o", relevance)
            }
        );

        url
            .Append(_searchUrl)
            .Append('?')
            .Append(encodedContent.ReadAsStringAsync().Result);

        _targetDoc = _htmlWeb.Load(url.ToString());
    }
    /// <summary>
    /// Scrape XDA Developers forums search.
    /// </summary>
    /// <exception cref="NoResultsException">Thrown if no results are found</exception>
    /// <returns>a List of scraped results.</returns>
    public List<ScrapedSearchResult> GetResults()
    {
        var sb = new StringBuilder();
        var endresult = new List<ScrapedSearchResult>(); // Create ScrapeResult List.

        HtmlNodeCollection hnc = _targetDoc.DocumentNode.SelectNodes(_xPathToLinks);

        if (hnc is null)
        {
            throw new NoResultsException($"No results found for \'{_query}\'");
        }

        for (int i = 0; i < hnc.Count; i++)
        {
            sb.Append(_site).Append(hnc[i].Attributes["href"].Value);

            if (hnc[i].InnerText.Split(';').Length > 1)
            {
                endresult.Add(new()
                {
                    ItemPosition = (uint)i,
                    URL = sb.ToString(),
                    Title = hnc[i].InnerText.Split(';')[1]
                });
            }
            else
            {
                endresult.Add(new()
                {
                    ItemPosition = (uint)i,
                    URL = sb.ToString(),
                    Title = hnc[i].InnerText
                });
            }
            sb.Clear();
        }
        return endresult;
    }
    /// <summary>
    /// Scrape XDA Developers forums search. Asynchronously
    /// </summary>
    /// <exception cref="NoResultsException">Thrown if no results are found</exception>
    /// <returns>a List of scraped results.</returns>
    public async Task<List<ScrapedSearchResult>> GetResultsAsync()
    {
        var sb = new StringBuilder();
        var endresult = new List<ScrapedSearchResult>(); // Create ScrapeResult List.

        HtmlNodeCollection hnc = await Task.Run(() => _targetDoc.DocumentNode.SelectNodes(_xPathToLinks));

        if (hnc is null)
        {
            throw new NoResultsException($"No results found for \'{_query}\'");
        }
        for (int i = 0; i < hnc.Count; i++)
        {
            sb.Append(_site).Append(hnc[i].Attributes["href"].Value);

            if (hnc[i].InnerText.Split(';').Length > 1)
            {
                endresult.Add(new()
                {
                    ItemPosition = (uint)i,
                    URL = sb.ToString(),
                    Title = hnc[i].InnerText.Split(';')[1]
                });
            }
            else
            {
                endresult.Add(new()
                {
                    ItemPosition = (uint)i,
                    URL = sb.ToString(),
                    Title = hnc[i].InnerText
                });
            }
            sb.Clear();
        }
        return endresult;
    }
}
public enum XDASearchOrder
{
    Relevance,
    Date
}