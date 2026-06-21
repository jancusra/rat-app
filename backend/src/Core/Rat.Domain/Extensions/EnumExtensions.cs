using System;
using System.Collections.Generic;
using System.Linq;

namespace Rat.Domain.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Convert enum to dictionary
        /// </summary>
        /// <typeparam name="TEnum">specific enum</typeparam>
        /// <returns>Dictionary (key = int value, value = enum option)</returns>
        public static Dictionary<int, string> GetEnumDictionary<TEnum>() where TEnum : struct
        {
            if (typeof(TEnum).IsEnum)
            {
                var enumValues = from TEnum enumValue in Enum.GetValues(typeof(TEnum))
                                 select enumValue;

                return enumValues.Cast<TEnum>()
                    .ToDictionary(ev => Convert.ToInt32(ev), ev => ev.ToString());
            }

            return new Dictionary<int, string>();
        }

        /// <summary>
        /// Convert input string to enum option
        /// </summary>
        /// <typeparam name="TEnum">enum for conversion</typeparam>
        /// <param name="value">input string value</param>
        /// <param name="ignoreCase">ignore string cases</param>
        /// <returns>final enum option</returns>
        public static TEnum ToEnum<TEnum>(this string value, bool ignoreCase = false)
        {
            return (TEnum)Enum.Parse(typeof(TEnum), value, ignoreCase);
        }
    }
}
