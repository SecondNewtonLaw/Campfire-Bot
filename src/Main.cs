using System;
using System.IO;
using System.Threading.Tasks;
using Spectre.Console;

namespace CampfireBot;

class MainActivity
{
    private static string Token { get; } = Environment.GetEnvironmentVariable("BOT_TOKEN_CFBOT");

    public static async Task Main()
    {
        AnsiConsole.MarkupLine("[red]Hello, World![/]");
    }
}