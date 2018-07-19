//------------------------------------------------------------------------------
// <copyright file="IPPacketInformation.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

          
          
namespace System.Net.Sockets {
    using System.Net;

    public struct IPPacketInformation {
        IPAddress address;
        int networkInterface;

        internal IPPacketInformation(IPAddress address, int networkInterface){
            this.address = address;
            this.networkInterface = networkInterface;
        }
        
        public IPAddress Address {
            get{
                return address;
            }
        }
        
        public int Interface {
            get{
                return networkInterface;
            }
        }

        public static bool operator == (IPPacketInformation packetInformation1, 
                                        IPPacketInformation packetInformation2 ) {
            return packetInformation1.Equals(packetInformation2);
        }

        public static bool operator != (IPPacketInformation packetInformation1, 
                                        IPPacketInformation packetInformation2 ) {
            return !packetInformation1.Equals(packetInformation2);
        }

        public override bool Equals(object comparand) {
            if ((object) comparand == null) {
                return false;
            }

            if (!(comparand is IPPacketInformation))
                return false;

            IPPacketInformation obj = (IPPacketInformation) comparand;

            if (address.Equals(obj.address) && networkInterface == obj.networkInterface)
                return (true);

            return false;
        }

        public override int GetHashCode() {
            return address.GetHashCode() + networkInterface.GetHashCode();
        }

    }; // enum SocketFlags
}





