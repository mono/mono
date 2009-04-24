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
		TemplateParser parser;

		protected PageParserFilter ()
		{
		}
		
		public virtual bool AllowCode {
			get { return false; }
		}

		[MonoTODO ("Need to implement support for this in the parser")]
		protected int Line {
			get { return parser.Location.BeginLine; }
		}

		public virtual int NumberOfControlsAllowed {
			get { return 0; }
		}
		
		public virtual int NumberOfDirectDependenciesAllowed {
			get { return 0; }
		}
		
		public virtual int TotalNumberOfDependenciesAllowed {
			get { return 0; }
		}
		
		protected string VirtualPath {
			get { return parser.VirtualPath.Absolute; }
		}

		protected void AddControl (Type type, IDictionary attributes)
		{
			if (parser == null)
				return;

			parser.AddControl (type, attributes);
		}
		
		public virtual bool AllowBaseType (Type baseType)
		{
			return false;
		}
		
		public virtual bool AllowControl (Type controlType, ControlBuilder builder)
		{
			return false;
		}
		
		public virtual bool AllowServerSideInclude (string includeVirtualPath)
		{
			return false;
		}
		
		public virtual bool AllowVirtualReference (string referenceVirtualPath, VirtualReferenceType referenceType)
		{
			return false;
		}
		
		public virtual CompilationMode GetCompilationMode (CompilationMode current)
		{
			return current;
		}

		public virtual Type GetNoCompileUserControlType ()
		{
			return null;
		}

		// LAMEDOC:
		// http://msdn.microsoft.com/en-us/library/system.web.ui.pageparserfilter.initialize.aspx
		// claims there's a virtualPath parameter, but there's none. Probably an internal
		// method is used to wrap a call to this one.
		protected virtual void Initialize ()
		{
		}

		internal void Initialize (TemplateParser parser)
		{
			this.parser = parser;
			Initialize ();
		}
		
		public virtual void ParseComplete (ControlBuilder rootBuilder)
		{
		}
		
		public virtual void PreprocessDirective (string directiveName, IDictionary attributes)
		{
		}
		
		public virtual bool ProcessCodeConstruct (CodeConstructType codeType, string code)
		{
			return false;
		}

		public virtual bool ProcessDataBindingAttribute (string controlId, string name, string value)
		{
			return false;
		}

		public virtual bool ProcessEventHookup (string controlId, string eventName, string handlerName)
		{
			return false;
		}

		protected void SetPageProperty (string filter, string name, string value)
		{
		}
	}
}
#endif
