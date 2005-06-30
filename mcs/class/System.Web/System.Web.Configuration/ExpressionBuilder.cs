//
// System.Web.Configuration.ExpressionBuilder
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
using System;
using System.Collections;
using System.Configuration;

namespace System.Web.Configuration
{
	public sealed class ExpressionBuilder : ConfigurationElement
	{
		static ConfigurationPropertyCollection props;
		static ConfigurationProperty expressionPrefix;
		static ConfigurationProperty type;

		static ExpressionBuilder ()
		{
			ConfigurationPropertyFlags flags = ConfigurationPropertyFlags.Required | ConfigurationPropertyFlags.IsKey;
			type = new NonEmptyStringConfigurationProperty ("type", "", flags);
			flags = ConfigurationPropertyFlags.Required;
			expressionPrefix = new NonEmptyStringConfigurationProperty ("expressionPrefix", "", flags);

			props = new ConfigurationPropertyCollection ();
			props.Add (type);
			props.Add (expressionPrefix);
		}

		public string ExpressionPrefix {
			get { return (string) this [expressionPrefix]; }
			set { this [expressionPrefix] = value; }
		}

		public string Type {
			get { return (string) this [type]; }
			set { this [type] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return props; }
		}
	}
}
#endif // NET_2_0

