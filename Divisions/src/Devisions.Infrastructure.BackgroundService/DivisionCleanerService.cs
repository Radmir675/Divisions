using Devisions.Application;
using Devisions.Application.Departments;

namespace Devisions.Infrastructure.BackgroundService;

public class DivisionCleanerService : IDivisionCleanerService
{
    private readonly IDepartmentRepository _departmentRepository;

    public DivisionCleanerService(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    public Task Process(CancellationToken cancellationToken)
    {
        //_departmentRepository.
        return Task.CompletedTask;
    }
}