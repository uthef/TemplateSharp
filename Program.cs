using System.Text.RegularExpressions;
using AngleSharp.Html;
using AngleSharp.Html.Parser;

namespace TemplateSharp;

internal class Program
{
    public static void Main(string[] args)
    {
        var arguments = ArgumentPack.Parse(args);
        var files = GetFiles(arguments.Path);

        var parser = new HtmlParser(new HtmlParserOptions {
            IsAcceptingCustomElementsEverywhere = true
        });
        var emptyLineRegex = new Regex(@"^(\t|\s)*\n", RegexOptions.Multiline);
        var compiler = new TemplateCompiler(arguments);
        var skippedCount = 0;

        Console.WriteLine("Processing...\n");

        var startTime = DateTime.Now;

        foreach (var file in files)
        {
            using var fs = new FileStream(file, FileMode.Open);
            var document = parser.ParseDocument(fs);

            var skip = document.QuerySelector("skip");
            if (skip is {})
            {
                skippedCount++;
                continue;
            } 

            document = compiler.ProcessDocument(parser, document, null, file);

            using var writer = new StringWriter();

            document.ToHtml(writer, new PrettyMarkupFormatter() {
                Indentation = arguments.Minify ? "" : "\t",
                NewLine = arguments.Minify ? "" : "\n"
            });

            var outputPath = Path.Combine(arguments.Output, Path.GetRelativePath(arguments.Path, file));
            var dir = Path.GetDirectoryName(outputPath);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir!);
            }

            File.WriteAllText(outputPath, emptyLineRegex.Replace(writer.ToString(), ""));
        }

        var time = (DateTime.Now - startTime).TotalSeconds;
        Console.WriteLine($"The template compilation took {time:0.000} seconds");
        Console.WriteLine($"Total amount of files: {files.Count}");
        Console.WriteLine($"Skipped files: {skippedCount}");
        Console.WriteLine($"Exported files: {files.Count - skippedCount}");
    }

    private static List<string> GetFiles(string path, List<string>? files = null)
    {
        if (files is not {}) 
        {
            files = new List<string>();
        }

        files.AddRange(Directory.GetFiles(path, "*.html").ToList());
        var dirs = Directory.GetDirectories(path);

        foreach (var dir in dirs)
        {
            GetFiles(Path.Combine(path, dir), files);
        }

        return files;
    }
}