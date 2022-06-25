using System;
namespace WebsiteScraper;
public class NoResultsException : Exception
{
    public NoResultsException() { }
    public NoResultsException(string message) : base(message) { }
    public NoResultsException(string message, System.Exception inner) : base(message, inner) { }
    public NoResultsException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}