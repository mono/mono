// System.Reflection.ManifestResourceInfo
//
// Sean MacIsaac (macisaac@ximian.com)
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc. 2001

namespace System.Reflection
{
	public class ManifestResourceInfo
	{
		private Assembly _assembly;
		private string _filename;
		private ResourceLocation _location;

		internal ManifestResourceInfo ()
		{
		}

		internal ManifestResourceInfo (Assembly assembly, string filename, ResourceLocation location)
		{
			_assembly = assembly;
			_filename = filename;
			_location = location;
		}
		[MonoTODO]
		public virtual string FileName {
			get { return _filename; }
		}

		[MonoTODO]
		public virtual Assembly ReferencedAssembly {
			get { return _assembly; }
		}

		[MonoTODO]
		public virtual ResourceLocation ResourceLocation {
			get { return _location; }
		}
	}
}
