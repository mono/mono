#if UNIFIED
namespace MyFramework {
#elif FRAMEWORK_A
namespace MyNamespaceA.MyFramework {
#elif FRAMEWORK_B
namespace MyNamespaceB.MyFramework {
#endif
	#if FRAMEWORK_A
	public class ClassA {
		public string MyProperty {get;set;}

		#if CROSSPROP
		public ClassC CProp {get;set;}
		public ClassA ConstructNew (ClassC val) {
			return null;
		}
		#endif
	}
	#endif

	#if FRAMEWORK_B
	public class ClassB {
		public string MyProperty {get;set;}
	}
	#endif

	public class ClassC {
		#if FRAMEWORK_A
		public string FrameworkAProperty {get;set;}
		#endif

		#if FRAMEWORK_B
		public string FrameworkBProperty {get;set;}
		#endif
		
		#if UNIFIED
		public void SomeMethod(ClassC unifiedname) { }
		#else
		public void SomeMethod(ClassC classicname) { }
		#endif

		public string FrameworkCProperty {get;set;}
	}

	#if X10
	public static class ClassExtensions {
		#if FRAMEWORK_A
		public static ClassC A_Extension (this ClassA theclass) { return null; }
		#endif
		#if FRAMEWORK_B
		public static ClassC B_Extension (this ClassB theclass) { return null; }
		#endif
		public static ClassC C_Extension (this ClassC theclass) { return null; }
	}
	#endif
}
