
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
namespace PrintIngredientsList
{
    internal class Network
    {

        public static string GetMacAddress()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in nics)
            {
                if( adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    return adapter.GetPhysicalAddress().ToString();
                }
            }

            return null;
        }
    }
}
