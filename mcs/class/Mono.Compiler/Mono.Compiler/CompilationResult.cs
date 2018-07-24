namespace Mono.Compiler {
	public enum CompilationResult : short {
		
		Ok,
		BadCode,
		OutOfMemory,
		InternalError,
		Skipped,
		RecoverableError
	}
}
