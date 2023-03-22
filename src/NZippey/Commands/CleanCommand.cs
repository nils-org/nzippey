using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using JetBrains.Annotations;
using Spectre.Console.Cli;

namespace NZippey.Commands;

/// <summary>
/// Read one zip file from stdin and write multiple (unzipped) VCS friendly files to stdout.
/// </summary>
[UsedImplicitly]
public class CleanCommand : AsyncCommand<CleanCommand.Settings>
{
    [UsedImplicitly]
    public class Settings : BaseSettings
    {
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var cancellationToken = CancellationToken.None;

        settings.WriteDebug($"Starting the {nameof(CleanCommand)}.");

        var oldInEnc = System.Console.InputEncoding;
        var oldOutEnc = System.Console.OutputEncoding;

        bool IsTextFile(string extension)
        {
            var ext = extension.ToLowerInvariant();
            return ext is ".txt" or ".html" or ".xml"; // todo: should this "ask" git somehow?
        }

        bool IsAscii(IEnumerable<byte> bytes)
        {
            return bytes.All(b => b is >= 32 and <= 127);
        }

        try
        {
            System.Console.InputEncoding = Encoding.UTF8;
            System.Console.OutputEncoding = Encoding.UTF8;

            await using var stdOut = System.Console.Out;
            using var cin = new MemoryStream(); // need a seekable stream
            await System.Console.OpenStandardInput().CopyToAsync(cin, cancellationToken);
            cin.Position = 0;
            await using var zipStream = new ZipInputStream(cin, StringCodec.FromEncoding(Encoding.UTF8));
            using var zip = new ZipFile(cin);

            foreach(ZipEntry file in zip)
            {
                await using var contentStream = zip.GetInputStream(file);
                var extension = Path.GetExtension(file.Name);
                var isTextFile = IsTextFile(extension);

                using var mem = new MemoryStream();
                await contentStream.CopyToAsync(mem, cancellationToken);
                mem.Position = 0;
                using var reader = new BinaryReader(mem);
                var content = reader.ReadBytes((int)mem.Length);

                if(isTextFile || IsAscii(content))
                {
                    // content is text
                    settings.WriteDebug($"Append text file: {file.Name}");
                    await stdOut.WriteLineAsync(FileMetadata.Ascii(file.Name, content.Length).ToString());
                    await stdOut.WriteLineAsync(Encoding.UTF8.GetString(content));
                }
                else
                {
                    // content is binary
                    settings.WriteDebug($"Append binary file: {file.Name}");
                    var data = Convert.ToBase64String(content);
                    await stdOut.WriteLineAsync(FileMetadata.Binary(file.Name, data.Length, content.Length).ToString());
                    await stdOut.WriteLineAsync(data);
                }
            }
        }
        finally
        {
            System.Console.InputEncoding = oldInEnc;
            System.Console.OutputEncoding = oldOutEnc;
        }

        return 0;
    }
}