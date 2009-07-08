using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

namespace MonoTests.Common
{
	public static class MiscExtensions
	{
		public static TChild FindChild <TChild> (this Control parent) where TChild: class
		{
			return FindChild <TChild> (parent, null);
		}

		public static TChild FindChild<TChild> (this Control parent, string id) where TChild: class
		{
			if (parent == null)
				return null;
			
			foreach (Control child in parent.Controls) {
				if (child == null)
					continue;

				if (typeof (TChild).IsAssignableFrom (child.GetType ())) {
					if (String.IsNullOrEmpty (id))
						return child as TChild;
					if (String.Compare (child.ID, id, StringComparison.OrdinalIgnoreCase) == 0)
						return child as TChild;
				}

				TChild ret = child.FindChild<TChild> (id);
				if (ret != null)
					return ret;
			}

			return null;
		}
	}
}
