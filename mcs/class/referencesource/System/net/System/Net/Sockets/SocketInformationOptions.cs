
namespace System.Net.Sockets {


    [Flags]
    public enum SocketInformationOptions{
        NonBlocking          = 0x1,
        //Even though getpeername can give a hint that we're connected, this needs to be passed because 
        //disconnect doesn't update getpeername to return a failure.
        Connected         = 0x2,  
        Listening         = 0x4,
        UseOnlyOverlappedIO = 0x8,
    }
}
