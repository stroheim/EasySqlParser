using System;

namespace EasySqlParser.SqlGenerator.Helpers
{
    /// <summary>
    ///     Helper for query command.
    /// </summary>
    public static class DbCommandHelper
    {
        // used by dapper and ef
        /// <summary>
        ///     Consumes the scalar value.
        /// </summary>
        /// <param name="scalarValue"></param>
        /// <param name="builderParameter"></param>
        /// <returns></returns>
        public static int ConsumeScalar(object scalarValue, QueryBuilderParameter builderParameter)
        {
            if (scalarValue == null || scalarValue is DBNull)
            {
                return 0;
            }

            var entityInfo = builderParameter.EntityTypeInfo;
            var instance = builderParameter.Entity;
            //var config = builderParameter.Config;
            if (entityInfo.IdentityColumn == null) return 1;
            var changed = Convert.ChangeType(scalarValue, entityInfo.IdentityColumn.PropertyInfo.PropertyType);
            entityInfo.IdentityColumn.PropertyInfo.SetValue(instance, changed);
            if (!builderParameter.IsSameVersion())
            {
                return 0;
            }
            return 1;

        }
       
    }

}
