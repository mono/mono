class t {

	void a ()
	{
		int b;
		
		try {
			b = 1;
		} catch {
			b = 2;
		}
	}

	void b ()
	{
		int a;

		try {
			a = 1;
		} catch (Exception) {
			a = 2;
		}
	}

	void c ()
	{
		int a;

		try {
			a = 2;
		} catch (Exception e) {
			a = 0x3;
		} catch {
			a = 0x1;
		}
	}

	void d ()
	{
		int a;

		try {
			a = 2;
		} catch (Exception e) {
			a = 0x3;
		} catch {
			a = 0x1;
		} finally {
			a = 111;
		}
	}
}
			
