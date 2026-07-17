using System.Reflection;
using Discord.Interactions;

namespace IndYBot.Extensions;

public static class EnumExtensions
{
   public static string GetChoiceDisplay(this Enum value)
   {
      Type type = value.GetType();
      FieldInfo? fieldInfo = type.GetField(value.ToString());

      if (fieldInfo == null) return value.ToString();

      var attribute = fieldInfo.GetCustomAttribute<ChoiceDisplayAttribute>();

      return attribute != null ? attribute.Name : value.ToString();
   }
}
