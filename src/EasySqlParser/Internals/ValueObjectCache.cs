using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using EasySqlParser.Configurations;
using EasySqlParser.Internals.Dialect;

namespace EasySqlParser.Internals
{
    /*
     * Value Object does not mean Domain Object.
     * Value Object means simple POCO class.
     */
    internal class ValueObjectCache
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCaches =
            new ConcurrentDictionary<Type, PropertyInfo[]>();

        internal static PropertyInfo[] GetPropertyInfoCaches(Type type)
        {
            if (PropertyCaches.TryGetValue(type, out var result))
            {
                return result;
            }

            result = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            PropertyCaches.TryAdd(type, result);
            return result;
        }

        // for unit test
        internal static ValueObjectWrapper GetValueObjects(List<ParameterEmulator> emulators, SqlParserConfig config)
        {
            if (emulators == null)
            {
                throw new ArgumentNullException(nameof(emulators));
            }

            var valueObjects = new Dictionary<string, ValueObject>();
            var propertyValues = new Dictionary<string, ValueWrapper>();
            foreach (var emulator in emulators)
            {
                var vo = new ValueObject(config, emulator.Name, emulator.ParameterValue, emulator.ParameterType);
                vo.Initialize();
                valueObjects.Add(emulator.Name, vo);
                var wrapper = new ValueWrapper
                {
                    Name = emulator.Name,
                    Value = vo.Value,
                    Type = emulator.ParameterType
                };
                propertyValues.Add(wrapper.Name, wrapper);
            }
            var result = new ValueObjectWrapper(propertyValues, valueObjects);

            return result;
        }

        internal static ValueObjectWrapper GetValueObjects(object instance, SqlParserConfig config)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            var type = instance.GetType();
            var valueObjects = new Dictionary<string, ValueObject>();
            var propertyValues = new Dictionary<string, ValueWrapper>();
            var properties = GetPropertyInfoCaches(type);
            foreach (var propertyInfo in properties)
            {
                var vo = new ValueObject(propertyInfo, config,
                    propertyInfo.Name, propertyInfo.GetValue(instance), propertyInfo.PropertyType);
                vo.Initialize();
                valueObjects.Add(propertyInfo.Name, vo);
                var wrapper = new ValueWrapper
                {
                    Name = propertyInfo.Name,
                    Value = vo.Value,
                    Type = propertyInfo.PropertyType
                };
                propertyValues.Add(propertyInfo.Name, wrapper);
            }

            var result = new ValueObjectWrapper(propertyValues, valueObjects);

