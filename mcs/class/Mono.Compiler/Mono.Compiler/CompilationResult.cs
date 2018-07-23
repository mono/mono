namespace Mono.Compiler {
	public enum CompilationResult {
		
		Ok,
		BadCode,
		OutOfMemory,
		InternalError,
		Skipped,
		RecoverableError
	}
}
