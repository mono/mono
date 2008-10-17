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
