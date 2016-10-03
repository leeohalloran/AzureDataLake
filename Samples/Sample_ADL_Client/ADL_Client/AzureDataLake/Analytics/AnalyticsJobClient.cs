using System.Collections.Generic;
using Microsoft.Azure.Management.DataLake.Analytics;
using Microsoft.Azure.Management.DataLake.Analytics.Models;
using ADL=Microsoft.Azure.Management.DataLake;

namespace AzureDataLake.Analytics
{
    public class AnalyticsJobClient : AccountClientBase
    {
        private ADL.Analytics.DataLakeAnalyticsJobManagementClient _adla_job_rest_client;
        private ADL.Analytics.IDataLakeAnalyticsCatalogManagementClient _adla_catalog_rest_client;

        public AnalyticsJobClient(string account, AzureDataLake.Authentication.AuthenticatedSession authSession) :
            base(account, authSession)
        {
            if (this._adla_job_rest_client == null)
            {
                this._adla_job_rest_client = new ADL.Analytics.DataLakeAnalyticsJobManagementClient(this.AuthenticatedSession.Credentials);
                this._adla_catalog_rest_client  = new ADL.Analytics.DataLakeAnalyticsCatalogManagementClient(this.AuthenticatedSession.Credentials);
            }
        }

        public ADL.Analytics.Models.USqlDatabase GetDatabase(GetJobListPagedOptions options, string name)
        {

            var db = this._adla_catalog_rest_client.Catalog.GetDatabase(this.Account, name);
            return db;
        }

        public IEnumerable<ADL.Analytics.Models.USqlDatabase[]> ListDatabases()
        {
            var oDataQuery = new Microsoft.Rest.Azure.OData.ODataQuery<ADL.Analytics.Models.USqlDatabase>();

            string @select = null;
            bool? count = null;
            string search = null;
            string format = null;

            // Handle the initial response
            var page = this._adla_catalog_rest_client.Catalog.ListDatabases(this.Account, oDataQuery, @select, count);
            foreach (var cur_page in RESTUtil.EnumPages<ADL.Analytics.Models.USqlDatabase> (page, p => this._adla_catalog_rest_client.Catalog.ListDatabasesNext(p.NextPageLink)))
            {
                yield return cur_page;
            }
        }

        public IEnumerable<ADL.Analytics.Models.USqlAssemblyClr[]> ListAssemblies(string name, string dbname)
        {
            var oDataQuery = new Microsoft.Rest.Azure.OData.ODataQuery<ADL.Analytics.Models.USqlAssembly>();

            string @select = null;
            bool? count = null;
            string search = null;
            string format = null;

            // Handle the initial response
            var page = this._adla_catalog_rest_client.Catalog.ListAssemblies(this.Account, dbname, oDataQuery, @select, count);
            foreach (var cur_page in RESTUtil.EnumPages<ADL.Analytics.Models.USqlAssemblyClr>(page, p => this._adla_catalog_rest_client.Catalog.ListAssembliesNext(p.NextPageLink)))
            {
                yield return cur_page;
            }
        }

        public IEnumerable<ADL.Analytics.Models.USqlExternalDataSource[]> ListExternalDatasources(string name, string dbname)
        {
            var oDataQuery = new Microsoft.Rest.Azure.OData.ODataQuery<ADL.Analytics.Models.USqlExternalDataSource>();

            string @select = null;
            bool? count = null;
            string search = null;
            string format = null;

            // Handle the initial response
            var page = this._adla_catalog_rest_client.Catalog.ListExternalDataSources(this.Account, dbname, oDataQuery, @select, count);
            foreach (var cur_page in RESTUtil.EnumPages<ADL.Analytics.Models.USqlExternalDataSource>(page, p => this._adla_catalog_rest_client.Catalog.ListExternalDataSourcesNext(p.NextPageLink)))
            {
                yield return cur_page;
            }
        }

        public IEnumerable<ADL.Analytics.Models.USqlProcedure[]> ListProcedures(string name, string dbname, string schema)
        {
            var oDataQuery = new Microsoft.Rest.Azure.OData.ODataQuery<ADL.Analytics.Models.USqlProcedure>();

            string @select = null;
            bool? count = null;
            string search = null;
            string format = null;

            // Handle the initial response
            var page = this._adla_catalog_rest_client.Catalog.ListProcedures(this.Account, dbname, schema, oDataQuery, @select, count);
            foreach (var cur_page in RESTUtil.EnumPages<ADL.Analytics.Models.USqlProcedure>(page, p => this._adla_catalog_rest_client.Catalog.ListProceduresNext(p.NextPageLink)))
            {
                yield return cur_page;
            }
        }


        public IEnumerable<ADL.Analytics.Models.JobInformation> GetJobList(GetJobListPagedOptions options)
        {

            foreach (var page in GetJobListPaged(options))
            {
                foreach (var job in page)
                {
                    yield return job;
                }
            }
        }


        public ADL.Analytics.Models.JobInformation GetJob(System.Guid jobid)
        {
            var job = this._adla_job_rest_client.Job.Get(this.Account, jobid);

            return job;
        }

        public IEnumerable<ADL.Analytics.Models.JobInformation[]> GetJobListPaged(GetJobListPagedOptions options)
        {
            var oDataQuery = new Microsoft.Rest.Azure.OData.ODataQuery<JobInformation>();
            
            string @select = null;
            bool? count = null;
            string search = null;
            string format = null;

            if (options.Top > 0)
            {
                oDataQuery.Top = options.Top;
                if (options.OrderByField != JobOrderByField.None)
                {
                    var fieldname = get_order_field_name(options.OrderByField);
                    var dir = (options.OrderByDirection == JobOrderByDirection.Ascending) ? "asc" : "desc";

                    oDataQuery.OrderBy = string.Format("{0} {1}", fieldname, dir);
                }
            }

            // Handle the initial response
            var page = this._adla_job_rest_client.Job.List(this.Account, oDataQuery, @select, count, search, format);
            foreach (var cur_page in RESTUtil.EnumPages<JobInformation>(page, p => this._adla_job_rest_client.Job.ListNext(p.NextPageLink)))
            {
                yield return cur_page;
            }
        }

        private static string get_order_field_name(JobOrderByField field)
        {
            if (field == JobOrderByField.None)
            {
                throw new System.ArgumentException();
            }

            string field_name_str = field.ToString();
            string result = field_name_str.Substring(0, 1).ToLowerInvariant() + field_name_str.Substring(1);
            return result;
        }


        public ADL.Analytics.Models.JobInformation  SubmitJob(SubmitJobOptions options)
        {
            if (options.JobID == default(System.Guid))
            {
                options.JobID = System.Guid.NewGuid();
            }

            if (options.JobName == null)
            {
                options.JobName = "ADL_Demo_Client_Job_" + System.DateTime.Now.ToString();
            }

            var jobprops = new USqlJobProperties();
            jobprops.Script = options.ScriptText;

            var jobType = JobType.USql;
            int priority = 1;
            int dop = 1;

            var parameters = new JobInformation(
                name: options.JobName, 
                type: jobType, 
                properties: jobprops,               
                priority: priority, 
                degreeOfParallelism: dop, 
                jobId: options.JobID);
            
            var jobInfo = this._adla_job_rest_client.Job.Create(this.Account, options.JobID, parameters);

            return jobInfo;
        }
    }
}