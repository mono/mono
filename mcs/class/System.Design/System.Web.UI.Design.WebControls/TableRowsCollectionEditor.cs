/**
 * Namespace:   System.Web.UI.Design.WebControls
 * Class:       TableRowsCollectionEditor
 *
 * Author:      Gaurav Vaish
 * Maintainer:  mastergaurav AT users DOT sf DOT net
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.ComponentModel.Design;
using System.Reflection;

namespace System.Web.UI.Design.WebControls
{
	public class TableRowsCollectionEditor : CollectionEditor
	{
		public TableRowsCollectionEditor(Type type) : base(type)
		{
		}
		
		protected override bool CanSelectMultipleInstances()
		{
			return false;
		}
		
		protected override object CreateInstance(Type itemType)
		{
			return Activator.CreateInstance(itemType,
			                                BindingFlags.CreateInstance |
			                                BindingFlags.Instance |
			                                BindingFlags.Public,
			                                null, null, null);
		}
	}
}
