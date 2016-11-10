using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Xml.Linq;
using AdobeForms.Processor;

namespace AdobeForms.Web.Controllers
{
    public class AdobeFormController : ApiController
    {
        // NOTE:  When we use the term "enhanced XML" we mean the CustomFormData or FormFillInData
        //        in the correct hierachical structure with additional attributes to allow for HTML
        //        generation

        /// <summary>
        /// GetForm returns html that can be used to provide values for XDP user entered fields
        /// </summary>
        /// <param name="adobeFormName">Full path to XDP file</param>
        /// <returns>HTML string</returns>
        [HttpGet]
        public HttpResponseMessage GetForm(string adobeFormName)
        {

            StringBuilder stringBuilder = new StringBuilder();
            XDPProcessor xdpProcessor = new XDPProcessor();

            // Get the enhanced CustomFormData or FormFillInData XML in the correct element hierarchy
            XElement customFormDataElement = xdpProcessor.GetXDPCustomFormData("CustomFormData", adobeFormName);

            // The leafs are what have values and are what are bound to the UI controls in the XDP
            var leafs = customFormDataElement.Descendants().Where(desc => !desc.Elements().Any());

            stringBuilder.AppendLine("<br />");
            stringBuilder.AppendLine("  <div class=\"container\">");
            foreach (var leaf in leafs)
            {
                stringBuilder.AppendLine(FieldString(leaf));
            }
            stringBuilder.AppendLine("  </div>");

            // We are going to encode the enhanced XML and send out as payload on the form
            var bytes = Convert.ToBase64String(Encoding.UTF8.GetBytes(customFormDataElement.ToString()));

            stringBuilder.AppendLine($"<input id=\"CustomFormData\" name=\"CustomFormData\" type=\"hidden\" value=\"{bytes}\"></input>");
            stringBuilder.AppendLine("<br />");

            return new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(stringBuilder.ToString())
                {
                    Headers =
                    {
                        ContentType =  new MediaTypeHeaderValue( MediaTypeNames.Text.Html )
                    }
                }
            };
        }

