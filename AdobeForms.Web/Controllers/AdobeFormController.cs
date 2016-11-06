using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Web;
using System.Web.Http;

namespace AdobeForms.Web.Controllers
{
    public class AdobeFormController : ApiController
    {

        public HttpResponseMessage GetForm(string adobeFormName)
        {

            string s = $"<div><h1>{DateTime.Now.ToString("O")}</h1></div>";

            return new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(s)
                {
                    Headers =
                    {
                        ContentLength = s.Length,
                        ContentType =  new MediaTypeHeaderValue( MediaTypeNames.Text.Html.ToString())
                    }
                }
            };
        }
    }
}
