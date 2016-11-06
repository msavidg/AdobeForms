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

            stringBuilder.AppendLine($"<h2>{DateTime.Now.ToString("o")}</h2>");

            AdobeForms.Processor.XDPProcessor xdpProcessor = new XDPProcessor();

            string s = HttpContext.Current.Server.MapPath(adobeFormName);

            XElement customFormDataElement = xdpProcessor.GetXDPCustomFormData(adobeFormName);

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
