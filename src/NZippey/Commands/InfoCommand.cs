using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Cli;

namespace NZippey.Commands;

[UsedImplicitly]
public class InfoCommand : Command
{
    private readonly IAnsiConsole _console;

    public InfoCommand(IAnsiConsole console)
    {
        _console = console;
    }
    
    public override int Execute(CommandContext context)
    {
        _console.WriteLine($"{GetType().Assembly.GetName().Name}, version {GetType().Assembly.GetName().Version}");
        _console.WriteLine($"Try `{Path.GetFileName(GetType().Assembly.Location)} --help` to see available commands.");
        return 1;
    }
}