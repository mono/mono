//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		ChartAreaCollection.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting
//
//	Classes:	ChartAreaCollection
//
//  Purpose:	ChartAreaCollection class represents a strongly 
//              typed collection of ChartArea objects.
//
//	Reviewed:	GS - Aug 8, 2002
//				AG - Aug 8, 2002
//              AG - Microsoft 16, 2007
//
//===================================================================


#region Used namespaces

using System;
using System.Collections;
using System.Drawing;
using System.ComponentModel;


#endregion

#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting

#endif
{
	/// <summary>
    /// The ChartAreaCollection class represents a strongly typed collection of 
    /// ChartArea objects. Each chart area has a unique name in the collection
    /// and can be retrieved by name or by index.
	/// </summary>
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class ChartAreaCollection : ChartNamedElementCollection<ChartArea>
	{

		#region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartAreaCollection"/> class.
        /// </summary>
        /// <param name="chartPicture">Parent chart picture.</param>
		internal ChartAreaCollection(ChartPicture chartPicture) : base(chartPicture)
		{
		}

        #endregion

        #region Properties
        /// <summary>
        /// Gets the default chart area name.
        /// </summary>
        internal string DefaultNameReference 
        {
            get { return this.Count > 0 ? this[0].Name : String.Empty; }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Creates a new ChartArea with the specified name and adds it to the collection.
        /// </summary>
        /// <param name="name">The new chart area name.</param>
        /// <returns></returns>
        public ChartArea Add(string name) 
        {
            ChartArea area = new ChartArea(name);
            this.Add(area);
            return area;
        }

        #endregion

        #region Event handlers
        /// <summary>
        /// Updates the ChartArea alignment references to another chart areas.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Charting.NameReferenceChangedEventArgs"/> instance containing the event data.</param>
        internal void ChartAreaNameReferenceChanged(object sender, NameReferenceChangedEventArgs e)
        {
            foreach (ChartArea chartArea in this)
                if (chartArea.AlignWithChartArea == e.OldName)
                    chartArea.AlignWithChartArea = e.NewName;
        }

        #endregion

    }
}
