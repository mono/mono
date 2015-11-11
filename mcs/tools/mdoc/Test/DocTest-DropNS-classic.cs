namespace MyFramework.MyNamespace {
	public class MyClass {
		public string MyProperty {get;set;}
		public float Hello(int value) {
			return 0.0f;
		}
		#if DELETETEST
		public string InBoth {get;set;}
		public string InBothClassic {get;set;}
		public int InBothMagicType {get;set;}
		#endif

		#if DELETETEST && V2
		public string AddedInV2 {get;set;}
		public string AddedInV2Classic {get;set;}
		#endif
		#if DELETETEST && !V2
		public string WillDeleteInV2 {get;set;}
		public string WillDeleteInV2Classic {get;set;}
		#endif
	}

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
