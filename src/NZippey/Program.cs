using Spectre.Console;
using Spectre.Console.Cli;
using NZippey.Commands;

/*
 *  This was inspired by Zippey https://bitbucket.org/sippey/zippey/
 */

var app = new CommandApp<InfoCommand>();
app.Configure(c =>
{
    c.AddCommand<SmudgeCommand>("smudge")
        .WithDescription("process smudge data");

    c.AddCommand<CleanCommand>("clean")
        .WithDescription("process clean data");

    // todo: add "unzip -c -a" equivalent.

    c.SetExceptionHandler(ex => {
        AnsiConsole.WriteException(ex, ExceptionFormats.Default);
    });
});
return app.Run(args);