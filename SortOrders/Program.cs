namespace DelApp
{
    class Program
    {
        static string assemblyDyrectory = "";

        static List<Order> orders = [];
        static List<Order> FilteredOrders = [];
        static DateTime MinimumPlus30min;
        static string Log = "";

        // Может быть получена из командной строки.
        static string PathSettings = "";

        // Могут быть заданы в командной строке или в настроечном файле.
        static string? PathInputOrders = null;
        static string? PathResultOrders = null;
        static string? PathLog = null;
        static DateTime? FilterMinimum = null;
        static string? FilterDistrict = null;

        /// <summary>
        /// На вход должен поступить фильтр по времени "с" и "по".
        /// Формат yyyy-MM-DD HH:mm:ss
        /// </summary>
        /// <param name="args">Фильтры по времени: "От" и "До"</param>
        public static void Main(string[] args)
        {
            Log = $"{DateTime.Now:yyyy.MM.dd-HH.mm.ss}: Начало работы";

            Filtrate(args);

            Log += $"\n{DateTime.Now:yyyy.MM.dd-HH.mm.ss}: " +
                $"Работа программы завершена.";
            PrintLog();
        }

        internal static void Filtrate(string[] args)
        {
            ReadFilter(args);
            // Прочитать файл с настройками, если он задан.
            // Приоритетными являются аргументы командной строки.
            if (File.Exists(PathSettings))
            {
                TryReadSettings();
            }
            SetDefaultPaths();

            MinimumPlus30min = ((DateTime)FilterMinimum!).AddMinutes(30);

            WriteParametersToLog();

            if (File.Exists(PathInputOrders))
            {
                TryReadOrders();
            }
            else
            {
                Log += $"\n{DateTime.Now:yyyy.MM.dd-HH.mm.ss}: " +
                    $"Возникла ошибка чтения файла заказов.";
                return;
            }

            Filtrate();
            PrintOutput();
        }

        private static void ReadFilter(string[] args)
        {
            foreach (string arg in args)
            {
                if (arg.StartsWith("_cityDistrict="))
                {
                    FilterDistrict = arg.Split('=')[1];
                }
                if (arg.StartsWith("_firstDeliveryDateTime="))
                {
                    if (DateTime.TryParse(arg.Split('=')[1], out DateTime min))
                        FilterMinimum = min;
                }
                if (arg.StartsWith("_deliveryLog="))
                {
                    PathLog = arg.Split('=')[1];
                }
                if (arg.StartsWith("_deliveryOrder="))
                {
                    PathResultOrders = arg.Split('=')[1];
                }
                if (arg.StartsWith("_settings="))
                {
                    PathSettings = arg.Split('=')[1];
                }
                if (arg.StartsWith("_inputOrders="))
                {
                    PathInputOrders = arg.Split("=")[1];
                }
            }

            Log += $"\n{DateTime.Now:yyyy.MM.dd-HH.mm.ss}: " +
                $"Получено параметров командной строки - {args.Length}";
        }

        private static void SetDefaultPaths()
        {
            assemblyDyrectory = Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location)!;


            PathInputOrders ??= assemblyDyrectory + "\\files\\Orders.json";
            PathSettings ??= assemblyDyrectory + "\\files\\Settings.json";
            PathResultOrders ??= assemblyDyrectory + "\\result\\ResultOrders.json";
            PathLog ??= assemblyDyrectory + "\\result\\Log.txt";
            FilterMinimum ??= DateTime.MinValue;
        }

        private static bool TryReadSettings()
        {
            SettingsRoot? settings = Settings_Json.ReadSettings(PathSettings);
            if (settings == null)
                return false;

            PathInputOrders ??= settings.PathOrders;

            PathResultOrders ??= settings.PathResultOrders;

            FilterDistrict ??= settings.DistrictId;

            if (DateTime.TryParse(settings.FirstDeliveryTime, out DateTime min))
                FilterMinimum ??= min;

            return true;
        }

        private static bool TryReadOrders()
        {
            OrdersRoot? root = Orders_Json.ReadOrderJson(PathInputOrders!);
            if (root == null)
            {
                Log += $"\n{DateTime.Now:yyyy.MM.dd-HH.mm.ss}: " +
                    $"Не удалось прочитать файл с заказами ({PathInputOrders})";
                return false;
            }

            if (root.Orders == null)
            {
                Log += $"\n{DateTime.Now:yyyy.MM.dd-HH.mm.ss}: " +
                    $"Отсутствуют записи заказов ({PathInputOrders})";
                return false;
            }


            List<Order> all_orders = new List<Order>();
            foreach (JsonOrder order in root.Orders)
            {
                all_orders.Add(new(order));
            }

            Log += $"\n{DateTime.Now:yyyy.MM.dd-HH.mm.ss}: " +
                $"Получено {all_orders.Count} записей о заказах.";

            // Не учитывать данные с ошибками ID, DistrictId и с неуникальными идентификаторами.
            orders = all_orders.Where(x
                => x.ErrorMessage == null
                && !all_orders.Any(y
                    => y != x
                    && y.Id == x.Id)
                ).ToList();

            Log += $"\n{DateTime.Now:yyyy.MM.dd-HH.mm.ss}: " +
                $"Получено {orders.Count} правильных заказов.";

            // Список записей с ошибками.
            // Можно вывести инфомрацию о них.
            List<Order> error_orders = all_orders.Where(x
                => x.ErrorMessage != null
                || all_orders.Any(y
                    => y != x
                    && y.Id == x.Id)
                ).ToList();

            Log += $"\n{DateTime.Now:yyyy.MM.dd-HH.mm.ss}: " +
                $"Получено {error_orders.Count} неправильных заказов.";

            return true;
        }

        private static void WriteParametersToLog()
        {
            Log += $"\n{DateTime.Now:yyyy.MM.dd-HH.mm.ss}: " +
                $"\nPathSettings = {PathSettings}" +
                $"\nPathInputOrders = {PathInputOrders}" +
                $"\nPathResultOrders = {PathResultOrders}" +
                $"\nPathLog = {PathLog}" +
                $"\nFilterMinimum = {FilterMinimum:yyyy.MM.dd-HH.mm.ss}" +
                $"\nFilterMaximum = {MinimumPlus30min:yyyy.MM.dd-HH.mm.ss}" +
                $"\nFilterDistrict = {FilterDistrict}";
        }

        private static void Filtrate()
        {
            if (FilterDistrict == null)
                FilteredOrders = orders.Where(x
                    => DateTime.Compare(x.DeliveryDateTame, (DateTime)FilterMinimum!) >= 0
                    && DateTime.Compare(x.DeliveryDateTame, MinimumPlus30min) <= 0)
                    .ToList();
            else
                FilteredOrders = orders.Where(x
                    => DateTime.Compare(x.DeliveryDateTame, (DateTime)FilterMinimum!) >= 0
                    && DateTime.Compare(x.DeliveryDateTame, MinimumPlus30min) <= 0
                    && x.DistrictId == FilterDistrict)
                    .ToList();

            Log += $"\n{DateTime.Now:yyyy.MM.dd-HH.mm.ss}: " +
                $"Получено {FilteredOrders.Count} отфильтрованных заказов";
        }

        private static void PrintOutput()
        {
            if (Orders_Json.PrintFilteredOrders(PathResultOrders!, FilteredOrders))
                Log += $"\n{DateTime.Now:yyyy.MM.dd-HH.mm.ss}: " +
                    $"Результат записан в файл {PathResultOrders}";
            else
                Log += $"\n{DateTime.Now:yyyy.MM.dd-HH.mm.ss}: " +
                    $"Не удалось сохранить результат ({PathResultOrders})";
        }

        private static void PrintLog()
        {
            string directoryLog = Path.GetDirectoryName(PathLog)!;
            if (!Directory.Exists(directoryLog))
            {
                try
                {
                    Directory.CreateDirectory(directoryLog);
                }
                catch { }
            }

            try
            {
                File.WriteAllText(PathLog!, Log);
            }
            catch { }
        }
    }
}