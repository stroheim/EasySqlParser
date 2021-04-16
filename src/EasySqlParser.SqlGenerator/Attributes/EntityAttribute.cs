using System;
using System.Globalization;
using System.Text;
using EasySqlParser.SqlGenerator.Enums;

namespace EasySqlParser.SqlGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class EntityAttribute : Attribute
    {
        public Naming Naming { get; set; } = Naming.None;
    }


    internal static class NamingExtension
    {
        /// <summary>
        /// Applies this convention.
        /// </summary>
        /// <param name="naming"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static string Apply(this Naming naming, string source)
        {
            switch (naming)
            {
                case Naming.LowerCase:
                    return source.ToLower();
                case Naming.UpperCase:
                    return source.ToUpper();
                case Naming.SnakeLowerCase:
                    return FromPascalCaseToSnakeCase(source).ToLower();
                case Naming.SnakeUpperCase:
                    return FromPascalCaseToSnakeCase(source).ToUpper();
            }

            return source;
        }

        /// <summary>
        /// Reverts the text to the original as much as possible.
        /// </summary>
        /// <param name="naming"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static string Revert(this Naming naming, string source)
        {
            switch (naming)
            {
                case Naming.LowerCase:
                    return source.ToUpper();
                case Naming.UpperCase:
                    return source.ToLower();
                case Naming.SnakeLowerCase:
                    return FromSnakeCaseToPascalCase(source);
                case Naming.SnakeUpperCase:
                    return FromSnakeCaseToPascalCase(source);
            }

            return source;
        }

        internal static string FromSnakeCaseToPascalCase(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            var info = CultureInfo.InvariantCulture.TextInfo;
            return info.ToTitleCase(source.ToLower()).Replace("_", "");
        }

        internal static string FromPascalCaseToSnakeCase(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            var builder = new StringBuilder();
            var appended = false;
            foreach (var c in source)
            {
                if (char.IsUpper(c) && appended)
                {
                    builder.Append("_");
                }

                builder.Append(char.ToLower(c));
                appended = true;
            }

            return builder.ToString();
        }
    }
}
