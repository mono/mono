using System;
using System.Collections.ObjectModel;

namespace System.Net.Mail
{
    /// <summary>
    /// Summary description for AttachmentCollection.
    /// </summary>
    public sealed class AttachmentCollection : Collection<Attachment>, IDisposable
    {
        bool disposed = false;
        internal AttachmentCollection() { }
        
        public void Dispose(){
            if(disposed){
                return;
            }
            foreach (Attachment attachment in this) {
                attachment.Dispose();
            }
            Clear();
            disposed = true;
        }
        
        protected override void RemoveItem(int index){
            if (disposed) {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            base.RemoveItem(index);
        }
        
        protected override void ClearItems(){
            if (disposed) {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            base.ClearItems();
        }

        protected override void SetItem(int index, Attachment item){
            if (disposed) {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
              
            if(item==null) {
                 throw new ArgumentNullException("item");
             }
    
             base.SetItem(index,item);
        }
        
        protected override void InsertItem(int index, Attachment item){
            if (disposed) {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
              
            if(item==null){
                 throw new ArgumentNullException("item");
            }
    
            base.InsertItem(index,item);
        }
    }
}
