//
// System.Windows.Forms.Design.ComponentEditorForm.cs
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
	public class FileNameEditor : UITypeEditor
	{
		#region Public Instance Constructors

		public FileNameEditor ()
		{
		}

		#endregion Public Instance Constructors

		#region Override implementation of UITypeEditor

		[MonoTODO]
		public override object EditValue (ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			throw new NotImplementedException ();
		}

		public override UITypeEditorEditStyle GetEditStyle (ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
		}

		#endregion Override implementation of UITypeEditor

		#region Protected Instance Methods

		[MonoTODO]
		protected virtual void InitializeDialog (OpenFileDialog openFileDialog)
		{
			throw new NotImplementedException ();
		}

		#endregion Protected Instance Methods
	}
}
