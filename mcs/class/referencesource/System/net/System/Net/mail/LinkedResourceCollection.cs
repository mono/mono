using System;
using System.Collections.ObjectModel;

namespace System.Net.Mail
{
    public sealed class LinkedResourceCollection : Collection<LinkedResource>, IDisposable
    {
        bool disposed = false;
        internal LinkedResourceCollection()
        { }

        public void Dispose()
        {
            if(disposed){
                return;
            }

            foreach (LinkedResource resource in this)
            {
                resource.Dispose();
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

        protected override void SetItem(int index, LinkedResource item){
            if (disposed) {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
              
            if(item==null) {
                throw new ArgumentNullException("item");
            }
    
            base.SetItem(index,item);
        }
        
        protected override void InsertItem(int index, LinkedResource item){
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
