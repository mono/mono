//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		SubAxis.cs
//
//  Namespace:	DataVisualization.Charting
//
//	Classes:	SubAxis, SubAxisCollection
//
//  Purpose:	Each chart area contains four main axes PrimaryX, 
//              PrimaryY, SecondaryX and SecondaryY which are usually 
//              positioned on each side of the plotting area. Most of 
//              the charts use only two axes; X and Y, but for some 
//              charts even 4 axes is not sufficient. Sub-axes were 
//              introduced to provide unlimited number of axes in 
//              the chart.
//  
//              Each main axis has a collection of SubAxis which is 
//              empty by default. By adding SubAxis into this collection 
//              user can add unlimited number of sub-axis which will 
//              be positioned next to the main axis.
//
//              Each of the SubAxis have a unique name. To associate 
//              data series with a sub axis YSubAxisName and XSubAxisName 
//              properties of the Series should be used.
//              
//	Reviewed:	AG - March 13, 2007
//
//===================================================================

#if SUBAXES

#region Used namespace
using System;
using System.Globalization;
using System.Reflection;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
#if WINFORMS_CONTROL
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.DataVisualization.Charting.Data;
using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
using System.Windows.Forms.DataVisualization.Charting.Utilities;
using System.Windows.Forms.DataVisualization.Charting.Borders3D;
using System.Windows.Forms.DataVisualization.Charting;

#else
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.DataVisualization.Charting;
	using System.Web.UI.DataVisualization.Charting.Data;
	using System.Web.UI.DataVisualization.Charting.Utilities;
	using System.Web.UI.DataVisualization.Charting.ChartTypes;
#endif


#endregion

#if WINFORMS_CONTROL
namespace System.Windows.Forms.DataVisualization.Charting

#else
namespace System.Web.UI.DataVisualization.Charting

#endif
{
	/// <summary>
	/// SubAxis class is derived from the main Axis class and provides
    /// additional axis associated with one of the main chart axis.
	/// </summary>
	[
	SRDescription("DescriptionAttributeSubAxis_SubAxis"),
	DefaultProperty("Enabled"),
#if WINFORMS_CONTROL
	TypeConverter(typeof(SubAxis.SubAxisConverter)),
#endif

	]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
	public class SubAxis : Axis
	{
#region Fields

		/// <summary>
		/// Sub-Axis parent axis object.
		/// </summary>
		internal Axis parentAxis = null;

		/// <summary>
		/// Sub axis offset from the parent axis
		/// </summary>
		internal double offsetFromParent = 0.0;

		/// <summary>
		/// Margin between prev. axis
		/// </summary>
		internal double locationOffset = 0.0;

#endregion // Fields

#region Constructor

		/// <summary>
		/// Default constructor
		/// </summary>
		public SubAxis() : base()
		{
			base.Name = string.Empty;
		}

		/// <summary>
		/// Object constructor.
		/// </summary>
		/// <param name="name">Unique name of the object.</param>
		public SubAxis(string name) : base()
		{
			base.Name = name;
		}

#endregion

#region Properties

		/// <summary>
		/// Axis automatic scale breaks style.
		/// </summary>
		[
		Browsable(false),
		EditorBrowsable(EditorBrowsableState.Never),
		SRCategory("CategoryAttributeScale"),
		SRDescription("DescriptionAttributeScaleBreakStyle"),
		TypeConverter(typeof(NoNameExpandableObjectConverter)),
		NotifyParentPropertyAttribute(true),
#if WINFORMS_CONTROL
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content), 
#else
		PersistenceMode(PersistenceMode.InnerProperty),
#endif
		]
		override public AxisScaleBreakStyle ScaleBreakStyle
		{
			get
			{
				return base.ScaleBreakStyle;
			}
			set
			{
				base.ScaleBreakStyle = value;
			}
		}

		/// <summary>
		/// Sub axis parent axis.
		/// </summary>
		[
		SRCategory("CategoryAttributeAxis"),
		Bindable(true),
		Browsable(false),
		DefaultValue(null),
		NotifyParentPropertyAttribute(true),
		SRDescription("DescriptionAttributeSubAxis_ParentAxis"),
#if !WINFORMS_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
#endif
		DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden)
		]
		public Axis ParentAxis
		{
			get
			{
				return this.parentAxis;
			}
		}


		/// <summary>
		/// Sub axis location offset relative to the previous axis.
		/// </summary>
		[
		SRCategory("CategoryAttributeLocation"),
		Bindable(true),
		DefaultValue(0.0),
		NotifyParentPropertyAttribute(true),
		SRDescription("DescriptionAttributeSubAxis_LocationOffset"),
#if !WINFORMS_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
#endif
		]
		public double LocationOffset
		{
			get
			{
				return this.locationOffset;
			}
			set
			{
				this.locationOffset = value;
				this.Invalidate();
			}
		}

		/// <summary>
		/// Axis position
		/// </summary>
		[
		Bindable(true),
		Browsable(false),
		DefaultValue(AxisPosition.Left),
		NotifyParentPropertyAttribute(true),
		SRDescription("DescriptionAttributeReverse"),
#if !WINFORMS_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
#endif
		DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden)
		]
		override internal AxisPosition AxisPosition
		{
			get
			{
				if(this.parentAxis != null)
				{
					return this.parentAxis.AxisPosition;
				}
				return AxisPosition.Left;
			}
			set
			{
			}
		}

		/// <summary>
		/// SubAxis name.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		Browsable(true),
		DefaultValue(""),
		SRDescription("DescriptionAttributeSubAxis_Name"),
