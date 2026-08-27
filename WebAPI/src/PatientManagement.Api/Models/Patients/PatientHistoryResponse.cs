using PatientManagement.Api.Models.Consultations;

namespace PatientManagement.Api.Models.Patients;

public class PatientHistoryResponse
{
    public Guid PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public List<VisitResponse> Visits { get; set; } = new();
}
