using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace WebsiteScraper;

/// <summary>
/// A structure that represents the data extracted from a Wattpad Story
/// </summary>
public struct WattpadStory
{
    /// <summary>
    /// The chapters of the story
    /// </summary>
    public List<WattpadChapter> Chapters;
    /// <summary>
    /// The title of the story.
    /// </summary>
    public string StoryTitle { get; set; }
    /// <summary>
    /// The author of the story.
    /// </summary>
    public string StoryAuthor { get; set; }
    public int ChapterCount { get => Chapters.Count; }
    internal void OrderChapters()
    {
        Chapters = Chapters.OrderBy(x => x.ChapterNumber).ToList();
    }
}
/// <summary>
/// A structure that represents the data extracted from a Wattpad Chapter.
/// </summary>
public struct WattpadChapter
{
    /// <summary>
    /// The chapter number of the story.
    /// </summary>
    public int ChapterNumber;
    /// <summary>
    /// The title of the chapter.
    /// </summary>
    public string ChapterTitle;
    /// <summary>
    /// The pages of the chapter
    /// </summary>
    public List<WattpadPage> Pages;
    /// <summary>
    /// Get the page count of the chapter
    /// </summary>
    public int PageCount { get => Pages.Count; }
    /// <summary>
    /// Get an specific page from the Pages list.
    /// </summary>
    /// <param name="page">The page (Index)</param>
    /// <returns>The Wattpad Page specified.</returns>
    /// <exception cref="InvalidOperationException">If the index specified is not valid this exception will be thrown.</exception>
    public WattpadPage GetPage(int page)
    {
        // Throw if PageCount is lower than the specified pages.
        if (PageCount < page) throw new InvalidOperationException($"There is no \'{page}\' page. Only \'{PageCount}\'");
        return Pages[page];
    }
}
/// <summary>
/// A structure that represents the data extracred from a Wattpad Page.
/// </summary>
public struct WattpadPage
{
    /// <summary>
    /// Each paragraph of the page.
    /// </summary>
    public List<string> Paragraphs;
    /// <summary>
    /// Get the paragraph count of the page.
    /// </summary>
    public int ParagraphCount { get => Paragraphs.Count; }
}