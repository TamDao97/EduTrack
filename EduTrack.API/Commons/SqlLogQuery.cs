using Dapper;
using System.Data;

namespace EduTrack.API.Commons
{
    public class SqlLogQuery
    {
        public static string BuildExecCommandV1(string procName, object parameters)
        {
            var sql = $"EXEC {procName} ";

            if (parameters == null)
                return sql;

            var props = parameters.GetType().GetProperties();

            var paramStrings = props.Select(p =>
            {
                var value = p.GetValue(parameters);

                if (value == null)
                    return $"@{p.Name} = NULL";

                if (value is string str)
                    return $"@{p.Name} = N'{str.Replace("'", "''")}'";

                if (value is DateTime dt)
                    return $"@{p.Name} = '{dt:yyyy-MM-dd HH:mm:ss}'";

                if (value is Guid guid)
                    return $"@{p.Name} = '{guid}'";

                if (value is bool b)
                    return $"@{p.Name} = {(b ? 1 : 0)}";

                return $"@{p.Name} = {value}";
            });

            sql += string.Join(", ", paramStrings);

            return sql;
        }

        public static string BuildExecCommandV2(string procName, DynamicParameters parameters)
        {
            var sql = $"EXEC {procName} ";

            var paramStrings = parameters.ParameterNames.Select(name =>
            {
                var value = parameters.Get<object>(name);

                if (value == null)
                    return $"{name} = NULL";

                if (value is string str)
                    return $"{name} = N'{str.Replace("'", "''")}'";

                if (value is DateTime dt)
                    return $"{name} = '{dt:yyyy-MM-dd HH:mm:ss}'";

                if (value is Guid guid)
                    return $"{name} = '{guid}'";

                if (value is bool b)
                    return $"{name} = {(b ? 1 : 0)}";

                return $"{name} = {value}";
            });

            sql += string.Join(", ", paramStrings);

            return sql;
        }
    }
}
