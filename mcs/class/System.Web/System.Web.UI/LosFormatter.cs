//
// System.Web.UI.LosFormatter
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.IO;
using System.Security.Permissions;

namespace System.Web.UI {

	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class LosFormatter {

		ObjectStateFormatter osf = new ObjectStateFormatter ();
		
		public LosFormatter ()
		{
		}

#if NET_1_1
		[MonoTODO]
		public LosFormatter (bool enableMac, string macKeyModifier)
		{
			// FIXME - is this used ???
		}
#endif
#if NET_2_0
		[MonoTODO]
		public LosFormatter (bool enableMac, byte[] macKeyModifier)
		{
			throw new NotImplementedException ();
		}
#endif

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
