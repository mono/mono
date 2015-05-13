//
// AssemblyRef
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

namespace System.Management
{
	/// <summary>
	/// Unix wbem method creator.
	/// </summary>
	internal static class UnixWbemMethodCreator
	{
		/// <summary>
		/// Creates the signature.
		/// </summary>
		/// <param name='info'>
		/// Info.
		/// </param>
		/// <param name='ppInSignature'>
		/// Pp in signature.
		/// </param>
		/// <param name='ppOutSignature'>
		/// Pp out signature.
		/// </param>
		public static void CreateSignature (UnixCimMethodInfo info, out IWbemClassObject_DoNotMarshal ppInSignature, out IWbemClassObject_DoNotMarshal ppOutSignature)
		{
			/*
			Type inType = null;
			Type outType = null; 
			if (string.IsNullOrEmpty (info.InSignatureType)) {
				inType = typeof(UNIX_MethodParameterClass);
			} else {
				inType = Type.GetType (info.InSignatureType, false, true);
			}

			if (string.IsNullOrEmpty (info.OutSignatureType)) {
				outType = typeof(UNIX_MethodParameterClass);
			} else {
				outType = Type.GetType (info.OutSignatureType, false, true);
			}

			var inClass = (UNIX_MethodParameterClass)WMIDatabaseFactory.GetHandler (inType).Get ((object)null);
			var outClass = (UNIX_MethodParameterClass)WMIDatabaseFactory.GetHandler (outType).Get ((object)null);
			
			if (info.InProperties != null) {
				foreach (var property in info.InProperties) {
					inClass.RegisterProperty (property);
				}
			}

			outClass.RegisterProperty (new UnixWbemPropertyInfo { Name = "ReturnValue", Type = CimType.UInt32, Flavor = 0  });

			if (info.OutProperties != null) 
			{
				foreach (var property in info.OutProperties) {
					outClass.RegisterProperty (property);
				}
			}

			ppInSignature = new UnixWbemClassObject(inClass);
			ppOutSignature = new UnixWbemClassObject(outClass);
			*/

			ppInSignature = null;
			ppOutSignature = null;
		}
	}
}
