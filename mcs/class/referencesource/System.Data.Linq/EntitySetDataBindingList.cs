using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Linq {
    internal class EntitySetBindingList<TEntity> : SortableBindingList<TEntity>
        where TEntity : class {
        private EntitySet<TEntity> data;
        private TEntity addNewInstance;
        private TEntity cancelNewInstance;
        private bool addingNewInstance;

        internal EntitySetBindingList(IList<TEntity> sequence, EntitySet<TEntity> data)
            : base(sequence) {
            if (sequence == null) {
                throw Error.ArgumentNull("sequence");
            }
            if (data == null) {
                throw Error.ArgumentNull("data");
            }

            this.data = data;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
        private void ThrowEntitySetErrorsIfTypeInappropriate() {
            Type type = typeof(TEntity);

            if (type.IsAbstract) {
                throw Error.EntitySetDataBindingWithAbstractBaseClass(type.Name);
            }
            if (type.GetConstructor(System.Type.EmptyTypes) == null) {
                throw Error.EntitySetDataBindingWithNonPublicDefaultConstructor(type.Name);
            }
        }

        protected override object AddNewCore() {
            ThrowEntitySetErrorsIfTypeInappropriate();
            addingNewInstance = true;
            addNewInstance = (TEntity)base.AddNewCore();
            return addNewInstance;
        }
  
        protected override void InsertItem(int index, TEntity item) {
            base.InsertItem(index, item);
            if (!addingNewInstance && index >= 0 && index <= Count) {
                this.data.Insert(index, item);
            }
        }

        protected override void RemoveItem(int index) {
            if (index >= 0 && index < Count && this[index] == cancelNewInstance) {
                cancelNewInstance = null;
            }
            else {
                this.data.Remove(this[index]);
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
                    this.data.Remove(removedItem);
                }
                this.data.Insert(index,item);
            }

        }

        protected override void ClearItems() {
            this.data.Clear();
            base.ClearItems();
        }

        public override void EndNew(int itemIndex) {
            if (itemIndex >= 0 && itemIndex < Count && this[itemIndex] == addNewInstance) {
                this.data.Add(addNewInstance);
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
