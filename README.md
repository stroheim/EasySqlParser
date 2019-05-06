EasySqlParser
===
Library that parses SQL written as so-called `2-Way-SQL`

**This is a paser, not an o/r mapper**   
It expects to pass parsed SQL, generated parameters into 
[Dapper](https://github.com/StackExchange/Dapper) ,
[EntityFramework](https://github.com/aspnet/EntityFramework6) ,
[EntityFrameworkCore](https://github.com/aspnet/EntityFrameworkCore),etc


Requirement
---
.NET Standard 2.0

GettingStart
---
1. Register Config
```csharp
// in entry point
ConfigContainer.AddDefault(
    DbConnectionKind.SqlServer, // A kind of DB connection
    () => new SqlParameter()    // A delegate for create IDbDataParameter instance
);
```
   

2. Create 2-Way-SQL file
```sql
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
```
3. execute SQL
```csharp
    // Model to pass to SQL file
    public class SqlCondition
    {
        public List<string> MiddleNames { get; set; }
        public DateTime? BirthDateFrom { get; set; }
        public DateTime? BirthDateTo { get; set; }
        public string FirstName { get; set; }
    }

    var condition = new SqlCondition
            {
                BirthDateFrom = new DateTime(1980, 1, 1),
                BirthDateTo = new DateTime(1990, 1, 1)
            };
    var parser = new SqlParser("path/to/SelectEmployees.sql", condition);
    var result = parser.Parse();
    Console.WriteLine(result.ParsedSql);
    Console.WriteLine(result.DebugSql);

    using (var context = new EmployeesContext())
    {
        var resultList = context.Employees
            .AsNoTracking()
            .FromSql(result.ParsedSql, result.DbDataParameters.Cast<object>().ToArray())
            .ToList();
    }

```


* Since SQL parsing is porting the code of DOMA, SQL comments etc. are basically compatible with DOMA
* However, there are the following differences
  * `expand`,`populate`,`for` are not supported
  * Built-in function name, type
  * Self-made built-in functions are not supported

* Supported built-in functions    


| Return type | Function name and parameters | Description |
|:--|:--|:--|
|string | @Escape(string text) | Indicates to escape for LIKE operation. <br/>The return value is a string with the input value escaped. <br/>The escape is done using the default escape character ($). <br/> If you pass null as an argument, it returns null.|
|string | @Escape(string text, char escapeChar) | Indicates to escape for LIKE operation. <br/> The return value is a string with the input value escaped. <br/> Escape is performed using the escape character specified in the second argument. <br/> If you pass null as the first argument, it returns null.|
|string|@StartsWith(string text)| Indicates to perform a forward match search. <br/> The return value is a string after escaping the input value and appending a wildcard. <br/> The escape is done using the default escape character ($). <br/> If you pass null as an argument, it returns null.|
|string|@StartsWith(string text, char escapeChar)| Indicates to perform a forward match search. <br/> The return value is a string after escaping the input value and appending a wildcard. <br/> Escape is performed using the escape character specified in the second argument. <br/> If you pass null as the first argument, it returns null.|
|string|@Contains(string text)| Indicates that an intermediate match search is to be performed. <br/> The return value is a string with the input value escaped and wildcards given before and after. <br/> Escape is done using the default escape character ($). <br/> If you pass null as an argument, it returns null.|
|string|@Contains(string text, char escapeChar)| Indicates that an intermediate match search is to be performed. <br/> The return value is a string with the input value escaped and wildcards given before and after. <br/> Escape is performed using the escape character specified in the second argument. <br/> If you pass null as the first argument, it returns null.|
|string|@EndsWith(string text)| Indicates to perform a backward match search. <br/> The return value is a string with the input value escaped and preceded by a wildcard. <br/> Escape is done using the default escape character ($). <br/> If you pass null as an argument, it returns null.|
|string|@EndsWith(string text, char escapeChar)| Indicates to perform a backward match search. <br/> The return value is a string with the input value escaped and preceded by a wildcard. <br/> Escape is performed using the escape character specified in the second argument. <br/> If you pass null as the first argument, it returns null.|
|DateTime|@TruncateTime(DateTime dateTime)| Indicates to truncate the time part. <br/> The return value is a new date with the time portion truncated. <br/> If you pass null as an argument, it returns null.|
|DateTimeOffset|@TruncateTime(DateTimeOffset dateTimeOffset)| Indicates to truncate the time part. <br/> The return value is a new date with the time portion truncated. <br/> If you pass null as an argument, it returns null.|

* The names `StartsWith`,`Contains`,`EndsWith` come from EntityFramework ,EndsWith

Install
---
* It's available on [nuget](https://www.nuget.org/packages/EasySqlParser/)
```
dotnet add package EasySqlParser
```
or
```
Install-Package EasySqlParser
```

Examples
---
https://github.com/stroheim/EasySqlParser.Examples

Document
---
https://stroheim.github.io/EasySqlParser-Doc/

License
---
[MIT License](https://github.com/stroheim/EasySqlParser/blob/master/LICENSE)

Reference
---
This product is based on the following source code
* [DOMA](https://github.com/domaframework/doma)
* [DynamicExpresso](https://github.com/davideicardi/DynamicExpresso)
* [Math-Expression-Evaluator](https://github.com/Giorgi/Math-Expression-Evaluator)
