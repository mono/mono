//
// System.ComponentModel.LicFileLicenseProvider.cs
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
//

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

namespace System.ComponentModel
{
	public class LicFileLicenseProvider : LicenseProvider
	{

		public LicFileLicenseProvider()
		{
		}

		public override License GetLicense (LicenseContext context,
						    Type type,
						    object instance,
						    bool allowExceptions)
		{
			try
			{
				if (context == null || context.UsageMode != LicenseUsageMode.Designtime)
					return null;
			
				string path = Path.GetDirectoryName (type.Assembly.Location);
				path = Path.Combine (path, type.FullName + ".LIC");
			
				if (!File.Exists (path)) return null;
			
				StreamReader sr = new StreamReader (path);
				string key = sr.ReadLine ();
				sr.Close ();
				
				if (IsKeyValid (key, type))
					return new LicFileLicense (key);
			}
			catch
			{
				if (allowExceptions) throw;
			}
			return null;
		}

		protected virtual string GetKey (Type type)
		{
			return (type.FullName + " is a licensed component.");
		}

		protected virtual bool IsKeyValid (string key, Type type)
		{
			if (key == null)
				return false;
			return key.Equals (GetKey (type));
		}
	}
	
	internal class LicFileLicense: License
	{
		string _key;
		
		public LicFileLicense (string key)
		{
			_key = key;
		}
		
		public override string LicenseKey
		{
			get { return _key; }
		}
		
		public override void Dispose ()
		{
		}
	}
}
