using FhirPathEvaluationExample.ExampleResources;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace FhirPathEvaluationExample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PractitionerController : ControllerBase
    {
        private readonly ILogger<PractitionerController> _logger;

        public PractitionerController(ILogger<PractitionerController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{id}")]
        public Resource Get(string id)
        {
            _logger.LogInformation($"Practitioner endpoint received request for id='{id}'");

            const string PractitionerId = "practitioner123";
            if (string.Equals(id, PractitionerId, StringComparison.Ordinal))
            {
                return ResourceLoader.Practitioner;
            }

            return new OperationOutcome()
            {
                Issue = new List<OperationOutcome.IssueComponent>()
                {
                    new OperationOutcome.IssueComponent()
                    {
                        Severity = OperationOutcome.IssueSeverity.Error,
                        Diagnostics = "Invalid practitioner id."
                    }
                }
            };
        }
    }
}
