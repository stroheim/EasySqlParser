using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace EasySqlParser.Extensions
{
    public static class TypeExtension
    {
        public static bool IsEspKnownType(this Type type)
        {
            if (type == typeof(string) ||
                type == typeof(short) ||
                type == typeof(int) ||
                type == typeof(long) ||
                type == typeof(bool) ||
                type == typeof(decimal) ||
                type == typeof(byte) ||
                type == typeof(byte[]) ||
                type == typeof(float) ||
                type == typeof(double) ||
                type == typeof(sbyte) ||
                type == typeof(ushort) ||
                type == typeof(uint) ||
                type == typeof(ulong) ||
                type == typeof(DateTime) ||
                type == typeof(DateTimeOffset) ||
                type == typeof(TimeSpan) ||
                type == typeof(short?) ||
                type == typeof(int?) ||
                type == typeof(long?) ||
                type == typeof(bool?) ||
                type == typeof(decimal?) ||
                type == typeof(byte?) ||
                type == typeof(float?) ||
                type == typeof(double?) ||
                type == typeof(sbyte?) ||
                type == typeof(ushort?) ||
                type == typeof(uint?) ||
                type == typeof(ulong?) ||
                type == typeof(DateTime?) ||
                type == typeof(DateTimeOffset?) ||
                type == typeof(TimeSpan?))
            {
                return true;
            }

            return false;
        }


        // code from Dapper
        // https://github.com/StackExchange/Dapper/blob/4fb1ea29d490d13251b0135658ecc337aeb60cdb/Dapper/SqlMapper.cs#L169
        private static readonly Dictionary<Type, DbType> Mappings = new Dictionary<Type, DbType>
                                                                    {
                                                                        [typeof(byte)] = DbType.Byte,
                                                                        [typeof(sbyte)] = DbType.SByte,
                                                                        [typeof(short)] = DbType.Int16,
                                                                        [typeof(ushort)] = DbType.UInt16,
                                                                        [typeof(int)] = DbType.Int32,
                                                                        [typeof(uint)] = DbType.UInt32,
                                                                        [typeof(long)] = DbType.Int64,
                                                                        [typeof(ulong)] = DbType.UInt64,
                                                                        [typeof(float)] = DbType.Single,
                                                                        [typeof(double)] = DbType.Double,
                                                                        [typeof(decimal)] = DbType.Decimal,
                                                                        [typeof(bool)] = DbType.Boolean,
                                                                        [typeof(string)] = DbType.String,
                                                                        [typeof(char)] = DbType.StringFixedLength,
                                                                        [typeof(Guid)] = DbType.Guid,
                                                                        [typeof(DateTime)] = DbType.DateTime,
                                                                        [typeof(DateTimeOffset)] =
                                                                            DbType.DateTimeOffset,
                                                                        [typeof(TimeSpan)] = DbType.Time,
                                                                        [typeof(byte[])] = DbType.Binary,
                                                                        [typeof(byte?)] = DbType.Byte,
                                                                        [typeof(sbyte?)] = DbType.SByte,
                                                                        [typeof(short?)] = DbType.Int16,
                                                                        [typeof(ushort?)] = DbType.UInt16,
                                                                        [typeof(int?)] = DbType.Int32,
                                                                        [typeof(uint?)] = DbType.UInt32,
                                                                        [typeof(long?)] = DbType.Int64,
                                                                        [typeof(ulong?)] = DbType.UInt64,
                                                                        [typeof(float?)] = DbType.Single,
                                                                        [typeof(double?)] = DbType.Double,
                                                                        [typeof(decimal?)] = DbType.Decimal,
                                                                        [typeof(bool?)] = DbType.Boolean,
                                                                        [typeof(char?)] = DbType.StringFixedLength,
                                                                        [typeof(Guid?)] = DbType.Guid,
                                                                        [typeof(DateTime?)] = DbType.DateTime,
                                                                        [typeof(DateTimeOffset?)] =
                                                                            DbType.DateTimeOffset,
                                                                        [typeof(TimeSpan?)] = DbType.Time,
                                                                        [typeof(object)] = DbType.Object
                                                                    };
        public static DbType ResolveDbType(this Type type)
        {
            return Mappings[type];
        }

    }
}