        /// <summary>
        /// GetForm returns html that can be used to edit existing values for XDP user entered fields
        /// </summary>
        /// <param name="customFormDataElement">The enhanced XML produced by calling SaveForm.</param>
        /// <returns>HTML string</returns>
        [HttpPost]
        public HttpResponseMessage LoadForm(XElement customFormDataElement)
        {

            StringBuilder stringBuilder = new StringBuilder();

            // The leafs are what have values and are what are bound to the UI controls in the XDP
            var leafs = customFormDataElement.Descendants().Where(desc => !desc.Elements().Any()).ToList();

            stringBuilder.AppendLine("<br />");
            stringBuilder.AppendLine("  <div class=\"container\">");
            foreach (var leaf in leafs)
            {
                stringBuilder.AppendLine(FieldString(leaf));
            }
            stringBuilder.AppendLine("  </div>");

            // We are going to encode the enhanced XML and send out as payload on the form
            var bytes = Convert.ToBase64String(Encoding.UTF8.GetBytes(customFormDataElement.ToString()));

            stringBuilder.AppendLine($"<input id=\"CustomFormData\" name=\"CustomFormData\" type=\"hidden\" value=\"{bytes}\"></input>");
            stringBuilder.AppendLine("<br />");

            return new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(stringBuilder.ToString())
                {
                    Headers =
                    {
                        ContentType =  new MediaTypeHeaderValue( MediaTypeNames.Text.Html )
                    }
                }
            };
        }

        /// <summary>
        /// SaveForm returns enhanced XML that is populated with the values
        /// </summary>
        /// <param name="data">Array of FormValues whicha are Name/Value pairs.</param>
        /// <returns>XML string</returns>
        [HttpPost]
        public HttpResponseMessage SaveForm(FormValues[] data)
        {

            //http://stackoverflow.com/questions/2941255/how-to-send-binary-data-within-an-xml-string

            string s = data.First(e => e.Name.Equals("CustomFormData")).Value;

            var byteArray = Convert.FromBase64String(s);

            XElement customFormDataElement = XElement.Parse(Encoding.UTF8.GetString(byteArray));

            var leafs = customFormDataElement.Descendants().Where(desc => !desc.Elements().Any());

            foreach (var leaf in leafs)
            {
                leaf.Value = data.First(e => e.Name.Equals(leaf.Name.LocalName)).Value;
            }

            return new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(customFormDataElement.ToString())
                {
                    Headers =
                    {
                        ContentType =  new MediaTypeHeaderValue( MediaTypeNames.Text.Html )
                    }
                }
            };

        }

        private string GetUIType(XElement e)
        {

            string uiType = String.Empty;

            // These are ALL of the UI controls from the Adobe template specification
            // Not all of them make sense in the context of XDP user entered data.

            switch (e.Attribute("datatype").Value)
            {

                case "barcode":
                    uiType = "text";
                    break;
                case "button":
                    uiType = "button";
                    break;
                case "checkButton":
                    uiType = "checkbox";
                    break;
                case "choiceList":
                    uiType = "text";
                    break;
                case "dateTimeEdit":
                    uiType = "date";
                    break;
                case "defaultUi":
                    uiType = "text";
                    break;
                case "imageEdit":
                    uiType = "image";
                    break;
                case "numericEdit":
                    uiType = "number";
                    break;
                case "passwordEdit":
                    uiType = "password";
                    break;
                case "signature":
                    uiType = "text";
                    break;
                case "textEdit":
                    uiType = "text";
                    break;
                default:
                    throw new ArgumentException($"Unknown datatype {e.Attribute("datatype").Value}");
            }

            return uiType;
        }

        private string LabelString(XElement e)
        {

            // If there is no toolTip attribute or speak attribute, the we default to the name attribute
            string name = e.Attribute("toolTip") != null ? e.Attribute("toolTip").Value : e.Attribute("speak") != null ? e.Attribute("speak").Value : e.Attribute("name").Value;

            // Boosted from SO question:
            //http://stackoverflow.com/questions/5796383/insert-spaces-between-words-on-a-camel-cased-token

            return Regex.Replace(name, @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $1");
        }

        private string FieldString(XElement e)
        {
            string htmlLabel = String.Empty;
            string htmlField = String.Empty;
            string defaultText = String.Empty;

            if (e.Attributes().Any(a => a.Name.LocalName.Equals("default")))
            {
                defaultText = e.Attributes().First(a => a.Name.LocalName.Equals("default")).Value;
            }

            switch (GetUIType(e))
            {

                case "button":

                    // This is an allowed UI type in the XDP, not sure how it would apply to our user entered fields

                    XElement buttonContainer = new XElement("div", new XAttribute("class", "form-group row"));
                    XElement buttonLabel = new XElement("label", new XAttribute("for", e.Name.LocalName), new XAttribute("class", "col-sm-2 col-form-label"), LabelString(e));
                    XElement buttonField = new XElement("div",
                                                new XAttribute("class", "col-sm-10"),
                                                    new XElement("input",
                                                        new XAttribute("type", GetUIType(e)),
                                                        new XAttribute("class", "form-check"),
                                                        new XAttribute("id", e.Name.LocalName),
                                                        new XAttribute("name", e.Name.LocalName)
                                                    )
                                            );

                    buttonContainer.Add(buttonLabel);
                    buttonContainer.Add(buttonField);

                    htmlField = buttonContainer.ToString();

                    break;

                case "checkbox":

                    XElement checkboxContainer = new XElement("div", new XAttribute("class", "form-group row"));
                    XElement checkboxLabel = new XElement("label", new XAttribute("for", e.Name.LocalName), new XAttribute("class", "col-sm-2 col-form-label"), LabelString(e));
                    XElement checkboxField = new XElement("div",
                                                new XAttribute("class", "col-sm-10"),
                                                    new XElement("input",
                                                        new XAttribute("type", GetUIType(e)),
                                                        new XAttribute("class", "form-check"),
                                                        new XAttribute("id", e.Name.LocalName),
                                                        new XAttribute("name", e.Name.LocalName)
                                                    )
                                            );

                    checkboxContainer.Add(checkboxLabel);
                    checkboxContainer.Add(checkboxField);

                    htmlField = checkboxContainer.ToString();

                    break;

                case "date":

                    XElement dateContainer = new XElement("div", new XAttribute("class", "form-group row"));
                    XElement dateLabel = new XElement("label", new XAttribute("for", e.Name.LocalName), new XAttribute("class", "col-sm-2 col-form-label"), LabelString(e));
                    XElement dateField = new XElement("div",
                                                new XAttribute("class", "col-sm-10"),
                                                    new XElement("input",
                                                        new XAttribute("type", GetUIType(e)),
                                                        new XAttribute("class", "form-control"),
                                                        new XAttribute("id", e.Name.LocalName),
                                                        new XAttribute("name", e.Name.LocalName),
                                                        new XAttribute("value", (!String.IsNullOrEmpty(e.Value)) ? (e.Value) : (defaultText))
                                                    )
                                            );

                    dateContainer.Add(dateLabel);
                    dateContainer.Add(dateField);

                    htmlField = dateContainer.ToString();

                    break;

                case "number":

                    XElement numberContainer = new XElement("div", new XAttribute("class", "form-group row"));
                    XElement numberLabel = new XElement("label", new XAttribute("for", e.Name.LocalName), new XAttribute("class", "col-sm-2 col-form-label"), LabelString(e));
                    XElement numberField = new XElement("div",
                                                new XAttribute("class", "col-sm-10"),
                                                    new XElement("input",
                                                        new XAttribute("type", GetUIType(e)),
                                                        new XAttribute("class", "form-control"),
                                                        new XAttribute("id", e.Name.LocalName),
                                                        new XAttribute("name", e.Name.LocalName),
                                                        new XAttribute("placeholder", LabelString(e)),
                                                        new XAttribute("value", (!String.IsNullOrEmpty(e.Value)) ? (e.Value) : (defaultText))
                                                    )
                                            );

                    numberContainer.Add(numberLabel);
                    numberContainer.Add(numberField);

                    htmlField = numberContainer.ToString();

                    break;

                case "password":

                    XElement passwordContainer = new XElement("div", new XAttribute("class", "form-group row"));
                    XElement passwordLabel = new XElement("label", new XAttribute("for", e.Name.LocalName), new XAttribute("class", "col-sm-2 col-form-label"), LabelString(e));
                    XElement passwordField = new XElement("div",
                                                new XAttribute("class", "col-sm-10"),
                                                    new XElement("input",
                                                        new XAttribute("type", GetUIType(e)),
                                                        new XAttribute("class", "form-control"),
                                                        new XAttribute("id", e.Name.LocalName),
                                                        new XAttribute("name", e.Name.LocalName),
                                                        new XAttribute("placeholder", "Password"),
                                                        new XAttribute("value", (!String.IsNullOrEmpty(e.Value)) ? (e.Value) : (defaultText))
                                                    )
                                            );

                    passwordContainer.Add(passwordLabel);
                    passwordContainer.Add(passwordField);

                    htmlField = passwordContainer.ToString();

                    break;

                case "text":

                    XElement textContainer = new XElement("div", new XAttribute("class", "form-group row"));
                    XElement textLabel = new XElement("label", new XAttribute("for", e.Name.LocalName), new XAttribute("class", "col-sm-2 col-form-label"), LabelString(e));
                    XElement textField = null;
                    if (e.Attribute("multiLine") != null)
                    {
                        textField = new XElement("div",
                            new XAttribute("class", "col-sm-10"),
                            new XElement("textarea",
                                new XAttribute("class", "form-control"),
                                new XAttribute("id", e.Name.LocalName),
                                new XAttribute("name", e.Name.LocalName),
                                new XAttribute("placeholder", LabelString(e)),
                                (!String.IsNullOrEmpty(e.Value)) ? (e.Value) : (defaultText)
                            )
                        );
                    }
                    else
                    {
                        textField = new XElement("div",
                            new XAttribute("class", "col-sm-10"),
                            new XElement("input",
                                new XAttribute("type", GetUIType(e)),
                                new XAttribute("class", "form-control"),
                                new XAttribute("id", e.Name.LocalName),
                                new XAttribute("name", e.Name.LocalName),
                                new XAttribute("placeholder", LabelString(e)),
                                new XAttribute("value", (!String.IsNullOrEmpty(e.Value)) ? (e.Value) : (defaultText))
                            )
                        );
                    }

                    textContainer.Add(textLabel);
                    textContainer.Add(textField);

                    htmlField = textContainer.ToString();

                    break;
            }

            return htmlLabel + htmlField;

        }
    }

    public class FormValues
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

}
