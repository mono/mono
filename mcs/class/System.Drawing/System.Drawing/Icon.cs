//
// System.Drawing.Icon.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System;
using System.Runtime.Serialization;

namespace System.Drawing
{
	/// <summary>
	/// Summary description for Icon.
	/// </summary>
	[Serializable]
	public class Icon : MarshalByRefObject, ISerializable, ICloneable, IDisposable
	{
		public Icon()
		{
			//
			// TODO: Add constructor logic here
			//
		}

        	private Icon (SerializationInfo info, StreamingContext context)
		{
		}

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
		}

		public void Dispose ()
		{
		}

		object ICloneable.Clone()
		{
			throw new NotImplementedException ();
		}
	}
}
