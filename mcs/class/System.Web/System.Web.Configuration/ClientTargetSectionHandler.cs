
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
/* System.Web.Configuration
 * Authors:
 *   Leen Toelen (toelen@hotmail.com)
 *  Copyright (C) 2001 Leen Toelen
*/
using System;
using System.Configuration;

namespace System.Web.Configuration
{
	/// <summary>
	/// Summary description for ClientTargetSectionHandler.
	/// </summary>
	class ClientTargetSectionHandler: NameValueSectionHandler
	{
		/// <summary>
		///		ClientTargetSectionHandler Constructor
		/// </summary>
		public ClientTargetSectionHandler(){}

		/// <summary>
		///		Gets the name of the key in the key-value pair.
		/// </summary>
		protected override string KeyAttributeName
		{
			get
			{
				return "alias";
			}
		}

		/// <summary>
		///		Gets the value for the key in the key-value pair.
		/// </summary>
		protected override string ValueAttributeName
		{
			get
			{
				return "userAgent";
			}
		}

	}
} //namespace System.Web.Configuration