#if !WINFORMS_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
#endif
		DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Visible),
		SerializationVisibilityAttribute(SerializationVisibility.Attribute)
		]
		override public string Name
		{
			get
			{
				return base.Name;
			}
			set
			{
				base.Name = value;
			}
		}

		/// <summary>
		/// Tick marks and labels move with axis when 
		/// the crossing value is changed.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		EditorBrowsable(EditorBrowsableState.Never),
		Bindable(true),
		DefaultValue(true),
		SRDescription("DescriptionAttributeMarksNextToAxis"),
		NotifyParentPropertyAttribute(true),
#if !WINFORMS_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
#endif
		DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden)
		]
		override public bool IsMarksNextToAxis
		{
			get
			{
				return base.IsMarksNextToAxis;
			}
			set
			{
				base.IsMarksNextToAxis = value;
			}
		}

		/// <summary>
		/// Point where axis is crossed by another axis.
		/// </summary>
		[
		SRCategory("CategoryAttributeScale"),
		Browsable(false),
		EditorBrowsable(EditorBrowsableState.Never),
		Bindable(true),
		DefaultValue(Double.NaN),
		NotifyParentPropertyAttribute(true),
		SRDescription("DescriptionAttributeCrossing"),
#if !WINFORMS_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
#endif
		TypeConverter(typeof(AxisCrossingValueConverter)),
		DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden)
		]
		override public double Crossing
		{
			get
			{
				return base.Crossing;
			}
			set
			{
				base.Crossing = value;
			}
		}

		/// <summary>
		/// Sub-axes collection.
		/// </summary>
		[
		SRCategory("CategoryAttributeSubAxes"),
		Browsable(false),
		EditorBrowsable(EditorBrowsableState.Never),
		Bindable(true),
		SRDescription("DescriptionAttributeSubAxes"),
        Editor(Editors.ChartCollectionEditor.Editor, Editors.ChartCollectionEditor.Base),
		DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden),
		]
		override public SubAxisCollection SubAxes
		{
			get
			{
				return base.SubAxes;
			}
		}

		/// <summary>
		/// Indicates if this axis object present the main or sub axis.
		/// </summary>
		override internal bool IsSubAxis
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Returns sub-axis name.
		/// </summary>
		override internal string SubAxisName
		{
			get
			{
				return base.Name;
			}
		}

