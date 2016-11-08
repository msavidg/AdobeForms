using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using AdobeForms.Processor;
using AdobeForms.Web.DataTypes;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;

namespace AdobeForms.Web.Controllers
{
    public class AdobeFormController : ApiController
    {

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
                stringBuilder.AppendLine(LabelString(leaf));
                stringBuilder.AppendLine(FieldString(leaf));
                stringBuilder.AppendLine("  </div>");
            }
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

            var x = this.Request.GetQueryNameValuePairs();

            return new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("")
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
            //http://stackoverflow.com/questions/5796383/insert-spaces-between-words-on-a-camel-cased-token

            XElement span = new XElement("span",
                Regex.Replace(e.Attribute("name").Value, @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $1"),
                new XAttribute("style", "width: 250px;text-align: right;"),
                new XAttribute("class", "input-group-addon"),
                new XAttribute("id", $"lbl{e.Name.LocalName}")
                );
            return span.ToString();
        }

        private string FieldString(XElement e)
        {
            string defaultText = String.Empty;

            if (e.Attributes().Any(a => a.Name.LocalName.Equals("default")))
            {
                defaultText = e.Attributes().First(a => a.Name.LocalName.Equals("default")).Value;
            }

            if (e.Attribute("multiLine") != null)
            {
                XElement textarea = new XElement("textarea",
                    defaultText,
                    new XAttribute("id", e.Name.LocalName),
                    new XAttribute("name", e.Name.LocalName),
                    new XAttribute("class", "form-control"),
                    new XAttribute("style", "width: 100%;"),
                    new XAttribute("aria-describedby", $"lbl{e.Name.LocalName}"),
                    new XAttribute("title", $"XML Element Name: {e.Name.LocalName}"),
                    new XAttribute("rows", "6")
                    );
                return textarea.ToString();
            }

            XElement input = new XElement("input",
                new XAttribute("id", e.Name.LocalName),
                new XAttribute("name", e.Name.LocalName),
                new XAttribute("type", GetUIType(e)),
                new XAttribute("class", "form-control"),
                new XAttribute("style", "width: 100%;"),
                new XAttribute("aria-describedby", $"lbl{e.Name.LocalName}"),
                new XAttribute("title", $"XML Element Name: {e.Name.LocalName}"),
                new XAttribute("value", defaultText)
                );
            return input.ToString();
        }
    }

    public class FormValues
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
