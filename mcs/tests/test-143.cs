using System;

struct MonoEnumInfo {
        int val;

        void stuff() { val = 1; }

        static int GetInfo (out MonoEnumInfo info) {
		info = new MonoEnumInfo ();
                info.stuff();
                return info.val;
        }

        public static int Main()
	{
		MonoEnumInfo m;

		if (GetInfo (out m) != 1)
			return 1;
		
		if (m.val != 1)
			return 2;

		return 0;
	}
};

