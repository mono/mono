//
// System.Net.NetworkInformation.NetworkChange
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (c) 2006, 2009 Novell, Inc. (http://www.novell.com)
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
#if NET_2_1
using System.Runtime.InteropServices;

namespace System.Net.NetworkInformation {
	public abstract class NetworkChange {
		internal delegate void NetworkStateChangedCallback (IntPtr sender, IntPtr data);
		static NetworkChange ()
		{
			state_changed_callback = new NetworkStateChangedCallback (StateChangedCallback);
			_moonlight_cbinding_moon_network_service_set_network_state_changed_callback (_moonlight_cbinding_runtime_get_network_service(),
										 state_changed_callback,
										 IntPtr.Zero);
		}

		static NetworkStateChangedCallback state_changed_callback;

		static void StateChangedCallback (IntPtr sender, IntPtr data)
		{
			try {
				NetworkAddressChangedEventHandler h = NetworkAddressChanged;
				if (h != null)
					h (null, EventArgs.Empty);
			} catch (Exception ex) {
				try {
					Console.WriteLine ("Unhandled exception: {0}", ex);
				} catch {
					// Ignore
				}
			}
		}

		protected NetworkChange ()
		{
		}

		public static event NetworkAddressChangedEventHandler NetworkAddressChanged;

		[DllImport ("moon")]
		internal extern static IntPtr _moonlight_cbinding_runtime_get_network_service ();

		[DllImport ("moon")]
		internal extern static void _moonlight_cbinding_moon_network_service_set_network_state_changed_callback (IntPtr service,
												     NetworkStateChangedCallback handler, IntPtr data);

		[DllImport ("moon")]
		internal extern static bool _moonlight_cbinding_moon_network_service_get_is_network_available (IntPtr service);
	}
}
#endif

