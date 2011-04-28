// 
// DataServiceProviderMethods.cs
//  
// Author:
//       Marek Habersack <grendel@twistedcode.net>
// 
// Copyright (c) 2011 Novell, Inc
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
using System.Collections.Generic;
using System.Data.Services.Providers;
using System.Reflection;

namespace System.Data.Services.Providers
{
	public static class DataServiceProviderMethods
	{
		public static object GetValue (object value, ResourceProperty property)
		{
			// LAMESPEC: this method is not implemented in the .NET assembly
			throw new NotImplementedException ();
		}

		public static IEnumerable <T> GetSequenceValue<T>  (object value, ResourceProperty property)
		{
			// LAMESPEC: this method is not implemented in the .NET assembly
			throw new NotImplementedException ();
		}

		public static object Convert (object value, ResourceType type)
		{
			// LAMESPEC: this method is not implemented in the .NET assembly
			throw new NotImplementedException ();
		}

		public static bool TypeIs (object value, ResourceType type)
		{
			// LAMESPEC: this method is not implemented in the .NET assembly
			throw new NotImplementedException ();
		}

		public static int Compare (string left, string right)
		{
			return Comparer <string>.Default.Compare (left, right);
		}

		public static int Compare (bool left, bool right)
		{
			return Comparer <bool>.Default.Compare (left, right);
		}

		public static int Compare (bool? left, bool? right)
		{
			return Comparer <bool?>.Default.Compare (left, right);
		}	

		public static int Compare (Guid left, Guid right)
		{
			return Comparer <Guid>.Default.Compare (left, right);
		}

		public static int Compare (Guid? left, Guid? right)
		{
			return Comparer <Guid?>.Default.Compare (left, right);
		}
	}
}
