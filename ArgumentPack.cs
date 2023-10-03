namespace TemplateSharp;

public class ArgumentPack
{
    public string Path { get; private set; } = "";
    public string Output { get; private set; } = "";
    public bool Minify { get; private set; } = false;

    private ArgumentPack()
    {
        
    }

    public static ArgumentPack Parse(string[] args)
    {
        var pack = new ArgumentPack();
        var properties = pack.GetType().GetProperties();

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i].ToLower().Trim();

            if (arg[0] != '-') 
            {
                continue;
            }
            
            foreach (var property in properties)
            {
                if (arg != $"-{property.Name.ToLower()}") continue;

                if (property.PropertyType == typeof(bool))
                {
                    property.SetValue(pack, true);
                    break;
                }

                if (i >= args.Length - 1) break;

                var nextArg = args[i + 1].Trim();

                if (nextArg[0] != '-')
                {
                    property.SetValue(pack, nextArg);
                    i++;
                    break;
                }
            }
        }

        if (pack.Path == "")
        {
            pack.Path = Directory.GetCurrentDirectory();
        }
        else 
        {
            pack.Path = System.IO.Path.GetFullPath(pack.Path, Directory.GetCurrentDirectory());
        }


        if (pack.Path.EndsWith("/") || pack.Path.EndsWith(@"\"))
        {
            pack.Path = pack.Path.Remove(pack.Path.Length - 1);
        }

        if (pack.Output == "")
        {
            pack.Output = Directory.CreateDirectory(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "TemplateSharp-output")).FullName;
        }

        if (pack.Output.EndsWith("/") || pack.Output.EndsWith(@"\"))
        {
            pack.Output = pack.Output.Remove(pack.Output.Length - 1);
        }


        return pack;
    }
}