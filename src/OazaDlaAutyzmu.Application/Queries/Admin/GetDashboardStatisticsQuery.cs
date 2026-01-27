using MediatR;
using OazaDlaAutyzmu.Application.ViewModels;

namespace OazaDlaAutyzmu.Application.Queries.Admin;

public class GetDashboardStatisticsQuery : IRequest<DashboardStatisticsViewModel>
{
}
