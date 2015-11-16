using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace System.Xml
{

    public class XmlNodeData
    {
        public Dictionary<string, string> Attributes { get; private set; }
        public List<XmlNodeData> Childs { get; private set; }
        public string Value { get; private set; }
        public string Name { get; private set; }

        public XmlNodeData(Stream stream) : this(XmlReader.Create(stream))
        {

        }

        public XmlNodeData(XmlReader reader)
        {
            Attributes = new Dictionary<string, string>();
            Childs = new List<XmlNodeData>();

            reader.MoveToContent();
            this.Name = reader.Name;

            if (reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                {
                    Attributes[reader.Name] = reader.Value;
                }
                reader.MoveToElement(); // Move the reader back to the element node.
            }

            // <more atr1='m1' atr2='m2'/>
            if (reader.IsEmptyElement)
                return;

            while (reader.Read())
            {

                switch (reader.NodeType)
                {
                    case XmlNodeType.Text:
                        this.Value += reader.Value;
                        break;

                    case XmlNodeType.CDATA:
                        this.Value += reader.Value;
                        break;

                    case XmlNodeType.Element:
                        Childs.Add(new XmlNodeData(reader));
                        break;

                    case XmlNodeType.EndElement:
                        return;
                }
            }
        }


        public override string ToString()
        {
            var res = this.Name;
            if (!string.IsNullOrEmpty(Value))
                res += ": " + Value;
            return res;
        }

        private void GetNodeChilds(XmlNodeData node, string path, List<XmlNodeData> list)
        {
            var pathParts = path.Trim('/').SplitValues(true, "/");
            bool isLastPathPart = pathParts.Length == 1;

            string childPath = path.Substring(pathParts[0].Length).Trim('/');

            if (node.Name == path)
            {
                list.Add(node);
                return;
            }

            foreach (var ch in node.Childs)
            {
                if (ch.Name == pathParts[0])
                {
                    if (isLastPathPart)
                        list.Add(ch);
                    else
                        GetNodeChilds(ch, childPath, list);
                }
            }
        }

        public List<XmlNodeData> GetNodes(string path)
        {
            var res = new List<XmlNodeData>();
            GetNodeChilds(this, path, res);
            return res;
        }

        public string NameAndAttributes
        {
            get
            {
                var res = this.Name;
                if (Attributes.Count > 0)
                {
                    res += ": " + Attributes.Select(a => a.Key + "='" + a.Value + "'").Join(", ");
                }
                return res;
            }
        }

        public string Attribute(string name, string defaultValue)
        {
            if (Attributes.ContainsKey(name))
                return Attributes[name];
            else
                return defaultValue;
        }

        public static XmlNodeData Test()
        {
            var xmlString =
                @"<?xml version='1.0'?>
                <!-- This is a sample XML document -->
                <Items>
                    <Item attr='111'>Item1</Item>
                    <Item attr='112'>Item2</Item>
                    <Item attr='113'>test with a child element <more atr1='m1' atr2='m2'/> stuff</Item>
                    <cdata><![CDATA[This is CDATA section. I can use all sorts of reserves characters like > < ' and &  or write things like <foo></bar>]]></cdata>
                    <bookstore>
                        <book genre='autobiography' publicationdate='1981-03-22' ISBN='1-861003-11-0'>
                            <title>The Autobiography of Benjamin Franklin</title>
                            <author>
                                <first-name>Benjamin</first-name>
                                <last-name>Franklin</last-name>
                            </author>
                            <price>8.99</price>
                        </book>
                    </bookstore>
                </Items>";

            return new XmlNodeData(XmlReader.Create(new StringReader(xmlString)));
        }
    }

}
