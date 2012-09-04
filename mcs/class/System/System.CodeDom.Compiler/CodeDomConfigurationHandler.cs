//
// System.Configuration.CodeDomConfigurationHandler
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

#if CONFIGURATION_DEP
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace System.CodeDom.Compiler
{
	internal sealed class CodeDomConfigurationHandler: ConfigurationSection
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty compilersProp;
		static CompilerCollection default_compilers;

		static CodeDomConfigurationHandler ()
		{
			default_compilers = new CompilerCollection ();
			compilersProp = new ConfigurationProperty ("compilers", typeof (CompilerCollection), default_compilers);
			properties = new ConfigurationPropertyCollection ();
			properties.Add (compilersProp);
		}

		public CodeDomConfigurationHandler ()
		{
		}
		
		protected override void InitializeDefault ()
		{
			compilersProp = new ConfigurationProperty ("compilers", typeof (CompilerCollection), default_compilers);
		}
		
		[MonoTODO]
                protected override void PostDeserialize ()
                {
                        base.PostDeserialize ();
                }

                protected override object GetRuntimeObject ()
                {
                        return this;
                }

		[ConfigurationProperty ("compilers")]
                public CompilerCollection Compilers {
                        get { return (CompilerCollection) base [compilersProp]; }
                }

		public CompilerInfo[] CompilerInfos {
			get {
				CompilerCollection cc = (CompilerCollection)base [compilersProp];
				if (cc == null)
					return null;
				return cc.CompilerInfos;
			}
		}
		
		protected override ConfigurationPropertyCollection Properties {
                        get { return properties; }
                }
	}
}
#endif

