using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AdobeForms.Processor
{
    public class XDPProcessor
    {
        public XElement GetXDPCustomFormData(string xdpFileName)
        {

            string filePath = String.Format(@"C:\dev\FormsLibrary\EXC\Documents\{0}", xdpFileName);

            XElement customFormData = new XElement("CustomFormData");

            XElement xdpElement = XElement.Load(filePath);


            return customFormData;
        }
    }
}
