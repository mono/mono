/**
 * Namespace:   System.Web.UI.Design.WebControls
 * Class:       TableCellsCollectionEditor
 *
 * Author:      Gaurav Vaish
 * Maintainer:  gvaish_mono@lycos.com
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.ComponentModel.Design;
using System.Reflection;
using System.Web.UI.WebControls;
using System.Web.UI.Design;

namespace System.Web.UI.Design.WebControls
{
	public class TableCellsCollectionEditor : CollectionEditor
	{
		public TableCellsCollectionEditor(Type type) : base(type)
		{
		}

		protected override bool CanSelectMultipleInstances()
		{
			return false;
		}

		protected override object CreateInstance(Type itemType)
		{
			return Activator.CreateInstance(itemType, BindingFlags.Public |
			                                BindingFlags.CreateInstance |
			                                BindingFlags.Instance,
			                                null, null, null);
		}
	}
}
