//
// System.Web.UI.LosFormatter
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

using System.IO;


namespace System.Web.UI {
	public sealed class LosFormatter {

		ObjectStateFormatter osf = new ObjectStateFormatter ();
		
		public object Deserialize (Stream stream)
		{
			return osf.Deserialize (stream);
		}

		public object Deserialize (TextReader input)
		{
			if (input == null)
				throw new ArgumentNullException ("input");

			return Deserialize (input.ReadToEnd ());
		}

		public object Deserialize (string input)
		{
			return osf.Deserialize (input);
		}

		public void Serialize (Stream stream, object value)
		{
			osf.Serialize (stream, value);
		}

		public void Serialize (TextWriter output, object value)
		{
			if (output == null)
				throw new ArgumentNullException ("output");
			
			output.Write (osf.Serialize (value));
		}	
	}
}