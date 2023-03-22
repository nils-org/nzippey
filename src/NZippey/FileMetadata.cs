namespace NZippey;

/// <summary>
/// length|raw_length|type|filename
///
/// where length is an ascii coded integer of the length of the following data section,
/// raw_length is the original length of data (if transformation is taken),
/// type is A for text data and B for binary data,
/// and filename is the original file name (including path if the zipped file contains directories).
/// </summary>
public class FileMetadata
{
    private readonly string _mode;

    public string FileName { get; }
    public int Length { get; }
    public int RawLength { get; }

    public bool IsAscii => _mode == "A";

    public bool IsBinary => _mode == "B";

    private FileMetadata(string mode, string fileName, int length, int rawLength)
    {
        FileName = fileName;
        Length = length;
        RawLength = rawLength;
        _mode = mode;
    }

    public static FileMetadata Parse(string line)
    {
        var parts = line.Split('|', 4);
        var length = int.Parse(parts[0]);
        var rawLen = int.Parse(parts[1]);
        var mode = parts[2];
        var name = parts[3].Trim();
        
        var meta = new FileMetadata(mode, name, length, rawLen);
        if (meta is { IsAscii: false, IsBinary: false })
        {
            throw new NotSupportedException($"Not supported conversion mode: {mode}");
        }

        return meta;
    }

    public static FileMetadata Ascii(string fileName, int length)
    {
        return new FileMetadata("A", fileName, length, length);
    }

    public static FileMetadata Binary(string fileName, int length, int rawLength)
    {
        return new FileMetadata("B", fileName, length, rawLength);
    }

    public override string ToString()
    {
        return $"{Length}|{RawLength}|{_mode}|{FileName}";
    }
}