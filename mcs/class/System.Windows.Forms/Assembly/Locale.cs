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
// Copyright (c) 2001-2005 Novell, Inc.
//
// Authors:
//	Miguel de Icaza (miguel@ximian.com)
//	Andreas Nahr	(ClassDevelopment@A-SoftTech.com)
//	Peter Bartok	(pbartok@novell.com)
//
//

// This class is basically disabled until we decide we are going to
// translate winforms.  There are only a handful of places where we
// actually use this.  The GetText method has been kept so those
// continue to work and people can still add new ones if they choose.
// At any rate, we really don't need to localize images (or at least
// any of the ones we had).

using System;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace System.Windows.Forms {
	internal static class Locale {
		#region Local Variables
		private static ResourceManager	rm;
		#endregion	// Local Variables

		#region Constructors
		static Locale () {
		        rm = new ResourceManager("System.Windows.Forms", Assembly.GetExecutingAssembly());
		}
		#endregion

		#region Static Properties
		public static ResourceManager ResourceManager {
		        get {
		                return rm;
		        }
		}
		#endregion	// Static Properties

		#region Static Methods
		public static string GetText (string msg) {
			string ret = ResourceManager.GetString (msg);
			if (ret != null)
				return ret;
			return msg;
			
//                        string ret;

//// This code and behaviour may change without notice. It's a placeholder until I
//// understand how Miguels wants localization of strings done.
//                        ret = (string)rm.GetObject(msg);
//                        if (ret != null) {
//                                return ret;
//                        }
//                        return msg;
		}

		public static string GetText (string msg, params object [] args) {
			return String.Format (GetText (msg), args);
		}

		//public static object GetResource(string name) {
		//        return rm.GetObject(name);
		//}
		#endregion	// Static Methods
	}
}
