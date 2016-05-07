using System;
using System.Collections.ObjectModel;

namespace System.Net.Mail
{
    public sealed class AlternateViewCollection : Collection<AlternateView>, IDisposable
    {
        bool disposed = false;

        internal AlternateViewCollection()
        {  }

        public void Dispose()
        {
            if (disposed) {
                return;
            }

            foreach (AlternateView view in this)
            {
                view.Dispose();
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


        protected override void SetItem(int index, AlternateView item){
            if (disposed) {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
              
            
            if(item==null) {
                throw new ArgumentNullException("item");
            }
    
            base.SetItem(index,item);
        }
        
        protected override void InsertItem(int index, AlternateView item){
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
