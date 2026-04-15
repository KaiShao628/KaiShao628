using System.Reflection;

namespace FamilyLedgeManagement.Utilities
{
    public class FamilyLedgeMapper
    {
        public static T Map<J, T>(J model)
     where T : class, new()
     where J : class, new()
        {
            if (model is null)
            {
                return default;
            }
            Type jType = typeof(J), tType = typeof(T);

            var tModel = new T();
            foreach (var tItem in tType.GetProperties())
            {
                var tItemName = tItem.Name;
                var tItemType = GetType(tItem);

                foreach (var jItem in jType.GetProperties())
                {
                    var jItemName = jItem.Name;
                    var jItemType = GetType(jItem);

                    if (tItemName == jItemName && tItemType == jItemType)
                    {
#pragma warning disable CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
                        object value = jItem.GetValue(model, null);
#pragma warning restore CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
                        tItem.SetValue(tModel, value, null);
                    }
                }
            }

            return tModel;
        }

        private static string GetType(PropertyInfo? p)
        {
            var type = p.PropertyType;
            if (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = p.PropertyType.GetGenericArguments()[0];
            }
            return type.Name;
        }

        public static List<T> MapList<J, T>(IEnumerable<J> listJ) where T : class, new()
         where J : class, new()
        {
            Type jType = typeof(J), tType = typeof(T);
            var listT = new List<T>();
            if (listJ is null)
            {
                return listT;
            }
            foreach (var i in listJ)
            {
                var tModel = new T();
                foreach (var tItem in tType.GetProperties())
                {
                    var tItemName = tItem.Name;
                    var tItemType = GetType(tItem);

                    foreach (var jItem in jType.GetProperties())
                    {
                        var jItemName = jItem.Name;
                        var jItemType = GetType(jItem);

                        if (tItemName == jItemName && tItemType == jItemType)
                        {
#pragma warning disable CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
                            object value = jItem.GetValue(i, null);
#pragma warning restore CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
                            tItem.SetValue(tModel, value, null);
                        }
                    }
                }
                listT.Add(tModel);
            }
            return listT;
        }
    }
}
