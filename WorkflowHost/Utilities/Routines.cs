// ***********************************************************************
// Assembly         : WorkflowHost
// Author           : rahulrai
// Created          : 01-04-2016
//
// Last Modified By : rahulrai
// Last Modified On : 01-07-2016
// ***********************************************************************
// <copyright file="Routines.cs" company="">
//     Copyright ©  2016
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace WorkflowHost.Utilities
{
    #region

    using System;
    using System.Activities;
    using System.Activities.XamlIntegration;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Xaml;

    #endregion

    /// <summary>
    /// Class Routines.
    /// </summary>
    public static class Routines
    {
        #region Public Methods and Operators

        /// <summary>
        /// Combines the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>System.String.</returns>
        public static string Combine(this IList<string> value)
        {
            return string.Join(string.Empty, value);
        }

        /// <summary>
        /// Compares the case invariant.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool CompareCaseInvariant(this string value, string newValue)
        {
            return value.Equals(newValue, StringComparison.OrdinalIgnoreCase);
        }

        public static Activity CreateWorkflowActivityFromXaml(string workflowXaml, Assembly type)
        {
            if (string.IsNullOrEmpty(workflowXaml) || type == null)
            {
                return null;
            }

            var xmlReaderSettings = new XamlXmlReaderSettings { LocalAssembly = type };
            var settings1 = xmlReaderSettings;
            StringReader stringReader = null;
            var settings2 = new ActivityXamlServicesSettings { CompileExpressions = true };
            try
            {
                stringReader = new StringReader(workflowXaml);
                var activity = ActivityXamlServices.Load(new XamlXmlReader(stringReader, settings1), settings2);
                return activity;
            }
            finally
            {
                stringReader?.Dispose();
            }
        }

        /// <summary>
        /// Formats the string invariant culture.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>System.String.</returns>
        public static string FormatStringInvariantCulture(string value, params object[] arguments)
        {
            return string.Format(CultureInfo.InvariantCulture, value, arguments);
        }

        /// <summary>
        /// Splits the length of the by.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="maxLength">The maximum length.</param>
        /// <returns>System.Collections.Generic.IEnumerable&lt;System.String&gt;.</returns>
        public static IEnumerable<string> SplitByLength(this string value, int maxLength)
        {
            for (var index = 0; index < value.Length; index += maxLength)
            {
                yield return value.Substring(index, Math.Min(maxLength, value.Length - index));
            }
        }

        #endregion
    }
}