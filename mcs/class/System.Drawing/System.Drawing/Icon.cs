//
// System.Drawing.Icon.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc
//
using System;
using System.IO;
using System.Runtime.Serialization;

namespace System.Drawing
{

	[Serializable]
	public sealed class Icon : MarshalByRefObject, ISerializable, ICloneable, IDisposable
	{

		private Icon ()
		{
		}

		[MonoTODO ("Implement")]
		public Icon (Icon original, int width, int height)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		public Icon (Icon original, Size size)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		public Icon (Stream stream)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		public Icon (Stream stream, int width, int height)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		public Icon (string fileName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		public Icon (Type type, string resource)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
        	private Icon (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		public void Dispose ()
		{
		}

		[MonoTODO ("Implement")]
		public object Clone ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		public static Icon FromHandle (IntPtr handle)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		public void Save (Stream outputStream)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		public Bitmap ToBitmap ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		public IntPtr Handle {
			get {throw new NotImplementedException ();}
		}

		[MonoTODO ("Implement")]
		public int Height {
			get {throw new NotImplementedException ();}
		}

		[MonoTODO ("Implement")]
		public Size Size {
			get {throw new NotImplementedException ();}
		}

		[MonoTODO ("Implement")]
		public int Width {
			get {throw new NotImplementedException ();}
		}
	}
}