            return result;
        }

        internal static ValueObjectWrapper GetValueObjects<T>(string name, T value, SqlParserConfig config)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            // type check
            var type = typeof(T);
            
            // TODO:要検討
            bool IsValidType()
            {
                if (type == typeof(string) ||
                    type == typeof(DateTime) ||
                    type == typeof(int) ||
                    type == typeof(long) ||
                    type == typeof(bool) ||
                    type == typeof(decimal) ||
                    type == typeof(byte) ||
                    type == typeof(byte[]) ||
                    type == typeof(double) ||
                    type == typeof(float) ||
                    type == typeof(sbyte) ||
                    type == typeof(short) ||
                    type == typeof(uint) ||
                    type == typeof(ulong) ||
                    type == typeof(ushort) ||
                    type == typeof(DateTimeOffset) ||
                    type == typeof(TimeSpan))
                {
                    return true;
                }

                if (type == typeof(IEnumerable))
                {

                }

                return false;
            }

            if (!IsValidType())
            {
                //TODO:
                throw new InvalidOperationException("");
            }


            var valueObjects = new Dictionary<string, ValueObject>();
            var vo = new ValueObject(config, name, value, type);
            vo.Initialize();
            valueObjects.Add(name, vo);
            var propertyValues = new Dictionary<string, ValueWrapper>();
            var wrapper = new ValueWrapper
                               {
                                   Name = name,
                                   Value = value,
                                   Type = type
                               };
            propertyValues.Add(name, wrapper);

            var result = new ValueObjectWrapper(propertyValues, valueObjects);

            return result;
        }

    }


    internal class ValueObjectWrapper
    {
        internal ValueObjectWrapper(Dictionary<string, ValueWrapper> propertyValues,
            Dictionary<string, ValueObject> valueObjects)
        {
            PropertyValues = propertyValues;
            ValueObjects = valueObjects;
        }
        internal Dictionary<string, ValueWrapper> PropertyValues { get; }
        internal Dictionary<string, ValueObject> ValueObjects { get; }
    }


    /// <summary>
    /// PropertyInfo Wrapper
    /// </summary>
    internal class ValueObject
    {
        private readonly StandardDialect _dialect;
        private readonly PropertyInfo _propertyInfo;

        internal ValueObject(PropertyInfo propertyInfo, SqlParserConfig config,
            string parameterName, object parameterValue, Type parameterType)
        {
            _propertyInfo = propertyInfo;
            _dialect = config.Dialect;
            PropertyName = parameterName;
            Value = parameterValue;
            DataType = parameterType;
        }

        internal ValueObject(SqlParserConfig config,
            string parameterName, object parameterValue, Type parameterType)
        {
            _dialect = config.Dialect;
            PropertyName = parameterName;
            Value = parameterValue;
            DataType = parameterType;
        }

        private bool _initialized;

        internal void Initialize()
        {
            if (_initialized) return;
            DbParameters = new Dictionary<string, object>();
            EasySqlParameterAttribute = _propertyInfo?.GetCustomAttribute<EasySqlParameterAttribute>();
            var name = _dialect.ParameterPrefix + PropertyName;
            if (Value != null)
            {
                IsEnumerable = DetectGenericListType(DataType);
                if (!IsEnumerable)
                {
                    //var name = _dialect.EnableNamedParameter
                    //    ? _dialect.ParameterPrefix + PropertyName
                    //    : _dialect.ParameterPrefix;
                    DbParameters.Add(name, Value);
                    if (Value is DateTime)
                    {
                        IsDateTime = true;
                    }
                    else if (Value is string)
                    {
                        IsString = true;
                    }
                    else if (Value is DateTimeOffset)
                    {
                        IsDateTimeOffset = true;
                    }
                    else if (Value is TimeSpan)
                    {
                        IsTimeSpan = true;
                    }
                }
            }
            else
            {
                DbParameters.Add(name, null);
            }

            _initialized = true;
        }

        internal EasySqlParameterAttribute EasySqlParameterAttribute { get; private set; }

        internal string PropertyName { get; }

        internal object Value { get; }

        internal Dictionary<string, object> DbParameters { get; private set; }

        internal bool IsEnumerable { get; private set; }

        internal Type DataType { get; }

        internal bool IsDateTime { get; private set; }

        internal bool IsDateTimeOffset { get; private set; }

        internal bool IsTimeSpan { get; private set; }

        internal bool IsString { get; private set; }

        internal Type GenericParameterType { get; private set; }

        private bool DetectGenericListType(Type type)
        {
            var isArray = type.IsArray;
            if (isArray ||
                (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)))
            {
                if (isArray)
                {
                    GenericParameterType = type.GetElementType();
                }
                else
                {
                    GenericParameterType = type.GetGenericArguments()[0];
                }

                var values = Value as IEnumerable;
                CreateParameters(values);
                return true;
            }

            return false;

        }


        private void CreateParameters(IEnumerable values)
        {
            var index = 1;
            foreach (var v in values)
            {
                if (v == null)
                {
                    continue;
                }

                //var name = _dialect.EnableNamedParameter
                //    ? $"{_dialect.ParameterPrefix}{PropertyName}{index}"
                //    : _dialect.ParameterPrefix;
                var name = $"{_dialect.ParameterPrefix}{PropertyName}{index}";
                DbParameters.Add(name, v);
                index++;
            }
        }
    }

    internal class ValueWrapper
    {
        internal string Name { get; set; }

        internal object Value { get; set; }

        internal Type Type { get; set; }
    }

    // for unit test
    internal class ParameterEmulator
    {
        internal string Name { get; set; }
        internal Type ParameterType { get; set; }
        internal object ParameterValue { get; set; }
    }
}
