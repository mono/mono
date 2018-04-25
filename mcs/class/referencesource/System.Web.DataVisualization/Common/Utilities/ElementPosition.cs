//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		ElementPosition.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting
//
//	Classes:	ElementPosition
//
//  Purpose:	Class is used to store relative position of the chart
//				elements like Legend, Title and others. It uses
//              relative coordinate system where top left corner is
//              0,0 and bottom right is 100,100.
//          
//              If Auto property is set to true, all position properties 
//              (X,Y,Width and Height) are ignored and they automatically
//              calculated during chart rendering.
//              
//              Note that setting any of the position properties will 
//              automatically set Auto property to false.
//
//	Reviewed:	AG - August 7, 2002
//              AG - Microsoft 5, 2007
//
//===================================================================


#region Used Namespaces

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

#if Microsoft_CONTROL
	using System.Windows.Forms.DataVisualization.Charting;
	using System.Windows.Forms.DataVisualization.Charting.Data;
	using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
	using System.Windows.Forms.DataVisualization.Charting.Utilities;
	using System.Windows.Forms.DataVisualization.Charting.Borders3D;
#else
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.DataVisualization.Charting;
	using System.Web.UI.DataVisualization.Charting.Data;
#endif


#endregion

#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting

#endif
{
	/// <summary>
    /// ElementPosition is the base class for many chart visual 
    /// elements like Legend, Title and ChartArea. It provides 
    /// the position of the chart element in relative coordinates, 
    /// from (0,0) to (100,100).
	/// </summary>
	[
		SRDescription("DescriptionAttributeElementPosition_ElementPosition"),
		DefaultProperty("Data"),
	]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class ElementPosition : ChartElement
	{
		#region Fields

		// Private data members, which store properties values
		private		float	_x = 0;
		private		float	_y = 0;
		private		float	_width = 0;
		private		float	_height = 0;
		internal	bool	_auto = true;

		// Indicates the auto position of all areas must be reset
		internal	bool			resetAreaAutoPosition = false;

		#endregion

		#region Constructors

		/// <summary>
		/// ElementPosition default constructor
		/// </summary>
		public ElementPosition()
		{
		}

        /// <summary>
        /// ElementPosition default constructor
        /// </summary>
        internal ElementPosition(IChartElement parent) 
            : base(parent)
        {
        }


		/// <summary>
        /// ElementPosition constructor.
		/// </summary>
		/// <param name="x">X position.</param>
		/// <param name="y">Y position.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "X and Y are cartesian coordinates and well understood")]       
		public ElementPosition(float x, float y, float width, float height)
		{
			this._auto = false;
			this._x = x;
			this._y = y;
			this._width = width;
			this._height = height;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Asks the user at design-time if he wants to change the Auto position
		/// of all areas at the same time.
		/// </summary>
		/// <param name="autoValue">Value to be set for the Auto property.</param>
		private void ResetAllAreasAutoPosition(bool autoValue)
		{
			if(resetAreaAutoPosition)
			{
				// Proceed only if at design time
				if(Chart != null && Chart.IsDesignMode() && !Chart.serializing && Chart.Site != null)
				{
					// Check if there is more than one area and Auto position set to the same value
					if(Chart.ChartAreas.Count > 1)
					{
						bool	firstAutoValue = Chart.ChartAreas[0].Position.Auto;
						bool	sameAutoValue = true;
						foreach(ChartArea area in Chart.ChartAreas)
						{
							if(area.Position.Auto != firstAutoValue)
							{
								sameAutoValue = false;
								break;
							}
						}

						// Proceed only all Auto values are the same
						if(sameAutoValue)
						{
                            string message = SR.MessageChangingChartAreaPositionProperty;
                            if (autoValue)
                            {
                                message += SR.MessageChangingChartAreaPositionConfirmAutomatic;
                            }
                            else
                            {
                                message += SR.MessageChangingChartAreaPositionConfirmCustom;
                            }

                            
                            IDesignerMessageBoxDialog confirm = Chart.Site.GetService(typeof(IDesignerMessageBoxDialog)) as IDesignerMessageBoxDialog;
                            if (confirm != null && confirm.ShowQuestion(message))
                            {
                                foreach (ChartArea area in Chart.ChartAreas)
                                {
                                    if (autoValue)
                                    {
                                        this.SetPositionNoAuto(0f, 0f, 0f, 0f);
                                    }
                                    area.Position._auto = autoValue;
                                }

                            }
						}
					}
				}
			}
		}

		/// <summary>
		/// Convert element position into RectangleF
		/// </summary>
		/// <returns>RectangleF structure.</returns>
		public RectangleF ToRectangleF()
		{
			return new RectangleF(_x, _y, _width, _height);
		}

		/// <summary>
        /// Initializes ElementPosition from RectangleF
		/// </summary>
        /// <param name="rect">RectangleF structure.</param>
		public void FromRectangleF(RectangleF rect)
		{
            if (rect == null)
                throw new ArgumentNullException("rect");

			this._x = rect.X;
			this._y = rect.Y;
			this._width = rect.Width;
			this._height = rect.Height;
			this._auto = false;
		}

		/// <summary>
        /// Gets the size of the ElementPosition object.
		/// </summary>
        /// <returns>The size of the ElementPosition object.</returns>
		[Browsable(false)]
        [Utilities.SerializationVisibility(Utilities.SerializationVisibility.Hidden)]
        public SizeF Size
		{
            get { return new SizeF(this._width, this._height); }
		}

		/// <summary>
		/// Gets the bottom position in relative coordinates.
		/// </summary>
		/// <returns>Bottom position.</returns>
        [Browsable(false)]
        [Utilities.SerializationVisibility(Utilities.SerializationVisibility.Hidden)]
        public float Bottom
		{
            get { return this._y + this._height; }
		}

		/// <summary>
		/// Gets the right position in relative coordinates.
		/// </summary>
		/// <returns>Right position.</returns>
        [Browsable(false)]
        [Utilities.SerializationVisibility(Utilities.SerializationVisibility.Hidden)]
        public float Right
		{
			get{ return this._x + this._width; }
		}

        /// <summary>
        /// Determines whether the specified Object is equal to the current Object.
        /// </summary>
        /// <param name="obj">The Object to compare with the current Object.</param>
        /// <returns>true if the specified Object is equal to the current Object; otherwise, false.</returns>
		internal override bool EqualsInternal(object obj)
		{
            ElementPosition pos = obj as ElementPosition;
			if(pos != null)
			{
				if(this._auto == true && this._auto == pos._auto)
				{
					return true;
				}
				else if(this._x == pos._x && this._y == pos._y &&
						this._width == pos._width && this._height == pos._height)
				{
					return true;
				}

			}
            return false;
		}

		/// <summary>
		/// Returns a string that represents the element position data.
		/// </summary>
		/// <returns>Element position data as a string.</returns>
		internal override string ToStringInternal()
		{
            string posString = Constants.AutoValue;
			if(!this._auto)
			{
				posString = 
					this._x.ToString(System.Globalization.CultureInfo.CurrentCulture)+", "+
					this._y.ToString(System.Globalization.CultureInfo.CurrentCulture)+", "+
					this._width.ToString(System.Globalization.CultureInfo.CurrentCulture)+", "+
					this._height.ToString(System.Globalization.CultureInfo.CurrentCulture);
			}
			return posString;
		}

		/// <summary>
		/// Set the element position without modifying the "Auto" property
		/// </summary>
		/// <param name="x">X position.</param>
		/// <param name="y">Y position.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		internal void SetPositionNoAuto(float x, float y, float width, float height)
		{
			bool oldValue = this._auto;
			this._x = x;
			this._y = y;
			this._width = width;
			this._height = height;
			this._auto = oldValue;
		}

		#endregion

		#region Element Position properties

		/// <summary>
		/// X position of element.
		/// </summary>
		[
		SRCategory("CategoryAttributeMisc"),
		Bindable(true),
		DefaultValue(0.0F),
		SRDescription("DescriptionAttributeElementPosition_X"),
		NotifyParentPropertyAttribute(true),
		RefreshPropertiesAttribute(RefreshProperties.All),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "X")]
		public float X
		{
			get
			{
				return _x;
			}
			set
			{
				if(value < 0.0 || value > 100.0)
				{
					throw(new ArgumentOutOfRangeException("value", SR.ExceptionElementPositionArgumentOutOfRange));
				}
				_x = value;
				Auto = false;

				// Adjust width
				if( (_x + Width) > 100)
				{
					Width = 100 - _x;
				}

				this.Invalidate();
			}
		}

		/// <summary>
		/// Y position of element.
		/// </summary>
		[
		SRCategory("CategoryAttributeMisc"),
		Bindable(true),
		DefaultValue(0.0F),
		SRDescription("DescriptionAttributeElementPosition_Y"),
		NotifyParentPropertyAttribute(true),
		RefreshPropertiesAttribute(RefreshProperties.All),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Y")]
		public float Y
		{
			get
			{
				return _y;
			}
			set
			{
				if(value < 0.0 || value > 100.0)
				{
					throw(new ArgumentOutOfRangeException("value", SR.ExceptionElementPositionArgumentOutOfRange));
				}
				_y = value;
				Auto = false;

				// Adjust heigth
				if( (_y + Height) > 100)
				{
					Height = 100 - _y;
				}

				this.Invalidate();
			}
		}

		/// <summary>
		/// Width of element.
		/// </summary>
		[
		SRCategory("CategoryAttributeMisc"),
		Bindable(true),
		DefaultValue(0.0F),
		SRDescription("DescriptionAttributeElementPosition_Width"),
		NotifyParentPropertyAttribute(true),
		RefreshPropertiesAttribute(RefreshProperties.All),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public float Width
		{
			get
			{
				return _width;
			}
			set
			{
				if(value < 0.0 || value > 100.0)
				{
					throw(new ArgumentOutOfRangeException("value", SR.ExceptionElementPositionArgumentOutOfRange));
				}
				_width = value;
				Auto = false;

				// Adjust x
				if( (_x + Width) > 100)
				{
					_x = 100 - Width;
				}

				this.Invalidate();
			}
		}

		/// <summary>
		/// Height of element.
		/// </summary>
		[
		SRCategory("CategoryAttributeMisc"),
		Bindable(true),
		DefaultValue(0.0F),
		SRDescription("DescriptionAttributeElementPosition_Height"),
		NotifyParentPropertyAttribute(true),
		RefreshPropertiesAttribute(RefreshProperties.All),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public float Height
		{
			get
			{
				return _height;
			}
			set
			{
				if(value < 0.0 || value > 100.0)
				{
					throw(new ArgumentOutOfRangeException("value", SR.ExceptionElementPositionArgumentOutOfRange));
				}
				_height = value;
				Auto = false;

				// Adjust y
				if( (_y + Height) > 100)
				{
					_y = 100 - Height;

				}

				this.Invalidate();
			}
		}

        /// <summary>
        /// Gets or sets a flag which indicates whether positioning is on.
        /// </summary>
		[
		SRCategory("CategoryAttributeMisc"),
		Bindable(true),
		DefaultValue(true),
		SRDescription("DescriptionAttributeElementPosition_Auto"),
		NotifyParentPropertyAttribute(true),
		RefreshPropertiesAttribute(RefreshProperties.All),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public bool Auto
		{
			get
			{
				return _auto;
			}
			set
			{
				if(value != _auto)
				{
					ResetAllAreasAutoPosition(value);

					if(value)
					{
						this._x = 0;
						this._y = 0;
						this._width = 0;
						this._height = 0;
					}
					_auto = value;

					this.Invalidate();
				}
			}
		}

		#endregion
	}

    /// <summary>
    /// Used for invoking windows forms MesageBox dialog.
    /// </summary>
    internal interface IDesignerMessageBoxDialog
    {
        /// <summary>
        /// Shows Yes/No MessageBox.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>
        /// true if user confirms with Yes
        /// </returns>
        bool ShowQuestion(string message);
    }
}
