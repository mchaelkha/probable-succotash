using CodeChallenge.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeChallenge.Repositories
{
    public interface IEmployeeRepository
    {
        Employee GetById(string id);
        Employee GetById(string id, bool includeDirectReports);
        Employee Add(Employee employee);
        Employee Remove(Employee employee);
        List<Employee> GetDirectReports(Employee employee);
        Task SaveAsync();
    }
}