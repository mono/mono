//
// System.Drawing.Imaging.EncoderParameters.cs
//
// Author: 
//	Ravindra (rkumar@novell.com)
//
// (C) 2004 Novell, Inc.  http://www.novell.com
//

using System;

namespace System.Drawing.Imaging 
{
	public sealed class EncoderParameters : IDisposable
	{
		EncoderParameter[] parameters;

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
	}
}
