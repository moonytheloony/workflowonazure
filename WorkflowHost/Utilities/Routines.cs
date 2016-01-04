namespace WorkflowHost.Utilities
{
    #region

    using System;
    using System.Activities;
    using System.Activities.XamlIntegration;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Xaml;

    #endregion

    public static class Routines
    {
        #region Public Methods and Operators

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
                var activity = ActivityXamlServices.Load(
                    new XamlXmlReader(stringReader, settings1),
                    settings2);
                return activity;
            }
            finally
            {
                stringReader?.Dispose();
            }
        }

        public static bool CompareCaseInvariant(this string value, string newValue)
        {
            Contract.Requires<Exception>(!string.IsNullOrEmpty(value), "value");
            Contract.Requires<Exception>(!string.IsNullOrEmpty(newValue), "newValue");
            return value.Equals(newValue, StringComparison.OrdinalIgnoreCase);
        }

        public static string FormatStringInvariantCulture(string value, params object[] arguments)
        {
            return string.Format(CultureInfo.InvariantCulture, value, arguments);
        }

        public static string Combine(this IList<string> value)
        {
            return string.Join(string.Empty, value);
        }

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