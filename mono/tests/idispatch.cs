using System;
using System.Runtime.InteropServices;

public unsafe class Tests {
	[DllImport("libtest", EntryPoint="mono_test_cominterop_ccw_getidofname")]
	static extern int getidofname (IntPtr dispatch, [MarshalAs (UnmanagedType.LPWStr)] string name);

	const string IID_DispInterface = "10C00388-57CE-4CD7-964C-A284EB5B0464";
	const string IID_DispInterface2 = "68818D10-A847-4136-9E0B-32544CE32951";
	const string IID_IDispatch = "00020400-0000-0000-C000-000000000046";

	const int DISP_E_UNKNOWN_NAME = unchecked((int)0x80020006);

	[ComImport()]
	[Guid(IID_DispInterface)]
	[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface DispInterface {
		void NoParams();

		[DispId(12345)]
		void NoParamsDispId();
	}

	[ComImport()]
	[Guid(IID_DispInterface2)]
	[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface DispInterface2 {
		[DispId(1)]
		void NoParams2();
	}

	[ClassInterface(ClassInterfaceType.None)]
	public class DispInstance : DispInterface, DispInterface2 {
		public void NoParams() { }

		public void NoParamsDispId() { }

		[DispId(1234)]
		public void ClsNoParamsDispId() { }

		public void ClsNoParams() { }

		public void NoParams2() { }
	}

	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	public class AutoDispClass : DispInterface {
		public void NoParams() { }

		public void NoParamsDispId() { }

		[DispId(1234)]
		public void ClsNoParamsDispId() { }

		public void ClsNoParams() { }
	}

	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	public class SubClass : AutoDispClass {
		public void SubClassMethod() { }
	}

	public static IntPtr get_interface(object obj, string iid) {
		Guid guid = new Guid (iid);
		IntPtr unk = Marshal.GetIUnknownForObject (obj);
		IntPtr result;
		int hr = Marshal.QueryInterface (unk, ref guid, out result);
		Marshal.Release (unk);
		Marshal.ThrowExceptionForHR (hr);
		return result;
	}

	public static bool check_dispid (object obj, string iid, string name, int expected) {
		IntPtr disp = get_interface (obj, iid);

		int actual = getidofname (disp, name);

		Marshal.Release (disp);

		if (actual != expected)
		{
			Console.WriteLine ("check_dispid {0}/{1}/{2} expected {3} got {4}",
				obj, iid, name, expected, actual);
				
			return false;
		}

		return true;
	}

	public static int test_0_get_dispid () {
		if (!check_dispid (new DispInstance(), IID_DispInterface, "NoParams", 0x60020000))
			return 1;
		if (!check_dispid (new DispInstance(), IID_DispInterface, "NoParamsDispId", 12345))
			return 2;
		if (!check_dispid (new DispInstance(), IID_DispInterface, "ClsNoParamsDispId", DISP_E_UNKNOWN_NAME))
			return 3;
		if (!check_dispid (new DispInstance(), IID_DispInterface, "ClsNoParams", DISP_E_UNKNOWN_NAME))
			return 4;
		if (!check_dispid (new DispInstance(), IID_DispInterface, "NoParams2", DISP_E_UNKNOWN_NAME))
			return 5;
		if (!check_dispid (new DispInstance(), IID_DispInterface2, "NoParams2", 1))
			return 6;
		if (!check_dispid (new DispInstance(), IID_IDispatch, "NoParams", 0x60020000))
			return 7;
		if (!check_dispid (new DispInstance(), IID_IDispatch, "NoParamsDispId", 12345))
			return 8;
		if (!check_dispid (new DispInstance(), IID_IDispatch, "ClsNoParamsDispId", DISP_E_UNKNOWN_NAME))
			return 9;
		if (!check_dispid (new DispInstance(), IID_IDispatch, "ClsNoParams", DISP_E_UNKNOWN_NAME))
			return 10;
		if (!check_dispid (new DispInstance(), IID_IDispatch, "NoParams2", DISP_E_UNKNOWN_NAME))
			return 11;
		if (!check_dispid (new AutoDispClass(), IID_DispInterface, "NoParams", 0x60020000))
			return 12;
		if (!check_dispid (new AutoDispClass(), IID_DispInterface, "NoParamsDispId", 12345))
			return 13;
		if (!check_dispid (new AutoDispClass(), IID_DispInterface, "ClsNoParamsDispId", DISP_E_UNKNOWN_NAME))
			return 14;
		if (!check_dispid (new AutoDispClass(), IID_DispInterface, "ClsNoParams", DISP_E_UNKNOWN_NAME))
			return 15;
		if (!check_dispid (new AutoDispClass(), IID_IDispatch, "NoParams", 0x60020004))
			return 16;
		if (!check_dispid (new AutoDispClass(), IID_IDispatch, "NoParamsDispId", 0x60020005))
			return 17;
		if (!check_dispid (new AutoDispClass(), IID_IDispatch, "ClsNoParamsDispId", 1234))
			return 18;
		if (!check_dispid (new AutoDispClass(), IID_IDispatch, "ClsNoParams", 0x60020007))
			return 19;

		/* Object methods */
		if (!check_dispid (new Object(), IID_IDispatch, "ToString", 0))
			return 20;
		if (!check_dispid (new Object(), IID_IDispatch, "Equals", 0x60020001))
			return 21;
		if (!check_dispid (new Object(), IID_IDispatch, "GetHashCode", 0x60020002))
			return 22;
		if (!check_dispid (new Object(), IID_IDispatch, "GetType", 0x60020003))
			return 23;
		if (!check_dispid (new AutoDispClass(), IID_DispInterface, "GetType", DISP_E_UNKNOWN_NAME))
			return 24;
		if (!check_dispid (new AutoDispClass(), IID_IDispatch, "GetType", 0x60020003))
			return 25;

		/* Inheritance */
		if (!check_dispid (new SubClass(), IID_IDispatch, "GetType", 0x60020003))
			return 26;
		if (!check_dispid (new SubClass(), IID_IDispatch, "NoParamsDispId", 0x60020005))
			return 27;
		if (!check_dispid (new SubClass(), IID_IDispatch, "SubClassMethod", 0x60020008))
			return 28;

		return 0;
	}

	public static int Main (string[] args)
	{
		return TestDriver.RunTests (typeof (Tests), args);
	}
}
