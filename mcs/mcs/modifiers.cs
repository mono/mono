//
// modifiers.cs: Modifier handling.
// 
using System;
using System.Reflection;

namespace Mono.CSharp {
	public class Modifiers {

		//
		// The ordering of the following 4 constants
		// has been carefully done.
		//
		public const int PROTECTED = 0x0001;
		public const int PUBLIC    = 0x0002;
		public const int PRIVATE   = 0x0004;
		public const int INTERNAL  = 0x0008;
		public const int NEW       = 0x0010;
		public const int ABSTRACT  = 0x0020;
		public const int SEALED    = 0x0040;
		public const int STATIC    = 0x0080;
		public const int READONLY  = 0x0100;
		public const int VIRTUAL   = 0x0200;
		public const int OVERRIDE  = 0x0400;
		public const int EXTERN    = 0x0800;
		public const int VOLATILE  = 0x1000;
		public const int UNSAFE    = 0x2000;
		private const int TOP      = 0x2000;

		public const int PROPERTY_CUSTOM = 0x4000; // Custom property modifier

		//
		// We use this internally to flag that the method contains an iterator
		//
		public const int METHOD_YIELDS			= 0x8000;
		public const int METHOD_GENERIC			= 0x10000;
		public const int PARTIAL					= 0x20000;
		public const int DEFAULT_ACCESS_MODIFER	= 0x40000;
		public const int METHOD_EXTENSION		= 0x80000;
		public const int COMPILER_GENERATED		= 0x100000;

		public const int Accessibility =
			PUBLIC | PROTECTED | INTERNAL | PRIVATE;
		public const int AllowedExplicitImplFlags =
			UNSAFE | EXTERN;
		
		static public string Name (int i)
		{
			string s = "";
			
			switch (i) {
			case Modifiers.NEW:
				s = "new"; break;
			case Modifiers.PUBLIC:
				s = "public"; break;
			case Modifiers.PROTECTED:
				s = "protected"; break;
			case Modifiers.INTERNAL:
				s = "internal"; break;
			case Modifiers.PRIVATE:
				s = "private"; break;
			case Modifiers.ABSTRACT:
				s = "abstract"; break;
			case Modifiers.SEALED:
				s = "sealed"; break;
			case Modifiers.STATIC:
				s = "static"; break;
			case Modifiers.READONLY:
				s = "readonly"; break;
			case Modifiers.VIRTUAL:
				s = "virtual"; break;
			case Modifiers.OVERRIDE:
				s = "override"; break;
			case Modifiers.EXTERN:
				s = "extern"; break;
			case Modifiers.VOLATILE:
				s = "volatile"; break;
			case Modifiers.UNSAFE:
				s = "unsafe"; break;
			}

			return s;
		}

		public static string GetDescription (MethodAttributes ma)
		{
			if ((ma & MethodAttributes.Assembly) != 0)
				return "internal";

			if ((ma & MethodAttributes.Family) != 0)
				return "protected";

			if ((ma & MethodAttributes.Public) != 0)
				return "public";

			if ((ma & MethodAttributes.FamANDAssem) != 0)
				return "protected internal";

			if ((ma & MethodAttributes.Private) != 0)
				return "private";

			throw new NotImplementedException (ma.ToString ());
		}

		public static TypeAttributes TypeAttr (int mod_flags, bool is_toplevel)
		{
			TypeAttributes t = 0;

			if (is_toplevel){
				if ((mod_flags & PUBLIC) != 0)
					t |= TypeAttributes.Public;
				if ((mod_flags & PRIVATE) != 0)
					t |= TypeAttributes.NotPublic;
			} else {
				if ((mod_flags & PUBLIC) != 0)
					t |= TypeAttributes.NestedPublic;
				if ((mod_flags & PRIVATE) != 0)
					t |= TypeAttributes.NestedPrivate;
				if ((mod_flags & PROTECTED) != 0 && (mod_flags & INTERNAL) != 0)
					t |= TypeAttributes.NestedFamORAssem;
				if ((mod_flags & PROTECTED) != 0)
					t |= TypeAttributes.NestedFamily;
				if ((mod_flags & INTERNAL) != 0)
					t |= TypeAttributes.NestedAssembly;
			}
			
			if ((mod_flags & SEALED) != 0)
				t |= TypeAttributes.Sealed;
			if ((mod_flags & ABSTRACT) != 0)
				t |= TypeAttributes.Abstract;

			return t;
		}

