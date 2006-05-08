using System;
using System.Web;
using System.Web.UI;
using System.Reflection;

namespace NunitWeb
{
	public class RenderControlHelper
	{
		[Serializable]
		class RenderCallbackData
		{
			public Type type;
			public string[] propNames;
			public object[] propValues;
		}

		static private void RenderCallback (HttpContext context, object param)
		{
			RenderCallbackData rcd = (RenderCallbackData) param;
			Control c = (Control) Activator.CreateInstance (rcd.type);
			if (rcd.propNames != null) {
				for (int i = 0; i < rcd.propNames.Length; i++) {
					if (rcd.propNames[i] == null)
						continue;
					PropertyInfo pi = rcd.type.GetProperty (rcd.propNames[i]);
					pi.SetValue (c, rcd.propValues[i], null);
				}
			}
			c.Page = new Page ();
			RenderCallbackImpl (context, c);
		}

		static private void RenderCallbackImpl (HttpContext context, object param)
		{
			Control c = (Control) param;
			using (HtmlTextWriter htw = new HtmlTextWriter (context.Response.Output)) {
				c.RenderControl (htw);
			}
		}

		static public string RenderControl (Type controlType, string[] propNames, object[] propValues)
		{
			if (propNames == null || propValues == null)
				throw new ArgumentException ("propNames and propValues should not be null");
			if (propNames.Length != propValues.Length)
				throw new ArgumentException ("propNames and propValues should have the same size");
			RenderCallbackData rcd = new RenderCallbackData ();
			rcd.type = controlType;
			rcd.propNames = propNames;
			rcd.propValues = propValues;
			return Helper.Instance.Run (new Helper.AnyMethod (RenderCallback), rcd);
		}

		static public string RenderControl (Type controlType)
		{
			RenderCallbackData rcd = new RenderCallbackData ();
			rcd.type = controlType;
			rcd.propNames = null;
			rcd.propValues = null;
			return Helper.Instance.Run (new Helper.AnyMethod (RenderCallback), rcd);
		}

		static public string RenderControl (Control control)
		{
			if (control.GetType ().IsSerializable)
				return Helper.Instance.Run (new Helper.AnyMethod (RenderCallbackImpl), control);
			
			RenderCallbackData rcd = new RenderCallbackData ();
			rcd.type = control.GetType ();
			PropertyInfo[] pi = rcd.type.GetProperties ();
			rcd.propNames = new string[pi.Length];
			rcd.propValues = new object[pi.Length];
			for (int i = 0; i < pi.Length; i++) {
				if (!pi[i].CanRead || !pi[i].CanWrite)
					continue;
				rcd.propNames[i] = pi[i].Name;
				try {
					rcd.propValues[i] = pi[i].GetValue (control, null);
				}
				catch {
					Console.Error.WriteLine (String.Format ("Could not get property {0}.{1}",
						rcd.type.Name, pi[i].Name));
				}
			}
			return Helper.Instance.Run (new Helper.AnyMethod (RenderCallback), rcd);
		}
	}
}
