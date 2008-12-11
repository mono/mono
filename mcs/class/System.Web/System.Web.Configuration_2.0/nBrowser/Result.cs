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

namespace System.Web.Configuration.nBrowser
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	internal class Result : System.Web.Configuration.CapabilitiesResult
	{
		private System.Collections.Generic.Dictionary<Type, Type> AdapterTypeMap;
		private System.Collections.Specialized.StringCollection Track;
		internal Type MarkupTextWriter;

		internal Result(System.Collections.IDictionary items)
			: base(items)
		{
			AdapterTypeMap = new System.Collections.Generic.Dictionary<Type, Type>();
			Track = new System.Collections.Specialized.StringCollection();
			MarkupTextWriter = typeof (System.Web.UI.HtmlTextWriter);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="track"></param>
		internal void AddTrack(string track)
		{
			Track.Add(track);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="controlTypeName"></param>
		/// <param name="adapterTypeName"></param>
		internal void AddAdapter(Type controlType, Type adapterType)
		{
			AdapterTypeMap[controlType] = adapterType;
		}
		
		/// <summary>
		/// 
		/// </summary>
		public System.Collections.Specialized.StringCollection Tracks
		{
			get
			{
				return Track;
			}
		}
		
		internal override Type GetTagWriter ()
		{
				return MarkupTextWriter;			
		}

		internal override System.Collections.IDictionary GetAdapters ()
		{
				return AdapterTypeMap;			
		}
	}
}
#endif