#endregion // Properties

#region Methods

		/// <summary>
		/// Find axis position using crossing value.
		/// </summary>
		/// <param name="ignoreCrossing">Axis crossing should be ignored.</param>
		/// <returns>Relative position</returns>
		override internal double GetAxisPosition(bool ignoreCrossing)
		{
			// Parent axis must be set
			if(this.parentAxis != null)
			{
				// Get position of the parent axis
				double position = this.parentAxis.GetAxisPosition(ignoreCrossing);

                // Addjust parent position by the offset
				if(this.parentAxis.AxisPosition == AxisPosition.Left)
				{
					position -= this.offsetFromParent;
				}
				else if(this.parentAxis.AxisPosition == AxisPosition.Right)
				{
					position += this.offsetFromParent;
				}
				else if(this.parentAxis.AxisPosition == AxisPosition.Top)
				{
					position -= this.offsetFromParent;
				}
				else if(this.parentAxis.AxisPosition == AxisPosition.Bottom)
				{
					position += this.offsetFromParent;
				}
				return position;
			}

			return 0.0;
		}

#endregion // Methods

#region Type converter

#if WINFORMS_CONTROL

		internal class SubAxisConverter : TypeConverter
		{
			/// <summary>
			/// This method overrides CanConvertTo from TypeConverter. This is called when someone
			/// wants to convert an instance of object to another type.  Here,
			/// only conversion to an InstanceDescriptor is supported.
			/// </summary>
			/// <param name="context">Descriptor context.</param>
			/// <param name="destinationType">Destination type.</param>
			/// <returns>True if object can be converted.</returns>
			public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			{
				if (destinationType == typeof(InstanceDescriptor))
				{
					return true;
				}

				// Always call the base to see if it can perform the conversion.
				return base.CanConvertTo(context, destinationType);
			}

			/// <summary>
			/// This code performs the actual conversion from an object to an InstanceDescriptor.
			/// </summary>
			/// <param name="context">Descriptor context.</param>
			/// <param name="culture">Culture information.</param>
			/// <param name="value">Object value.</param>
			/// <param name="destinationType">Destination type.</param>
			/// <returns>Converted object.</returns>
			public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
			{
				if (destinationType == typeof(InstanceDescriptor))
				{
					ConstructorInfo ci = typeof(SubAxis).GetConstructor(System.Type.EmptyTypes);
					return new InstanceDescriptor(ci, null, false);
				}

				// Always call base, even if you can't convert.
				return base.ConvertTo(context, culture, value, destinationType);
			}
		}

#endif //#if WINFORMS_CONTROL	

