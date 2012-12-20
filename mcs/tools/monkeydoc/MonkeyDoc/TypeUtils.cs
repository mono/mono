using System;

namespace Monodoc
{
	public static class TypeUtils
	{
		public static bool GetNamespaceAndType (string url, out string ns, out string type)
		{
			int nsidx = -1;
			int numLt = 0;
			for (int i = 0; i < url.Length; ++i) {
				char c = url [i];
				switch (c) {
				case '<':
				case '{':
					++numLt;
					break;
				case '>':
				case '}':
					--numLt;
					break;
				case '.':
					if (numLt == 0)
						nsidx = i;
					break;
				}
			}

			if (nsidx == -1) {
				ns = null;
				type = null;
				return false;
			}
			ns = url.Substring (0, nsidx);
			type = url.Substring (nsidx + 1);
		
			return true;
		}
	}
}
