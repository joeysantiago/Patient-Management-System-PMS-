namespace PatientManagement.Api.Data.Entities;

public class Medication
{
    public Guid Id { get; set; }

    public Guid VisitId { get; set; }

    public Visit? Visit { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Dosage { get; set; } = string.Empty;

    public string Frequency { get; set; } = string.Empty;

    public string Duration { get; set; } = string.Empty;

    public string? Instructions { get; set; }
}
