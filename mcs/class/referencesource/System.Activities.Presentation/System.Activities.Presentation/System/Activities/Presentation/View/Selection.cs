namespace System.Activities.Presentation.View {

    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;

    using System.Activities.Presentation.Internal.Properties;

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// The Selection class defines a selection of objects.  Selections
    /// consist of zero or more objects.  The first object in a selection
    /// is defined as the "primary" selection, which is used when
    /// one object in a group must be used as a key.
    /// </summary>
    [SuppressMessage(FxCop.Category.Naming, "CA1724:TypeNamesShouldNotMatchNamespaces",
        Justification = "Code imported from Cider; keeping changes to a minimum as it impacts xaml files as well")]
    public class Selection : ContextItem
    {

        private ICollection<ModelItem> _selectedObjects;

        /// <summary>
        /// Creates an empty Selection object.
        /// </summary>
        public Selection() {
            _selectedObjects = new ModelItem[0];
        }

        /// <summary>
        /// Creates a collection object comprising the given
        /// selected objects.  The first object in the enumeration
        /// is considered the "primary" selection.
        /// </summary>
        /// <param name="selectedObjects">An enumeration of objects that should be selected.</param>
        /// <exception cref="ArgumentNullException">If selectedObjects is null.</exception>
        public Selection(IEnumerable<ModelItem> selectedObjects) {
            if (selectedObjects == null) {
                throw FxTrace.Exception.ArgumentNull("selectedObjects");
            }

            List<ModelItem> selection = new List<ModelItem>();
            selection.AddRange(selectedObjects);
            _selectedObjects = selection;
        }

        /// <summary>
        /// Creates a collection object comprising the given
        /// selected objects.  The first object in the enumeration
        /// is considered the "primary" selection.
        /// </summary>
        /// <param name="selectedObjects">An enumeration of objects that should be selected.</param>
        /// <param name="match">If provided, only those objects in selectedObjects that match the predicate will be added to the selection.</param>
        /// <exception cref="ArgumentNullException">If selectedObjects or match is null.</exception>
        public Selection(IEnumerable<ModelItem> selectedObjects, Predicate<ModelItem> match) {
            if (selectedObjects == null) throw FxTrace.Exception.ArgumentNull("selectedObjects");
            if (match == null) throw FxTrace.Exception.ArgumentNull("match");

            List<ModelItem> selection = new List<ModelItem>();
            foreach (ModelItem o in selectedObjects) {
                if (match(o)) {
                    selection.Add(o);
                }
            }

            _selectedObjects = selection;
        }

        /// <summary>
        /// Creates a collection object comprising the given
        /// selected objects.  The first object in the enumeration
        /// is considered the "primary" selection.
        /// </summary>
        /// <param name="selectedObjects">An enumeration of objects that should be selected.</param>
        /// <exception cref="ArgumentNullException">If selectedObjects is null.</exception>
        public Selection(IEnumerable selectedObjects) {
            if (selectedObjects == null) throw FxTrace.Exception.ArgumentNull("selectedObjects");

            List<ModelItem> selection = new List<ModelItem>();
            foreach (object o in selectedObjects) {
                ModelItem item = o as ModelItem;
                if (item != null) {
                    selection.Add(item);
                }
            }

            _selectedObjects = selection;
        }

        /// <summary>
        /// Creates a collection object comprising the given
        /// selected objects.  The first object in the enumeration
        /// is considered the "primary" selection.
        /// </summary>
        /// <param name="selectedObjects">An enumeration of objects that should be selected.</param>
        /// <param name="match">If provided, only those objects in selectedObjects that match the predicate will be added to the selection.</param>
        /// <exception cref="ArgumentNullException">If selectedObjects is null.</exception>
        public Selection(IEnumerable selectedObjects, Predicate<ModelItem> match) {
            if (selectedObjects == null) throw FxTrace.Exception.ArgumentNull("selectedObjects");
            if (match == null) throw FxTrace.Exception.ArgumentNull("match");

            List<ModelItem> selection = new List<ModelItem>();
            foreach (object o in selectedObjects) {
                ModelItem item = o as ModelItem;
                if (item != null && match(item)) {
                    selection.Add(item);
                }
            }

            _selectedObjects = selection;
        }

        /// <summary>
        /// Creates a collection object comprising the given
        /// objects.  The first object is considered the "primary"
        /// selection.
        /// </summary>
        /// <param name="selectedObjects">A parameter array of objects that should be selected.</param>
        /// <exception cref="ArgumentNullException">If selectedObjects is null.</exception>
        public Selection(params ModelItem[] selectedObjects)
            : this((IEnumerable<ModelItem>)selectedObjects) {
        }

        /// <summary>
        /// The primary selection.  Some functions require a "key"
        /// element.  For example, an "align lefts" command needs
        /// to know which element's "left" to align to.
        /// </summary>
        public ModelItem PrimarySelection {
            get {
                foreach (ModelItem obj in _selectedObjects) {
                    return obj;
                }

                return null;
            }
        }

        /// <summary>
        /// The enumeration of selected objects.
        /// </summary>
        public IEnumerable<ModelItem> SelectedObjects {
            get {
                return _selectedObjects;
            }
        }

        /// <summary>
        /// The number of objects that are currently selected into
        /// this selection.
        /// </summary>
        public int SelectionCount {
            get { return _selectedObjects.Count; }
        }

        /// <summary>
        /// Override of ContextItem's ItemType
        /// property.  The ItemType of Selection is
        /// always "typeof(Selection)".
        /// </summary>
        public sealed override Type ItemType {
            get {
                return typeof(Selection);
            }
        }
        
        
        /// <summary>
        /// Selection helper method.  This takes the existing selection in the
        /// context and selects an item into it.  If the item is already in the
        /// selection the selection is preserved and the item is promoted
        /// to the primary selection.
        /// </summary>
        /// <param name="context">The editing context to apply this selection change to.</param>
        /// <param name="itemToSelect">The item to select.</param>
        /// <returns>A Selection object that contains the new selection.</returns>
        /// <exception cref="ArgumentNullException">If context or itemToSelect is null.</exception>
        public static Selection Select(EditingContext context, ModelItem itemToSelect) {

            if (context == null) throw FxTrace.Exception.ArgumentNull("context");
            if (itemToSelect == null) throw FxTrace.Exception.ArgumentNull("itemToSelect");

            Selection existing = context.Items.GetValue<Selection>();

            // short cut if we're already in the right state.
            if (existing.PrimarySelection == itemToSelect) {
                return existing;
            }

            Selection selection = null;

            foreach (ModelItem obj in existing.SelectedObjects) {
                if (obj == itemToSelect) {
                    List<ModelItem> list = new List<ModelItem>(existing.SelectedObjects);
                    list.Remove(itemToSelect);
                    list.Insert(0, itemToSelect);
                    selection = new Selection(list);
                }
            }

            if (selection == null) {
                selection = new Selection(itemToSelect);
            }
            
            context.Items.SetValue(selection);
            return selection;
        }

        /// <summary>
        /// Selection helper method.  This sets itemToSelect into the selection.
        /// Any existing items are deselected.
        /// </summary>
        /// <param name="context">The editing context to apply this selection change to.</param>
        /// <param name="itemToSelect">The item to select.</param>
        /// <returns>A Selection object that contains the new selection.</returns>
        /// <exception cref="ArgumentNullException">If context or itemToSelect is null.</exception>
        public static Selection SelectOnly(EditingContext context, ModelItem itemToSelect) {

            if (context == null) throw FxTrace.Exception.ArgumentNull("context");
            if (itemToSelect == null) throw FxTrace.Exception.ArgumentNull("itemToSelect");

            // Check to see if only this object is selected.  If so, bail.
            Selection existing = context.Items.GetValue<Selection>();
            if (existing.PrimarySelection == itemToSelect) {
                IEnumerator<ModelItem> en = existing.SelectedObjects.GetEnumerator();
                en.MoveNext();
                if (!en.MoveNext()) {
                    return existing;
                }
            }

            DesignerPerfEventProvider designerPerfEventProvider = context.Services.GetService<DesignerPerfEventProvider>();
            if (designerPerfEventProvider != null)
            {
                designerPerfEventProvider.SelectionChangedStart();
            }

            Selection selection = new Selection(itemToSelect);
            context.Items.SetValue(selection);
            return selection;
        }

        /// <summary>
        /// Helper method that subscribes to selection change events.
        /// </summary>
        /// <param name="context">The editing context to listen to.</param>
        /// <param name="handler">The handler to be invoked when the selection changes.</param>
        public static void Subscribe(EditingContext context, SubscribeContextCallback<Selection> handler) {
            if (context == null) throw FxTrace.Exception.ArgumentNull("context");
            if (handler == null) throw FxTrace.Exception.ArgumentNull("handler");
            context.Items.Subscribe<Selection>(handler);
        }

        /// <summary>
        /// Selection helper method.  This takes the existing selection in the
        /// context and creates a new selection that contains the toggled
        /// state of the item.  If the item is to be
        /// added to the selection, it is added as the primary selection.
        /// </summary>
        /// <param name="context">The editing context to apply this selection change to.</param>
        /// <param name="itemToToggle">The item to toggle selection for.</param>
        /// <returns>A Selection object that contains the new selection.</returns>
        /// <exception cref="ArgumentNullException">If context or itemToToggle is null.</exception>
        public static Selection Toggle(EditingContext context, ModelItem itemToToggle) {
            
            if (context == null) throw FxTrace.Exception.ArgumentNull("context");
            if (itemToToggle == null) throw FxTrace.Exception.ArgumentNull("itemToToggle");

            Selection existing = context.Items.GetValue<Selection>();

            // Is the item already in the selection?  If so, remove it.
            // If not, add it to the beginning.

            List<ModelItem> list = new List<ModelItem>(existing.SelectedObjects);
            if (list.Contains(itemToToggle)) {
                list.Remove(itemToToggle);
            }
            else {
                list.Insert(0, itemToToggle);
            }

            Selection selection = new Selection(list);
            context.Items.SetValue(selection);
            return selection;
        }

        /// <summary>
        /// Selection helper method.  This takes the existing selection in the
        /// context and creates a new selection that contains the original
        /// selection and the itemToAdd.  If itemToAdd is already in the 
        /// original selection it is promoted to the primary selection.
        /// </summary>
        /// <param name="context">The editing context to apply this selection change to.</param>
        /// <param name="itemToAdd">The item to add to the selection.</param>
        /// <returns>A Selection object that contains the new selection.</returns>
        /// <exception cref="ArgumentNullException">If context or itemToAdd is null.</exception>
        public static Selection Union(EditingContext context, ModelItem itemToAdd) {

            if (context == null) throw FxTrace.Exception.ArgumentNull("context");
            if (itemToAdd == null) throw FxTrace.Exception.ArgumentNull("itemToAdd");

            Selection existing = context.Items.GetValue<Selection>();

            // short cut if we're already in the right state.
            if (existing.PrimarySelection == itemToAdd) {
                return existing;
            }

            // Is the item already in the selection?  If not, add it.
            List<ModelItem> list = new List<ModelItem>(existing.SelectedObjects);
            if (list.Contains(itemToAdd)) {
                list.Remove(itemToAdd);
            }

            list.Insert(0, itemToAdd);
            Selection selection = new Selection(list);
            context.Items.SetValue(selection);
            return selection;
        }

        internal static bool MultipleObjectsSelected(EditingContext context)
        {
            Selection selection = context.Items.GetValue<Selection>();
            if (selection != null && selection.SelectionCount > 1)
            {
                return true;
            }
            return false;
        }

        internal static bool IsSelection(ModelItem item)
        {
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(item)["IsSelection"];
            if (descriptor != null)
            {
                return (bool)descriptor.GetValue(item);
            }
            return false;
        }

        internal static bool IsPrimarySelection(ModelItem item)
        {
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(item)["IsPrimarySelection"];
            if (descriptor != null)
            {
                return (bool)descriptor.GetValue(item);
            }
            return false;
        }

        /// <summary>
        /// Helper method that removes a previously added selection change event.
        /// </summary>
        /// <param name="context">The editing context to listen to.</param>
        /// <param name="handler">The handler to be invoked when the selection changes.</param>
        public static void Unsubscribe(EditingContext context, SubscribeContextCallback<Selection> handler) {
            if (context == null) throw FxTrace.Exception.ArgumentNull("context");
            if (handler == null) throw FxTrace.Exception.ArgumentNull("handler");
            context.Items.Unsubscribe<Selection>(handler);
        }
    }
}
