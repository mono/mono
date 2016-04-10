﻿using System;
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
			Member.Append ("<span class='added ").Append (breaking ? "added-breaking-inline" : string.Empty).Append ("'>");
			Member.Append (text);
			Member.Append ("</span>");
			Breaking |= breaking;
			AnyChange = true;
			return this;
		}

		public ApiChange AppendRemoved (string text, bool breaking = true)
		{
			Member.Append ("<span class='removed removed-inline ").Append (breaking ? "removed-breaking-inline" : string.Empty).Append ("'>");
			Member.Append (text);
			Member.Append ("</span>");
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
				if (!change.HasIgnoredChanges) {
					var isField = source.Name.LocalName == "field";
					if (isField) {
						Console.WriteLine ("Comparison resulting in no changes (src: {2} dst: {3}) :\n{0}\n{1}\n\n", source.ToString (), target.ToString (), source.GetFieldAttributes (), target.GetFieldAttributes ());
					} else {
						Console.WriteLine ("Comparison resulting in no changes (src: {2} dst: {3}) :\n{0}\n{1}\n\n", source.ToString (), target.ToString (), source.GetMethodAttributes (), target.GetMethodAttributes ());
					}
				}
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

