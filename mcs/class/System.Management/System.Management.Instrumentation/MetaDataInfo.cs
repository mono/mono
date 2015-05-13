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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Management.Instrumentation
{
	internal class MetaDataInfo : IDisposable
	{
		private IMetaDataImportInternalOnly importInterface;

		private string name;

		private Guid mvid;

		public Guid Mvid
		{
			get
			{
				this.InitNameAndMvid();
				return this.mvid;
			}
		}

		public string Name
		{
			get
			{
				this.InitNameAndMvid();
				return this.name;
			}
		}

		public MetaDataInfo(Assembly assembly) : this(assembly.Location)
		{
		}

		public MetaDataInfo(string assemblyName)
		{
			Guid guid = new Guid(((GuidAttribute)Attribute.GetCustomAttribute(typeof(IMetaDataImportInternalOnly), typeof(GuidAttribute), false)).Value);
			IMetaDataDispenser corMetaDataDispenser = (IMetaDataDispenser)(new CorMetaDataDispenser());
			this.importInterface = (IMetaDataImportInternalOnly)corMetaDataDispenser.OpenScope(assemblyName, 0, ref guid);
			Marshal.ReleaseComObject(corMetaDataDispenser);
		}

		public void Dispose()
		{
			if (this.importInterface == null)
			{
				Marshal.ReleaseComObject(this.importInterface);
			}
			this.importInterface = null;
			GC.SuppressFinalize(this);
		}

		~MetaDataInfo()
		{
			try
			{
				this.Dispose();
			}
			finally
			{
				//this.Finalize();
			}
		}

		public static Guid GetMvid(Assembly assembly)
		{
			Guid mvid;
			using (MetaDataInfo metaDataInfo = new MetaDataInfo(assembly))
			{
				mvid = metaDataInfo.Mvid;
			}
			return mvid;
		}

		private void InitNameAndMvid()
		{
			int num = 0;
			if (this.name == null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Capacity = 0;
				this.importInterface.GetScopeProps(stringBuilder, stringBuilder.Capacity, out num, out this.mvid);
				stringBuilder.Capacity = num;
				this.importInterface.GetScopeProps(stringBuilder, stringBuilder.Capacity, out num, out this.mvid);
				this.name = stringBuilder.ToString();
			}
		}
	}
}