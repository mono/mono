/**
 * Namespace:   System.Web.UI.Design.WebControls
 * Class:       BordersPageInternal
 *
 * Author:      Gaurav Vaish
 * Maintainer:  mastergaurav AT users DOT sf DOT net
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Web.UI.Design;
using System.Windows.Forms.Design;
using System.Web.UI.WebControls;

namespace System.Web.UI.Design.WebControls
{
	class BordersPageInternal : BaseDataListPageInternal
	{
		public BordersPageInternal()
		{
		}

		[MonoTODO]
		protected override void LoadComponent()
		{
			InitializePage();
			BaseDataList baseCtrl = GetBaseControl();
			int cellPadding = baseCtrl.CellPadding;
			if(cellPadding >= 0)
			{
				throw new NotImplementedException();
			}
			int cellSpacing = baseCtrl.CellSpacing;
			if(cellSpacing >= 0)
			{
				throw new NotImplementedException();
			}
			throw new NotImplementedException();
		}

		[MonoTODO]
		private void InitializePage()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected override void SaveComponent()
		{
			throw new NotImplementedException();
		}

		private void OnBordersChanged(object source, EventArgs e)
		{
			if(!IsLoading())
			{
				SetDirty();
			}
		}

		private void OnClickColorPicker(object source, EventArgs e)
		{
			throw new NotImplementedException();
		}

		public override void SetComponent(IComponent component)
		{
			base.SetComponent(component);
			InitializeForm();
		}

		[MonoTODO]
		private void InitializeForm()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected override string HelpKeyword
		{
			get
			{
				throw new NotImplementedException();
			}
		}
	}
}
