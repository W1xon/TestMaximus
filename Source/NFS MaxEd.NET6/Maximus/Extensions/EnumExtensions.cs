using System.ComponentModel;
using System.Reflection;

namespace Maximus.Extensions;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        FieldInfo fieldInfo = value.GetType().GetField(value.ToString());
        DescriptionAttribute attribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
        
            return attribute is null ? value.ToString() : attribute.Description;
    }
}