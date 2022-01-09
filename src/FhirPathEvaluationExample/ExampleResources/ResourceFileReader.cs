using System.IO;

namespace FhirPathEvaluationExample.ExampleResources
{
    public static class ResourceFileReader
    {
        public static string AppointmentWithContainedResources = File.ReadAllText("ExampleResources\\appointment_with_contained_resources.json");
        public static string AppointmentWithExternalResources = File.ReadAllText("ExampleResources\\appointment_with_external_resources.json");
        public static string Patient = File.ReadAllText(@"ExampleResources\patient.json");
        public static string Practitioner = File.ReadAllText(@"ExampleResources\practitioner.json");
        public static string PractitionerRole = File.ReadAllText(@"ExampleResources\practitionerRole.json");
    }
}
