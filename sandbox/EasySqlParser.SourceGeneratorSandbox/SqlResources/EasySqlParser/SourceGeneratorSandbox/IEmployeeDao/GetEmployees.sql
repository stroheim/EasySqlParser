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
    /*%if MiddleNames != null && MiddleNames.Count > 0 */
    t1.MiddleName IN /* MiddleNames */('M')
    /*%end*/

    /*%if BirthDateFrom != null && BirthDateTo != null */
    AND t0.BirthDate BETWEEN /* BirthDateFrom */'1980-01-01' AND /* BirthDateTo */'1990-01-01'
    /*%end*/

    /*%if FirstName != null && FirstName != "" */
    AND t1.FirstName LIKE /* @StartsWith(FirstName) */'A%'
    /*%end*/
ORDER BY
  t0.BusinessEntityID
