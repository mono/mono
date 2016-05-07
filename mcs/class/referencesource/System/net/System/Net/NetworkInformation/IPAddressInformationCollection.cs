using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Net.NetworkInformation{
    public class IPAddressInformationCollection :ICollection<IPAddressInformation>
    {
        Collection<IPAddressInformation> addresses = new Collection<IPAddressInformation>();

        internal IPAddressInformationCollection(){
        }

        /// <include file='doc\HttpListenerPrefixCollection.uex' path='docs/doc[@for="HttpListenerPrefixCollection.CopyTo"]/*' />
        public virtual void CopyTo(IPAddressInformation[] array, int offset) {
            addresses.CopyTo(array,offset);
        }

        
        /// <include file='doc\HttpListenerPrefixCollection.uex' path='docs/doc[@for="HttpListenerPrefixCollection.Count"]/*' />
        public virtual int Count {
            get {
                return addresses.Count;
            }
        }
        
        public virtual bool IsReadOnly {
            get {
                return true;
            }
        }


        /// <include file='doc\HttpListenerPrefixCollection.uex' path='docs/doc[@for="HttpListenerPrefixCollection.Add"]/*' />
        public virtual void Add(IPAddressInformation address) {
                throw new NotSupportedException(SR.GetString(SR.net_collection_readonly));
        }



        internal void InternalAdd(IPAddressInformation address) {
            addresses.Add(address);
        }


        /// <include file='doc\HttpListenerPrefixCollection.uex' path='docs/doc[@for="HttpListenerPrefixCollection.Contains"]/*' />
        public virtual bool Contains(IPAddressInformation address) {
            return addresses.Contains(address);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return this.GetEnumerator();	
        }

        public virtual IEnumerator<IPAddressInformation> GetEnumerator() {
            return (IEnumerator<IPAddressInformation>) addresses.GetEnumerator();
        }


        public virtual IPAddressInformation this[int index]
        {
            get{
                return (IPAddressInformation)addresses[index];
            }
        }

        /// <include file='doc\HttpListenerPrefixCollection.uex' path='docs/doc[@for="HttpListenerPrefixCollection.Remove"]/*' />
        public virtual bool Remove(IPAddressInformation address) {
            throw new NotSupportedException(SR.GetString(SR.net_collection_readonly));
        }

        /// <include file='doc\HttpListenerPrefixCollection.uex' path='docs/doc[@for="HttpListenerPrefixCollection.Clear"]/*' />
        public virtual void Clear() {
            throw new NotSupportedException(SR.GetString(SR.net_collection_readonly));
        }
    }
}