		public static FieldAttributes FieldAttr (int mod_flags)
		{
			FieldAttributes fa = 0;

			if ((mod_flags & PUBLIC) != 0)
				fa |= FieldAttributes.Public;
			if ((mod_flags & PRIVATE) != 0)
				fa |= FieldAttributes.Private;
			if ((mod_flags & PROTECTED) != 0){
				if ((mod_flags & INTERNAL) != 0)
					fa |= FieldAttributes.FamORAssem;
				else 
					fa |= FieldAttributes.Family;
			} else {
				if ((mod_flags & INTERNAL) != 0)
					fa |= FieldAttributes.Assembly;
			}
			
			if ((mod_flags & STATIC) != 0)
				fa |= FieldAttributes.Static;
			if ((mod_flags & READONLY) != 0)
				fa |= FieldAttributes.InitOnly;

			return fa;
		}

		public static MethodAttributes MethodAttr (int mod_flags)
		{
			MethodAttributes ma = MethodAttributes.HideBySig;

			if ((mod_flags & PUBLIC) != 0)
				ma |= MethodAttributes.Public;
			if ((mod_flags & PRIVATE) != 0)
				ma |= MethodAttributes.Private;
			if ((mod_flags & PROTECTED) != 0){
				if ((mod_flags & INTERNAL) != 0)
					ma |= MethodAttributes.FamORAssem;
				else 
					ma |= MethodAttributes.Family;
			} else {
				if ((mod_flags & INTERNAL) != 0)
					ma |= MethodAttributes.Assembly;
			}

			if ((mod_flags & STATIC) != 0)
				ma |= MethodAttributes.Static;
			if ((mod_flags & ABSTRACT) != 0){
				ma |= MethodAttributes.Abstract | MethodAttributes.Virtual;
			}
			if ((mod_flags & SEALED) != 0)
				ma |= MethodAttributes.Final;

			if ((mod_flags & VIRTUAL) != 0)
				ma |= MethodAttributes.Virtual;

			if ((mod_flags & OVERRIDE) != 0)
				ma |= MethodAttributes.Virtual;
			else {
				if ((ma & MethodAttributes.Virtual) != 0)
					ma |= MethodAttributes.NewSlot;
			}
			
			return ma;
		}

		// <summary>
		//   Checks the object @mod modifiers to be in @allowed.
		//   Returns the new mask.  Side effect: reports any
		//   incorrect attributes. 
		// </summary>
		public static int Check (int allowed, int mod, int def_access, Location l)
		{
			int invalid_flags  = (~allowed) & (mod & (Modifiers.TOP - 1));
			int i;

			if (invalid_flags == 0){
				int a = mod;

				if ((mod & Modifiers.UNSAFE) != 0){
					RootContext.CheckUnsafeOption (l);
				}
				
				//
				// If no accessibility bits provided
				// then provide the defaults.
				//
				if ((mod & Accessibility) == 0){
					mod |= def_access;
					if (def_access != 0)
						mod |= DEFAULT_ACCESS_MODIFER;
					return mod;
				}

				//
				// Make sure that no conflicting accessibility
				// bits have been set.  Protected+Internal is
				// allowed, that is why they are placed on bits
				// 1 and 4 (so the shift 3 basically merges them)
				//
				a &= 15;
				a |= (a >> 3);
				a = ((a & 2) >> 1) + (a & 5);
				a = ((a & 4) >> 2) + (a & 3);
				if (a > 1)
					Report.Error (107, l, "More than one protection modifier specified");
				
				return mod;
			}
			
			for (i = 1; i <= TOP; i <<= 1){
				if ((i & invalid_flags) == 0)
					continue;

				Error_InvalidModifier (l, Name (i));
			}

			return allowed & mod;
		}

		public static void Error_InvalidModifier (Location l, string name)
		{
			Report.Error (106, l, "The modifier `" + name + "' is not valid for this item");
		}
	}
}
