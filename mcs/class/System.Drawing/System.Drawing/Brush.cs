//
// System.Drawing.Brush.cs
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Ravindra (rkumar@novell.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) Novell, Inc.  Http://www.novell.com
//

using System;
using System.Drawing;

namespace System.Drawing {

	public abstract class Brush : MarshalByRefObject, ICloneable, IDisposable {

		internal IntPtr nativeObject;
		abstract public object Clone ();

                internal Brush ()
                { }
        
		internal Brush (IntPtr ptr)
		{
                        nativeObject = ptr;
		}
		
		internal IntPtr NativeObject{
			get{
				return nativeObject;
			}
			set	{
				nativeObject = value;
			}
		}
	

                internal Brush CreateBrush (IntPtr brush, System.Drawing.BrushType type)
                {
                        switch (type) {

                        case BrushType.BrushTypeSolidColor:
                                return new SolidBrush (brush);

                        default:
                                throw new NotImplementedException ();
                        }
                }

		public void Dispose ()
		{
			Dispose (true);
			System.GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			// Nothing for now.
		}

		~Brush ()
		{
			Dispose (false);
		}

		internal Exception GetException (Status status)
		{
			String message;

			switch (status) {
				// TODO: Test and add more status code mappings here
				case Status.GenericError:
					message = String.Format ("Generic Error.");
					return new Exception (message);

				case Status.InvalidParameter:
					message = String.Format ("Invalid Parameter.");
					return new ArgumentException (message);

				case Status.OutOfMemory:
					message = String.Format ("Out of memory.");
					return new OutOfMemoryException (message);

				case Status.ObjectBusy:
					message = String.Format ("Object busy.");
					return new MemberAccessException (message);

				case Status.InsufficientBuffer:
					message = String.Format ("Insufficient buffer.");
					return new IO.InternalBufferOverflowException (message);

				case Status.PropertyNotSupported:
					message = String.Format ("Property not supported.");
					return new NotSupportedException (message);

				case Status.FileNotFound:
					message = String.Format ("File not found.");
					return new IO.FileNotFoundException (message);

				case Status.AccessDenied:
					message = String.Format ("Access denied.");
					return new UnauthorizedAccessException (message);

				case Status.UnknownImageFormat:
					message = String.Format ("Unknown image format.");
					return new NotSupportedException (message);

				case Status.NotImplemented:
					message = String.Format ("Feature not implemented.");
					return new NotImplementedException (message);

				default:
					return new Exception ("Unknown Error.");
			}
		}
	}
}

