//
// System.Net.NetworkInformation.NetworkInterfaceType
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
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
namespace System.Net.NetworkInformation {
	public enum NetworkInterfaceType {
		Unknown = 1,
		Ethernet = 6,
		TokenRing = 9,
		Fddi = 15,
		BasicIsdn = 20,
		PrimaryIsdn = 21,
		Ppp = 23,
		Loopback = 24,
		Ethernet3Megabit = 26,
		Slip = 28,
		Atm = 37,
		GenericModem = 48,
		FastEthernetT = 62,
		Isdn = 63,
		FastEthernetFx = 69,
		Wireless80211 = 71,
		AsymmetricDsl = 94,
		RateAdaptDsl = 95,
		SymmetricDsl = 96,
		VeryHighSpeedDsl = 97,
		IPOverAtm = 114,
		GigabitEthernet = 117,
		Tunnel = 131,
		MultiRateSymmetricDsl = 143,
		HighPerformanceSerialBus = 144
	}
}

