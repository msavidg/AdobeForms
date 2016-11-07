using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace AdobeForms.Processor
{
    public class XDPProcessor
    {
        public XElement GetXDPCustomFormData(string customFormData, string xdpFileName)
        {

            XElement customFormDataElement = new XElement(customFormData);

            XElement xdpElement = XElement.Load(xdpFileName);

            foreach (var field in xdpElement.Descendants().Where(e => e.Name.LocalName.Equals("field")))
            {
                foreach (var bind in field.Descendants().Where(d => d.Name.LocalName.Equals("bind") && d.Attributes().Any(a => a.Name.LocalName.Equals("ref"))))
                {

                    // Get the name of the containing field of the <bind> element
                    var fieldName = (field.Attributes().Any(a => a.Name.LocalName == "name")) ? (field.Attribute("name").Value) : ("");

                    // Get the type of <ui> control
                    var uiControlType = field.Descendants().First(d => d.Name.LocalName.Equals("ui")).Elements().First().Name.LocalName;

                    // This will be the path in the forms XML that this control is bound to
                    string refAttr = bind.Attribute("ref").Value;

                    // Split path elements
                    string[] tokens = refAttr.Split("$.".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    // Find the CustomFormData or FormFillInData element in the bind path
                    int i = Array.IndexOf(tokens, customFormData);

                    // Create a destination array
                    string[] elements = tokens.Skip(i + 1).ToArray();

                    // Create the XML from the reconstructed path
                    MakeXPath(customFormDataElement, String.Join("/", (String[])elements));

                    // Get the leaf in the path
                    XElement leaf = customFormDataElement.XPathSelectElement($"//{elements[elements.Length - 1]}");

                    // Add attributes to the leaf
                    if (leaf != null)
                    {
                        leaf.Add(new XAttribute("name", fieldName));
                        leaf.Add(new XAttribute("datatype", uiControlType));
                    }
                }
            }

            return customFormDataElement;
        }

        //http://stackoverflow.com/questions/508390/create-xml-nodes-based-on-xpath

        private static XElement MakeXPath(XElement parent, string xpath)
        {

            // grab the next node name in the xpath; or return parent if empty
            string[] partsOfXPath = xpath.Trim('/').Split('/');

            string nextNodeInXPath = partsOfXPath.First();

            if (string.IsNullOrEmpty(nextNodeInXPath))
            {
                return parent;
            }

            // get or create the node from the name
            XElement node = parent.XPathSelectElement(nextNodeInXPath);

            if (node == null)
            {
                node = new XElement(nextNodeInXPath);
                parent.Add(node);
            }

            // rejoin the remainder of the array as an xpath expression and recurse
            string rest = String.Join("/", partsOfXPath.Skip(1).ToArray());

            return MakeXPath(node, rest);
        }

    }
}
