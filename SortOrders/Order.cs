using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelApp
{
    internal class Order
    {
        public int Id { get; private set; } = -1;
        public string? ErrorId { get; private set; } = null;
        public string? DistrictId { get; private set; }
        //public string? ErrorDistrictId { get; private set; }
        public DateTime DeliveryDateTame { get; private set; } = DateTime.MaxValue;
        public string? ErrorDeliveryDateTame { get; private set; } = null;
        public double Weight { get; private set; } = -1;
        public string? ErrorWeight { get; private set; } = null;

        public string? ErrorMessage { get; private set; } = null;

        internal Order(JsonOrder jOrder)
        {
            if (int.TryParse(jOrder.OrderId, out int id)
             && id > 0)
                Id = id;
            else
            {
                ErrorMessage += "\nНе удалось определить идентификатор заказа.";
                ErrorId = jOrder.OrderId;
            }

            //if (int.TryParse(jOrder.DistrictId, out int districtId)
            // && districtId > 0)
            //    DistrictId = districtId;
            //else
            //{ 
            //    ErrorMessage += "\nНе удалось определить идентификатор района.";
            //    ErrorDistrictId = jOrder.DistrictId;
            //}
            DistrictId = jOrder.DistrictId;

            if (jOrder.DeliveryDateTime != null
             && DateTime.TryParse(jOrder.DeliveryDateTime, out DateTime collapsedDateTime))
                DeliveryDateTame = collapsedDateTime;
            else
            {
                ErrorMessage += "\nНе удалось определить время доставки заказа.";
                ErrorDeliveryDateTame = jOrder.DeliveryDateTime;
            }

            if (double.TryParse(jOrder.Weight.Replace('.', ','), out double weight)
             && weight >= 0)
                Weight = weight;
            else
            {
                ErrorMessage += "Не удалось определить вес заказа.";
                ErrorWeight = jOrder.Weight;
            }
        }
    }
}
