using Hl7.Fhir.ElementModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FhirPathEvaluationExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (IHost host = CreateHostBuilder(args).Build())
            {
                host.Start();

                // Location of the local server
                string baseUrl = LocalHostUrl(host);

                // Create a new logger
                ILoggerFactory loggerFactory = host.Services.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
                ILogger logger = loggerFactory?.CreateLogger(nameof(Program));

                IEnumerable<ITypedElement> results = FhirPathEvaluator.Get_Patient_GivenNames_From_Patient_Resource(logger);
                Output_Results(logger, results);

                // See functions in FhirPathEvaluator for more information
                results = FhirPathEvaluator.Get_Patient_LastName_From_Appointment_With_Contained_Resources(logger);
                Output_Results(logger, results);

                results = FhirPathEvaluator.Get_Patient_LastName_From_Appointment_With_External_Resources_Via_Cache(logger);
                Output_Results(logger, results);

                results = FhirPathEvaluator.Get_Practitioner_LastName_From_Appointment_With_External_Resources_Via_Rest(logger, baseUrl);
                Output_Results(logger, results);

                results = FhirPathEvaluator.Get_Practitioner_Specialty_From_Appointment_With_Custom_FhirPath_Expression(logger, baseUrl);
                Output_Results(logger, results);

                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static string LocalHostUrl(IHost host)
        {
            IServer server = host.Services.GetService(typeof(IServer)) as IServer;
            IServerAddressesFeature feature = server.Features.Get<IServerAddressesFeature>();
            return feature.Addresses.FirstOrDefault();
        }

        private static void Output_Results(ILogger logger, IEnumerable<ITypedElement> elements)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Result Count: {elements.Count()}");
            sb.AppendLine("Values:");

            foreach (ITypedElement element in elements)
            {
                sb.AppendLine($"              {element.Value.ToString()}");
            }

            sb.AppendLine();
            logger.LogInformation(sb.ToString());
        }
    }
}