#endregion
	}

	/// <summary>
	/// <b>SubAxisCollection</b> is a strongly typed collection of chart sub-axes objects.
    /// Collection indexer can accept sub-axis index or it's unique name as a parameter.
	/// </summary>
	[
		SRDescription("DescriptionAttributeSubAxisCollection_SubAxisCollection"),
	]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
	public class SubAxisCollection : CollectionBase
	{
#region Fields

		/// <summary>
		/// Sub-Axis parent axis object.
		/// </summary>
		internal Axis parentAxis = null;

#endregion

#region Construction and Initialization

		/// <summary>
		/// Default public constructor.
		/// </summary>
		/// <remarks>
		/// This constructor is for internal use and should not be part of documentation.
		/// </remarks>
		public SubAxisCollection()
		{
			this.parentAxis = null;
		}

		/// <summary>
		/// Public constructor.
		/// </summary>
		/// <param name="parentAxis">
		/// Chart <see cref="Axis"/> object.
		/// </param>
		/// <remarks>
		/// This constructor is for the internal use and should not be part of documentation.
		/// </remarks>
		internal SubAxisCollection(Axis parentAxis)
		{
			this.parentAxis = parentAxis;
		}

#endregion

#region Indexer

		/// <summary>
		/// SubAxis collection indexer.
		/// </summary>
		/// <remarks>
		/// The <b>SubAxis</b> object's name or index can be provided as a parameter. Returns the <see cref="SubAxis"/> object. 
		/// Make sure to cast the SubAxis to it's type (e.g. LineSubAxis) to access type 
		/// specific properties.
		/// </remarks>
		[
		SRDescription("DescriptionAttributeSubAxisCollection_Item"),
		]
		public SubAxis this[object parameter] 
		{
			get 
			{ 
				// Get SubAxis by index
				if(parameter is int)
				{
					return (SubAxis)this.List[(int)parameter]; 
				}

					// Get SubAxis by name
				else if(parameter is string)
				{
					// Find SubAxis with specified name
					foreach(SubAxis SubAxis in this.List)
					{
						if(SubAxis.Name == (string)parameter)
						{
							return SubAxis;
						}
					}

					// SubAxis with specified name was not found
					throw(new ArgumentException( SR.ExceptionSubAxisNameNotFound( (string)parameter ) ) );
				}

				// Invalid type of the indexer argument
				throw(new ArgumentException(SR.ExceptionInvalidIndexerArgumentType));
			} 

			set 
			{ 
				// Check new SubAxis name
				int indexSubAxis = -1;
				if(value.Name.Length != 0)
				{
					indexSubAxis = this.List.IndexOf(value);
				}
				else
				{
					AssignUniqueName(value);
				}

				// Set using index in the collection
				if(parameter is int)
				{
					// Check if SubAxis with this name already exists
					if( indexSubAxis != -1 && indexSubAxis != (int)parameter)
					{
						throw( new ArgumentException( SR.ExceptionSubAxisNameAlreadyExistsInCollection( value.Name ) ) );
					}

					this.List[(int)parameter] = value;
				}

					// Set using name in the collection
				else if(parameter is string)
				{
					// Find legend with specified name
					int index = 0;
					foreach(SubAxis SubAxis in this.List)
					{
						if(SubAxis.Name == (string)parameter)
						{
							// Check if SubAxis with this name already exists
							if( indexSubAxis != -1 && indexSubAxis != index)
							{
								throw( new ArgumentException( SR.ExceptionSubAxisNameAlreadyExistsInCollection( value.Name ) ) );
							}

							this.List[index] = value;
							break;
						}
						++index;
					}
				}
				else
				{
					throw(new ArgumentException(SR.ExceptionInvalidIndexerArgumentType));
				}

				this.Invalidate();			
			}
		}

#endregion

#region Collection Add and Insert methods

		/// <summary>
		/// Removes the SubAxis with the specified name from the collection.
		/// </summary>
		/// <param name="name">
		/// Name of the SubAxis to be removed.
		/// </param>
		public void Remove(string name)
		{
			SubAxis axis = FindByName(name);
			if(axis != null)
			{
				this.List.Remove(axis);
			}
		}

		/// <summary>
		/// Removes the given SubAxis from the collection.
		/// </summary>
		/// <param name="SubAxis">
		/// <see cref="SubAxis"/> object to be removed.
		/// </param>
		public void Remove(SubAxis SubAxis)
		{
			if(SubAxis != null)
			{
				this.List.Remove(SubAxis);
			}
		}

		/// <summary>
		/// Adds a SubAxis to the end of the collection.
		/// </summary>
		/// <param name="SubAxis">
		/// <see cref="SubAxis"/> object to add.
		/// </param>
		/// <returns>
		/// Index of the newly added object.
		/// </returns>
		public int Add(SubAxis SubAxis)
		{
			return this.List.Add(SubAxis);
		}

		/// <summary>
		/// Inserts a SubAxis into the collection.
		/// </summary>
		/// <param name="index">
		/// Index to insert the object at.
		/// </param>
		/// <param name="SubAxis">
		/// <see cref="SubAxis"/> object to insert.
		/// </param>
		public void Insert(int index, SubAxis SubAxis)
		{
			this.List.Insert(index, SubAxis);
		}

#endregion

#region Items Inserting and Removing Notification methods

		/// <summary>
		/// Called before the new item is inserted.
		/// </summary>
		/// <param name="index">Item index.</param>
		/// <param name="value">Item object.</param>
		/// <remarks>
		/// This is an internal method and should not be part of the documentation.
		/// </remarks>
		protected override  void OnInsert(int index, object value)
		{
			// Check SubAxis object name
			if( ((SubAxis)value).Name.Length == 0 )
			{
				AssignUniqueName((SubAxis)value);
			}
			else
			{
				if(this.FindByName(((SubAxis)value).Name) != null)
				{
					throw(new InvalidOperationException(SR.ExceptionSubAxisNameIsNotUnique( ((SubAxis)value).Name )));
				}
			}
		}

		/// <summary>
		/// After new item inserted.
		/// </summary>
		/// <param name="index">Item index.</param>
		/// <param name="value">Item object.</param>
		/// <remarks>
		/// This is an internal method and should not be part of the documentation.
		/// </remarks>
		protected override void OnInsertComplete(int index, object value)
		{
			// Set SubAxis parent axis reference
			SubAxis subAxis = (SubAxis)value;
			subAxis.parentAxis = this.parentAxis;
			if(this.parentAxis != null)
			{
				subAxis.chart = this.parentAxis.chart;
				subAxis.Common = this.parentAxis.Common;
				subAxis.chartArea = this.parentAxis.chartArea;
				subAxis.axisType= this.parentAxis.axisType;
				subAxis.AxisPosition= this.parentAxis.AxisPosition;
			}
			this.Invalidate();
		}

		/// <summary>
		/// After item removed.
		/// </summary>
		/// <param name="index">Item index.</param>
		/// <param name="value">Item object.</param>
		/// <remarks>
		/// This is an internal method and should not be part of the documentation.
		/// </remarks>
		protected override void OnRemoveComplete(int index, object value)
		{
			// Reset SubAxis parent axis reference
			((SubAxis)value).parentAxis = null;

			this.Invalidate();
		}

		/// <summary>
		/// After all items removed.
		/// </summary>
		/// <remarks>
		/// This is an internal method and should not be part of the documentation.
		/// </remarks>
		protected override void OnClearComplete()
		{
			this.Invalidate();
		}

#endregion

#region Helper Methods

		/// <summary>
		/// Invalidates chart the collection belongs to.
		/// </summary>
		private void Invalidate()
		{
#if WINFORMS_CONTROL
			if(this.parentAxis != null && this.parentAxis.chart != null)
			{
				this.parentAxis.chart.dirtyFlag = true;
				this.parentAxis.chart.Invalidate();
			}
#endif
		}

		/// <summary>
		/// Assigns a unique name to the SubAxis object based on it's type.
		/// </summary>
		/// <param name="SubAxis">SubAxis object to be named.</param>
		internal void AssignUniqueName(SubAxis SubAxis)
		{
			// Generate name using SubAxis type name and unique index
			string name = string.Empty;
			int	index = 1;
			do
			{
				name = "SubAxis" + index.ToString();
				++index;
			} while(this.FindByName(name) != null && index < 10000 );

			// Asign unique name;
			SubAxis.Name = name;
		}
        
		/// <summary>
		/// Finds SubAxis by name.
		/// </summary>
		/// <param name="name">Name of the chart SubAxis.</param>
		/// <returns>SubAxis or null if it does not exist.</returns>
		internal SubAxis FindByName(string name) 
		{
			SubAxis result = null;
			for(int index = 0; index < this.List.Count; index ++)
			{
				// Compare SubAxis name 
				if(String.Compare(this[index].Name, name, true, System.Globalization.CultureInfo.CurrentCulture) == 0)
				{
					result = this[index];
					break;
				}
			}

			return result;
		}

#endregion 
	}
}

#endif // SUBAXES

