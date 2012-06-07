/*
  Copyright (C) 2011 Jeroen Frijters

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Jeroen Frijters
  jeroen@frijters.net
  
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using IKVM.Reflection.Emit;
using IKVM.Reflection.Reader;

namespace IKVM.Reflection
{
	public struct CustomModifiers : IEquatable<CustomModifiers>, IEnumerable<CustomModifiers.Entry>
	{
		// note that FromReqOpt assumes that Initial == ModOpt
		private static Type Initial { get { return MarkerType.ModOpt; } }
		private readonly Type[] types;

		internal CustomModifiers(List<CustomModifiersBuilder.Item> list)
		{
			bool required = Initial == MarkerType.ModReq;
			int count = list.Count;
			foreach (CustomModifiersBuilder.Item item in list)
			{
				if (item.required != required)
				{
					required = item.required;
					count++;
				}
			}
			types = new Type[count];
			required = Initial == MarkerType.ModReq;
			int index = 0;
			foreach (CustomModifiersBuilder.Item item in list)
			{
				if (item.required != required)
				{
					required = item.required;
					types[index++] = required ? MarkerType.ModReq : MarkerType.ModOpt;
				}
				types[index++] = item.type;
			}
		}

		private CustomModifiers(Type[] types)
		{
			Debug.Assert(types == null || types.Length != 0);
			this.types = types;
		}

		public struct Enumerator : IEnumerator<Entry>
		{
			private readonly Type[] types;
			private int index;
			private bool required;

			internal Enumerator(Type[] types)
			{
				this.types = types;
				this.index = -1;
				this.required = Initial == MarkerType.ModReq;
			}

			void System.Collections.IEnumerator.Reset()
			{
				this.index = -1;
				this.required = Initial == MarkerType.ModReq;
			}

			public Entry Current
			{
				get { return new Entry(types[index], required); }
			}

			public bool MoveNext()
			{
				if (types == null || index == types.Length)
				{
					return false;
				}
				index++;
				if (index == types.Length)
				{
					return false;
				}
				else if (types[index] == MarkerType.ModOpt)
				{
					required = false;
					index++;
				}
				else if (types[index] == MarkerType.ModReq)
				{
					required = true;
					index++;
				}
				return true;
			}

			object System.Collections.IEnumerator.Current
			{
				get { return Current; }
			}

			void IDisposable.Dispose()
			{
			}
		}

		public struct Entry
		{
			private readonly Type type;
			private readonly bool required;

			internal Entry(Type type, bool required)
			{
				this.type = type;
				this.required = required;
			}

			public Type Type
			{
				get { return type; }
			}

			public bool IsRequired
			{
				get { return required; }
			}
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(types);
		}

		IEnumerator<Entry> IEnumerable<Entry>.GetEnumerator()
		{
			return GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public bool IsEmpty
		{
			get { return types == null; }
		}

		public bool Equals(CustomModifiers other)
		{
			return Util.ArrayEquals(types, other.types);
		}

		public override bool Equals(object obj)
		{
			CustomModifiers? other = obj as CustomModifiers?;
			return other != null && Equals(other.Value);
		}

		public override int GetHashCode()
		{
			return Util.GetHashCode(types);
		}

		public override string ToString()
		{
			if (types == null)
			{
				return string.Empty;
			}
			StringBuilder sb = new StringBuilder();
			string sep = "";
			foreach (Entry e in this)
			{
				sb.Append(sep).Append(e.IsRequired ? "modreq(" : "modopt(").Append(e.Type.FullName).Append(')');
				sep = " ";
			}
			return sb.ToString();
		}

		private Type[] GetRequiredOrOptional(bool required)
		{
			if (types == null)
			{
				return Type.EmptyTypes;
			}
			int count = 0;
			foreach (Entry e in this)
			{
				if (e.IsRequired == required)
				{
					count++;
				}
			}
			Type[] result = new Type[count];
			foreach (Entry e in this)
			{
				if (e.IsRequired == required)
				{
					// FXBUG reflection (and ildasm) return custom modifiers in reverse order
					// while SRE writes them in the specified order
					result[--count] = e.Type;
				}
			}
			return result;
		}

		internal Type[] GetRequired()
		{
			return GetRequiredOrOptional(true);
		}

		internal Type[] GetOptional()
		{
			return GetRequiredOrOptional(false);
		}

		internal CustomModifiers Bind(IGenericBinder binder)
		{
			if (types == null)
			{
				return this;
			}
			Type[] result = types;
			for (int i = 0; i < types.Length; i++)
			{
				if (types[i] == MarkerType.ModOpt || types[i] == MarkerType.ModReq)
				{
					continue;
				}
				Type type = types[i].BindTypeParameters(binder);
				if (!ReferenceEquals(type, types[i]))
				{
					if (result == types)
					{
						result = (Type[])types.Clone();
					}
					result[i] = type;
				}
			}
			return new CustomModifiers(result);
		}

		internal static CustomModifiers Read(ModuleReader module, ByteReader br, IGenericContext context)
		{
			byte b = br.PeekByte();
			if (!IsCustomModifier(b))
			{
				return new CustomModifiers();
			}
			List<Type> list = new List<Type>();
			Type mode = Initial;
			do
			{
				Type cmod = br.ReadByte() == Signature.ELEMENT_TYPE_CMOD_REQD ? MarkerType.ModReq : MarkerType.ModOpt;
				if (mode != cmod)
				{
					mode = cmod;
					list.Add(mode);
				}
				list.Add(Signature.ReadTypeDefOrRefEncoded(module, br, context));
				b = br.PeekByte();
			}
			while (IsCustomModifier(b));
			return new CustomModifiers(list.ToArray());
		}

		internal static void Skip(ByteReader br)
		{
			byte b = br.PeekByte();
			while (IsCustomModifier(b))
			{
				br.ReadByte();
				br.ReadCompressedInt();
				b = br.PeekByte();
			}
		}

		internal static CustomModifiers FromReqOpt(Type[] req, Type[] opt)
		{
			List<Type> list = null;
			if (opt != null && opt.Length != 0)
			{
				Debug.Assert(Initial == MarkerType.ModOpt);
				list = new List<Type>(opt);
			}
			if (req != null && req.Length != 0)
			{
				if (list == null)
				{
					list = new List<Type>();
				}
				list.Add(MarkerType.ModReq);
				list.AddRange(req);
			}
			if (list == null)
			{
				return new CustomModifiers();
			}
			else
			{
				return new CustomModifiers(list.ToArray());
			}
		}

		private static bool IsCustomModifier(byte b)
		{
			return b == Signature.ELEMENT_TYPE_CMOD_OPT || b == Signature.ELEMENT_TYPE_CMOD_REQD;
		}

		internal static CustomModifiers Combine(CustomModifiers mods1, CustomModifiers mods2)
		{
			if (mods1.IsEmpty)
			{
				return mods2;
			}
			else if (mods2.IsEmpty)
			{
				return mods1;
			}
			else
			{
				Type[] combo = new Type[mods1.types.Length + mods2.types.Length];
				Array.Copy(mods1.types, combo, mods1.types.Length);
				Array.Copy(mods2.types, 0, combo, mods1.types.Length, mods2.types.Length);
				return new CustomModifiers(combo);
			}
		}
	}
}
