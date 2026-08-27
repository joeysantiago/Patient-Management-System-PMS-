namespace PatientManagement.Api.Data.Entities;

public class Vitals
{
    public Guid VisitId { get; set; }

    public Visit? Visit { get; set; }

    public decimal TemperatureCelsius { get; set; }

    public int BloodPressureSystolic { get; set; }

    public int BloodPressureDiastolic { get; set; }

    public int PulseBpm { get; set; }
}
