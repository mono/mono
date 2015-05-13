//
// System.Management.AuthenticationLevel
//
// Author:
//	Bruno Lauze     (brunolauze@msn.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2015 Microsoft (http://www.microsoft.com)
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
using System;
using System.Globalization;
using System.Resources;
using System.Threading;

namespace System.Management
{
	internal sealed class SR
	{
		internal const string ASSEMBLY_NOT_REGISTERED = "ASSEMBLY_NOT_REGISTERED";

		internal const string FAILED_TO_BUILD_GENERATED_ASSEMBLY = "FAILED_TO_BUILD_GENERATED_ASSEMBLY";

		internal const string COMMENT_SHOULDSERIALIZE = "COMMENT_SHOULDSERIALIZE";

		internal const string COMMENT_ISPROPNULL = "COMMENT_ISPROPNULL";

		internal const string COMMENT_RESETPROP = "COMMENT_RESETPROP";

		internal const string COMMENT_DATECONVFUNC = "COMMENT_DATECONVFUNC";

		internal const string COMMENT_TIMESPANCONVFUNC = "COMMENT_TIMESPANCONVFUNC";

		internal const string COMMENT_ATTRIBPROP = "COMMENT_ATTRIBPROP";

		internal const string COMMENT_GETINSTANCES = "COMMENT_GETINSTANCES";

		internal const string COMMENT_CLASSBEGIN = "COMMENT_CLASSBEGIN";

		internal const string COMMENT_PRIVAUTOCOMMIT = "COMMENT_PRIVAUTOCOMMIT";

		internal const string COMMENT_CONSTRUCTORS = "COMMENT_CONSTRUCTORS";

		internal const string COMMENT_ORIGNAMESPACE = "COMMENT_ORIGNAMESPACE";

		internal const string COMMENT_CLASSNAME = "COMMENT_CLASSNAME";

		internal const string COMMENT_SYSOBJECT = "COMMENT_SYSOBJECT";

		internal const string COMMENT_LATEBOUNDOBJ = "COMMENT_LATEBOUNDOBJ";

		internal const string COMMENT_MGMTSCOPE = "COMMENT_MGMTSCOPE";

		internal const string COMMENT_AUTOCOMMITPROP = "COMMENT_AUTOCOMMITPROP";

		internal const string COMMENT_MGMTPATH = "COMMENT_MGMTPATH";

		internal const string COMMENT_PROPTYPECONVERTER = "COMMENT_PROPTYPECONVERTER";

		internal const string COMMENT_SYSPROPCLASS = "COMMENT_SYSPROPCLASS";

		internal const string COMMENT_ENUMIMPL = "COMMENT_ENUMIMPL";

		internal const string COMMENT_LATEBOUNDPROP = "COMMENT_LATEBOUNDPROP";

		internal const string COMMENT_CREATEDCLASS = "COMMENT_CREATEDCLASS";

		internal const string COMMENT_CREATEDWMINAMESPACE = "COMMENT_CREATEDWMINAMESPACE";

		internal const string COMMENT_STATICMANAGEMENTSCOPE = "COMMENT_STATICMANAGEMENTSCOPE";

		internal const string COMMENT_STATICSCOPEPROPERTY = "COMMENT_STATICSCOPEPROPERTY";

		internal const string COMMENT_TODATETIME = "COMMENT_TODATETIME";

		internal const string COMMENT_TODMTFDATETIME = "COMMENT_TODMTFDATETIME";

		internal const string COMMENT_TODMTFTIMEINTERVAL = "COMMENT_TODMTFTIMEINTERVAL";

		internal const string COMMENT_TOTIMESPAN = "COMMENT_TOTIMESPAN";

		internal const string COMMENT_EMBEDDEDOBJ = "COMMENT_EMBEDDEDOBJ";

		internal const string COMMENT_CURRENTOBJ = "COMMENT_CURRENTOBJ";

		internal const string COMMENT_FLAGFOREMBEDDED = "COMMENT_FLAGFOREMBEDDED";

		internal const string EMBEDDED_COMMENT1 = "EMBEDDED_COMMENT1";

		internal const string EMBEDDED_COMMENT2 = "EMBEDDED_COMMENT2";

		internal const string EMBEDDED_COMMENT3 = "EMBEDDED_COMMENT3";

		internal const string EMBEDDED_COMMENT4 = "EMBEDDED_COMMENT4";

		internal const string EMBEDDED_COMMENT5 = "EMBEDDED_COMMENT5";

		internal const string EMBEDDED_COMMENT6 = "EMBEDDED_COMMENT6";

		internal const string EMBEDDED_COMMENT7 = "EMBEDDED_COMMENT7";

		internal const string EMBEDED_VB_CODESAMP4 = "EMBEDED_VB_CODESAMP4";

