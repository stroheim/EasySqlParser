/**
this tables from AdventureWorks.
if you not have.
download from github.
https://github.com/Microsoft/sql-server-samples
*/
SELECT 
        t0.BusinessEntityID
      , t1.FirstName
      , t1.MiddleName
      , t1.LastName
      , t0.BirthDate
      , t0.MaritalStatus
      , t0.Gender
      , t0.HireDate
  FROM HumanResources.Employee t0
  INNER JOIN Person.Person t1
    ON t0.BusinessEntityID = t1.BusinessEntityID
  WHERE 
    t0.BusinessEntityID = /* Id */1
