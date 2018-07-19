using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Net.NetworkInformation{
    public class UnicastIPAddressInformationCollection :ICollection<UnicastIPAddressInformation>
    {
        Collection<UnicastIPAddressInformation> addresses = new Collection<UnicastIPAddressInformation>() ;


        protected internal UnicastIPAddressInformationCollection(){
        }


        /// <include file='doc\HttpListenerPrefixCollection.uex' path='docs/doc[@for="HttpListenerPrefixCollection.CopyTo"]/*' />
        public virtual void CopyTo(UnicastIPAddressInformation[] array, int offset) {
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


        public virtual void Add(UnicastIPAddressInformation address) {
                throw new NotSupportedException(SR.GetString(SR.net_collection_readonly));
        }


        internal void InternalAdd(UnicastIPAddressInformation address) {
            addresses.Add(address);
        }


        /// <include file='doc\HttpListenerPrefixCollection.uex' path='docs/doc[@for="HttpListenerPrefixCollection.Contains"]/*' />
        public virtual bool Contains(UnicastIPAddressInformation address) {
            return addresses.Contains(address);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
	        return this.GetEnumerator();
        }


        public virtual IEnumerator<UnicastIPAddressInformation> GetEnumerator() {
            return (IEnumerator<UnicastIPAddressInformation>) addresses.GetEnumerator();
        }


        
        // Consider removing.
        public virtual UnicastIPAddressInformation this[int index]
        {
            get{
                return (UnicastIPAddressInformation)addresses[index];
            }
        }
        


        /// <include file='doc\HttpListenerPrefixCollection.uex' path='docs/doc[@for="HttpListenerPrefixCollection.Remove"]/*' />
        public virtual bool Remove(UnicastIPAddressInformation address) {
            throw new NotSupportedException(SR.GetString(SR.net_collection_readonly));
        }

        /// <include file='doc\HttpListenerPrefixCollection.uex' path='docs/doc[@for="HttpListenerPrefixCollection.Clear"]/*' />
        public virtual void Clear() {
            throw new NotSupportedException(SR.GetString(SR.net_collection_readonly));
        }
    }
}
