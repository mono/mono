using System;
using System.Collections.Specialized;

namespace Mono.Doc.Core
{
	public abstract class AbstractDoc
	{
		protected string           summary = null;
		protected string           remarks = null;
		protected StringCollection seeAlso;

		protected AbstractDoc()
		{
			seeAlso = new StringCollection();
		}

		public string Summary
		{
			get { return summary;  }
			set { summary = value; }
		}

		public string Remarks
		{
			get { return remarks;  }
			set { remarks = value; }
		}

		public StringCollection SeeAlso
		{
			get { return seeAlso; }
		}
	}
}
