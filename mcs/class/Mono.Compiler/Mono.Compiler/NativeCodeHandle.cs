namespace Mono.Compiler {
	public unsafe class NativeCodeHandle {
		public byte *Blob { get; }
		private long length;

		public unsafe NativeCodeHandle (byte *codeBlob, long codeLength) {
			this.Blob = codeBlob;
			this.length = codeLength;
		}
	}
}
