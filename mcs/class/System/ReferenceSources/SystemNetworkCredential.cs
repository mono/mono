namespace System.Net {
    //
    // Object representing default credentials
    //
    internal class SystemNetworkCredential : NetworkCredential {
        internal static readonly SystemNetworkCredential defaultCredential = new SystemNetworkCredential();

        // We want reference equality to work.  Making this private is a good way to guarantee that.
        private SystemNetworkCredential() :
            base(string.Empty, string.Empty, string.Empty) {
        }
    }
}