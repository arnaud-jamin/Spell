using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Spell
{
    public static class ReflectionHelper
    {
        // ----------------------------------------------------------------------------------------
        public static bool HasCustomAttribute<T>(this Type type, bool inherit = false) where T : Attribute
        {
            return type.GetCustomAttributes(typeof(T), inherit).Length > 0;
        }

        // ----------------------------------------------------------------------------------------
        public static bool Inherit<T>(this Type type)
        {
            return (typeof(T).IsAssignableFrom(type));
        }

        // ----------------------------------------------------------------------------------------
        public static bool IsEnumMask(this Type type)
        {
            return type.IsEnum && type.HasCustomAttribute<FlagsAttribute>();
        }

        // ----------------------------------------------------------------------------------------
        public static FieldInfo[] GetAllFields(this Type type)
        {
            return type.GetFields(BindingFlags.Instance | BindingFlags.Public);
        }
    }
}
