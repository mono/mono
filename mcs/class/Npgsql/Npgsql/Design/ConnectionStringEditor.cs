using System;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Npgsql.Design
{
	/// <summary>
	/// An UITypeEditor that simply initializes a
	/// ConnectionStringEditorForm if possible
	/// </summary>
	internal class ConnectionStringEditor : UITypeEditor
	{
		/// <summary>
		/// Edits the Value of the given Object using the EditSyle given by GetEditStyle.
		/// </summary>
		/// <param name="context">An ITypeDescriptorContext, through wich you can get additional context information.</param>
		/// <param name="provider">An IServiceProvider, through which this editor may order services.</param>
		/// <param name="value">The Object to edit</param>
		/// <returns>The new value of the Object</returns>
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && context.Instance != null && provider != null) {
				IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

				if (edSvc != null) {
					ConnectionStringEditorForm eform;

					if(value != null && value.ToString() != String.Empty) {
						eform = new ConnectionStringEditorForm(value.ToString());
					} else {
						eform = new ConnectionStringEditorForm();
					}

					if(edSvc.ShowDialog(eform) == DialogResult.OK) {
						return eform.ConnectionString;
					} else {
						return value;
					}
				} else {
					return value;
				}
			}

			return value;
		}

		/// <summary>
		/// Requests the EditSyle to be used by EditValue
		/// </summary>
		/// <param name="context">An ITypeDescriptorContext, through wich you can get additional context information.</param>
		/// <returns>An UITypeEditorEditStyle-Value, indicating the EditStyle used by EditValue. If UITypeEditor doesn't support this method, GetEditStyle returns the value None.</returns>
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			if (context != null && context.Instance != null) {
				return UITypeEditorEditStyle.Modal;
			}

			return base.GetEditStyle (context);
		}
	}
}

