//
// System.Web.UI.WebControls.ControlParameter
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace System.Web.UI.WebControls {
	public class ControlParameter : Parameter {
		protected ControlParameter (ControlParameter original) : base (original)
		{
			this.ControlID = original.ControlID;
			this.PropertyName = original.PropertyName;
		}
		
		public ControlParameter (string name, string controlID) : base (name)
		{
			ControlID = controlID;
		}
		
		public ControlParameter (string name, string controlID, string propertyName) : base (name)
		{
			ControlID = controlID;
			PropertyName = propertyName;
		}
		
		public ControlParameter (string name, TypeCode type, string controlID, string propertyName) : base (name, type)
		{
			ControlID = controlID;
			PropertyName = propertyName;
		}
		
		protected override Parameter Clone ()
		{
			return new ControlParameter (this);
		}
		
		[MonoTODO]
		protected override object Evaluate (Control control)
		{
			throw new NotImplementedException ();
		}
		
		public string ControlID {
			get {
				string s = ViewState ["ControlID"] as string;
				if (s != null)
					return s;
				
				return "";
			}
			set {
				if (ControlID != value) {
					ViewState ["ControlID"] = value;
					OnParameterChanged ();
				}
			}
		}
		
		public string PropertyName {
			get {
				string s = ViewState ["PropertyName"] as string;
				if (s != null)
					return s;
				
				return "";
			}
			set {
				
				if (PropertyName != value) {
					ViewState ["PropertyName"] = value;
					OnParameterChanged ();
				}
			}
		}
	}
}
#endif

