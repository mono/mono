//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant, victark

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Collections;

#if Microsoft_CONTROL
namespace System.Windows.Forms.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting
#endif
{

    /// <summary>
    /// Base class for all chart element collections
    /// </summary>
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public abstract class ChartElementCollection<T> : Collection<T>, IChartElement, IDisposable
        where T : ChartElement
    {
        #region Member variables

        private IChartElement _parent = null;
        private CommonElements _common = null;
        internal int _suspendUpdates = 0;
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        internal IChartElement Parent
        {
            get { return _parent; }
            set
            {
                _parent = value;
                Invalidate();
            }
        }
        /// <summary>
        /// Gets the CommonElements of the chart.
        /// </summary>
        internal CommonElements Common
        {
            get
            {
                if (_common == null && _parent != null)
                {
                    _common = _parent.Common;
                }
                return _common;
            }
        }

        /// <summary>
        /// Gets the chart.
        /// </summary>
        internal Chart Chart
        {
            get
            {
                if (Common != null)
                    return Common.Chart;
                else
                    return null;
            }
        }

        /// <summary>
        /// Gets the items as List&lt;T&gt;. Use this property to perform advanced List specific operations (Sorting, etc)
        /// </summary>
        internal List<T> ItemList 
        {
            get { return Items as List<T>; }
        }

        internal bool IsSuspended
        {
            get { return _suspendUpdates > 0; }
        }
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartElementCollection&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="parent">The parent chart element.</param>
        internal ChartElementCollection(IChartElement parent)
        {
            _parent = parent;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Forces the invalidation of the parent chart element
        /// </summary>
        public virtual void Invalidate()
        {
            if (_parent != null && !IsSuspended)
                _parent.Invalidate();
        }

        /// <summary>
        /// Suspends invalidation
        /// </summary>
        public virtual void SuspendUpdates() 
        {
            _suspendUpdates++;
        }

        /// <summary>
        /// Resumes invalidation.
        /// </summary>
        public virtual void ResumeUpdates()
        {
            if (_suspendUpdates>0)
                _suspendUpdates--;

            if (_suspendUpdates==0)
                this.Invalidate(); 
        }

        /// <summary>
        /// Removes all elements from the <see cref="T:System.Collections.ObjectModel.Collection`1"/>.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        protected override void ClearItems()
        {
            SuspendUpdates();
            while (this.Count > 0)
            {
                this.RemoveItem(0);
            }
            ResumeUpdates();
        }

        /// <summary>
        /// Deinitializes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        internal virtual void Deinitialize( T item)
        {

        }

        /// <summary>
        /// Initializes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        internal virtual void Initialize(T item)
        {

        }

        /// <summary>
        /// Removes the element at the specified index of the <see cref="T:System.Collections.ObjectModel.Collection`1"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        protected override void RemoveItem(int index)
        {
            this.Deinitialize(this[index]);
            this[index].Parent = null;
            base.RemoveItem(index);
            Invalidate();
        }

        /// <summary>
        /// Inserts an element into the <see cref="T:System.Collections.ObjectModel.Collection`1"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The object to insert. The value can be null for reference types.</param>
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        protected override void InsertItem(int index, T item)
        {
            this.Initialize(item);
            item.Parent = this;
            base.InsertItem(index, item);
            Invalidate();
        }

        /// <summary>
        /// Replaces the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">The new value for the element at the specified index. The value can be null for reference types.</param>
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        protected override void SetItem(int index, T item)
        {
            this.Initialize(item);
            item.Parent = this;
            base.SetItem(index, item);
            Invalidate();
        }

        #endregion

        #region IChartElement Members

        IChartElement IChartElement.Parent
        {
            get { return this.Parent; }
            set { this.Parent = value; }
        }

        void IChartElement.Invalidate()
        {
            this.Invalidate();
        }

        CommonElements IChartElement.Common
        {
            get{ return this.Common; }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources
                foreach (T element in this)
                {
                    element.Dispose();
                }
            }
        }

        /// <summary>
        /// Performs freeing, releasing, or resetting managed resources.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }

    /// <summary>
    /// Base class for all collections of named chart elements. Performs the name management and enforces the uniquness of the names
    /// </summary>
    /// <typeparam name="T"></typeparam>
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public abstract class ChartNamedElementCollection<T> : ChartElementCollection<T>, INameController
        where T : ChartNamedElement
    {

        #region Fields
        private List<T> _cachedState = null;
        private int _disableDeleteCount = 0;
        #endregion

        #region Properties

        /// <summary>
        /// Gets the name prefix that is used to create unique chart element names.
        /// </summary>
        /// <value>The default name prefix of the chart elements stored in the collection.</value>
        protected virtual string NamePrefix
        {
            get { return typeof(T).Name; }
        }

        /// <summary>
        /// Gets or sets the chart element with the specified name.
        /// </summary>
        /// <value></value>
        public T this[string name]
        {
            get
            {
                int index = this.IndexOf(name);
                if (index != -1)
                {
                    return this[index];
                }
                throw new ArgumentException(SR.ExceptionNameNotFound(name, this.GetType().Name));
            }
            set
            {
                int nameIndex = this.IndexOf(name);
                int itemIndex = this.IndexOf(value);
                bool nameFound = nameIndex > -1;
                bool itemFound = itemIndex > -1;

                if (!nameFound && !itemFound)
                    this.Add(value);

                else if (nameFound && !itemFound)
                    this[nameIndex] = value;

                else if (!nameFound && itemFound)
                    throw new ArgumentException(SR.ExceptionNameAlreadyExistsInCollection(name, this.GetType().Name));
                    
                else if (nameFound && itemFound && nameIndex != itemIndex)
                    throw new ArgumentException(SR.ExceptionNameAlreadyExistsInCollection(name, this.GetType().Name));
                    
            }
        }
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartNamedElementCollection&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="parent">The parent chart element.</param>
        internal ChartNamedElementCollection(IChartElement parent)
            : base(parent)
        {
        }

        #endregion

        #region Events

        internal event EventHandler<NameReferenceChangedEventArgs> NameReferenceChanged;
        internal event EventHandler<NameReferenceChangedEventArgs> NameReferenceChanging;

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the chart element with the specified name already exists in the collection.
        /// </summary>
        /// <param name="name">The new chart element name.</param>
        /// <returns>
        /// 	<c>true</c> if new chart element name is unique; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsUniqueName(string name)
        {
            return FindByName(name)==null;
        }

        /// <summary>
        /// Finds the unique name for a new element being added to the collection
        /// </summary>
        /// <returns>Next unique chart element name</returns>
        public virtual string NextUniqueName()
        {
            // Find unique name
            string result = string.Empty;
            string prefix = this.NamePrefix;
            for (int i = 1; i < System.Int32.MaxValue; i++)
            {
                result = prefix + i.ToString(CultureInfo.InvariantCulture);
                // Check whether the name is unique
                if (IsUniqueName(result))
                {
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Indexes the of chart element with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public int IndexOf(string name)
        {
            int i = 0;
            foreach (T namedObj in this)
            {
                if (namedObj.Name == name)
                    return i;
                i++;
            }
            return -1;
        }

        /// <summary>
        /// Verifies the name reference to a chart named element stored in this collection and throws the argument exception if its not valid.
        /// </summary>
        /// <param name="name">Chart element name.</param>
        internal void VerifyNameReference(string name)
        {
            if (Chart!=null && !Chart.serializing && !IsNameReferenceValid(name))
                throw new ArgumentException(SR.ExceptionNameNotFound(name, this.GetType().Name));
        }

        /// <summary>
        /// Verifies the name reference to a chart named element stored in this collection.
        /// </summary>
        /// <param name="name">Chart element name.</param>
        internal bool IsNameReferenceValid(string name)
        {
            return  String.IsNullOrEmpty(name) || 
                    name == Constants.NotSetValue ||
                    IndexOf(name) >= 0;
        }

        /// <summary>
        /// Finds the chart element by the name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public virtual T FindByName(string name)
        {
            foreach (T namedObj in this)
            {
                if (namedObj.Name == name)
                    return namedObj;
            }
            return null;
        }

        /// <summary>
        /// Inserts the specified item in the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index where the item is to be inserted.</param>
        /// <param name="item">The object to insert.</param>
        protected override void InsertItem(int index, T item)
        {
            if (String.IsNullOrEmpty(item.Name))
                item.Name = this.NextUniqueName();
            else if (!IsUniqueName(item.Name))
                throw new ArgumentException(SR.ExceptionNameAlreadyExistsInCollection(item.Name, this.GetType().Name));

            //If the item references other named references we might need to fix the references
            FixNameReferences(item);

            base.InsertItem(index, item);

            if (this.Count == 1 && item != null)
            { 
                // First element is added to the list -> fire the NameReferenceChanged event to update all the dependent elements
                ((INameController)this).OnNameReferenceChanged(new NameReferenceChangedEventArgs(null, item));
            }
        }

        /// <summary>
        /// Replaces the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">The new value for the element at the specified index.</param>
        protected override void SetItem(int index, T item)
        {
            if (String.IsNullOrEmpty(item.Name))
                item.Name = this.NextUniqueName();
            else if (!IsUniqueName(item.Name) && IndexOf(item.Name) != index)
                throw new ArgumentException(SR.ExceptionNameAlreadyExistsInCollection(item.Name, this.GetType().Name));

            //If the item references other named references we might need to fix the references
            FixNameReferences(item);

            // Remember the removedElement
            ChartNamedElement removedElement = index<Count ? this[index] : null;
            
            ((INameController)this).OnNameReferenceChanging(new NameReferenceChangedEventArgs(removedElement, item));
            base.SetItem(index, item);
            // Fire the NameReferenceChanged event to update all the dependent elements
            ((INameController)this).OnNameReferenceChanged(new NameReferenceChangedEventArgs(removedElement, item));
        }

        /// <summary>
        /// Removes the element at the specified index of the collection.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            // Remember the removedElement
            ChartNamedElement removedElement = index < Count ? this[index] : null;
            if (_disableDeleteCount == 0)
            {
                ((INameController)this).OnNameReferenceChanged(new NameReferenceChangedEventArgs(removedElement, null));
            }            
            base.RemoveItem(index);
            if (_disableDeleteCount == 0)
            {
                // All elements referencing the removed element will be redirected to the first element in collection
                // Fire the NameReferenceChanged event to update all the dependent elements
                ChartNamedElement defaultElement = this.Count > 0 ? this[0] : null;
                ((INameController)this).OnNameReferenceChanged(new NameReferenceChangedEventArgs(removedElement, defaultElement));
            }
        }

        /// <summary>
        /// Fixes the name references of the item.
        /// </summary>
        internal virtual void FixNameReferences(T item)
        { 
            //Nothing to fix at the base class...
        }

        #endregion

        #region INameController Members

        /// <summary>
        /// Determines whether is the name us unique.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// 	<c>true</c> if is the name us unique; otherwise, <c>false</c>.
        /// </returns>
        bool INameController.IsUniqueName(string name)
        {
            return this.IsUniqueName(name);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is in edit mode by collecrtion editor.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance the colection is editing; otherwise, <c>false</c>.
        /// </value>
        bool INameController.IsColectionEditing
        {
            get
            {
                return _disableDeleteCount == 0;
            }
            set
            {
                _disableDeleteCount += value ? 1 : -1;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:NameReferenceChanging"/> event.
        /// </summary>
        /// <param name="e">The <see cref="NameReferenceChangedEventArgs"/> instance containing the event data.</param>
        void INameController.OnNameReferenceChanging(NameReferenceChangedEventArgs e)
        {
            if (!IsSuspended)
            {
                if (this.NameReferenceChanging != null)
                    this.NameReferenceChanging(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:NameReferenceChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="NameReferenceChangedEventArgs"/> instance containing the event data.</param>
        void INameController.OnNameReferenceChanged(NameReferenceChangedEventArgs e)
        {
            if (!IsSuspended)
            {
                if (this.NameReferenceChanged != null)
                    this.NameReferenceChanged(this, e);
            }
        }

        /// <summary>
        /// Does the snapshot of collection items.
        /// </summary>
        /// <param name="save">if set to <c>true</c> collection items will be saved.</param>
        /// <param name="changingCallback">The changing callback.</param>
        /// <param name="changedCallback">The changed callback.</param>
        void INameController.DoSnapshot(bool save, 
            EventHandler<NameReferenceChangedEventArgs> changingCallback, 
            EventHandler<NameReferenceChangedEventArgs> changedCallback)
        {
            if (save)
            {
                _cachedState = new List<T>(this);
                if (changingCallback != null) this.NameReferenceChanging += changingCallback;
                if (changedCallback  != null) this.NameReferenceChanged += changedCallback;
            }
            else
            {
                if (changingCallback != null) this.NameReferenceChanging -= changingCallback;
                if (changedCallback != null) this.NameReferenceChanged -= changedCallback;
                _cachedState.Clear();
                _cachedState = null;
            }
        }

        /// <summary>
        /// Gets the snapshot of saved collection items.
        /// </summary>
        /// <value>The snapshot.</value>
        IList INameController.Snapshot
        {
            get { return _cachedState; }
        }


        #endregion

        
    }

}
