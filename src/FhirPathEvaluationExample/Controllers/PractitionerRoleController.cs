using FhirPathEvaluationExample.ExampleResources;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace FhirPathEvaluationExample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PractitionerRoleController : ControllerBase
    {
        private readonly ILogger<PractitionerRoleController> _logger;

        public PractitionerRoleController(ILogger<PractitionerRoleController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public Resource Get(string practitioner)
        {
            _logger.LogInformation($"PractitionerRole endpoint received request for practitioner='{practitioner}'");

            const string PractitionerId = "practitioner123";
            const string PractitionerRole_Practitioner = "practitioner/" + PractitionerId;

            Bundle bundle = new Bundle()
            {
                Type = Bundle.BundleType.Searchset,
                Timestamp = DateTime.UtcNow
            };

            if (string.Equals(practitioner, PractitionerRole_Practitioner, StringComparison.Ordinal))
            {
                bundle.Entry.Add(new Bundle.EntryComponent()
                {
                    Resource = ResourceLoader.PractitionerRole
                });
            }

            return bundle;
        }
    }
}
