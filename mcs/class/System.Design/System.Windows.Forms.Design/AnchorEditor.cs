//
// System.Windows.Forms.Design.AnchorEditor.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.ComponentModel;
using System.Drawing.Design;

namespace System.Windows.Forms.Design
{
	[MonoTODO]
	public sealed class AnchorEditor : UITypeEditor
	{
		#region Public Instance Constructors

		public AnchorEditor()
		{
		}

		#endregion Public Instance Constructors

		#region Override implementation of UITypeEditor

		[MonoTODO]
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, Object value)
		{
			throw new NotImplementedException ();
		}

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		#endregion Override implementation of UITypeEditor
	}
}
