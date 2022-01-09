using FhirPathEvaluationExample.ExampleResources;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.FhirPath;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.FhirPath;
using Hl7.FhirPath.Expressions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FhirPathEvaluationExample
{
    public static class FhirPathEvaluator
    {
        private static void LogExpression(ILogger logger, string expression, string functionName)
        {
            logger.LogInformation($"Evaluating expression '{expression}' in function '{functionName}'");
        }

        /// <summary>
        /// Using a pre-determined patient, compiles and executes the FhirPath expression
        /// to find all given names for the patient.
        /// </summary>
        /// <param name="logger">Used to log messages</param>
        /// <returns>All given names for the patient</returns>
        public static IEnumerable<ITypedElement> Get_Patient_GivenNames_From_Patient_Resource(ILogger logger)
        {
            // Pre-determined patient resource
            Patient patient = ResourceLoader.Patient;

            const string PatientGivenNames_FhirPathExpression = "name.given";
            LogExpression(logger, PatientGivenNames_FhirPathExpression, MethodBase.GetCurrentMethod().Name);

            // Compile the expression.  Note the compiler only uses the standard SymbolTable by default
            FhirPathCompiler compiler = new FhirPathCompiler();
            CompiledExpression expression = compiler.Compile(PatientGivenNames_FhirPathExpression);

            // Evaluate the expression against the patient resource
            return expression(patient.ToTypedElement(), EvaluationContext.CreateDefault());
        }

        /// <summary>
        /// Using a pre-determined appointment, compiles and executes the FhirPath expression
        /// to find the last name for the contained patient resource.
        /// </summary>
        /// <param name="logger">Used to log messages</param>
        /// <returns>All last names for all referenced patients</returns>
        public static IEnumerable<ITypedElement> Get_Patient_LastName_From_Appointment_With_Contained_Resources(ILogger logger)
        {
            // Pre-determined appointment resource with contained resources (referenced resources are contained within the appointment resource)
            Appointment appointment = ResourceLoader.Appointment_With_Contained_Resources;

            // FhirPath expression.  Note the 'resolve()' function for resolving referenced resources
            const string PatientLastName_FhirPathExpression = "participant.actor.where(type = 'Patient').resolve().name.family";
            LogExpression(logger, PatientLastName_FhirPathExpression, MethodBase.GetCurrentMethod().Name);

            // In order to use the resolve() function, the extension functions need to be added to the SymbolTable using the AddFhirExtensions() extension method.
            // .AddStandardFP() - https://github.com/FirelyTeam/firely-net-common/blob/3d981d0679a4431b9b50dc55578b50fc9d7ae426/src/Hl7.FhirPath/FhirPath/Expressions/SymbolTableInit.cs#L24
            // .AddFhirExtensions() -  https://github.com/FirelyTeam/firely-net-common/blob/3d981d0679a4431b9b50dc55578b50fc9d7ae426/src/Hl7.Fhir.Support.Poco/FhirPath/ElementNavFhirExtensions.cs#L24
            SymbolTable st = new SymbolTable()
                                    .AddStandardFP() 
                                    .AddFhirExtensions();

            // Use the generated SymbolTable in the compiler
            FhirPathCompiler compiler = new FhirPathCompiler(st);

            // Compile the expression.
            CompiledExpression expression = compiler.Compile(PatientLastName_FhirPathExpression);

            // In order to resolve() referenced resources that are contained resources, a ScopedNode MUST be used,
            // otherwise only the ElementResolver will be used.  See the other examples for use of the ElementResolver.
            ScopedNode node = new ScopedNode(appointment.ToTypedElement());
            return expression(node, EvaluationContext.CreateDefault());
        }

        /// <summary>
        /// Using a pre-determined appointment, compiles and executes the FhirPath expression
        /// to find the last name for referenced external patient resources.  This example demonstrates the 
        /// use of the external ElementResolver that is used for locating non-contained references
        /// utilizing a built-in 'cache' of static resources
        /// </summary>
        /// <param name="logger">Used to log messages</param>
        /// <returns>All last names for all referenced patients</returns>

        public static IEnumerable<ITypedElement> Get_Patient_LastName_From_Appointment_With_External_Resources_Via_Cache(ILogger logger)
        {
            // Pre-determined appointment resource with referenced resources
            Appointment appointment = ResourceLoader.Appointment_With_External_Resources;

            // FhirPath expression.  Note the 'resolve()' function for resolving referenced resources
            const string PatientLastName_FhirPathExpression = "participant.actor.where(type = 'Patient').resolve().name.family";
            LogExpression(logger, PatientLastName_FhirPathExpression, MethodBase.GetCurrentMethod().Name);

            // In order to use the resolve() function, the extension functions need to be added to the SymbolTable using the AddFhirExtensions() extension method.
            // .AddStandardFP() - https://github.com/FirelyTeam/firely-net-common/blob/3d981d0679a4431b9b50dc55578b50fc9d7ae426/src/Hl7.FhirPath/FhirPath/Expressions/SymbolTableInit.cs#L24
            // .AddFhirExtensions() -  https://github.com/FirelyTeam/firely-net-common/blob/3d981d0679a4431b9b50dc55578b50fc9d7ae426/src/Hl7.Fhir.Support.Poco/FhirPath/ElementNavFhirExtensions.cs#L24
            SymbolTable st = new SymbolTable()
                                    .AddStandardFP()
                                    .AddFhirExtensions();

            // Use the generated SymbolTable in the compiler
            FhirPathCompiler compiler = new FhirPathCompiler(st);

            // Compile the expression.
            CompiledExpression expression = compiler.Compile(PatientLastName_FhirPathExpression);

            // The ElementResolver is used to locate referenced resources when the 'resolve()' function is used.
            FhirEvaluationContext context = new FhirEvaluationContext();
            context.ElementResolver = (reference) =>
            {
                // Treat a known patient resource as a cached resource.  Normally the reference parameter would
                // be used to perform a lookup such as 'patient/patient123', but is being ignored in this example.
                return ResourceLoader.Patient.ToTypedElement();
            };

            // Note that a ScopedNode is not required since contained resources are not being evaluated
            return expression(appointment.ToTypedElement(), context);
        }

        /// <summary>
        /// Using a pre-determined appointment, compiles and executes the FhirPath expression
        /// to find the last name for referenced external practitioner resources.  This example demonstrates the 
        /// use of the external ElementResolver that is used for locating non-contained references
        /// by performing a REST call using the FhirClient class.
        /// </summary>
        /// <param name="logger">Used to log messages</param>
        /// <param name="baseUrl">The url of the Fhir server</param>
        /// <returns>All last names for all referenced patients</returns>
        public static IEnumerable<ITypedElement> Get_Practitioner_LastName_From_Appointment_With_External_Resources_Via_Rest(ILogger logger, string baseUrl)
        {
            // Pre-determined appointment resource with referenced resources
            Appointment appointment = ResourceLoader.Appointment_With_External_Resources;

            // FhirPath expression.  Note the 'resolve()' function for resolving referenced resources
            const string PractitionerLastName_FhirPathExpression = "participant.actor.where(type = 'Practitioner').resolve().name.family";
            LogExpression(logger, PractitionerLastName_FhirPathExpression, MethodBase.GetCurrentMethod().Name);

            // In order to use the resolve() function, the extension functions need to be added to the SymbolTable using the AddFhirExtensions() extension method.
            // .AddStandardFP() - https://github.com/FirelyTeam/firely-net-common/blob/3d981d0679a4431b9b50dc55578b50fc9d7ae426/src/Hl7.FhirPath/FhirPath/Expressions/SymbolTableInit.cs#L24
            // .AddFhirExtensions() -  https://github.com/FirelyTeam/firely-net-common/blob/3d981d0679a4431b9b50dc55578b50fc9d7ae426/src/Hl7.Fhir.Support.Poco/FhirPath/ElementNavFhirExtensions.cs#L24
            SymbolTable st = new SymbolTable()
                                    .AddStandardFP()
                                    .AddFhirExtensions();

            // Use the generated SymbolTable in the compiler
            FhirPathCompiler compiler = new FhirPathCompiler(st);

            // Compile the expression.
            CompiledExpression expression = compiler.Compile(PractitionerLastName_FhirPathExpression);

            // The ElementResolver is used to locate referenced resources when the 'resolve()' function is used.
            FhirEvaluationContext context = new FhirEvaluationContext();
            context.ElementResolver = (reference) =>
            {
                // The FhirClient is used for making REST calls against a Fhir server.  In this use case,
                // locate the referenced resource on the server calling https://localhost:5001/patient/patient123.
                // The resource is loaded and returned within the PatientController.Get function
                FhirClient client = new FhirClient(baseUrl);
                Resource resource = client.Get(reference);

                return resource?.ToTypedElement();
            };

            // Note that a ScopedNode is not required since contained resources are not being evaluated
            return expression(appointment.ToTypedElement(), context);
        }

        /// <summary>
        /// Using a pre-determined appointment, compiles and executes a custom FhirPath expression
        /// to find the specialties for referenced practitioner resources.  This example demonstrates the 
        /// creation and use of a custom FhirPath function and a custom FhirEvaluationContext to pass information
        /// to the custom function.
        /// </summary>
        /// <param name="logger">Used to log messages</param>
        /// <param name="baseUrl">The url of the Fhir server</param>
        /// <returns>All last names for all referenced patients</returns>
        public static IEnumerable<ITypedElement> Get_Practitioner_Specialty_From_Appointment_With_Custom_FhirPath_Expression(ILogger logger, string baseUrl)
        {
            // Pre-determined appointment resource with referenced resources
            Appointment appointment = ResourceLoader.Appointment_With_External_Resources;

            const string PractitionerSpecialties_FhirPathExpression = "participant.actor.where(type = 'Practitioner')[0].getPractitionerSpecialties()";
            LogExpression(logger, PractitionerSpecialties_FhirPathExpression, MethodBase.GetCurrentMethod().Name);

            // Note that only the standard functions are being used and the extension functions are not needed.
            SymbolTable st = new SymbolTable()
                                    .AddStandardFP();

            // Add custom FhirPath function to the SymbolTable.  The function takes in the current ITypedElement node being evaluated
            // as well as the evaluation context being used and returns a collection of ITypedElement values.  In this case it will
            // return FhirString elements containing the practitioner specialties.  The doNullProp parameter allows for execution of the
            // function even if the parent element contains no child elements
            st.Add("getPractitionerSpecialties", (ITypedElement f, EvaluationContext ctx) => GetSpecialties(f, ctx), doNullProp: false);

            // Use the generated SymbolTable in the compiler
            FhirPathCompiler compiler = new FhirPathCompiler(st);

            // Compile the expression.
            CompiledExpression expression = compiler.Compile(PractitionerSpecialties_FhirPathExpression);

            // Use a custom context to pass information into the custom FhirPath function.  In this example the server location
            // and logger are needed when executing the custom function
            CustomEvaluationContext context = new CustomEvaluationContext(logger, baseUrl);

            return expression(appointment.ToTypedElement(), context);
        }

        /// <summary>
        /// Custom context that is passed into the custom FhirPath function during execution
        /// </summary>
        private class CustomEvaluationContext : FhirEvaluationContext
        {
            public ILogger Logger { get; }
            public string BaseUrl { get; }
            public CustomEvaluationContext(ILogger logger, string baseUrl)
                : base()
            {
                Logger = logger;
                BaseUrl = baseUrl;
            }
        }

        /// <summary>
        /// Custom function that is called when executing a FhirPath expression.  This function evaluates the current
        /// ITypedElement node to determine if if is a resource reference and if it is, it makes a REST call to search
        /// for the PractitionerRole associated with the referenced resource.  Then, it will evaluate the returned search
        /// results and for each returned PractitionerRole, it will return a collection of the associatd specialties.
        /// </summary>
        /// <param name="element">The current element node being evaluated</param>
        /// <param name="context">The custom context that contains the logger and server url</param>
        /// <returns></returns>
        private static IEnumerable<ITypedElement> GetSpecialties(ITypedElement element, EvaluationContext context)
        {
            // Make sure that the custom context was passed in
            CustomEvaluationContext customContext = context as CustomEvaluationContext;
            if (customContext == null)
            {
                return Enumerable.Empty<ITypedElement>();
            }

            customContext.Logger.LogInformation("Received FhirPath call for custom function 'getPractitionerSpecialty'");

            FhirClient client = new FhirClient(customContext.BaseUrl);

            // Make sure the current element node is a reference
            ResourceReference reference = element.ParseResourceReference();
            if (reference == null)
            {
                customContext.Logger.LogInformation("Typed element must be of type 'ResourceReference'.");
                return Enumerable.Empty<ITypedElement>();
            }

            // Look for all PractitionerRole resources where the practitioner matches the given reference id.  Example practitioner=practitioner123
            SearchParams parameters = new SearchParams("practitioner", reference.Reference);

            // Searches always return a Bundle of search results
            Bundle bundle = client.Search(parameters, nameof(PractitionerRole));
            if (bundle == null || !bundle.Entry.Any())
            {
                customContext.Logger.LogInformation($"PractitionerRole not found for reference '{reference.Reference}'.");
                return Enumerable.Empty<ITypedElement>();
            }

            // Get the display of all specialties for all returned practitioner roles
            return bundle.Entry.Where(e => e.Resource is PractitionerRole)
                                .SelectMany(e =>
                                    (e.Resource as PractitionerRole).Specialty.SelectMany(s =>
                                        s.Coding.Select(c => new FhirString(c.Display).ToTypedElement())
                                    )
                                );
        }
    }
}
