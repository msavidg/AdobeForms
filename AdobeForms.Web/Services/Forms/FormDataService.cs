using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace AdobeForms.Web.Services.Forms
{
    public class FormDataService
    {

        private string GetConnectionString()
        {
            string connectionString = null;

            System.Configuration.Configuration rootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/AdobeForms.Web");
            if (rootWebConfig.ConnectionStrings.ConnectionStrings.Count > 0)
            {
                var connString = rootWebConfig.ConnectionStrings.ConnectionStrings["Forms"];

                connectionString = connString.ConnectionString;
            }
            else
            {
                SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder();
                sqlConnectionStringBuilder.DataSource = "SVUSRYE-SQL3D1";
                sqlConnectionStringBuilder.InitialCatalog = "Navigate_NPR";
                sqlConnectionStringBuilder.IntegratedSecurity = true;

                connectionString = sqlConnectionStringBuilder.ConnectionString;
            }

            return connectionString;

        }

        public List<DataTypes.FormData> GetFormData()
        {
            string connectionString = GetConnectionString();

            List<DataTypes.FormData> formData = new List<DataTypes.FormData>();

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {

                sqlConnection.Open();

                using (SqlCommand sqlCommand = new SqlCommand("select distinct [FormEditionId], [AdobeId], [BaseFormIdString], [EditionDate], [FormIdString], [Name], [Description_FD] from [vw_FormData] where [PolicyClassId] = 4 and [DocumentTypeId] in ( 1, 5 ) and [Active_FD] = 1 and [Active_FE] = 1 and [AdobeId] is not null and HasUserFillIns = 1 order by [FormIdString];", sqlConnection))
                {

                    using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                    {

                        while (sqlDataReader.Read())
                        {

                            formData.Add(new DataTypes.FormData()
                            {
                                FormEditionId = sqlDataReader.GetGuid(sqlDataReader.GetOrdinal("FormEditionId")),
                                AdobeId = sqlDataReader.GetString(sqlDataReader.GetOrdinal("AdobeId")),
                                BaseFormIdString = sqlDataReader.GetString(sqlDataReader.GetOrdinal("BaseFormIdString")),
                                EditionDate = sqlDataReader.GetDateTime(sqlDataReader.GetOrdinal("EditionDate")),
                                FormIdString = sqlDataReader.GetString(sqlDataReader.GetOrdinal("FormIdString")),
                                Name = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Name"))
                            });

                        }
                    }
                }

                sqlConnection.Close();

            }

            return formData;
        }

    }
}