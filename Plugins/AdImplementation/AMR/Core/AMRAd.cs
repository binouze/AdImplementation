using System;

namespace AMR
{
    public class AMRAd
    {
		public string ZoneId;
		public string Network;
		public double Revenue;
		public string AdSpaceId;
		public string Currency;

		public AMRAd(string zoneId, string network) {
			ZoneId = zoneId;
			Network = network;
			Currency = "USD";
		}

	}
}