using System.Diagnostics;
using CompileLzCompressor;
using Kompression.Configuration;
using Kompression.Contract;

if (args.Length <= 0)
{
    PrintHelp();
    return;
}

string? mode = null;
string? path = null;

for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "-h":
            PrintHelp();
            return;

        case "-m":
            if (i + 1 >= args.Length)
            {
                Console.WriteLine("No mode given. Use '-m' to specify a mode.");
                return;
            }

            mode = args[++i];
            if (mode is not "c" and not "d")
            {
                Console.WriteLine($"Mode '{mode}' is not valid. Use '-h' to see available options.");
                return;
            }

            break;

        case "-f":
            if (i + 1 >= args.Length)
            {
                Console.WriteLine("No file or directory given. Use '-f' to specify a file or directory.");
                return;
            }

            path = args[++i];
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                Console.WriteLine($"File or directory '{path}' does not exist.");
                return;
            }

            break;

        default:
            Console.WriteLine($"Invalid option '{args[i]}'. Use '-h' to see available options.");
            return;
    }
}

if (mode is null)
{
    Console.WriteLine("No mode given. Use '-m' to specify a mode.");
    return;
}

if (path is null)
{
    Console.WriteLine("No file or directory given. Use '-f' to specify a file or directory.");
    return;
}

ICompression compression = new CompressionConfigurationBuilder()
    .Decode.With(() => new CompileLzDecoder())
    .Encode.With(() => new CompileLzEncoder())
    .Build();

var timer = new Stopwatch();
timer.Start();

ProcessFiles(File.Exists(path)
    ? [path]
    : Directory.GetFiles(path));

Console.WriteLine($"Time: {timer.Elapsed}");

return;

static void PrintHelp()
{
    Console.WriteLine("This is the command line tool to compress and decompress files with CompileLz.");
    Console.WriteLine();
    Console.WriteLine("-h\tShows this help text.");
    Console.WriteLine("-m\tThe mode to operate in. Available modes:");
    Console.WriteLine("\t'c' for compression");
    Console.WriteLine("\t'd' for decompression");
    Console.WriteLine("-f\tThe file or directory to process");
}

void ProcessFiles(string[] filePaths)
{
    foreach (string filePath in filePaths)
        TryProcessFile(filePath);
}

void TryProcessFile(string filePath)
{
    Console.Write($"Process file '{Path.GetFileName(filePath)}'... ");

    try
    {
        ProcessFile(filePath);
    }
    catch (Exception e)
    {
        Console.WriteLine("Error");
        Console.WriteLine($"{e}");
        return;
    }

    Console.WriteLine("Ok");
}

void ProcessFile(string filePath)
{
    using Stream inputStream = File.OpenRead(filePath);

    switch (mode)
    {
        case "c":
            using (Stream outputStream = File.Create(filePath + ".comp"))
                compression.Compress(inputStream, outputStream);

            return;

        case "d":
            using (Stream outputStream = File.Create(filePath + ".dec"))
                compression.Decompress(inputStream, outputStream);

            return;
    }
}
