using System.Collections.Generic;
using CodeChallenge.Models;
using Microsoft.Extensions.Logging;
using CodeChallenge.Repositories;

namespace CodeChallenge.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(ILogger<EmployeeService> logger, IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
            _logger = logger;
        }

        public Employee Create(Employee employee)
        {
            if (employee != null)
            {
                _employeeRepository.Add(employee);
                _employeeRepository.SaveAsync().Wait();
            }

            return employee;
        }

        public Employee GetById(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                return _employeeRepository.GetById(id);
            }

            return null;
        }

        public Employee GetById(string id, bool includeDirectReports)
        {
            if (!string.IsNullOrEmpty(id))
            {
                return _employeeRepository.GetById(id, includeDirectReports);
            }

            return null;
        }

        public Employee Replace(Employee originalEmployee, Employee newEmployee)
        {
            if (originalEmployee != null)
            {
                _employeeRepository.Remove(originalEmployee);
                if (newEmployee != null)
                {
                    // ensure the original has been removed, otherwise EF will complain another entity w/ same id already exists
                    _employeeRepository.SaveAsync().Wait();

                    _employeeRepository.Add(newEmployee);
                    // overwrite the new id with previous employee id
                    newEmployee.EmployeeId = originalEmployee.EmployeeId;
                }
                _employeeRepository.SaveAsync().Wait();
            }

            return newEmployee;
        }

        public ReportingStructure GetReportingStructure(Employee employee)
        {
            return new ReportingStructure
            {
                Employee = employee,
                NumberOfReports = CountTotalNumberOfReports(employee)
            };
        }

        private int CountTotalNumberOfReports(Employee employee)
        {
            int totalNumberOfReports = 0;
            if (employee == null)
            {
                return totalNumberOfReports;
            }

            var queue = new Queue<Employee>(new List<Employee> { employee });
            while (queue.Count > 0)
            {
                Employee currentEmployee = queue.Dequeue();
                totalNumberOfReports++;

                List<Employee> directReports = _employeeRepository.GetDirectReports(currentEmployee);
                directReports.ForEach(report => queue.Enqueue(_employeeRepository.GetById(report.EmployeeId)));
            }

            return totalNumberOfReports - 1;
        }

        public Compensation Create(Compensation compensation)
        {
            if (compensation != null)
            {
                // replace reference to correct entity, otherwise EF will complain another entity w/ same id already exists
                if (compensation.Employee != null)
                {
                    compensation.Employee = _employeeRepository.GetById(compensation.Employee.EmployeeId);
                }
                _employeeRepository.Add(compensation);
                _employeeRepository.SaveAsync().Wait();
            }

            return compensation;
        }

        public Compensation GetCompensation(Employee employee)
        {
            if (employee != null)
            {
                return _employeeRepository.GetCompensation(employee);
            }

            return null;
        }
    }
}
