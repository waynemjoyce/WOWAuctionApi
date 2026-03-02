using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WOWApi
{
    public static class Sys
    {
        public static string Version
        {
            get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }

        public static string BuildDate
        {
            get { return GetBuildDate(System.Reflection.Assembly.GetExecutingAssembly()).ToLongDateString(); }
        }

        private static DateTime GetBuildDate(Assembly assembly)
        {
            const string BuildVersionMetadataPrefix = "+build";

            var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (attribute?.InformationalVersion != null)
            {
                var value = attribute.InformationalVersion;
                var index = value.IndexOf(BuildVersionMetadataPrefix);
                if (index > 0)
                {
                    value = value.Substring(index + BuildVersionMetadataPrefix.Length);
                    if (DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                    {
                        return result;
                    }
                }
            }

            return DateTime.Now;
        }
    }
}
