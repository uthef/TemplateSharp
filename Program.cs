using TemplateSharp;

class Program
{
    public static void Main(string[] args)
    {
        var startTime = DateTime.Now;
        var arguments = ArgumentPack.Parse(args);

        var writer = new TemplateWriter();

        Console.WriteLine("Processing...\n");   

        var result = writer.BuildTemplates(arguments);

        var time = (DateTime.Now - startTime).TotalSeconds;

        Console.WriteLine($"The template compilation took {time:0.000} seconds");
        Console.WriteLine($"Total amount of files: {result.TotalCount}");
        Console.WriteLine($"Skipped files: {result.SkippedCount}");
        Console.WriteLine($"Exported files: {result.TotalCount - result.SkippedCount}");
        Console.WriteLine($"Output directory: {arguments.Output}");
    }
}