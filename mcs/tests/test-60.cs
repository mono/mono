//
// Really lame test, but it should be fine for now
//
#define TEST

#region "dunno"
#endregion

#if FLAG_FALSE
        namespace ns1
#else
        #if FLAG_FALSE
                        #if FLAG_FALSE
                                namespace ns2
                        #else
                                namespace ns3
                        #endif
        #else
                #if FLAG_TRUE
                        namespace ns4
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
#errro this should not be printed
#if X
#elif Y
#else
#endif
#else
class X {
	static int Main ()
	{
#if (TEST)
		ns5.Y y = new ns5.Y ();

		y.Run ();
		return 0;
#endif
	}
}
#endif

