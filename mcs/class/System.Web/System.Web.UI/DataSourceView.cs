//
// System.Web.UI.DataSourceView
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace System.Web.UI {
	public abstract class DataSourceView
	{
		protected DataSourceView ()
		{
		}
		
		public virtual int Delete (IDictionary keys)
		{
			throw new NotSupportedException ();
		}
		
		public virtual int Insert (IDictionary values)
		{
			throw new NotSupportedException ();
		}
		
		public virtual int Update (IDictionary keys, IDictionary values)
		{
			throw new NotSupportedException ();
		}
		
		public abstract IEnumerable Select ();

		public virtual bool CanDelete { get { return false; } }
		public virtual bool CanInsert { get { return false; } }
		public virtual bool CanSort { get { return false; } }
		public virtual bool CanUpdate { get { return false; } }
		
		public virtual string Name { get { return ""; } }
		public virtual string SortExpression {
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
	}
	
}
#endif

