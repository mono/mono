using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Data.Linq.Mapping;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Data.Linq.Provider {
    internal static class BindingList {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static IBindingList Create<T>(DataContext context, IEnumerable<T> sequence) {
            List<T> list = sequence.ToList();
            MetaTable metaTable = context.Services.Model.GetTable(typeof(T));
            if (metaTable != null) {
                ITable table = context.GetTable(metaTable.RowType.Type);
                Type bindingType = typeof(DataBindingList<>).MakeGenericType(metaTable.RowType.Type);
                return (IBindingList)Activator.CreateInstance(bindingType,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
                    new object[] { list, table }, null
                    );
            } else {
                return new SortableBindingList<T>(list);
            }
        }
    }

    internal class DataBindingList<TEntity> : SortableBindingList<TEntity> 
        where TEntity : class {
        private Table<TEntity> data;
        private TEntity addNewInstance;
        private TEntity cancelNewInstance;
        private bool addingNewInstance;

        internal DataBindingList(IList<TEntity> sequence, Table<TEntity> data)
            : base(sequence != null ? sequence : new List<TEntity>()) {
            if (sequence == null) {
                throw Error.ArgumentNull("sequence");
            }
            if (data == null) {
                throw Error.ArgumentNull("data");
            }

            this.data = data;
        }

        protected override object AddNewCore() {
            addingNewInstance = true;
            addNewInstance = (TEntity)base.AddNewCore();
            return addNewInstance;
        }

        protected override void InsertItem(int index, TEntity item) {
            base.InsertItem(index, item);
            if (!addingNewInstance && index >= 0 && index <= Count) {
                this.data.InsertOnSubmit(item);
            }
        }

        protected override void RemoveItem(int index) {
            if (index >= 0 && index < Count && this[index] == cancelNewInstance) {
                cancelNewInstance = null;
            }
            else {
                this.data.DeleteOnSubmit(this[index]);
            }

            base.RemoveItem(index);
        }

        protected override void SetItem(int index, TEntity item) {
            TEntity removedItem = this[index];
            base.SetItem(index, item);
            if (index >= 0 && index < Count) {
                //Check to see if the user is trying to set an item that is currently being added via AddNew
                //If so then the list should not continue the AddNew; but instead add the item
                //that is being passed in.
                if (removedItem == addNewInstance) {
                    addNewInstance = null;
                    addingNewInstance = false;
                }
                else {
                    this.data.DeleteOnSubmit(removedItem);
                }
                this.data.InsertOnSubmit(item);
            }
        }

        protected override void ClearItems() {
            this.data.DeleteAllOnSubmit(this.data.ToList());
            base.ClearItems();
        }

        public override void EndNew(int itemIndex) {
            if (itemIndex >= 0 && itemIndex < Count && this[itemIndex] == addNewInstance) {
                this.data.InsertOnSubmit(addNewInstance);
                addNewInstance = null;
                addingNewInstance = false;
            }

            base.EndNew(itemIndex);
        }

        public override void CancelNew(int itemIndex) {
            if (itemIndex >= 0 && itemIndex < Count && this[itemIndex] == addNewInstance) {
                cancelNewInstance = addNewInstance;
                addNewInstance = null;
                addingNewInstance = false;
            }
            
            base.CancelNew(itemIndex);
        }
    }
}
