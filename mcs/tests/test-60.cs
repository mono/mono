//
// Really lame test, but it should be fine for now
//
#define TEST

#region "dunno"
#endregion

#if FALSE
	#region Fields
		#if B
			int a;
		#else
			bool a;
		#endif
	#endregion
#endif

#if FLAG_FALSE
        #pragma foo
        namespace ns1
#else
        #if FLAG_FALSE
                        #if FLAG_FALSE
                                #error No error
                                namespace ns2
                        #else
                                #line aa
                                namespace ns3
                        #endif
        #else
                #if FLAG_TRUE
                        namespace ns4
                #elif FLAG_FALSE
                        namespace ns41
                #else
                        namespace ns5
                #endif
        #endif
#endif
{
        public class Y
        {
                public Y()
                {
                }

		public void Run () {}
        }
}

#if (X)
#endif

#if YY
//#errro this should not be printed // It used to compile under 1.x csc, but never under 2.x csc.
#if X
#elif Y
#else
#endif
#else
class X {
	public static int Main ()
	{
#if (TEST)
		ns5.Y y = new ns5.Y ();

		y.Run ();
		return 0;
#endif
	}
}
#endif

