//
// System.Windows.Forms.Design.AxParameterData.cs
//
// Author:
//   Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
//

using System.CodeDom;
using System.Reflection;

namespace System.Windows.Forms.Design
{
	public class AxParameterData
	{
		[MonoTODO]
		public AxParameterData (ParameterInfo info) : this (info, false)
		{
		}

		[MonoTODO]
		public AxParameterData (ParameterInfo info, bool ignoreByRefs)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public AxParameterData (string inname, string typeName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public AxParameterData (string inname, Type type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static AxParameterData[] Convert (ParameterInfo[] infos)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static AxParameterData[] Convert (ParameterInfo[] infos, bool ignoreByRefs)
		{
			throw new NotImplementedException ();
		}

		public FieldDirection Direction {
			get {
				if (this.IsOut)
					return FieldDirection.Out;

				if (this.IsByRef)
					return FieldDirection.Ref;

				return FieldDirection.In;
			}
		}
		public bool IsByRef {
			get {
				return isByRef;
			}
		}

		public bool IsIn {
			get {
				return isIn;
			}
		}

		public bool IsOptional {
			get {
				return isOptional;
			}
		}

		public bool IsOut {
			get {
				return isOut;
			}
		}

		public string Name {
			get {
				return name;
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		public Type ParameterType {
			get {
				return type;
			}
		}

		[MonoTODO]
		public string TypeName {
			get {
				throw new NotImplementedException ();
			}
		}

		private bool isByRef;
		private bool isIn;
		private bool isOptional;
		private bool isOut;
		private string name;
		private Type type;
		private string typeName;
	}
}
