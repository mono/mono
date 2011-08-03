//
// System.Security.Cryptography.CngUIPolicy
//
// Copyright (C) 2011 Juho Vähä-Herttua
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

using System;

namespace System.Security.Cryptography {

	// note: CNG stands for "Cryptography API: Next Generation"

	public sealed class CngUIPolicy {
		private CngUIProtectionLevels level;
		private string name;
		private string desc;
		private string context;
		private string title;

		public CngUIPolicy (CngUIProtectionLevels protectionLevel)
			: this (protectionLevel, null)
		{
		}

		public CngUIPolicy (CngUIProtectionLevels protectionLevel,
		                    string friendlyName)
			: this (protectionLevel, friendlyName, null)
		{
		}

		public CngUIPolicy (CngUIProtectionLevels protectionLevel,
		                    string friendlyName,
		                    string description)
			: this (protectionLevel, friendlyName, description, null)
		{
		}

		public CngUIPolicy (CngUIProtectionLevels protectionLevel,
		                    string friendlyName,
		                    string description,
		                    string useContext)
			: this (protectionLevel, friendlyName, description, useContext, null)
		{
		}

		public CngUIPolicy (CngUIProtectionLevels protectionLevel,
		                    string friendlyName,
		                    string description,
		                    string useContext,
		                    string creationTitle)
		{
			level = protectionLevel;
			name = friendlyName;
			desc = description;
			context = useContext;
			title = creationTitle;
		}

		public CngUIProtectionLevels ProtectionLevel {
			get { return level; }
		}

		public string FriendlyName {
			get { return name; }
		}

		public string Description {
			get { return desc; }
		}

		public string UseContext {
			get { return context; }
		}

		public string CreationTitle {
			get { return title; }
		}
	}
}
