using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using JetBrains.Annotations;
using Spectre.Console.Cli;

namespace NZippey.Commands;

/// <summary>
/// Read multiple VCS friendly files from stdin and write one zip file to stdout.
/// </summary>
[UsedImplicitly]
public class SmudgeCommand : AsyncCommand<SmudgeCommand.Settings>
{
    [UsedImplicitly]
    public class Settings : BaseSettings
    {
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var cancellationToken = CancellationToken.None;

        settings.WriteDebug($"Starting the {nameof(SmudgeCommand)}.");

        var oldInEnc = System.Console.InputEncoding;
        var oldOutEnc = System.Console.OutputEncoding;
        try
        {
            System.Console.InputEncoding = Encoding.UTF8;
            System.Console.OutputEncoding = Encoding.UTF8;

            var cin = System.Console.In;
            await using var stdOut = System.Console.OpenStandardOutput();
            await using var zip = new ZipOutputStream(stdOut, StringCodec.FromEncoding(Encoding.UTF8));

            do
            {
                var line = await cin.ReadLineAsync(cancellationToken);
                settings.WriteDebug("line: "+line);
                if (line == null)
                {
                    break;
                }

                var metadata = FileMetadata.Parse(line);

                var entry = new ZipEntry(metadata.FileName)
                {
                    CompressionMethod = CompressionMethod.Deflated,
                };
                
                // write FileHeader
                await zip.PutNextEntryAsync(entry, cancellationToken);
                
                // write FileData
                var content = new char[metadata.Length];
                await cin.ReadAsync(content, 0, metadata.Length);
                
                byte[]? bytes = null;
                if (metadata.IsAscii)
                {
                    // content ist UTF-8 text
                    settings.WriteDebug($"Append text file: {metadata.FileName}");
                    bytes = Encoding.UTF8.GetBytes(content);
                }
                else if(metadata.IsBinary)
                {
                    // content is base64 binary
                    settings.WriteDebug($"Append binary file: {metadata.FileName}");
                    bytes = System.Convert.FromBase64CharArray(content, 0, metadata.Length);
                }
                
                await zip.WriteAsync(bytes, cancellationToken);
                await cin.ReadLineAsync(cancellationToken);
            } while (true);
        }
        finally
        {
            System.Console.InputEncoding = oldInEnc;
            System.Console.OutputEncoding = oldOutEnc;
        }

        return 0;
    }
}