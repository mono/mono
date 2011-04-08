//
// MoonlightHelper
//
// Authors:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005-2006, 2011 Novell, Inc.  http://www.novell.com
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

#if MOONLIGHT

using System;
using System.Reflection;
using System.Security;
using System.Threading;

namespace Mono {

	// since most Moonlight platform code assemblies requires mscorlib to have [InternalsVisibleTo] on them
	// this helper class allows some (of them) to avoid dependencies on Moonlight and let them be totally transparent
	// i.e. without any [SecuritySafeCritical] or [SecurityCritical] code
	static class MoonlightHelper {

		static readonly PropertyInfo dispatcher_main_property;
		static readonly MethodInfo dispatcher_begin_invoke_method;

		static MoonlightHelper ()
		{
			Type dispatcher_type = Type.GetType ("System.Windows.Threading.Dispatcher, System.Windows, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e", true);

			dispatcher_main_property = dispatcher_type.GetProperty ("Main", BindingFlags.NonPublic | BindingFlags.Static);
			if (dispatcher_main_property == null)
				throw new SecurityException ("System.Windows.Threading.Dispatcher.Main not found");

			dispatcher_begin_invoke_method = dispatcher_type.GetMethod ("BeginInvoke", new Type [] { typeof (Action) });
			if (dispatcher_begin_invoke_method == null)
				throw new SecurityException ("System.Windows.Threading.Dispatcher.BeginInvoke not found");
		}

		// System.ServiceModel.dll can access mscorlib.dll internals (so we let it do the dirty work)
		static internal void RunOnMainThread (SendOrPostCallback callback, EventArgs args)
		{
			object dispatcher = dispatcher_main_property.GetValue (null, null);
			if (dispatcher == null) {
				callback (args);
				return;
			}

			Action a = delegate {
				try {
					callback (args); 
				} catch (Exception ex) {
					throw;
				}
			};
			dispatcher_begin_invoke_method.Invoke (dispatcher, new object [] { a });
		}
	}
}

#endif
