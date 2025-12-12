

## Endpoints

|Method|Endpoint|POST params|Auth|Description|
|------|--------|-----------|----|-----------|
|POST|/api/Admin/check-if-admin||||
||||||
|POST|/Auth/sign-in|username and password|||
|POST|/Auth/sign-up|username, password, firstName, lastName, address, phone, email and experienceLevel|||
||||||
|GET|/api/Bicycles|||Retrieve all Bicycles|
|POST|/api/Bicycles|bicycleNumber and inOperate||Create a new Bicycles. All parameters are mandatory|
|GET|/api/Bicycles/{id}|||	Retrieve information about a Bicycles with the id|
|PUT|/api/Bicycles/{id}|||bicycleNumber and inOperate, All parameters are mandatory|
|DELETE|/api/Bicycles/{id}|||Delete a Bicycles|
||||||
|GET|/api/Employees|||Retrieve all Employees|
|POST|/api/Employees|firstName, lastName, address, phone, email, experienceLevel||Create a new Employees. All parameters are mandatory|
|GET|/api/Employees/{id}|||	Retrieve information about a Employees with the id|
|PUT|/api/Employees/{id}|||firstName, lastName, address, phone and email, All parameters are mandatory|
|DELETE|/api/Employees/{id}|||Delete a Employees|
||||||
|GET|/api/Health||||
|GET|/api/Health/db||||
|GET|/api/Health/emloyee/{id}||||
|GET|/api/Health/info||||
|GET|/api/Health/conig||||
||||||
|GET|/api/reports/monthly-hours|||employeeId, year and month, All parameters are mandatory. Retrieve a Employes monthly hours |
||||||
|GET|/api/Routes|||Retrieve all Routes|
|POST|/api/Routes|routeNumber||Create a new Routes. Parameter are mandatory|
|GET|/api/Routes/{id}|||Retrieve information about a Routes with the id|
|PUT|/api/Routes/{id}|||routeNumber, Parameter are mandatory|
|DELETE|/api/Routes/{id}|||Delete a Routes|
||||||
|POST|/api/Shift|dateOfShift, employeeId, bicycleId, routeId and substitutedId|| Create a new shift. All parameters are mandatory|
|put|/api/Shift/{shiftId}/start|||startTime. Parameter are mandatory |
|put|/api/Shift/{shiftId}/end|||Time. Parameter are mandatory |
|put|/api/Shift/{shiftId}/substitution-flag|||hasSubstituted. Parameter are mandatory |
||||||
|GET|/api/ShiftPlans|||Retrieve all ShiftPlans|
|GET|/api/ShiftPlans/{id}|||Retrieve ShiftPlans on id|
|DELETE|/api/ShiftPlans/{id}|||DELETE a shipPlans on id|
|POST|/api/ShiftPlans/generate-6Week|startDate and weeks||genrate a 6 week shiftplan |
|PUT|/api/ShiftPlans/{id}/name|||change name on id|
|PUT|/api/ShiftPlans/{id}/shift/{index}||||



