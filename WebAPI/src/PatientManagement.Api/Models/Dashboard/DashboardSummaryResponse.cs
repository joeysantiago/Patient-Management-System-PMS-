using PatientManagement.Api.Models.Appointments;
using PatientManagement.Api.Models.Patients;

namespace PatientManagement.Api.Models.Dashboard;

public class DashboardSummaryResponse
{
    public int NewPatientsLast30Days { get; set; }

    public int TodaysAppointmentCount { get; set; }

    public List<AppointmentResponse> TodaysAppointments { get; set; } = new();

    public List<PatientResponse> RecentPatients { get; set; } = new();
}
