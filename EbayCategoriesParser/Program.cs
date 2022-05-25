// See https://aka.ms/new-console-template for more information
using System.Reflection;
using System.Xml;



IEnumerable<(string vendorId, string name)> GetCategories()
{
    var currFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    var categoriesXmlFilePath = Path.Combine(currFolder, "Categories.xml");
    using (var reader = XmlReader.Create(categoriesXmlFilePath))
    {
        reader.ReadToFollowing("GetCategoriesResponse", "urn:ebay:apis:eBLBaseComponents");
        reader.ReadToFollowing("CategoryArray");
        reader.ReadToFollowing("Category");
        string categoryId, categoryName;
        while (reader.ReadState != ReadState.EndOfFile && (reader.Name != "CategoryArray" || reader.NodeType == XmlNodeType.EndElement))
        {
            reader.ReadToFollowing("CategoryID"); reader.Read();
            categoryId = reader.Value;
            reader.ReadToFollowing("CategoryName"); reader.Read();
            categoryName = reader.Value;
            yield return (categoryId, categoryName);
            reader.ReadToFollowing("Category");
        }
    }
}

Console.WriteLine("Hello, World!");
var maxItems = 1000;
var src = GetCategories()
    .GroupBy(x => x.name)
    .Select(x => x.First())
    .Select((item, inx) => new { item, inx })
                    .GroupBy(x => x.inx / maxItems)
                    .Select(g => g.Select(x => x.item));

var index = 0;
foreach (var part in src)
{
    var categories = "INSERT INTO [Categories]([Name],[VendorId]) VALUES\r\n" + string.Join("\r\n", part
    .Select(c => $"('{c.name.Replace("'", "''")}', '{c.vendorId}'),"));


    File.WriteAllText($"ebay-categories-{index++}.sql", categories);
}

Console.WriteLine("Done");


