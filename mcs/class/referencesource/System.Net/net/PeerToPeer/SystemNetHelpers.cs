namespace System.Net.PeerToPeer
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Collections.Generic;
    using System.Text;
    using System.Security.Permissions;

    internal static class SystemNetHelpers
    {
        internal const int IPv6AddressSize = 28;
        internal const int IPv4AddressSize = 16;
        internal static byte[] SOCKADDRFromIPEndPoint(IPEndPoint ipEndPoint)
        {
            byte[] buffer = new byte[ipEndPoint.AddressFamily == AddressFamily.InterNetworkV6 ? IPv6AddressSize : IPv4AddressSize];
#if BIGENDIAN
            buffer[0] = unchecked((byte)((int)family>>8));
            buffer[1] = unchecked((byte)((int)family   ));
#else
            buffer[0] = unchecked((byte)((int)ipEndPoint.AddressFamily));
            buffer[1] = unchecked((byte)((int)ipEndPoint.AddressFamily >> 8));
#endif
            if (ipEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
            {
                buffer[2] = (byte)(ipEndPoint.Port >> 8);
                buffer[3] = (byte)ipEndPoint.Port;

                buffer[4] = (byte)0;
                buffer[5] = (byte)0;
                buffer[6] = (byte)0;
                buffer[7] = (byte)0;

                long scope = ipEndPoint.Address.ScopeId;
                buffer[24] = (byte)scope;
                buffer[25] = (byte)(scope >> 8);
                buffer[26] = (byte)(scope >> 16);
                buffer[27] = (byte)(scope >> 24);

                byte[] addressBytes = ipEndPoint.Address.GetAddressBytes();
                for (int i = 0; i < addressBytes.Length; i++)
                {
                    buffer[8 + i] = addressBytes[i];
                }
            }
            else
            {
                buffer[2] = unchecked((byte)(ipEndPoint.Port >> 8));
                buffer[3] = unchecked((byte)(ipEndPoint.Port));
                byte[] addressBytes = ipEndPoint.Address.GetAddressBytes();
                for (int i = 0; i < addressBytes.Length; i++)
                {
                    buffer[4 + i] = addressBytes[i];
                }
            }
            return buffer;
        }
        internal static IPEndPoint IPEndPointFromSOCKADDRBuffer(byte[] buffer)
        {
            IPAddress ip = null;
            int addressFamily = 0;
#if BIGENDIAN
            addressFamily = buffer[1] + ((int)buffer[0] << 8);
#else
            addressFamily = buffer[0] + ((int)buffer[1] << 8);
#endif
            //Get port
            int port = buffer[3] + ((int)buffer[2] << 8);

            if ((AddressFamily)addressFamily == AddressFamily.InterNetwork)
            {
                byte[] v4bytes = new byte[] { buffer[4], buffer[5], buffer[6], buffer[7] };
                ip = new IPAddress(v4bytes);
            }
            else if ((AddressFamily)addressFamily == AddressFamily.InterNetworkV6)
            {
                byte[] v6Bytes = new byte[16];
                for (int i = 0; i < 16; i++)
                    v6Bytes[i] = buffer[8 + i];
                long scope = ((long)(long)buffer[24] + ((long)buffer[25] << 8) + ((long)buffer[26] << 16) + ((long)buffer[27] << 24));
                ip = new IPAddress(v6Bytes);
                ip.ScopeId = scope;
            }

            return new IPEndPoint(ip, port);

        }
    }
}

