using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.Text;

namespace FhirPathEvaluationExample.Formatters
{
    public class FhirJsonFormatter : TextOutputFormatter
    {
        private readonly FhirJsonSerializer _serializer;
        public FhirJsonFormatter()
            : base()
        {
            _serializer = new FhirJsonSerializer();

            SupportedMediaTypes.Add("application/fhir+json");
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        protected override bool CanWriteType(Type type)
        {
            return typeof(Base).IsAssignableFrom(type);
        }

        public override async System.Threading.Tasks.Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var httpContext = context.HttpContext;

            httpContext.Response.ContentType = "application/fhir+json";
            await httpContext.Response.WriteAsync(await _serializer.SerializeToStringAsync(context.Object as Base), selectedEncoding);
        }
    }
}
