using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApi.Helpers
{
    public class DataHelper
    {
        public static bool IsValidUrl(string url)
        {
            Uri result;
            return Uri.TryCreate(url, UriKind.Absolute, out result)
                && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }

        public static String GetConnectionString()
        {
            return System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ToString();
        }

        public static Dictionary<string, string> GetAppSettings()
        {
            return System.Configuration.ConfigurationManager.AppSettings.AllKeys.ToDictionary(k => k);
        }
    }
}
