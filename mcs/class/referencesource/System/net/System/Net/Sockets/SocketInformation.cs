using System.Runtime.Serialization;

namespace System.Net.Sockets {

    [Serializable]
    public struct SocketInformation{
        byte[] protocolInformation;
        SocketInformationOptions options;

        [OptionalField]
        EndPoint remoteEndPoint;

        public byte[] ProtocolInformation{
            get{
               return protocolInformation;
            }
            set{
                protocolInformation = value;
            }
        }

        
        public SocketInformationOptions Options{
            get{
               return options;
            }
            set{
                options = value;
            }
        }
        
        internal bool IsNonBlocking{
            get{
               return ((options&SocketInformationOptions.NonBlocking)!=0);
            }
            set{
                if(value){
                    options |= SocketInformationOptions.NonBlocking;
                }
                else{
                    options &= ~SocketInformationOptions.NonBlocking;
                }
            }
        }

        internal bool IsConnected{
            get{
                return ((options&SocketInformationOptions.Connected)!=0);
            }
            set{
                if(value){
                    options |= SocketInformationOptions.Connected;
                }
                else{
                    options &= ~SocketInformationOptions.Connected;
                }
            }
        }

        internal bool IsListening{
            get{
                return ((options&SocketInformationOptions.Listening)!=0);
            }
            set{
                if(value){
                    options |= SocketInformationOptions.Listening;
                }
                else{
                    options &= ~SocketInformationOptions.Listening;
                }
            }
        }

        internal bool UseOnlyOverlappedIO{
            get{
                return ((options&SocketInformationOptions.UseOnlyOverlappedIO)!=0);
            }
            set{
                if(value){
                    options |= SocketInformationOptions.UseOnlyOverlappedIO;
                }
                else{
                    options &= ~SocketInformationOptions.UseOnlyOverlappedIO;
                }
            }
        }

        internal EndPoint RemoteEndPoint{
            get{
                return remoteEndPoint;
            }
            set{
                remoteEndPoint = value;
            }
        }
    }
}
