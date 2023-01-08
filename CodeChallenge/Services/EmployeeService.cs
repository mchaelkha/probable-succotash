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
            var reportingStructure = new ReportingStructure();
            int numberofReports = 0;
            if (employee != null)
            {
                reportingStructure.Employee = employee;
                numberofReports = CountTotalNumberOfReports(employee);
            }
            reportingStructure.NumberOfReports = numberofReports;
            return reportingStructure;
        }

        private int CountTotalNumberOfReports(Employee employee)
        {
            int totalNumberOfReports = 0;
            if (employee == null)
            {
                return totalNumberOfReports;
            }
            List<Employee> directReports = _employeeRepository.GetDirectReports(employee);
            if (directReports == null)
            {
                return totalNumberOfReports;
            }

            Queue<Employee> queue = new Queue<Employee>(employee.DirectReports);
            while (queue.Count > 0)
            {
                Employee currentEmployee = queue.Dequeue();
                totalNumberOfReports++;

                List<Employee> reports = _employeeRepository.GetDirectReports(currentEmployee);
                reports?.ForEach(report => queue.Enqueue(_employeeRepository.GetById(report.EmployeeId)));
            }

            return totalNumberOfReports;
        }
    }
}
