using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace FhirPathEvaluationExample.ExampleResources
{
    static class ResourceLoader
    {
        private static FhirJsonParser _parser = new FhirJsonParser();
        public static Appointment Appointment_With_Contained_Resources => _parser.Parse<Appointment>(ResourceFileReader.AppointmentWithContainedResources);
        public static Appointment Appointment_With_External_Resources => _parser.Parse<Appointment>(ResourceFileReader.AppointmentWithExternalResources);
        public static Patient Patient => _parser.Parse<Patient>(ResourceFileReader.Patient);
        public static Practitioner Practitioner => _parser.Parse<Practitioner>(ResourceFileReader.Practitioner);
        public static PractitionerRole PractitionerRole => _parser.Parse<PractitionerRole>(ResourceFileReader.PractitionerRole);
    }
}
