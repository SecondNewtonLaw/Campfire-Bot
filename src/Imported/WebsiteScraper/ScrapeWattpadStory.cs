using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace WebsiteScraper;
public class ScrapeWattpadStory
{
    private const string _xPathToText = "//div[contains(concat(' ', normalize-space(@class), ' '), 'page')]/div/pre/p";
    private const string _xPathIsLastPage = "//div[contains(concat(' ', normalize-space(@class), ' '), 'last-page')]";
    private const string _xPathToChapterTitle = "//header[contains(concat(' ', normalize-space(@class), ' '), 'text-center') and contains(concat(' ', normalize-space(@class), ' '), 'panel')]/h1";
    private const string _xPathToChapterTitle_FALLBACK = "//header[contains(concat(' ', normalize-space(@class), ' '), 'panel') and contains(concat(' ', normalize-space(@class), ' '), 'panel-reading')]/h3[@class='part-restart-chapter']";
    private const string _xPathStoryTitle = "//div[@class='story-info']/span";
    private const string _xPathStoryAuthor = "//div[@class='author-info']/div/a";
    private const string _site = "https://www.wattpad.com/";
    private static readonly HtmlWeb _htmlWeb = new HtmlWeb()
    {
        OverrideEncoding = Encoding.UTF8,
        UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.75 Safari/537.36 Edg/100.0.1185.39"
    };
    private HtmlDocument _targetDoc;
    /// <summary>
    /// Gets the specified page in the URL of the chapter
    /// </summary>
    /// <param name="URLOfChapter">The URL of the chapter</param>
    public WattpadPage GetChapterPage(string URLOfChapter)
    {
        _targetDoc = _htmlWeb.Load(URLOfChapter);
        HtmlNodeCollection textContainers = _targetDoc.DocumentNode.SelectNodes(_xPathToText);

        WattpadPage result = new()
        {
            Paragraphs = new List<string>()
        };

        for (int i = 0; i < textContainers.Count; i++)
        {
            string text = textContainers[i].InnerText.Trim('\"').TrimEnd('\"');
            Utilities.FixString(ref text, false);
            result.Paragraphs.Add(text);
        }

        return result;
    }
    /// <summary>
    /// Gets the specified chapter
    /// </summary>
    /// <param name="URLOfChapter">The URL of the chapter to the book</param>
    /// <param name="start">The starter page.</param>
    /// <param name="end">The end page.</param>
    /// <remarks>If start or end are null, all pages will be downloaded.</remarks>
    public WattpadChapter GetChapter(string URLOfChapter, byte chapterNumber, int? start, int? end)
    {
        HtmlDocument doc;
        bool all = false,
        isEnd = false;

        if (start is null || end is null) { start = 1; end = 99; all = true; }
        if (start is 0) start = 1;

        WattpadChapter result = new()
        {
            Pages = new(),
            ChapterNumber = chapterNumber
        };

        while (!isEnd)
        {
            // Go in an infinite loop if the status code is NOT '200 OK'
            while (true)
            {
                doc = _htmlWeb.Load(URLOfChapter + $"/page/{start}");

                if (_htmlWeb.StatusCode is System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    Debug.WriteLine("Wattpad Ratelimit.");
                    Thread.Sleep(500);
                }
                else if (_htmlWeb.StatusCode is System.Net.HttpStatusCode.OK)
                {
                    break;
                }
            }

            isEnd = IsLastPage(doc);
            HtmlNodeCollection textContainers = doc.DocumentNode.SelectNodes(_xPathToText);

            if (textContainers is null)
            {
                throw new Exception("Failed to parse HTML!");
            }

            WattpadPage page = new()
            {
                Paragraphs = new()
            };
            #region Parse Page
            for (int i = 0; i < textContainers.Count; i++)
            {
                string text = textContainers[i].InnerText.Trim('\"').TrimEnd('\"');
                Utilities.FixString(ref text, false);
                page.Paragraphs.Add(text);
            }
            #endregion
            result.Pages.Add(page);

            #region Set Title
            if (IsLastPage(doc)) // If last page, Set title.
            {
                string x = GetChapterTitle(doc);
                Utilities.FixString(ref x, false);
                result.ChapterTitle = x;
            }
            #endregion

            if (start > end && !all) break;
            else start++;
        }

        return result;
    }
    public async Task<WattpadStory> GetBookAsync(string URLOfBook)
    {
        List<Task<WattpadChapter>> tasks = new();
        _targetDoc = _htmlWeb.Load(URLOfBook);
        WattpadStory storyContent = new()
        {
            Chapters = new(),
            StoryTitle = GetStoryTitle(),
            StoryAuthor = GetStoryAuthor()
        };
        for (byte i = 1; i < 255; i++)
        {
            try
            {
                byte asyncI = i; // Int32 uses more than byte, so we use byte :p
                string target = _site + _targetDoc.DocumentNode.SelectSingleNode($"//div[@class='story-parts']/ul/li[{i}]/a").Attributes["href"].Value.TrimStart('/');
                tasks.Add(Task.Run(async () => GetChapter(target, asyncI, null, null)));
                storyContent.Chapters.Add(new());
            }
            catch
            {
                break;
            }
        }

        while (tasks.Count > 0)
        {
            Task<WattpadChapter> completedTask = await Task.WhenAny(tasks);

            WattpadChapter chapter = await completedTask;

            storyContent.Chapters[chapter.ChapterNumber - 1] = chapter;
            tasks.Remove(completedTask);
        }
        GC.Collect();
        storyContent.OrderChapters();
        return storyContent;
    }
    private string GetChapterTitle(HtmlDocument _document)
    {
        string title = _document.DocumentNode.SelectSingleNode(_xPathToChapterTitle)?.InnerText.Trim('\"');
        // part-restart-chapter
        return title ?? _document.DocumentNode.SelectSingleNode(_xPathToChapterTitle_FALLBACK).InnerText.Trim('\"');
    }
    private string GetStoryTitle()
        => _targetDoc.DocumentNode.SelectSingleNode(_xPathStoryTitle).InnerText;
    private string GetStoryAuthor()
        => _targetDoc.DocumentNode.SelectSingleNode(_xPathStoryAuthor).InnerText;

    private static bool IsLastPage(HtmlDocument _document) => _document.DocumentNode.SelectNodes(_xPathIsLastPage) is not null;
}
