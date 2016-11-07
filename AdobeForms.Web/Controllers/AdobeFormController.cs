using System;
using System.Collections.Generic;
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
                stringBuilder.AppendLine("<div class=\"input-group input-group-lg\">");
                stringBuilder.AppendLine($"    <span class=\"input - group - addon\" id=\"basic - addon1\">{x.Attribute("name").Value}</span>");
                stringBuilder.AppendLine($"    <input type=\"text\" class=\"form-control\" placeholder=\"{x.Attribute("name").Value}\" aria-describedby=\"basic-addon1\">");
                stringBuilder.AppendLine("</div>");
            }

            return new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(stringBuilder.ToString())
                {
                    Headers =
                    {
                        ContentType =  new MediaTypeHeaderValue( MediaTypeNames.Text.Html.ToString())
                    }
                }
            };
        }
    }
}
