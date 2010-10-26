using System;
using System.Net.Sockets;
using System.Net;

public class Main2
{
    static void Main()
	{
            var sk = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

			EndPoint my = new IPEndPoint(IPAddress.Loopback,1233);
            sk.BeginReceiveFrom(new byte[1500], 0, 1500, SocketFlags.None, ref my, new AsyncCallback(ReceiveCallback), null);
            sk.Close();
	}
    
    static void ReceiveCallback(IAsyncResult ar)
    {
    }
}