using System.Xml.Linq;
using System.Xml;
using System.Text;

namespace ConfigLocker;

public class XmlConfigProcessor : IConfigProcessor {
    public bool SupportsMerging => true;

    public Dictionary<string, object> ParseInput(string content) {
        if (string.IsNullOrWhiteSpace(content)) {
            return new Dictionary<string, object>();
        }

        try {
            var doc = XDocument.Parse(content);
            return ParseXmlElement(doc.Root);
        } catch (XmlException ex) {
            throw new InvalidOperationException($"Failed to parse XML: {ex.Message}");
        }
    }

    private Dictionary<string, object> ParseXmlElement(XElement? element) {
        var result = new Dictionary<string, object>();

        if (element == null) return result;

        // Handle attributes
        foreach (var attr in element.Attributes()) {
            result[$"@{attr.Name.LocalName}"] = attr.Value;
        }

        // Handle child elements
        var childGroups = element.Elements().GroupBy(e => e.Name.LocalName);
        foreach (var group in childGroups) {
            if (group.Count() == 1) {
                var child = group.First();
                if (child.HasElements || child.Attributes().Any()) {
                    result[group.Key] = ParseXmlElement(child);
                } else {
                    result[group.Key] = child.Value;
                }
            } else {
                // Multiple elements with same name - create array
                var children = group.Select(child => 
                    child.HasElements || child.Attributes().Any() 
                        ? ParseXmlElement(child) 
                        : (object)child.Value
                ).ToList();
                result[group.Key] = children;
            }
        }

        // Handle text content if no child elements
        if (!element.HasElements && !string.IsNullOrWhiteSpace(element.Value)) {
            result["#text"] = element.Value.Trim();
        }

        return result;
    }

    public string MergeAndSerialize(Dictionary<string, object> inputs, string existingContent) {
        try {
            XDocument existingDoc;
            if (string.IsNullOrWhiteSpace(existingContent)) {
                existingDoc = new XDocument(new XElement("root"));
            } else {
                existingDoc = XDocument.Parse(existingContent);
            }

            var mergedDoc = MergeXmlDocuments(existingDoc, inputs);
            
            var settings = new XmlWriterSettings {
                Indent = true,
                IndentChars = "  ",
                Encoding = Encoding.UTF8
            };

            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, settings);
            mergedDoc.Save(xmlWriter);
            
            return stringWriter.ToString();
        } catch (XmlException ex) {
            throw new InvalidOperationException($"Failed to merge/serialize XML: {ex.Message}");
        }
    }

    private XDocument MergeXmlDocuments(XDocument existingDoc, Dictionary<string, object> inputs) {
        var root = existingDoc.Root ?? new XElement("root");
        
        foreach (var kvp in inputs) {
            MergeXmlElement(root, kvp.Key, kvp.Value);
        }

        return new XDocument(root);
    }

    private void MergeXmlElement(XElement parent, string key, object value) {
        if (key.StartsWith("@")) {
            // Handle attribute
            var attrName = key.Substring(1);
            parent.SetAttributeValue(attrName, value);
        } else if (key == "#text") {
            // Handle text content
            parent.Value = value.ToString() ?? "";
        } else if (value is Dictionary<string, object> dict) {
            // Handle nested object
            var element = parent.Element(key);
            if (element == null) {
                element = new XElement(key);
                parent.Add(element);
            }
            
            foreach (var kvp in dict) {
                MergeXmlElement(element, kvp.Key, kvp.Value);
            }
        } else if (value is List<object> list) {
            // Handle array
            // Remove existing elements with this name
            parent.Elements(key).Remove();
            
            foreach (var item in list) {
                if (item is Dictionary<string, object> itemDict) {
                    var element = new XElement(key);
                    foreach (var kvp in itemDict) {
                        MergeXmlElement(element, kvp.Key, kvp.Value);
                    }
                    parent.Add(element);
                } else {
                    parent.Add(new XElement(key, item));
                }
            }
        } else {
            // Handle simple value
            var element = parent.Element(key);
            if (element == null) {
                parent.Add(new XElement(key, value));
            } else {
                element.Value = value.ToString() ?? "";
            }
        }
    }
} 