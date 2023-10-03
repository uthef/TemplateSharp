using System.Text.RegularExpressions;
using AngleSharp.Html;
using AngleSharp.Html.Parser;

namespace TemplateSharp;

public class TemplateWriter
{
    public BuildResult BuildTemplates(ArgumentPack arguments)
    {
        var files = GetFiles(arguments.Path);

        var parser = new HtmlParser(new HtmlParserOptions {
            IsAcceptingCustomElementsEverywhere = true
        });

        var emptyLineRegex = new Regex(@"^(\t|\s)*\n", RegexOptions.Multiline);
        var compiler = new TemplateBuilder(arguments);
        var skippedCount = 0;

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

        return new(files.Count, skippedCount);
    }

    private List<string> GetFiles(string path, List<string>? files = null)
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