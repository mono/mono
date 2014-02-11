//
// NtlmSettings.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Net;
using System.Reflection;

namespace Mono.Security.Protocol.Ntlm {

	/*
	 * On Windows, this is controlled by a registry setting
	 * (http://msdn.microsoft.com/en-us/library/ms814176.aspx)
	 *
	 * This can be configured by setting the static
	 * NtlmSettings.DefaultAuthLevel property, the default value
	 * is LM_and_NTLM_and_try_NTLMv2_Session.
	 */

	#if INSIDE_SYSTEM
	internal
	#else
	public
	#endif
	static class NtlmSettings {

		static NtlmAuthLevel defaultAuthLevel = NtlmAuthLevel.LM_and_NTLM_and_try_NTLMv2_Session;

		static FieldInfo GetDefaultAuthLevelField ()
		{
			#if INSIDE_SYSTEM
			return null;
			#else
			var type = typeof (HttpWebRequest).Assembly.GetType ("Mono.Security.Protocol.Ntlm.NtlmSettings", false);
			if (type == null)
				return null;
			return type.GetField ("defaultAuthLevel", BindingFlags.Static | BindingFlags.NonPublic);
			#endif
		}

		#if INSIDE_SYSTEM
		internal
		#else
		public
		#endif
		static NtlmAuthLevel DefaultAuthLevel {
			get {
				var field = GetDefaultAuthLevelField ();
				if (field != null)
					return (NtlmAuthLevel)field.GetValue (null);
				else
					return defaultAuthLevel;
			}

			set {
				var field = GetDefaultAuthLevelField ();
				if (field != null)
					field.SetValue (null, value);
				else
					defaultAuthLevel = value;
			}
		}
	}
}
