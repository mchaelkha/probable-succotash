using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CodeChallenge.Services;
using CodeChallenge.Models;

namespace CodeChallenge.Controllers
{
    [ApiController]
    [Route("api/employee")]
    public class EmployeeController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IEmployeeService _employeeService;

        public EmployeeController(ILogger<EmployeeController> logger, IEmployeeService employeeService)
        {
            _logger = logger;
            _employeeService = employeeService;
        }

        [HttpPost]
        public IActionResult CreateEmployee([FromBody] Employee employee)
        {
            _logger.LogDebug($"Received employee create request for '{employee.FirstName} {employee.LastName}'");

            _employeeService.Create(employee);

            return CreatedAtRoute(nameof(GetEmployeeById), new { id = employee.EmployeeId }, employee);
        }

        [HttpGet("{id}", Name = nameof(GetEmployeeById))]
        public IActionResult GetEmployeeById(string id)
        {
            _logger.LogDebug($"Received employee get request for '{id}'");

            var employee = _employeeService.GetById(id);

            if (employee == null)
            {
                return NotFound();
            }

            return Ok(employee);
        }

        [HttpPut("{id}")]
        public IActionResult ReplaceEmployee(string id, [FromBody]Employee newEmployee)
        {
            _logger.LogDebug($"Received employee update request for '{id}'");

            var existingEmployee = _employeeService.GetById(id);
            if (existingEmployee == null)
            {
                return NotFound();
            }

            _employeeService.Replace(existingEmployee, newEmployee);

            return Ok(newEmployee);
        }

        [HttpGet("{id}/reporting-structure")]
        public IActionResult GetReportingStructure(string id)
        {
            _logger.LogDebug($"Received reporting structure get request for '{id}'");

            var employee = _employeeService.GetById(id, true);
            if (employee == null)
            {
                return NotFound();
            }

            var reportingStructure = _employeeService.GetReportingStructure(employee);

            return Ok(reportingStructure);
        }

        [HttpPost("{id}/compensation")]
        public IActionResult CreateCompensation([FromBody]Compensation compensation)
        {
            _logger.LogDebug($"Received compensation post request for '{compensation.Employee.EmployeeId}'");

            if (_employeeService.GetById(compensation.Employee.EmployeeId) == null)
            {
                return NotFound();
            }

            _employeeService.Create(compensation);

            return CreatedAtRoute(nameof(GetCompensation), new { id = compensation.Employee.EmployeeId }, compensation);
        }

        [HttpGet("{id}/compensation", Name = nameof(GetCompensation))]
        public IActionResult GetCompensation(string id)
        {
            _logger.LogDebug($"Received compensation get request for '{id}'");

            var employee = _employeeService.GetById(id);
            if (employee == null)
            {
                return StatusCode(404, "Employee does not exist.");
            }

            var compensation = _employeeService.GetCompensation(employee);
            if (compensation == null)
            {
                return StatusCode(404, "Compensation does not exist for employee.");
            }

            return Ok(compensation);
        }
    }
}
