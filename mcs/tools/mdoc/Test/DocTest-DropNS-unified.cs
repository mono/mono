namespace MyNamespace {
	public class MyClass {
		public string MyProperty {get;set;}
		public float Hello(int value) {
			return 0.0f;
		}
		public char OnlyInUnified {get;set;} 
		
		#if DELETETEST
		public string InBoth {get;set;}
		public string InBothUnified {get;set;}
		public nint InBothMagicType {get;set;}
		#endif

		#if DELETETEST && V2
		public string AddedInV2 {get;set;}
		public string AddedInV2Unified {get;set;}
		#endif
		#if DELETETEST && !V2
		public string WillDeleteInV2 {get;set;}
		public string WillDeleteInV2Unified {get;set;}
		#endif
	}

	public static class MyClassExtensions {
		public static bool AnExtension (this MyClass value) { return false; }
	}
	#if DELETETEST
	public struct nint {

	}
	#endif
	#if DELETETEST && !V2
	public class WillDelete {
		public string Name {get;set;}
	}
	#endif

	#if MULTITEST
	public class OnlyInMulti {
	}
	#endif
}
