//
// System.Web.UI.OutputCacheParameters.cs
//
// Noam Lampert  (noaml@mainsoft.com)
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
#if NET_2_0

using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI
{
	public sealed class OutputCacheParameters
	{
		#region Fields

		string _cacheProfile;
		int _duration;
		bool _enabled;
		OutputCacheLocation _location;
		bool _noStore;
		string _sqlDependency;
		string _varByControl;
		string _varByCustom;
		string _varByHeader;
		string _varByParam;
		string _varyByContentEncoding;
		
		#endregion

		#region Constructors

		public OutputCacheParameters () {
			Duration = 0;
			Enabled = true;
			Location = OutputCacheLocation.Any;
			NoStore = false;
		}

		#endregion

		#region Properties

		public string CacheProfile {
			get { return _cacheProfile; }
			set { _cacheProfile = value; }
		}

		public int Duration {
			get { return _duration; }
			set { _duration = value; }
		}

		public bool Enabled {
			get { return _enabled; }
			set { _enabled = value; }
		}

		public OutputCacheLocation Location {
			get { return _location; }
			set { _location = value; }
		}

		public bool NoStore {
			get { return _noStore; }
			set { _noStore = value; }
		}

		public string SqlDependency {
			get { return _sqlDependency; }
			set { _sqlDependency = value; }
		}

		public string VaryByContentEncoding {
			get { return _varyByContentEncoding; }
			set { _varyByContentEncoding = value; }
		}
		
		public string VaryByControl {
			get { return _varByControl; }
			set { _varByControl = value; }
		}

		public string VaryByCustom {
			get { return _varByCustom; }
			set { _varByCustom = value; }
		}

		public string VaryByHeader {
			get { return _varByHeader; }
			set { _varByHeader = value; }
		}

		public string VaryByParam {
			get { return _varByParam; }
			set { _varByParam = value; }
		}

		#endregion

		#region Methods

		#endregion
	}
}

#endif
