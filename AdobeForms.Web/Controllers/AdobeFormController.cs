using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
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

            foreach (var x in leafs)
            {
                stringBuilder.AppendLine("<br />");
                stringBuilder.AppendLine("  <div class=\"input-group input-group-lg\">");
                stringBuilder.AppendLine($"    <span class=\"input-group-addon\" id=\"lbl{x.Name.LocalName}\">{x.Attribute("name").Value}</span>");
                stringBuilder.AppendLine($"    <input id=\"{x.Name.LocalName}\" name=\"{x.Name.LocalName}\" type=\"{GetUIType(x.Attribute("datatype").Value)}\" class=\"form-control\" aria-describedby=\"lbl{x.Name.LocalName}\" title=\"XML Element Name: {x.Name.LocalName}\">");
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

        private string GetUIType(string elementType)
        {
            string uiType = "text";

            switch (elementType)
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

            }

            return uiType;
        }

    }

    public class FormValues
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
