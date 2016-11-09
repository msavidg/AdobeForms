using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using System.Xml.XPath;
using AdobeForms.Processor;
using AdobeForms.Web.DataTypes;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;

namespace AdobeForms.Web.Controllers
{
    public class AdobeFormController : ApiController
    {

        [HttpGet]
        public HttpResponseMessage GetForm(string adobeFormName)
        {
            StringBuilder stringBuilder = new StringBuilder();

            XDPProcessor xdpProcessor = new XDPProcessor();

            XElement customFormDataElement = xdpProcessor.GetXDPCustomFormData("CustomFormData", adobeFormName);

            var leafs = customFormDataElement.Descendants().Where(desc => !desc.Elements().Any());

            foreach (var leaf in leafs)
            {
                stringBuilder.AppendLine("<br />");
                stringBuilder.AppendLine("  <div class=\"input-group input-group-lg\">");
                stringBuilder.AppendLine(FieldString(leaf));
                stringBuilder.AppendLine("  </div>");
            }

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

        [HttpPost]
        public HttpResponseMessage LoadForm(XElement customFormDataElement)
        {
            StringBuilder stringBuilder = new StringBuilder();

            var leafs = customFormDataElement.Descendants().Where(desc => !desc.Elements().Any()).ToList();

            foreach (var leaf in leafs)
            {
                stringBuilder.AppendLine("<br />");
                stringBuilder.AppendLine("  <div class=\"input-group input-group-lg\">");
                stringBuilder.AppendLine(FieldString(leaf));
                stringBuilder.AppendLine("  </div>");
            }

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

            string name = e.Attribute("toolTip") != null ? e.Attribute("toolTip").Value : e.Attribute("speak") != null ? e.Attribute("speak").Value : e.Attribute("name").Value;

            //http://stackoverflow.com/questions/5796383/insert-spaces-between-words-on-a-camel-cased-token

            XElement span = new XElement("span",
                Regex.Replace(name, @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $1"),
                new XAttribute("style", "width: 250px;text-align: right;"),
                new XAttribute("class", "input-group-addon"),
                new XAttribute("id", $"lbl{e.Name.LocalName}")
                );
            return span.ToString();
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

                #region date, number, password, and text

                case "date":
                case "number":
                case "password":
                case "text":

                    htmlLabel = LabelString(e);

                    if (e.Attribute("multiLine") != null)
                    {
                        XElement textarea = new XElement("textarea",
                            (!String.IsNullOrEmpty(e.Value)) ? (e.Value) : (defaultText),
                            new XAttribute("id", e.Name.LocalName),
                            new XAttribute("name", e.Name.LocalName),
                            new XAttribute("class", "form-control"),
                            new XAttribute("style", "width: 100%;"),
                            new XAttribute("aria-describedby", $"lbl{e.Name.LocalName}"),
                            new XAttribute("title", $"XML Element Name: {e.Name.LocalName}"),
                            new XAttribute("rows", "6")
                        );

                        htmlField = textarea.ToString();

                    }
                    else
                    {
                        XElement input = new XElement("input",
                            new XAttribute("id", e.Name.LocalName),
                            new XAttribute("name", e.Name.LocalName),
                            new XAttribute("type", GetUIType(e)),
                            new XAttribute("class", "form-control"),
                            new XAttribute("style", "width: 100%;"),
                            new XAttribute("aria-describedby", $"lbl{e.Name.LocalName}"),
                            new XAttribute("title", $"XML Element Name: {e.Name.LocalName}"),
                            new XAttribute("value", (!String.IsNullOrEmpty(e.Value)) ? (e.Value) : (defaultText))
                        );

                        htmlField = input.ToString();

                    }

                    break;

                #endregion

                case "button":
                    break;
                case "checkbox":

                    XElement checkbox =
                        new XElement("label", new XAttribute("class", "custom-control custom-checkbox"),
                            new XElement("input", new XAttribute("type", "checkbox"), new XAttribute("class", "custom-control-input")),
                            new XElement("span", new XAttribute("class", "custom-control-indicator")),
                            new XElement("span", new XAttribute("class", "custom-control-description"), (!String.IsNullOrEmpty(e.Value)) ? (e.Value) : (defaultText))
                        );

                    htmlField = checkbox.ToString();

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
