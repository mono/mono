using System;


namespace WebAssembly {
	public interface IJSObject {
		int JSHandle { get; }
		int Length { get; }
	}
}


namespace WebAssembly.Core {
	public interface ITypedArray {
		int BytesPerElement { get; }
		string Name { get; }
		int ByteLength { get; }
		ArrayBuffer Buffer { get; }

		void Set (Array array);
		void Set (Array array, int offset);

		void Set (ITypedArray typedArray);
		void Set (ITypedArray typedArray, int offset);
		TypedArrayTypeCode GetTypedArrayType ();
	}

	public interface ITypedArray<T, U> where U : struct {

		T Slice ();
		T Slice (int begin);
		T Slice (int begin, int end);

		T SubArray ();
		T SubArray (int begin);
		T SubArray (int begin, int end);

	}
}
