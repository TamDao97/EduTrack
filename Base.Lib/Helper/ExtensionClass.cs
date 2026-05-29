using System.ComponentModel;
using System.Reflection;

namespace TD.Lib.Helper
{
    public static class EnumExtensions
    {
        public static string ToDescription(this Enum value)
        {
            var da = (DescriptionAttribute[])value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            return da.Length > 0 ? da[0].Description : value.ToString();
        }

        public static List<(int Value, string Description)> ToList<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
                .Cast<T>()
                .Select(e => (
                    Convert.ToInt32(e),
                    GetEnumDescription(e)
                )).ToList();
        }

        private static string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attr = field.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description ?? value.ToString();
        }
    }

    public static class GuidExtensions
    {
        public static bool GuidIsNullOrEmpty(this Guid? guid)
        {
            return !guid.HasValue || guid == Guid.Empty;
        }
    }

}
