﻿using HP.Palette.Security.Exceptions;
using System;
using System.Linq;
using System.Reflection;

namespace HP.Palette.Security {
    /// <summary>
    /// Builds Values from a string
    /// </summary>
    /// <remarks>Types must be a string or support a static parse method, and the ToString method
    /// must be overridden to give same value as used for the Parse method</remarks>
    public static class ValueExtensions {
        /// <summary>
        /// Types the has parse method.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns>true if type has a Parse method or type is string; otherwise false</returns>
        internal static bool TypeHasParseMethod(this Type t) {
            return t == typeof(string) || t.GetParseMethod() != null;
        }

        /// <summary>
        /// Gets the parse method.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        internal static MethodInfo GetParseMethod(this Type t) {
            return t.GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(x => x.Name == "Parse");
        }

        /// <summary>
        /// Casts the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="valueAsString">The value as string.</param>
        /// <returns>Value as T</returns>
        public static T CastValue<T>(string valueAsString) {
            if (typeof(T) == typeof(string)) {
                return (T)(object)valueAsString;
            }
            var method = typeof(T).GetStaticParseMethod();
            var val = method.Invoke(null, new object[] { valueAsString });
            return (T)val;
        }

        /// <summary>
        /// Gets the static parse method.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns>Parse Method</returns>
        public static MethodInfo GetStaticParseMethod(this Type t) {
            return t == typeof(string)
                ? null
                : !t.TypeHasParseMethod()
                    ? throw new StorageException($"{t.FullName} must contain a static Parse method.")
                    : t.GetParseMethod();
        }
    }
}