using PatientManagement.Api.Models.Patients;
using PatientManagement.Api.Models.Settings;

namespace PatientManagement.Api.Models.Consultations;

public class PrescriptionResponse
{
    public ClinicSettingsResponse Clinic { get; set; } = new();

    public PatientResponse Patient { get; set; } = new();

    public VisitResponse Visit { get; set; } = new();
}
