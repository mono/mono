
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
/**
 * Namespace: System.Web.UI.WebControls
 * Class:     AdCreatedEventArgs
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Implementation: yes
 * Contact: <gvaish@iitk.ac.in>
 * Status:  100%
 * 
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Collections;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
#if !NET_2_0
	sealed
#endif
	public class AdCreatedEventArgs: EventArgs
	{

		private IDictionary adProperties;
		private string      alternateText;
		private string      imageUrl;
		private string      navigateUrl;

		public AdCreatedEventArgs(IDictionary adProperties): base()
		{
			Initialize();
			this.adProperties = adProperties;
			if(adProperties!=null)
			{
				imageUrl = (string)adProperties["ImageUrl"];
				navigateUrl = (string)adProperties["NavigateUrl"];
				alternateText = (string)adProperties["AlternateText"];
			}
		}

		private void Initialize()
		{
			alternateText = string.Empty;
			imageUrl      = string.Empty;
			navigateUrl   = string.Empty;
		}

		public IDictionary AdProperties
		{
			get
			{
				return adProperties;
			}
		}
		
		public string AlternateText
		{
			get
			{
				return alternateText;
			}
			set
			{
				alternateText = value;
			}
		}
		
		public string ImageUrl
		{
			get
			{
				return imageUrl;
			}
			set
			{
				imageUrl = value;
			}
		}
		
		public string NavigateUrl
		{
			get
			{
				return navigateUrl;
			}
			set
			{
				navigateUrl = value;
			}
		}
	}
}
