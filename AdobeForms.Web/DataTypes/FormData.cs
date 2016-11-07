using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdobeForms.Web.DataTypes
{
    public class FormData
    {
        public Guid FormEditionId { get; set; }

        public string AdobeId { get; set; }

        public string BaseFormIdString { get; set; }

        public DateTime EditionDate { get; set; }

        public string FormIdString { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

    }
}