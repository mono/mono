

namespace System.Windows.Forms{
	
	using Pango;
	using System.Drawing;

	internal class SWFGtkConv{

		private SWFGtkConv(){}

		public static MouseEventArgs MouseUpDownArgs (Gdk.EventButton ev){
			uint boton = ev.button;
			MouseButtons btn;
			switch (boton){
				case 1: btn = MouseButtons.Left;
					break;
				case 2: btn = MouseButtons.Middle;
					break;
				case 3: btn = MouseButtons.Right;
					break;
					
				//FIXME : Does it work? I've a typical wheelmouse.
				case 4: btn = MouseButtons.XButton1;
					break;
				case 5: btn = MouseButtons.XButton2;
					break;
				default:
					btn = MouseButtons.None;
					break;
			}
			int clicks = 0;
			switch (ev.type){
				case Gdk.EventType.ButtonRelease: 
				case Gdk.EventType.ButtonPress: clicks = 1;
											break;
				case Gdk.EventType.TwoButtonPress: 
				case Gdk.EventType.ThreeButtonPress: clicks = 2;
											break;
				default:
					clicks = 1;
					break;
			}					
			MouseEventArgs ret = new MouseEventArgs 
				(btn, clicks, (int)ev.x, (int)ev.y, 0);
			return ret;
		}
		
		// TODO: && is a literal &
		public static string AccelString (string st){
			//return st.Replace("_", "__").Replace ("&", "_");
			String s = st.Replace ("_", "__");
			String ret = "";
			int i=0;
			
			while (i < s.Length){
				if (s[i] == '&'){
					if ((i < (s.Length -1)) && (s[i+1] == '&')){
						i+=2;
						ret +="&";
					}
					else{
						i++;
						ret += "_";
					}
				}
				else{
					ret += s[i++];					
				}
			}
			
			return ret;
		}
		
		
		// Font not implemented.
		[MonoTODO]
		public static Pango.FontDescription Font (System.Drawing.Font f){
			FontDescription ret = new FontDescription();
			//ret.Weight = (f.Bold) ? Pango.Weight.Bold :  Pango.Weight.Normal;
			//ret.Family = f.FontFamily.Name;
			//ret.Style = (f.Italic) ? Pango.Style.Italic : Pango.Style.Normal;
			
			return ret;
		}
	}
}
