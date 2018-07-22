namespace Mono.Compiler {
	public unsafe class NativeCodeHandle {
		private byte *blob;
		private long length;

		public unsafe NativeCodeHandle (byte *codeBlob, long codeLength) {
			this.blob = codeBlob;
			this.length = codeLength;
		}
	}
}