		internal const string EMBEDED_VB_CODESAMP5 = "EMBEDED_VB_CODESAMP5";

		internal const string EMBEDDED_COMMENT8 = "EMBEDDED_COMMENT8";

		internal const string EMBEDED_CS_CODESAMP4 = "EMBEDED_CS_CODESAMP4";

		internal const string EMBEDED_CS_CODESAMP5 = "EMBEDED_CS_CODESAMP5";

		internal const string CLASSNOT_FOUND_EXCEPT = "CLASSNOT_FOUND_EXCEPT";

		internal const string NULLFILEPATH_EXCEPT = "NULLFILEPATH_EXCEPT";

		internal const string EMPTY_FILEPATH_EXCEPT = "EMPTY_FILEPATH_EXCEPT";

		internal const string NAMESPACE_NOTINIT_EXCEPT = "NAMESPACE_NOTINIT_EXCEPT";

		internal const string CLASSNAME_NOTINIT_EXCEPT = "CLASSNAME_NOTINIT_EXCEPT";

		internal const string UNABLE_TOCREATE_GEN_EXCEPT = "UNABLE_TOCREATE_GEN_EXCEPT";

		internal const string FORCE_UPDATE = "FORCE_UPDATE";

		internal const string FILETOWRITE_MOF = "FILETOWRITE_MOF";

		internal const string WMISCHEMA_INSTALLATIONSTART = "WMISCHEMA_INSTALLATIONSTART";

		internal const string REGESTRING_ASSEMBLY = "REGESTRING_ASSEMBLY";

		internal const string WMISCHEMA_INSTALLATIONEND = "WMISCHEMA_INSTALLATIONEND";

		internal const string MOFFILE_GENERATING = "MOFFILE_GENERATING";

		internal const string UNSUPPORTEDMEMBER_EXCEPT = "UNSUPPORTEDMEMBER_EXCEPT";

		internal const string CLASSINST_EXCEPT = "CLASSINST_EXCEPT";

		internal const string MEMBERCONFLILCT_EXCEPT = "MEMBERCONFLILCT_EXCEPT";

		internal const string NAMESPACE_ENSURE = "NAMESPACE_ENSURE";

		internal const string CLASS_ENSURE = "CLASS_ENSURE";

		internal const string CLASS_ENSURECREATE = "CLASS_ENSURECREATE";

		internal const string CLASS_NOTREPLACED_EXCEPT = "CLASS_NOTREPLACED_EXCEPT";

		internal const string NONCLS_COMPLIANT_EXCEPTION = "NONCLS_COMPLIANT_EXCEPTION";

		internal const string INVALID_QUERY = "INVALID_QUERY";

		internal const string INVALID_QUERY_DUP_TOKEN = "INVALID_QUERY_DUP_TOKEN";

		internal const string INVALID_QUERY_NULL_TOKEN = "INVALID_QUERY_NULL_TOKEN";

		internal const string WORKER_THREAD_WAKEUP_FAILED = "WORKER_THREAD_WAKEUP_FAILED";

		private static SR loader;

		private ResourceManager resources;

		private static CultureInfo Culture
		{
			get
			{
				return null;
			}
		}

		public static ResourceManager Resources
		{
			get
			{
				return SR.GetLoader().resources;
			}
		}

		static SR()
		{
		}

		internal SR()
		{
			this.resources = new ResourceManager("System.Management", this.GetType().Assembly);
		}

		private static SR GetLoader()
		{
			if (SR.loader == null)
			{
				SR sR = new SR();
				Interlocked.CompareExchange<SR>(ref SR.loader, sR, null);
			}
			return SR.loader;
		}

		public static object GetObject(string name)
		{
			SR loader = SR.GetLoader();
			if (loader != null)
			{
				return loader.resources.GetObject(name, SR.Culture);
			}
			else
			{
				return null;
			}
		}

		public static string GetString(string name, object[] args)
		{
			SR loader = SR.GetLoader();
			if (loader != null)
			{
				string str = loader.resources.GetString(name, SR.Culture);
				if (args == null || (int)args.Length <= 0)
				{
					return str;
				}
				else
				{
					for (int i = 0; i < (int)args.Length; i++)
					{
						string str1 = args[i] as string;
						if (str1 != null && str1.Length > 0x400)
						{
							args[i] = string.Concat(str1.Substring(0, 0x3fd), "...");
						}
					}
					return string.Format(CultureInfo.CurrentCulture, str, args);
				}
			}
			else
			{
				return null;
			}
		}

		public static string GetString(string name)
		{
			SR loader = SR.GetLoader();
			if (loader != null)
			{
				return loader.resources.GetString(name, SR.Culture);
			}
			else
			{
				return null;
			}
		}

		public static string GetString(string name, out bool usedFallback)
		{
			usedFallback = false;
			return SR.GetString(name);
		}
	}
}