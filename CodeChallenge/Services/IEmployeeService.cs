using CodeChallenge.Models;

namespace CodeChallenge.Services
{
    public interface IEmployeeService
    {
        Employee GetById(string id);
        Employee GetById(string id, bool includeDirectReports);
        Employee Create(Employee employee);
        Employee Replace(Employee originalEmployee, Employee newEmployee);
        ReportingStructure GetReportingStructure(Employee employee);
        Compensation GetCompensation(Employee employee);
        Compensation Create(Compensation compensation);
    }
}
