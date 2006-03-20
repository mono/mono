//
// System.Data.ProviderBase.DbStringManager
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//	Boris Kirzner <borisk@mainsoft.com>
//	
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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


using System.Resources;
using System.Reflection;

namespace System.Data.ProviderBase
{
	public class DbStringManager
	{
		#region Fields

		private readonly string _resourceName;
        private readonly ResourceManager _resourceManager;
		private readonly char[] _delimiters;

		#endregion // Fields

		#region Constructors

		public DbStringManager(string resourceName)
		{
			_resourceName = resourceName;
			_resourceManager = new ResourceManager(resourceName,Assembly.GetExecutingAssembly());
			_delimiters = new char[] {','};
		}        

		#endregion // Constructors

		#region Methods

        public string GetString(string key)
        {
			return _resourceManager.GetString(key);
        }
	
        public string GetString(string key, string defaultValue)
        {
            try {
                return GetString(key);
            }
            catch (Exception) {
                return defaultValue;
            }
        }
	
        public string[] GetStringArray(String key)
        {
            string tmp  = _resourceManager.GetString(key);
            string[] values = tmp.Split(_delimiters);
			
			for(int i=0; i < values.Length; i++) {
				values[i] = values[i].Trim();
			}
            return values;			
        }

		#endregion // Methods
    }
}
