struct T {
        int val;
        void one () {
                two (this);
        }
        void two (T t)  {
		this = t;
        }
        void three (ref T t) {
                two (t);
        }
        static int  Main() 	
	{
		T t;

		t.one ();	
		return 0;
        }
}





        
