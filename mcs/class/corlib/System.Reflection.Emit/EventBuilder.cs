
//
// System.Reflection.Emit/EventBuilder.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection.Emit {
	public sealed class EventBuilder {
		string name;
		Type type;
		TypeBuilder typeb;
		CustomAttributeBuilder[] cattrs;
		MethodBuilder add_method;
		MethodBuilder remove_method;
		MethodBuilder raise_method;
		MethodBuilder[] other_methods;
		EventAttributes attrs;
		int table_idx;

		internal EventBuilder (TypeBuilder tb, string eventName, EventAttributes eventAttrs, Type eventType) {
			name = eventName;
			attrs = eventAttrs;
			type = eventType;
			typeb = tb;
			table_idx = get_next_table_index (this, 0x14, true);
		}

		internal int get_next_table_index (object obj, int table, bool inc) {
			return typeb.get_next_table_index (obj, table, inc);
		}

		public void AddOtherMethod( MethodBuilder mdBuilder) {
			if (mdBuilder == null)
				throw new ArgumentNullException ("mdBuilder");
			RejectIfCreated ();
			if (other_methods != null) {
				MethodBuilder[] newv = new MethodBuilder [other_methods.Length + 1];
				other_methods.CopyTo (newv, 0);
				other_methods = newv;
			} else {
				other_methods = new MethodBuilder [1];
			}
			other_methods [other_methods.Length - 1] = mdBuilder;
		}
		
		public EventToken GetEventToken () {
			return new EventToken (0x14000000 | table_idx);
		}
		public void SetAddOnMethod( MethodBuilder mdBuilder) {
			if (mdBuilder == null)
				throw new ArgumentNullException ("mdBuilder");
			RejectIfCreated ();
			add_method = mdBuilder;
		}
		public void SetRaiseMethod( MethodBuilder mdBuilder) {
			if (mdBuilder == null)
				throw new ArgumentNullException ("mdBuilder");
			RejectIfCreated ();
			raise_method = mdBuilder;
		}
		public void SetRemoveOnMethod( MethodBuilder mdBuilder) {
			if (mdBuilder == null)
				throw new ArgumentNullException ("mdBuilder");
			RejectIfCreated ();
			remove_method = mdBuilder;
		}

		public void SetCustomAttribute( CustomAttributeBuilder customBuilder) {
			if (customBuilder == null)
				throw new ArgumentNullException ("customBuilder");
			RejectIfCreated ();
			if (cattrs != null) {
				CustomAttributeBuilder[] new_array = new CustomAttributeBuilder [cattrs.Length + 1];
				cattrs.CopyTo (new_array, 0);
				new_array [cattrs.Length] = customBuilder;
				cattrs = new_array;
			} else {
				cattrs = new CustomAttributeBuilder [1];
				cattrs [0] = customBuilder;
			}
		}
		public void SetCustomAttribute( ConstructorInfo con, byte[] binaryAttribute) {
			if (con == null)
				throw new ArgumentNullException ("con");
			if (binaryAttribute == null)
				throw new ArgumentNullException ("binaryAttribute");
			SetCustomAttribute (new CustomAttributeBuilder (con, binaryAttribute));
		}

		private void RejectIfCreated () {
			if (typeb.is_created)
				throw new InvalidOperationException ("Type definition of the method is complete.");
		}
	}
}

