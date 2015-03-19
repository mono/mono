using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Xamarin.ApiDiff
{
	public class ApiChange
	{
		public string Header;
		public StringBuilder Member = new StringBuilder ();
		public bool Breaking;
		public bool AnyChange;
		public bool HasIgnoredChanges;

		public ApiChange Append (string text)
		{
			Member.Append (text);
			return this;
		}

		public ApiChange AppendAdded (string text, bool breaking = false)
		{
			if (breaking)
				Member.Append ("<u>");
			if (State.Colorize)
				Member.Append ("<font color='green'>");
			Member.Append (text);
			if (State.Colorize)
				Member.Append ("</font>");
			if (breaking)
				Member.Append ("</u>");
			Breaking |= breaking;
			AnyChange = true;
			return this;
		}

		public ApiChange AppendRemoved (string text, bool breaking = true)
		{
			Member.Append ("<s>");
			if (State.Colorize && breaking)
				Member.Append ("<font color='red'>");
			Member.Append (text);
			if (State.Colorize && breaking)
				Member.Append ("</font>");
			Member.Append ("</s>");
			Breaking |= breaking;
			AnyChange = true;
			return this;
		}

		public ApiChange AppendModified (string old, string @new, bool breaking = true)
		{
			if (old.Length > 0)
				AppendRemoved (old, breaking);
			if (old.Length > 0 && @new.Length > 0)
				Append (" ");
			if (@new.Length > 0)
				AppendAdded (@new);
			Breaking |= breaking;
			AnyChange = true;
			return this;
		}
	}

	public class ApiChanges : Dictionary<string, List<ApiChange>> {
		public void Add (XElement source, XElement target, ApiChange change)
		{
			if (!change.AnyChange) {
				// This is most likely because the rendering doesn't take into account something that's different (solution: fix rendering).
				if (!change.HasIgnoredChanges)
					Console.WriteLine ("Comparison resulting in no changes (src: {2} dst: {3}) :\n{0}\n{1}\n\n", source.ToString (), target.ToString (), source.GetMethodAttributes (), target.GetMethodAttributes ());
				return;
			}

			List<ApiChange> list;
			if (!TryGetValue (change.Header, out list)) {
				list = new List<ApiChange> ();
				base.Add (change.Header, list);
			}
			list.Add (change);
		}
	}
}

