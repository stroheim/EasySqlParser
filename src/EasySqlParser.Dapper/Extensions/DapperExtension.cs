using System.Collections.Generic;
using System.Data;
using Dapper;

namespace EasySqlParser.Dapper.Extensions
{
    public static class DapperExtension
    {
        public static DynamicParameters ToDynamicParameters(this List<IDbDataParameter> parameters)
        {
            var result = new DynamicParameters();
            foreach (var parameter in parameters)
            {

                result.Add(parameter.ParameterName,
                    parameter.Value,
                    parameter.DbType,
                    parameter.Direction,
                    parameter.Size,
                    parameter.Precision,
                    parameter.Scale);
            }

            return result;
        }
    }

}
