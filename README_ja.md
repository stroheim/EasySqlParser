EasySqlParser
===

[DOMA](https://github.com/domaframework/doma)や
[uroboroSQL](https://github.com/future-architect/uroborosql)
で使うようなSQLファイル(いわゆる`2-Way-SQL`として書かれたSQL)をパースするライブラリ   

**名前の通り、SQLファイルをパースするだけでSQLの実行やマッピングは行いません**   
[Dapper](https://github.com/StackExchange/Dapper)や
[EntityFramework](https://github.com/aspnet/EntityFramework6)、
[EntityFrameworkCore](https://github.com/aspnet/EntityFrameworkCore)などに
パースされたSQL、生成されたパラメータを渡すことを想定しています


Requirement
---
.NET Standard 2.0

GettingStart
---
1. Configの登録
```csharp
// アプリケーションのエントリーポイントで行います
ConfigContainer.AddDefault(
    DbConnectionKind.SqlServer, // DBコネクションの種類
    () => new SqlParameter()    // SQLパラメータインスタンス作成のデリゲート
);
```
   

2. 2-Way-SQL作成
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
3. SQL実行
```csharp
    // SQLファイルに渡すモデル
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


* SQLの解析はDOMAのコードを移植しているためSQLコメントなど基本的にDOMAと互換性があります
* ただし下記の違いがあります
  * expand、populate、forは非サポート
  * 組み込み関数の名前、種類
  * 自作組み込み関数の非サポート

* サポートされる組み込み関数   

| 戻り値の型 | 関数名とパラメータ | 概要 |
|:--|:--|:--|
|string | @Escape(string text) | LIKE演算のためのエスケープを行うことを示します。<br/> 戻り値は入力値をエスケープした文字列です。<br/> エスケープにはデフォルトのエスケープ文字（$）を用いて行われます。<br/> 引数にnullを渡した場合、nullを返します。|
|string | @Escape(string text, char escapeChar) | LIKE演算のためのエスケープを行うことを示します。<br/> 戻り値は入力値をエスケープした文字列です。<br/> エスケープは第2引数で指定したエスケープ文字を用いて行われます。<br/> 最初の引数にnullを渡した場合、nullを返します。|
|string | @StartsWith(string text)| 前方一致検索を行うことを示します。<br/> 戻り値は入力値をエスケープしワイルドカードを後ろに付与した文字列です。<br/> エスケープにはデフォルトのエスケープ文字（$）を用いて行われます。<br/> 引数にnullを渡した場合、nullを返します。|
|string | @StartsWith(string text, char escapeChar)| 前方一致検索を行うことを示します。<br/> 戻り値は入力値をエスケープしワイルドカードを後ろに付与した文字列です。<br/> エスケープは第2引数で指定したエスケープ文字を用いて行われます。<br/> 最初の引数にnullを渡した場合、nullを返します。|
|string | @Contains(string text)| 中間一致検索を行うことを示します。<br/> 戻り値は入力値をエスケープしワイルドカードを前と後ろに付与した文字列です。<br/> エスケープはデフォルトのエスケープ文字（$）を用いて行われます。<br/> 引数にnullを渡した場合、nullを返します。|
|string | @Contains(string text, char escapeChar)| 中間一致検索を行うことを示します。<br/> 戻り値は入力値をエスケープしワイルドカードを前と後ろに付与した文字列です。<br/> エスケープは第2引数で指定したエスケープ文字を用いて行われます。<br/> 最初の引数にnullを渡した場合、nullを返します。|
|string | @EndsWith(string text)| 後方一致検索を行うことを示します。<br/> 戻り値は入力値をエスケープしワイルドカードを前に付与した文字列です。<br/> エスケープはデフォルトのエスケープ文字（$）を用いて行われます。<br/> 引数にnullを渡した場合、nullを返します。|
|string | @EndsWith(string text, char escapeChar)| 後方一致検索を行うことを示します。<br/> 戻り値は入力値をエスケープしワイルドカードを前に付与した文字列です。<br/> エスケープは第2引数で指定したエスケープ文字を用いて行われます。<br/> 最初の引数にnullを渡した場合、nullを返します。|
|DateTime | @TruncateTime(DateTime dateTime)| 時刻部分を切り捨てることを示します。<br/> 戻り値は時刻部分が切り捨てられた新しい日付です。<br/> 引数にnullを渡した場合、nullを返します。|
|DateTimeOffset | @TruncateTime(DateTimeOffset dateTimeOffset)| 時刻部分を切り捨てることを示します。<br/> 戻り値は時刻部分が切り捨てられた新しい日付です。<br/> 引数にnullを渡した場合、nullを返します。|

* StartsWith,Contains,EndsWithの名前はEntityFramework由来

Install
---
* [nuget](https://www.nuget.org/packages/EasySqlParser/)からインストールできます
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

License
---
[MIT License](https://github.com/stroheim/EasySqlParser/blob/master/LICENSE)

Reference
---
このプロダクトは下記のソースコードを参考にしています
* [DOMA](https://github.com/domaframework/doma)
* [DynamicExpresso](https://github.com/davideicardi/DynamicExpresso)
* [Math-Expression-Evaluator](https://github.com/Giorgi/Math-Expression-Evaluator)
