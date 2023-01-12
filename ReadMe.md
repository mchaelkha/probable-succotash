# Mindex Coding Challenge
## What's Provided
A simple [.Net 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) web application has been created and bootstrapped
with data. The application contains information about all employees at a company. On application start-up, an in-memory
database is bootstrapped with a serialized snapshot of the database. While the application runs, the data may be
accessed and mutated in the database without impacting the snapshot.

### How to Run
You can run this by executing `dotnet run` on the command line or in [Visual Studio Community Edition](https://www.visualstudio.com/downloads/).

### How to Use
The following endpoints are available to use:
```
* CREATE
    * HTTP Method: POST
    * URL: localhost:8080/api/employee
    * PAYLOAD: Employee
    * RESPONSE: Employee
* READ
    * HTTP Method: GET
    * URL: localhost:8080/api/employee/{id}
    * RESPONSE: Employee
* UPDATE
    * HTTP Method: PUT
    * URL: localhost:8080/api/employee/{id}
    * PAYLOAD: Employee
    * RESPONSE: Employee
```
The Employee type has a JSON schema of:
```json
{
  "type":"Employee",
  "properties": {
    "employeeId": {
      "type": "string"
    },
    "firstName": {
      "type": "string"
    },
    "lastName": {
      "type": "string"
    },
    "position": {
      "type": "string"
    },
    "department": {
      "type": "string"
    },
    "directReports": {
      "type": "array",
      "items" : "string"
    }
  }
}
```
For all endpoints that require an "id" in the URL, this is the "employeeId" field.

## What to Implement
Clone or download the repository, do not fork it.

### Task 1
Create a new type, ReportingStructure, that has two properties: employee and numberOfReports.

For the field "numberOfReports", this should equal the total number of reports under a given employee. The number of
reports is determined to be the number of directReports for an employee and all of their direct reports. For example,
given the following employee structure:
```
                    John Lennon
                /               \
         Paul McCartney         Ringo Starr
                               /        \
                          Pete Best     George Harrison
```
The numberOfReports for employee John Lennon (employeeId: 16a596ae-edd3-4847-99fe-c4518e82c86f) would be equal to 4.

This new type should have a new REST endpoint created for it. This new endpoint should accept an employeeId and return
the fully filled out ReportingStructure for the specified employeeId. The values should be computed on the fly and will
not be persisted.

#### Task 1 Solution
The ReportingStructure type has a JSON schema of:
```json
{
  "type":"ReportingStructure",
  "properties": {
    "employee": {
      "type": "Employee"
    },
    "numberOfReports": {
      "type": "number"
    }
  }
}
```
The following endpoint will be created:
```
* READ
    * HTTP Method: GET
    * URL: localhost:8080/api/employee/{id}/reporting-structure
    * RESPONSE: ReportingStructure
```

#### Task 1 Afterthoughts
* To implement this, we need to make additional database calls since the direct reports are not loaded from the ORM by
default lazy loading behavior.
* To calculate the number of direct reports, we will need to be able to traverse the ReportingStructure and continue
traversing if we find a "manager". Assuming an employee will only have exactly one manager, this can be done through
a DFS or BFS approach until there are no more managers found. If I had to account for this I would track every unique
employee and any duplicates would not be traversed again.

### Task 2
Create a new type, Compensation. A Compensation has the following fields: employee, salary, and effectiveDate. Create
two new Compensation REST endpoints. One to create and one to read by employeeId. These should persist and query the
Compensation from the persistence layer.

#### Task 2 Solution
The Compensation type has a JSON schema of:
```json
{
  "type":"Compensation",
  "properties": {
    "employee": {
      "type": "Employee"
    },
    "salary": {
      "type": "number"
    },
    "effectiveDate": {
      "type": "string"
    }
  }
}
```
The following endpoints will be created:
```
* CREATE
    * HTTP Method: POST
    * URL: localhost:8080/api/employee/{id}/compensation
    * PAYLOAD: Compensation
    * RESPONSE: Compensation
* READ
    * HTTP Method: GET
    * URL: localhost:8080/api/employee/{id}/compensation
    * RESPONSE: Compensation
```
> These should persist and query the Compensation from the persistence layer.

Persistence is already implemented for creating an Employee, so this would only require calling SaveAsync() and Wait() on
the EmployeeRepository.

#### Task 2 Afterthoughts
* Task 2 was more complicated due to how Entity Framework ORM works.
* When writing unit tests, I ran into another entity with the same key being tracked and looked at the comment in the
EmployeeService::Replace API and some MS docs. This was because creating an employee then deserializing the employee
as an object and serializing it again into the request payload is not the same entity. The ORM sees this as a duplicate
entity and cause the error.
* I attempted to make unit tests while making them more modular to reduce duplicate code.

## Delivery
Please upload your results to a publicly accessible Git repo. Free ones are provided by Github and Bitbucket.

## Solution
Review my design decisions with the changes made in this ReadMe.md under the specific task sections. I installed a code
coverage extension for VS 2022 Community Edition.
