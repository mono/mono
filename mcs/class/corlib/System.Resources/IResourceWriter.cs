//
// System.Resources.IResourceWriter.cs
//
// Author:
//	Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc. http://www.ximian.com
//

using System;

namespace System.Resources {

	   public interface IResourceWriter : IDisposable {
			 void AddResource (string name, byte[] value);
			 void AddResource (string name, object value);
			 void AddResource (string name, string value);

			 void Close();

			 void Generate();
	   }
}
