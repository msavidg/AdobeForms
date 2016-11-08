using System;
using System.Linq;
using System.Security;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace AdobeForms.Processor
{
    public class XDPProcessor
    {
        public XElement GetXDPCustomFormData(string customFormData, string xdpFileName)
        {

            // NOTE:  The namespace of the <template> element can vary by what the target version of the XDP is set to.
            //        So for example the namespace of the <template> can  be something like th following:
            //        http://www.xfa.org/schema/xfa-template/X.Y/ where X.Y can be 2.5, 2.6, 2.8, 3.3 etc.
            //
            //        So we will find the <template> element using the LocalName (element name sans namespace) and get the 
            //        default namespcae for use in XPath queries


            XElement customFormDataElement = new XElement(customFormData);
            XElement xdpElement = XElement.Load(xdpFileName);
            XElement template = xdpElement.Descendants().First(d => d.Name.LocalName.Equals("template"));

            XmlNamespaceManager nsManager = new XmlNamespaceManager(new NameTable());
            nsManager.AddNamespace("xdp", "http://ns.adobe.com/xdp/");
            nsManager.AddNamespace("ns", template.GetDefaultNamespace().ToString());

            foreach (var field in xdpElement.Descendants().Where(e => e.Name.LocalName.Equals("field")))
            {
                foreach (var bind in field.Descendants().Where(d => d.Name.LocalName.Equals("bind") && d.Attributes().Any(a => a.Name.LocalName.Equals("ref"))))
                {

                    bool requiredField = false;
                    string requiredFieldMessage = String.Empty;
                    string defaultValueText = String.Empty;

                    // Get the name of the containing field of the <bind> element
                    var fieldElementName = (field.Attributes().Any(a => a.Name.LocalName == "name")) ? (field.Attribute("name").Value) : ("");

                    // Get the first child of the <ui> element, this will be the actual control seen by the user
                    /* <barcode>, <button>, <checkButton>, <choiceList>, <dateTimeEdit>, <defaultUi>, <imageEdit>, <numericEdit>, <passwordEdit>, <signature>, <textEdit> */
                    var uiControlType = field.Descendants().First(d => d.Name.LocalName.Equals("ui")).Elements().First();

                    // The <assist> element can have <speak> and/or <tooTip> child elements
                    var assist = field.Descendants().FirstOrDefault(d => d.Name.LocalName.Equals("assist"));

                    // See if the field is required and grab the validation message
                    var validate = field.XPathSelectElement("ns:validate[@nullTest='error']", nsManager);
                    if (validate != null)
                    {
                        requiredField = true;
                        var validateMessage = validate.XPathSelectElement("ns:message/ns:text[@name='nullTest']", nsManager);
                        if (validateMessage != null)
                        {
                            requiredFieldMessage = validateMessage.Value;
                        }
                    }

                    var defaultValue = field.XPathSelectElement("ns:value/ns:text", nsManager);
                    if (defaultValue != null)
                    {
                        defaultValueText = SecurityElement.Escape(defaultValue.Value);
                    }

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

                        leaf.Add(new XAttribute("name", fieldElementName));
                        leaf.Add(new XAttribute("datatype", uiControlType.Name.LocalName));

                        // This will add attributes like multiline="1" to the <textEdit>
                        foreach (var a in uiControlType.Attributes())
                        {

                            leaf.Add(new XAttribute(a.Name.LocalName, a.Value));

                        }

                        if (requiredField)
                        {

                            leaf.Add(new XAttribute("required", true));

                            if (!String.IsNullOrEmpty(requiredFieldMessage))
                            {
                                leaf.Add(new XAttribute("requiredFieldMessage", requiredFieldMessage));
                            }
                        }

                        #region Assist Values

                        // If we have and <assist> element
                        if (assist != null)
                        {

                            // We are using the <assist> and it's possible child elements to provide 
                            // an unobtrusive way to specify alternative labels for fields on the 
                            // dynamically generated forms

                            // See if there is a <speak> element
                            XElement speak = assist.Descendants("speak").FirstOrDefault();

                            // See if there is a <tooTip> element
                            XElement toolTip = assist.Descendants("toolTip").FirstOrDefault();

                            // Add these values as attributes
                            if (speak != null)
                            {
                                leaf.Add(new XAttribute("speak", speak.Value));
                            }

                            if (assist.Descendants("toolTip").FirstOrDefault() != null)
                            {
                                leaf.Add(new XAttribute("toolTip", toolTip.Value));
                            }
                        }

                        #endregion

                        #region Default Values

                        if (!String.IsNullOrEmpty(defaultValueText))
                        {
                            leaf.Add(new XAttribute("default", defaultValueText));
                        }

                        #endregion

                    }
                }
            }

            return customFormDataElement;

        }

        #region Helpers

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

        #endregion

    }

}
