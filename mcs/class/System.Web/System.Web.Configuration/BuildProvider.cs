//
// System.Web.Configuration.BuildProvider
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// Copyright (c) 2005 Novell, Inc (http://www.novell.com)
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
using System.Configuration;

namespace System.Web.Configuration
{
	public class BuildProvider : ConfigurationElement {
		string extension;
		string type;
		BuildProviderAppliesTo appliesTo;

		public BuildProvider ()
		{
		}

		public BuildProvider (string extension, string type, BuildProviderAppliesTo appliesTo)
		{
			this.extension = extension;
			this.type = type;
			this.appliesTo = appliesTo;
		}

		public string Extension {
			get { return extension; }
			set { extension = value; }
		}

		public string Type {
			get { return type; }
			set { type = value; }
		}

		public BuildProviderAppliesTo AppliesTo {
			get { return appliesTo; }
			set { appliesTo = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get {
				return base.Properties;
			}
		}

		public override bool Equals (object provider)
		{
			if (!(provider is BuildProvider))
				return false;

			BuildProvider p = (BuildProvider) provider;
			return (extension == p.extension && type == p.type && appliesTo == p.appliesTo);
		}

		public override int GetHashCode ()
		{
			return (extension.GetHashCode () ^ (int) appliesTo + type.GetHashCode ());
		}
	}
	
}
#endif // NET_2_0

