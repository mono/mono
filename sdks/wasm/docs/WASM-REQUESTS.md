# Introduction

This document describes possible extensions to WebAssembly which would enable Mono/the
WASM runtime to emit smaller and/or faster code or implement additional functionality.

## Nullref checks

Currently, reading from address 0 or close to it doesn't throw. This forces mono
to emit explicit null checks which are very common and lead to increased code
size.

Division by 0 is a similar situation. For integer division, its possible
to emit fairly simple explicit checks, but fp division is more complicated.

Some possible solutions:
* Add some kind of signal handler facility where hitting a nullref would transfer
control to a specific wasm function which could throw an exception.
* The 'macro operations' section below.

## Branch hints

Add support for llvm.expected ().

This is useful for generating better native code for

	if (unlikely(cond))
	...

which is fairly common in .net IL because of argument checks, etc.

## Cold calling convention

Add support for coldcc.

This is useful for calls which are rarely executed, like the call in the GC safe
point code:

	if <need to do a gc> {
		<call into runtime to do a gc>
   	}

## Macro operations

Many higher level .net opcodes are translated to a series of llvm opcodes. To make
the wasm executable smaller, it would be useful to have some kind of macro facility
so these can be emitted in a more compact manner. Something similar to
https://github.com/WebAssembly/decompressor-prototype/blob/master/CompressionLayer1.md.

Alternatively, emitting small functions which are always inlined by the wasm JIT would also
work.

## Computed goto

Currently, the llvm wasm backend doesn't support computed gotos. This is important for
efficient implementation of interpreters.

## Programmatic stack walking

Languages like .net/java allow programs to obtain their own stacktraces as an array of stackframe objects.
Would be useful to have this facility in wasm.

This could be implemented by associating some data like an id
with a subset of wasm methods, then having a wasm intrinsics which returns a list of such data,
which the app could then use to construct the stack trace objects. Returning only
data which the app writer explicitly added to the wasm module would help alleviate
some of the security concerns with stack walks.

## Noinline flag

Currently, the LLVM 'noinline' flag has no corresponding wasm flag, so
the wasm optimizer will inline functions which are marked noinline. This
causes problems for the mono interpreter because the inlined functions
increase the stack size for the main interpreter function.

# Process wide memory barrier

This is useful to reduce synchronization overhead in various parallel
systems including synchronization with the GC. Its the same as
membarrier (MEMBARRIER_CMD_PRIVATE_EXPEDITED) on linux and
FlushProcessWriteBuffers on windows.
