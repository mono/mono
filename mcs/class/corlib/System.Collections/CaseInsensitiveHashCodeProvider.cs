//
// System.Collections.CaseInsensitiveHashCodeProvider
//
// Author:
//   Sergey Chaban (serge@wildwestsoftware.com)
//



using System;
using System.Collections;



namespace System.Collections {

	public class CaseInsensitiveHashCodeProvider : IHashCodeProvider {

		private static CaseInsensitiveHashCodeProvider singleton;


		// Class constructor

		static CaseInsensitiveHashCodeProvider ()
		{
			singleton=new CaseInsensitiveHashCodeProvider ();
		}



		// Public instance constructor

		public CaseInsensitiveHashCodeProvider ()
		{
		}



		//
		// Public static properties
		//

		public static CaseInsensitiveHashCodeProvider Default {
			get {
				return singleton;
			}
		}


		//
		// Instance methods
		//

		public override string ToString ()
		{
			return "mono::System.Collections.CaseInsensitiveHashCodeProvider";
		}


		//
		// IHashCodeProvider
		//

		[MonoTODO]
		public virtual int GetHashCode (object obj)
		{
			if (obj == null) {
				throw new ArgumentNullException ("obj is null");
			}

			string str = obj as string;

			if (str == null) {
				// FIXME:
				return 0;
			}

			int h = 0;
			char c;

			if (str.Length > 0) {
				for (int i = 0;i<str.Length;i++) {
					c = str [i];

					if (Char.IsLetter (c))
						c = Char.ToLower (c);

					h = h * 31 + c;
				}
			}

			return h;
		}

	} // CaseInsensitiveHashCodeProvider
}

