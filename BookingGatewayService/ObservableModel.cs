using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingGatewayService
{
    internal class ObservableModel
    {
        public BookingGateway Gateway { get; set; }
        public bool isInProgress { get; set; }

        public ObservableModel(BookingGateway gateway, bool isInProgress)
        {
            Gateway = gateway;
            this.isInProgress = isInProgress;
        }
    }
}
