﻿using System.Collections.Generic;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace WebsiteScraper;

public class ScrapeHTMLDuckDuckGo : IScrape
{
    private const string _xPathToLinks = "//div[@id='links']/div/div/h2[@class='result__title']/a";

    private static readonly HtmlWeb _htmlWeb = new HtmlWeb()
    {
        UserAgent = Utilities.GetRandomUserAgent()
    };
    private const string _site = "https://html.duckduckgo.com/html/";
    private readonly HtmlDocument _targetDoc;
    private readonly string _query;
    /// <summary>
    /// Try to scrape results of the search with the specified keywords.
    /// </summary>
    /// <param name="keywords">The keywords to search in duckduckgo HTML</param>
    public ScrapeHTMLDuckDuckGo(string keywords)
    {
        // replace some chars.
        Utilities.FixURL(ref keywords, true);
        _query = keywords;
        _targetDoc = _htmlWeb.Load(string.Format("{0}?q={1}", _site, keywords));
    }
    /// <summary>
    /// Scrape DuckDuckGo HTML search. Asynchronously
    /// </summary>
    /// <exception cref="NoResultsException">Thrown if no results are found</exception>
    /// <returns>a List of scraped results.</returns>
    public async Task<List<ScrapedSearchResult>> GetResultsAsync()
    {
        var endresult = new List<ScrapedSearchResult>(); // Create ScrapeResult List.
        HtmlNodeCollection hnc = await Task.Run(() => _targetDoc.DocumentNode.SelectNodes(_xPathToLinks));

        if (hnc is null)
        {
            throw new NoResultsException($"No results found for \'{_query}\'");
        }

        for (int i = 0; i < hnc.Count; i++)
        {
            string title = hnc[i].InnerText;
            string url = hnc[i].Attributes["href"].Value;

            // Fixup URL.
            url = url.Replace("//duckduckgo.com/l/?uddg=", "\0");
            url = url.Split("&amp")[0];
            Utilities.FixURL(ref url, false);

            endresult.Add(new()
            {
                ItemPosition = (uint)i,
                URL = url,
                Title = title
            });
        }
        return endresult;
    }
    /// <summary>
    /// Scrape DuckDuckGo HTML search.
    /// </summary>
    /// <exception cref="NoResultsException">Thrown if no results are found</exception>
    /// <returns>a List of scraped results.</returns>
    public List<ScrapedSearchResult> GetResults()
    {
        var endresult = new List<ScrapedSearchResult>(); // Create ScrapeResult List.
        HtmlNodeCollection hnc = _targetDoc.DocumentNode.SelectNodes(_xPathToLinks);

        if (hnc is null)
        {
            throw new NoResultsException($"No results found for \'{_query}\'");
        }

        for (int i = 0; i < hnc.Count; i++)
        {
            string title = hnc[i].InnerText;
            string url = hnc[i].Attributes["href"].Value;

            // Fixup URL.
            url = url.Replace("//duckduckgo.com/l/?uddg=", "\0");
            url = url.Split("&amp")[0];
            Utilities.FixURL(ref url, false);

            endresult.Add(new()
            {
                ItemPosition = (uint)i,
                URL = url,
                Title = title
            });
        }
        return endresult;
    }
}