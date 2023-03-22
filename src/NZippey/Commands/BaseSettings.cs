using JetBrains.Annotations;
using Spectre.Console.Cli;

namespace NZippey.Commands;

public abstract class BaseSettings : CommandSettings
{
    [CommandOption("-d|--debug")]
    [UsedImplicitly]
    public bool Debug { get; init; }

    public void WriteDebug(string message)
    {
        if (!Debug)
        {
            return;
        }

        System.Console.Error.WriteLine(message);
    }
}