using Arity.Service.Helpers;
using System;
using System.Configuration;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.Management.Common;
using Smo = Microsoft.SqlServer.Management.Smo;

namespace ArityApp.Helpers
{
    public class DbHelper : IDbHelper
    {
        private readonly IEmailSender _emailSender;
        public DbHelper(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public void DoBackup(string path)
        {
            var fileName = $"{path}\\{DateTime.Now.ToString("hh-mm")}_backup.sql"; 

            var connectionString = ConfigurationManager.ConnectionStrings["RMNEntities"].ConnectionString;
            SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = sqlConnectionStringBuilder.InitialCatalog;
            var schemaName = "dbo";

            if (File.Exists(fileName))
                File.Delete(fileName);

            var server = new Smo.Server(new ServerConnection(new SqlConnection(connectionString)));
            var options = new Smo.ScriptingOptions();
            var databases = server.Databases[databaseName];

            options.FileName = fileName;
            options.EnforceScriptingOptions = true;
            //options.WithDependencies = true;
            options.IncludeHeaders = true;
            options.ScriptDrops = false;
            options.AppendToFile = true;
            options.ScriptSchema = true;
            options.ScriptData = true;
            options.Indexes = true;

            var tables = databases.Tables.Cast<Smo.Table>().Where(i => i.Schema == schemaName);
            var views = databases.Views.Cast<Smo.View>().Where(i => i.Schema == schemaName);
            var procedures = databases.StoredProcedures.Cast<Smo.StoredProcedure>().Where(i => i.Schema == schemaName);
            var functions = databases.UserDefinedFunctions.Cast<Smo.UserDefinedFunction>().Where(i => i.Schema == schemaName);

            Console.WriteLine("SQL Script Generator");

            Console.WriteLine("\nTable Scripts:");
            foreach (Smo.Table table in tables)
            {
                databases.Tables[table.Name, schemaName].EnumScript(options);
            }

            options.ScriptData = false;
            options.WithDependencies = false;

            Console.WriteLine("\nView Scripts:");
            foreach (Smo.View view in views)
            {
                databases.Views[view.Name, schemaName].Script(options);
            }

            Console.WriteLine("\nStored Procedure Scripts:");
            foreach (Smo.StoredProcedure procedure in procedures)
            {
                databases.StoredProcedures[procedure.Name, schemaName].Script(options);
            }


            Console.WriteLine("\nFunction Scripts:");
            foreach (Smo.UserDefinedFunction function in functions)
            {
                databases.UserDefinedFunctions[function.Name, schemaName].Script(options);
            }

            _emailSender.SendEmail($"Database backup has been taken successfully at {DateTime.Now.ToString("dd-MM-yyyy hh:mm")}",
                $"Database backup has been taken successfully at {DateTime.Now.ToString("dd-MM-yyyy hh:mm")}. PFA for backup file " +
                $"<br/> database : {sqlConnectionStringBuilder.InitialCatalog}" +
                $"<br/> server: {server.Name}", fileName);
        }
    }
}