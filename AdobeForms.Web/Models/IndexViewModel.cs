using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdobeForms.Web.DataTypes;

namespace AdobeForms.Web.Models
{
    public class IndexViewModel
    {

        public List<FormData> Forms { get; set; }

        public IndexViewModel()
        {

            Services.Forms.FormDataService formData = new Services.Forms.FormDataService();
            this.Forms = formData.GetFormData();

        }

    }
}