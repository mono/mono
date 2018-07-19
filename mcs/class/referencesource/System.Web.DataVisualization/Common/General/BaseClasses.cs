//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant, victark

using System;
using System.Text;
using System.Globalization;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;

#if Microsoft_CONTROL
namespace System.Windows.Forms.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting
#endif
{

    /// <summary>
    /// ChartElement is the most basic element of the chart element hierarchy. 
    /// </summary>
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public abstract class ChartElement : IChartElement, IDisposable
    {
        #region Member variables

        private IChartElement  _parent = null;
        private CommonElements _common = null;
        private object _tag = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets an object associated with this chart element.
        /// </summary>
        /// <value>
        /// An <see cref="Object"/> associated with this chart element.
        /// </value>
        /// <remarks>
        /// This property may be used to store additional data with this chart element.
        /// </remarks>
        [
        Browsable(false),
        DefaultValue(null),
        DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
        Utilities.SerializationVisibilityAttribute(Utilities.SerializationVisibility.Hidden)
        ]
        public object Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        /// <summary>
        /// Gets or sets the parent chart element or collection.
        /// </summary>
        /// <value>The parent chart element or collection.</value>
        internal virtual IChartElement Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        /// <summary>
        /// Gets a shortcut to Common intance providing access to the various chart related services.
        /// </summary>
        /// <value>The Common instance.</value>
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
            set
            {
                _common = value;
            }
        }

        /// <summary>
        /// Gets the chart.
        /// </summary>
        /// <value>The chart.</value>
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

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartElement"/> class.
        /// </summary>
        protected ChartElement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartElement"/> class.
        /// </summary>
        /// <param name="parent">The parent chart element or collection.</param>
        internal ChartElement(IChartElement parent)
        {
            _parent = parent;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invalidates this chart element.
        /// </summary>
        internal virtual void Invalidate() 
        {
            if (_parent != null)
                _parent.Invalidate();
        }

        #endregion

        #region IChartElement Members


        IChartElement IChartElement.Parent
        {
            get { return _parent; }
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

        #region Methods

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <remarks>For internal use.</remarks>
        internal virtual string ToStringInternal()
        {
            return GetType().Name;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override string ToString()
        {
            return this.ToStringInternal();
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">The <paramref name="obj"/> parameter is null.</exception>
        /// <remarks>For internal use.</remarks>
        internal virtual bool EqualsInternal(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">The <paramref name="obj"/> parameter is null.</exception>
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override bool Equals(object obj)
        {
            return this.EqualsInternal(obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion

    }
    
    /// <summary>
    /// ChartNamedElement is a base class for most chart elements. Series, ChartAreas, Legends and other chart elements have a Name and reuse the unique name generation and validation logic provided by the ChartNamedElementCollection.
    /// </summary>
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public abstract class ChartNamedElement : ChartElement
    {
        #region Member variables

        private string _name = String.Empty;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of the chart element.
        /// </summary>
        /// <value>The name.</value>
        [DefaultValue("")]
        public virtual string Name 
        {
            get { return _name; }
            set 
            {
                if (_name != value)
                {
                    if (Parent is INameController)
                    {
                        INameController nameController = Parent as INameController;
                        
                        if (!nameController.IsUniqueName(value))
                            throw new ArgumentException(SR.ExceptionNameAlreadyExistsInCollection(value, nameController.GetType().Name));
                        
                        // Fire the name change events in case when the old name is not empty
                        NameReferenceChangedEventArgs args = new NameReferenceChangedEventArgs(this, _name, value);
                        nameController.OnNameReferenceChanging(args);
                        _name = value;
                        nameController.OnNameReferenceChanged(args);
                    }
                    else
                    {
                        _name = value;
                    }
                    Invalidate();
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartNamedElement"/> class.
        /// </summary>
        protected ChartNamedElement()
            : base()
        { 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartNamedElement"/> class.
        /// </summary>
        /// <param name="name">The name of the new chart element.</param>
        protected ChartNamedElement(string name)
            : base()
        {
            _name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartNamedElement"/> class.
        /// </summary>
        /// <param name="parent">The parent chart element.</param>
        /// <param name="name">The name of the new chart element.</param>
        internal ChartNamedElement(IChartElement parent, string name) : base(parent)
        {
            _name = name;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        internal override string ToStringInternal()
        {
            string typeName = GetType().Name;
            return (string.IsNullOrEmpty(_name)) ? typeName : typeName + '-' + _name;
        }
        
        #endregion

    }


    /// <summary>
    /// NameReferenceChanged events help chart maintain referencial integrity.
    /// </summary>
    internal class NameReferenceChangedEventArgs : EventArgs
    {
        #region MemberValiables

        ChartNamedElement _oldElement;
        string _oldName;
        string _newName;

        #endregion

        #region Properties
        public ChartNamedElement OldElement
        {
            get { return _oldElement; }
        }
        public string OldName
        {
            get { return _oldName; }
        }
        public string NewName
        {
            get { return _newName; }
        }
        #endregion

        #region Constructor
        public NameReferenceChangedEventArgs(ChartNamedElement oldElement, ChartNamedElement newElement)
        {
            _oldElement = oldElement;
            _oldName = oldElement!=null ? oldElement.Name : string.Empty;
            _newName = newElement!=null ? newElement.Name : string.Empty;
        }
        public NameReferenceChangedEventArgs(ChartNamedElement oldElement, string oldName, string newName)
        {
            _oldElement = oldElement;
            _oldName = oldName;
            _newName = newName;
        }
        #endregion
    }

}
