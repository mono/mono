//
// System.Resources.ResXResourceWriter.cs
//
// Author:
//	Duncan Mak <duncan@ximian.com>
//
// 2001 (C) Ximian, Inc. 	http://www.ximian.com
//

using System.IO;

namespace System.Resources
{
	public sealed class ResXResourceWriter : IResourceWriter
	{
		[MonoTODO]
		public ResXResourceWriter (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream is null");
			if (stream.CanWrite == false)
				throw new ArgumentException ("stream is not writable.");
		}
		
		[MonoTODO]
		public ResXResourceWriter (String fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName is null.");
		}
		
		[MonoTODO]
		public void AddResource (string name, byte[] value)
		{
			if (name == null || value == null)
				throw new ArgumentNullException ("Parameter is a null reference.");
		}
		
		[MonoTODO]
		public void AddResource (string name, object value)
		{			 
			if (name == null || value == null)
				throw new ArgumentNullException ("Parameter is a null reference.");
		}
		
		[MonoTODO]
		public void AddResource (string name, string value)
		{
			if (name == null || value == null)
				throw new ArgumentNullException ("Parameter is a null reference.");
		}

		[MonoTODO]
		public void Close () {}
		
		public void Dispose ()
		{
			Close();
		}

		[MonoTODO]
		public void Generate () {}
	}
}
