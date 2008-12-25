#if NET_2_0
/*
Used to determine Browser Capabilities by the Browsers UserAgent String and related
Browser supplied Headers.
Copyright (C) 2002-Present  Owen Brady (Ocean at owenbrady dot net) 
and Dean Brettle (dean at brettle dot com)

Permission is hereby granted, free of charge, to any person obtaining a copy 
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights 
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all 
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
namespace System.Web.Configuration
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Text;
	using System.Reflection;
	using System.IO;

	internal class CapabilitiesResult : System.Web.HttpBrowserCapabilities
	{
		/// <summary>
		/// Initializes a new instance of the Result class.
		/// </summary>
		/// <param name="items">
		/// This is the data which this class will be handle request made though this class.
		/// </param>
		internal CapabilitiesResult(System.Collections.IDictionary items)
			: base()
		{
			base.Capabilities = items;
			Capabilities ["browsers"] = new ArrayList ();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		internal void AddCapabilities(string name, string value)
		{
			this.Capabilities[name] = value;
		}

		internal void AddMatchingBrowserId (string id)
		{
			ArrayList al = Capabilities ["browsers"] as ArrayList;
			if (al != null && !al.Contains (id))
				al.Add (id);
		}
		
		internal virtual string Replace(string item)
		{
			if (item.IndexOf('$') > -1)
			{
				//nasty hack to convert regular expression replacement text into  Capability item
				//which we can use to replace with the actual values they are looking for.
				System.Text.RegularExpressions.MatchCollection regxmatch;
				regxmatch = System.Text.RegularExpressions.Regex.Matches(item, @"\$\{(?'Capability'\w*)\}");
				if (regxmatch.Count == 0)
				{
					return item;
				}
				for (int i = 0;i <= regxmatch.Count - 1;i++)
				{
					if (regxmatch[i].Success == true)
					{
						string c = regxmatch[i].Result("${Capability}");
						item = item.Replace("${" + c + "}", this[c]);
					}
				}
			}
			if (item.IndexOf('%') > -1)
			{
				//nasty hack to convert regular expression replacement text into  Capability item
				//which we can use to replace with the actual values they are looking for.
				System.Text.RegularExpressions.MatchCollection regxmatch;
				regxmatch = System.Text.RegularExpressions.Regex.Matches(item, @"\%\{(?'Capability'\w*)\}");
				if (regxmatch.Count == 0)
				{
					return item;
				}
				for (int i = 0;i <= regxmatch.Count - 1;i++)
				{
					if (regxmatch[i].Success == true)
					{
						string c = regxmatch[i].Result("${Capability}");
						item = item.Replace("%{" + c + "}", this[c]);
					}
				}
			}
			return item;
		}
		/// <summary>
		/// Gets the keys returned from processing.
		/// </summary>
		public System.Collections.Specialized.StringCollection Keys
		{
			get
			{
				string[] a = new string[this.Capabilities.Keys.Count];
				this.Capabilities.Keys.CopyTo(a, 0);
				System.Array.Sort(a);
				System.Collections.Specialized.StringCollection l;
				l = new System.Collections.Specialized.StringCollection();
				l.AddRange(a);
				return l;
			}
		}
		public string UserAgent
		{
			get
			{
				return this[""];
			}
		}
	}
}
#endif
