using System.Collections.Generic;
using VYaml.Annotations;

namespace DocFXMustache.Models.Yaml;

[YamlObject]
public partial class Root
{
    public required List<Item> Items { get; set; }
    public required Reference[] References { get; set; }
}