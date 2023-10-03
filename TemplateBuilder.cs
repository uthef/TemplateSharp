using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace TemplateSharp;

class TemplateBuilder
{
    public ArgumentPack Arguments { get; private set; }
    public TemplateBuilder(ArgumentPack arguments)
    {
        Arguments = arguments;
    }

    public IHtmlDocument ProcessDocument(IHtmlParser parser, IHtmlDocument document, Dictionary<string, string>? attributes = null, string filePath = "")
    {
        var inclusions = document.QuerySelectorAll("include");
        var parent = document.QuerySelector("parent");
        
        var skips = document.QuerySelectorAll("skip");

        foreach (var skip in skips)
        {
            skip.Remove();
        }

        CheckRequirements(document, attributes);
        ReplaceAttributes(document, attributes);

        foreach (var inclusion in inclusions)
        {
            if (inclusion.Text().Trim().Length > 0)
            {
                var inclusionAttributes = inclusion.Attributes.ToDictionary(x => x.Name, x => x.Value);

                if (attributes is {})
                {
                    attributes.ToList().ForEach(x => inclusionAttributes.Add(x.Key, x.Value));
                }

                var path = inclusion.Text().Trim();

                var newPath = Path.Combine(new [] { Arguments.Path, Path.GetDirectoryName(filePath)!, path });
                using var fs = new FileStream(newPath, FileMode.Open);
                var newDoc = parser.ParseDocument(fs);
                var includedDocument = ProcessDocument(parser, newDoc, inclusionAttributes, newPath);

                var innerSkips = includedDocument.QuerySelectorAll("skip");
                
                foreach (var innerSkip in innerSkips)
                {
                    innerSkip.Remove();
                }

                if (includedDocument is null || includedDocument.Body is null)
                {
                    continue;
                }

                inclusion.Replace(includedDocument.Body.ChildNodes.ToArray());
            }
        }

        if (parent is {})
        {
            var childAttributes = parent.Attributes.ToDictionary(x => x.Name, x => x.Value);

            parent.Remove();

            if (parent.Text().Trim().Length > 0)
            {
                var path = parent.Text().Trim();
                var newPath = Path.Combine(new [] { Arguments.Path, Path.GetDirectoryName(filePath)!, path });
                using var fs = new FileStream(newPath, FileMode.Open);
                var newDoc = parser.ParseDocument(fs);

                if (attributes is {})
                {
                    attributes.ToList().ForEach(x => childAttributes.Add(x.Key, x.Value));
                }

                var parentDocument = ProcessDocument(parser, newDoc, childAttributes, newPath);
            
                var content = parentDocument.QuerySelector("content");

                if (content is {} && parentDocument is not null && parentDocument.Body is not null && document.Body is {})
                {
                    content.Replace(document.Body.ChildNodes.ToArray());

                    return parentDocument;
                }
            }
        }

        MoveHeadData(document);

        return document;
    }

    private void ReplaceAttributes(IHtmlDocument document, Dictionary<string, string>? attributes)
    {
        var tags = document.QuerySelectorAll("attr");

        foreach (var variable in tags)
        {
            string? name = variable.Text();
            variable.Parent?.ReplaceChild(document.CreateTextNode(GetValue(attributes, name) ?? "NULL"), variable);
        }

        var inlineVariables = document.QuerySelectorAll("[attr]");
        var regex = new Regex(@"@@\w+");

        foreach (var tag in inlineVariables)
        {
            string text = tag.Text();

            var matches = regex.Matches(text);

            foreach (Match match in matches)
            {
                var name = match.Value.Replace("@@", "");
                tag.InnerHtml = tag.InnerHtml.Replace(match.Value, GetValue(attributes, name) ?? "NULL");
            }

            tag.RemoveAttribute("attr");
        }
    }

    private void CheckRequirements(IHtmlDocument document, Dictionary<string, string>? attributes)
    {
        var tags = document.QuerySelectorAll("require[name]");

        if (attributes is not {})
        {
            tags.All(x => {
                x.Remove();
                return true;
            });

            return;
        }

        foreach (var tag in tags)
        {
            if (tag.HasAttribute("name") && attributes.ContainsKey(tag.Attributes["name"]!.Value))
            {
                if (!tag.HasAttribute("value") || attributes[tag.Attributes["name"]!.Value] == tag.Attributes["value"]!.Value)
                {
                    var children = tag.ChildNodes;
                    tag.Replace(children.ToArray());
                }
            }

            tag.Remove();
        }
    }

    private void MoveHeadData(IHtmlDocument document)
    {
        var headdataTags = document.QuerySelectorAll("headdata");

        foreach (var tag in headdataTags)
        {
            document.Head?.AppendNodes(tag.ChildNodes.ToArray());
            tag.Remove();
        }
    }

    private string? GetValue(Dictionary<string, string>? dictionary, string name)
    {
        if (dictionary?.ContainsKey(name) == true)
        {
            return dictionary[name];
        }

        return null;
    }
}