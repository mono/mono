//
// System.Drawing.Imaging.EncoderParameters.cs
//
// Author: 
//	Ravindra (rkumar@novell.com)
//  Vladimir Vukicevic (vladimir@pobox.com)
//
// (C) 2004 Novell, Inc.  http://www.novell.com
//

using System;
using System.Runtime.InteropServices;

namespace System.Drawing.Imaging 
{
	public sealed class EncoderParameters : IDisposable
	{
		private EncoderParameter[] parameters;

		public EncoderParameters () {
			parameters = new EncoderParameter[1];
		}

		public EncoderParameters (int count) {
			parameters = new EncoderParameter[count];
		}

		public EncoderParameter[] Param {
			get {
				return parameters;
			}

			set {
				parameters = value;
			}
		}

		public void Dispose () {
			// Nothing
		}

		internal IntPtr ToNativePtr () {
			IntPtr result;
			IntPtr ptr;

			// 4 is the initial int32 "count" value
			result = Marshal.AllocHGlobal (4 + parameters.Length * EncoderParameter.NativeSize());

			ptr = result;
			Marshal.WriteInt32 (ptr, parameters.Length);

			ptr = (IntPtr) ((int) ptr + 4);
			for (int i = 0; i < parameters.Length; i++) {
				parameters[i].ToNativePtr (ptr);
				ptr = (IntPtr) ((int) ptr + EncoderParameter.NativeSize());
			}

			return result;
		}

		/* The IntPtr passed in here is a blob returned from
		 * GdipImageGetEncoderParameterList.  Its internal pointers
		 * (i.e. the Value pointers in the EncoderParameter entries)
		 * point to areas within this block of memeory; this means
		 * that we need to free it as a whole, and also means that
		 * we can't Marshal.PtrToStruct our way to victory.
		 */
		internal static EncoderParameters FromNativePtr (IntPtr epPtr) {
			if (epPtr == IntPtr.Zero)
				return null;

			IntPtr ptr = epPtr;

			int count = Marshal.ReadInt32 (ptr);
			ptr = (IntPtr) ((int) ptr + 4);

			if (count == 0)
				return null;

			EncoderParameters result = new EncoderParameters (count);

			for (int i = 0; i < count; i++) {
				result.parameters[i] = EncoderParameter.FromNativePtr (ptr);
				ptr = (IntPtr) ((int) ptr + EncoderParameter.NativeSize());
			}

			return result;
		}
	}
}
