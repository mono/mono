//
// System.Web.UI.PageParserFilter.cs
//
// Authors:
//     Arina Itkes (arinai@mainsoft.com)
//
// (C) 2007 Mainsoft Co. (http://www.mainsoft.com)
//
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

#if NET_2_0
using System.Collections;

namespace System.Web.UI
{
	public abstract class PageParserFilter
	{
		protected PageParserFilter () {
			throw new NotImplementedException ();
		}
		public virtual bool AllowCode {
			get {
				throw new NotImplementedException ();
			}
		}
		public virtual int NumberOfControlsAllowed {
			get {
				throw new NotImplementedException ();
			}
		}
		public virtual int NumberOfDirectDependenciesAllowed {
			get {
				throw new NotImplementedException ();
			}
		}
		public virtual int TotalNumberOfDependenciesAllowed {
			get {
				throw new NotImplementedException ();
			}
		}
		protected string VirtualPath {
			get {
				throw new NotImplementedException ();
			}
		}
		public virtual bool AllowBaseType (
			Type baseType
		) {
			throw new NotImplementedException ();
		}
		public virtual bool AllowControl (Type controlType, ControlBuilder builder) {
			throw new NotImplementedException ();
		}
		public virtual bool AllowServerSideInclude (string includeVirtualPath) {
			throw new NotImplementedException ();
		}
		public virtual bool AllowVirtualReference (string referenceVirtualPath, VirtualReferenceType referenceType) {
			throw new NotImplementedException ();
		}
		public virtual CompilationMode GetCompilationMode (CompilationMode current) {
			throw new NotImplementedException ();
		}
		public virtual void ParseComplete (ControlBuilder rootBuilder) {
			throw new NotImplementedException ();
		}
		public virtual void PreprocessDirective (string directiveName, IDictionary attributes) {
			throw new NotImplementedException ();
		}
	}
}
#endif
