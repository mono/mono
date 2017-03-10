//
// System.Windows.Forms.Design.AxParameterData.cs
//
// Author:
//   Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
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
//		private string typeName;
	}
}
