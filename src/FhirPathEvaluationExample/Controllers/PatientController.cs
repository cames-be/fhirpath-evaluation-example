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
    public class PatientController : ControllerBase
    {
        private readonly ILogger<PatientController> _logger;

        public PatientController(ILogger<PatientController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{id}")]
        public Resource Get([FromRoute]string id)
        {
            _logger.LogInformation($"Patient endpoint received request for id='{id}'");

            const string PatientId = "patient123";
            if (string.Equals(id, PatientId, StringComparison.Ordinal))
            {
                return ResourceLoader.Patient;
            }

            return new OperationOutcome()
            {
                Issue = new List<OperationOutcome.IssueComponent>()
                {
                    new OperationOutcome.IssueComponent()
                    {
                        Severity = OperationOutcome.IssueSeverity.Error,
                        Diagnostics = "Invalid patient id."
                    }
                }
            };
        }
    }
}
