using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebsiteScraper;
public interface IScrape
{
    public List<ScrapedSearchResult> GetResults();
    public Task<List<ScrapedSearchResult>> GetResultsAsync();
}