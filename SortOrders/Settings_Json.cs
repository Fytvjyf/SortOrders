using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Reflection;

namespace DelApp
{
    public class SettingsRoot
    {
        public string PathOrders { get; set; } = "";
        public string PathLog { get; set; } = "";
        public string PathResultOrders { get; set; } = "";
        public string FirstDeliveryTime { get; set; } = "";
        public string DistrictId { get; set; } = "";
    }

    internal class Settings_Json
    {
        private static string AssemblyDirectory = "";

        internal static SettingsRoot? ReadSettings(string path)
        {
            SettingsRoot? root = null;
            try
            {
                using (FileStream fs = new(path, FileMode.OpenOrCreate))
                {
                    root = JsonSerializer.Deserialize<SettingsRoot>(fs)!;
                }

                SwitchVariablesToValues(root);
            }
            catch { }
            return root;
        }

        private static void SwitchVariablesToValues(SettingsRoot root)
        {
            AssemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

            root.PathOrders = GetValues(root.PathOrders);
            root.PathLog = GetValues(root.PathLog);
            root.PathResultOrders = GetValues(root.PathResultOrders);
        }

        private static string GetValues(string path)
        {
            string result = path;
            if (path.Contains("%ASSEMBLYDIRECTORY%"))
                result = result.Replace("%ASSEMBLYDIRECTORY%", AssemblyDirectory);

            return result;
        }
    }
}
