
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
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
