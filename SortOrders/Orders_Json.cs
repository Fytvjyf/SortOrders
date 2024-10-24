using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace DelApp
{
    public class JsonOrder
    {
        public string? OrderId { get; set; }
        public string? DistrictId { get; set; }
        public string? DeliveryDateTime { get; set; }
        public string? Weight { get; set; }
    }

    public class OrdersRoot
    {
        public List<JsonOrder>? Orders { get; set; }
    }

    internal class Orders_Json
    {
        internal static OrdersRoot? ReadOrderJson(string path)
        {
            OrdersRoot? root = null;

            try
            {
                using (FileStream fs = new(path, FileMode.OpenOrCreate))
                {
                    root = JsonSerializer.Deserialize<OrdersRoot>(fs)!;
                }
            }
            catch { }

            return root;
        }

        internal static bool PrintFilteredOrders(string pathResultOrders, List<Order> filteredOrders)
        {
            OrdersRoot root = CreateOutput(filteredOrders);
            CreateDirectory(pathResultOrders);
            try
            {
                // Для красивой табуляции.
                JsonSerializerOptions options = new()
                {
                    WriteIndented = true
                };
                using (FileStream fs = new(pathResultOrders, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    JsonSerializer.Serialize<OrdersRoot>(fs, root, options);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        private static OrdersRoot CreateOutput(List<Order> filteredOrders)
        {
            string dateFormat = "yyyy-MM-dd HH:mm:ss";
            OrdersRoot result = new()
            {
                Orders = []
            };
            foreach (Order order in filteredOrders)
            {
                result.Orders.Add(new()
                {
                    OrderId = order.Id.ToString(),
                    DistrictId = order.DistrictId,
                    DeliveryDateTime = order.DeliveryDateTame.ToString(dateFormat),
                    Weight = order.Weight.ToString().Replace(',', '.')
                });
            }

            return result;
        }

        private static void CreateDirectory(string fullPath)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            }
            catch { }
            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch { }
            }
        }

        private static string OutputFileName(string directoryOutput)
        {
            if (!Directory.Exists(directoryOutput))
            {
                try
                {
                    Directory.CreateDirectory(directoryOutput);
                }
                catch { }
            }
            //string time = string.Format("yyyy.MM.dd-HH.mm.ss", DateTime.Now);
            return directoryOutput + $"\\FilteredOrders({DateTime.Now:yyyy.MM.dd-HH.mm.ss}).json";
        }
    }
}
