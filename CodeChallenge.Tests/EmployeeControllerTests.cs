
using System;
using System.Net;
using System.Net.Http;
using System.Text;

using CodeChallenge.Models;

using CodeCodeChallenge.Tests.Integration.Extensions;
using CodeCodeChallenge.Tests.Integration.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeCodeChallenge.Tests.Integration
{
    [TestClass]
    public class EmployeeControllerTests
    {
        private static HttpClient _httpClient;
        private static TestServer _testServer;

        private static readonly Employee MOCK_EMPLOYEE = new() 
        {
            Department = "Complaints",
            FirstName = "Debbie",
            LastName = "Downer",
            Position = "Receiver",
        };
        private static readonly string LENNON_EMPLOYEE_ID = "16a596ae-edd3-4847-99fe-c4518e82c86f";

        [ClassInitialize]
        // Attribute ClassInitialize requires this signature
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static void InitializeClass(TestContext context)
        {
            _testServer = new TestServer();
            _httpClient = _testServer.NewClient();
        }

        [ClassCleanup]
        public static void CleanUpTest()
        {
            _httpClient.Dispose();
            _testServer.Dispose();
        }

        [TestMethod]
        public void CreateEmployee_Returns_Created()
        {
            // Arrange and Execute
            var employee = MOCK_EMPLOYEE;
            var newEmployee = CreateEmployeeHelper(employee);

            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var postRequestTask = _httpClient.PostAsync("api/employee",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.IsNotNull(newEmployee.EmployeeId);
            Assert.AreEqual(employee.FirstName, newEmployee.FirstName);
            Assert.AreEqual(employee.LastName, newEmployee.LastName);
            Assert.AreEqual(employee.Department, newEmployee.Department);
            Assert.AreEqual(employee.Position, newEmployee.Position);
        }

        [TestMethod]
        public void GetEmployeeById_Returns_Ok()
        {
            // Arrange
            var employeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";
            var expectedFirstName = "John";
            var expectedLastName = "Lennon";

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var employee = response.DeserializeContent<Employee>();
            Assert.AreEqual(expectedFirstName, employee.FirstName);
            Assert.AreEqual(expectedLastName, employee.LastName);
            Assert.IsNull(employee.DirectReports);
        }

        [TestMethod]
        public void UpdateEmployee_Returns_Ok()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = "03aa1462-ffa9-4978-901b-7c001562cf6f",
                Department = "Engineering",
                FirstName = "Pete",
                LastName = "Best",
                Position = "Developer VI",
            };
            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var putRequestTask = _httpClient.PutAsync($"api/employee/{employee.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var putResponse = putRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, putResponse.StatusCode);
            var newEmployee = putResponse.DeserializeContent<Employee>();

            Assert.AreEqual(employee.FirstName, newEmployee.FirstName);
            Assert.AreEqual(employee.LastName, newEmployee.LastName);
        }

        [TestMethod]
        public void UpdateEmployee_Returns_NotFound()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = "Invalid_Id",
                Department = "Music",
                FirstName = "Sunny",
                LastName = "Bono",
                Position = "Singer/Song Writer",
            };
            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var postRequestTask = _httpClient.PutAsync($"api/employee/{employee.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void GetReportingStructure_Returns_Ok()
        {
            // Arrange
            var employeeId = LENNON_EMPLOYEE_ID;
            var expectedFirstName = "John";
            var expectedNumberOfReports = 4;

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}/reporting-structure");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var reportingStructure = response.DeserializeContent<ReportingStructure>();
            Assert.AreEqual(expectedFirstName, reportingStructure.Employee.FirstName);
            Assert.AreEqual(expectedNumberOfReports, reportingStructure.NumberOfReports);
        }

        [TestMethod]
        public void GetReportingStructure_Returns_Empty_Ok()
        {
            // Arrange
            var employeeId = "b7839309-3348-463b-a7e3-5de1c168beb3";
            var expectedFirstName = "Paul";
            var expectedNumberOfReports = 0;

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}/reporting-structure");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var reportingStructure = response.DeserializeContent<ReportingStructure>();
            Assert.AreEqual(expectedFirstName, reportingStructure.Employee.FirstName);
            Assert.AreEqual(expectedNumberOfReports, reportingStructure.NumberOfReports);
        }

        [TestMethod]
        public void GetReportingStructure_Returns_NotFound()
        {
            // Arrange
            var employeeId = "Invalid_Id";

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}/reporting-structure");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void CreateCompensation_Returns_Created()
        {
            // Arrange and Execute
            var employee = GetEmployeeHelper(LENNON_EMPLOYEE_ID, HttpStatusCode.OK);

            var compensation = new Compensation()
            {
                Employee = employee,
                Salary = 50000,
                EffectiveDate = DateTime.Now.ToLongDateString()
            };

            // Execute and Assert
            var newCompensation = CreateCompensationHelper(compensation, HttpStatusCode.Created);

            Assert.IsNotNull(newCompensation.Employee);
            Assert.AreEqual(compensation.Employee.EmployeeId, newCompensation.Employee.EmployeeId);
            Assert.AreEqual(compensation.Salary, newCompensation.Salary);
            Assert.AreEqual(compensation.EffectiveDate, newCompensation.EffectiveDate);
        }

        [TestMethod]
        public void CreateEmployeeAndCompensation_Returns_Created()
        {
            // Arrange and Execute
            var newEmployee = CreateEmployeeHelper(MOCK_EMPLOYEE);

            var compensation = new Compensation()
            {
                Employee = newEmployee,
                Salary = 50000,
                EffectiveDate = DateTime.Now.ToLongDateString()
            };

            // Execute and Assert
            var newCompensation = CreateCompensationHelper(compensation, HttpStatusCode.Created);

            Assert.IsNotNull(newCompensation.Employee);
            Assert.AreEqual(compensation.Employee.EmployeeId, newCompensation.Employee.EmployeeId);
            Assert.AreEqual(compensation.Salary, newCompensation.Salary);
            Assert.AreEqual(compensation.EffectiveDate, newCompensation.EffectiveDate);
        }

        [TestMethod]
        public void CreateCompensation_Returns_NotFound()
        {
            // Arrange
            var fakeEmployee = MOCK_EMPLOYEE;

            var compensation = new Compensation()
            {
                Employee = fakeEmployee,
                Salary = 50000,
                EffectiveDate = DateTime.Now.ToLongDateString()
            };

            // Execute and Assert
            CreateCompensationHelper(compensation, HttpStatusCode.NotFound);
        }

        [TestMethod]
        public void GetCompensation_Returns_Ok()
        {
            // Arrange
            var newEmployee = CreateEmployeeHelper(MOCK_EMPLOYEE);

            var compensation = new Compensation()
            {
                Employee = newEmployee,
                Salary = 50000,
                EffectiveDate = DateTime.Now.ToLongDateString()
            };
            var newCompensation = CreateCompensationHelper(compensation, HttpStatusCode.Created);

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{newEmployee.EmployeeId}/compensation");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var responseCompensation = response.DeserializeContent<Compensation>();
            Assert.AreEqual(newCompensation.Employee.EmployeeId, responseCompensation.Employee.EmployeeId);
            Assert.AreEqual(newCompensation.Salary, responseCompensation.Salary);
            Assert.AreEqual(newCompensation.EffectiveDate, responseCompensation.EffectiveDate);
        }

        private Employee GetEmployeeHelper(string employeeId, HttpStatusCode statusCode)
        {
            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(statusCode, response.StatusCode);

            // Deserialize
            return response.DeserializeContent<Employee>();
        }

        private Employee CreateEmployeeHelper(Employee employee)
        {
            // Arrange
            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var postRequestTask = _httpClient.PostAsync("api/employee",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            // Deserialize
            return response.DeserializeContent<Employee>();
        }

        private Compensation CreateCompensationHelper(Compensation compensation, HttpStatusCode statusCode)
        {
            // Arrange
            var requestContent = new JsonSerialization().ToJson(compensation);

            // Execute
            var postRequestTask = _httpClient.PostAsync($"api/employee/{compensation.Employee.EmployeeId}/compensation",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(statusCode, response.StatusCode);

            // Deserialize
            return response.DeserializeContent<Compensation>();
        }
    }
}
