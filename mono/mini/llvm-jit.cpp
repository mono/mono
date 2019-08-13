//
// jit-llvm.cpp: Support code for using LLVM as a JIT backend
//
// (C) 2009-2011 Novell, Inc.
// Copyright 2011-2015 Xamarin, Inc (http://www.xamarin.com)
//
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//
// Mono's internal header files are not C++ clean, so avoid including them if 
// possible
//

#include "config.h"

#include <llvm-c/Core.h>
#include <llvm-c/ExecutionEngine.h>

#include "mini-llvm-cpp.h"
#include "llvm-jit.h"

#if defined(MONO_ARCH_LLVM_JIT_SUPPORTED) && !defined(MONO_CROSS_COMPILE) && LLVM_API_VERSION > 600

#include <llvm/Support/raw_ostream.h>
#include <llvm/Support/Host.h>
#include <llvm/Support/TargetSelect.h>
#include <llvm/IR/Mangler.h>
#include <llvm/ExecutionEngine/ExecutionEngine.h>
#include "llvm/ExecutionEngine/Orc/CompileUtils.h"
#include "llvm/ExecutionEngine/Orc/IRCompileLayer.h"
#include "llvm/ExecutionEngine/Orc/LambdaResolver.h"
#include "llvm/ExecutionEngine/RTDyldMemoryManager.h"
#include "llvm/ExecutionEngine/Orc/RTDyldObjectLinkingLayer.h"
#include "llvm/ExecutionEngine/JITSymbol.h"
#include "llvm/Transforms/Scalar.h"

#include <cstdlib>

#include <mono/utils/mono-dl.h>

using namespace llvm;
using namespace llvm::orc;

extern cl::opt<bool> EnableMonoEH;
extern cl::opt<std::string> MonoEHFrameSymbol;

void
mono_llvm_set_unhandled_exception_handler (void)
{
}

template <typename T>
static std::vector<T> singletonSet(T t) {
  std::vector<T> Vec;
  Vec.push_back(std::move(t));
  return Vec;
}

#ifdef __MINGW32__

#include <stddef.h>
extern void *memset(void *, int, size_t);
void bzero (void *to, size_t count) { memset (to, 0, count); }

#endif

static AllocCodeMemoryCb *alloc_code_mem_cb;

class MonoJitMemoryManager : public RTDyldMemoryManager
{
public:
	~MonoJitMemoryManager() override;

	uint8_t *allocateDataSection(uintptr_t Size,
								 unsigned Alignment,
								 unsigned SectionID,
								 StringRef SectionName,
								 bool IsReadOnly) override;

	uint8_t *allocateCodeSection(uintptr_t Size,
								 unsigned Alignment,
								 unsigned SectionID,
								 StringRef SectionName) override;

	bool finalizeMemory(std::string *ErrMsg = nullptr) override;
};

MonoJitMemoryManager::~MonoJitMemoryManager()
{
}

uint8_t *
MonoJitMemoryManager::allocateDataSection(uintptr_t Size,
										  unsigned Alignment,
										  unsigned SectionID,
										  StringRef SectionName,
										  bool IsReadOnly) {
	uint8_t *res = (uint8_t*)malloc (Size);
	assert (res);
	memset (res, 0, Size);
	return res;
}

uint8_t *
MonoJitMemoryManager::allocateCodeSection(uintptr_t Size,
										  unsigned Alignment,
										  unsigned SectionID,
										  StringRef SectionName)
{
	return alloc_code_mem_cb (NULL, Size);
}

bool
MonoJitMemoryManager::finalizeMemory(std::string *ErrMsg)
{
	return false;
}

#if LLVM_API_VERSION >= 900

struct MonoLLVMJIT {
	std::shared_ptr<MonoJitMemoryManager> mmgr;
	ExecutionSession execution_session;
	std::map<VModuleKey, std::shared_ptr<SymbolResolver>> resolvers;
	TargetMachine *target_machine;
	LegacyRTDyldObjectLinkingLayer object_layer;
	LegacyIRCompileLayer<decltype(object_layer), SimpleCompiler> compile_layer;
	DataLayout data_layout;

	MonoLLVMJIT (TargetMachine *tm)
		: mmgr (std::make_shared<MonoJitMemoryManager>())
		, target_machine (tm)
		, object_layer (
			AcknowledgeORCv1Deprecation, execution_session,
			[this] (VModuleKey k) {
				return LegacyRTDyldObjectLinkingLayer::Resources{
					mmgr, resolvers[k] };
			})
		, compile_layer (
			AcknowledgeORCv1Deprecation, object_layer,
			SimpleCompiler{*target_machine})
		, data_layout (target_machine->createDataLayout())
	{
		compile_layer.setNotifyCompiled ([] (VModuleKey, std::unique_ptr<Module> module) {
			module.release ();
		});
	}

	VModuleKey
	add_module (std::unique_ptr<Module> m)
	{
		auto k = execution_session.allocateVModule();
		auto lookup_name = [this] (const std::string &namestr) {
			auto jit_sym = compile_layer.findSymbol(namestr, false);
			if (jit_sym) {
				return jit_sym;
			}
			auto namebuf = namestr.c_str();
			JITSymbolFlags flags{};
			if (!strcmp(namebuf, "___bzero")) {
				return JITSymbol{(uint64_t)(gssize)(void*)bzero, flags};
			}
			auto current = mono_dl_open (NULL, 0, NULL);
			g_assert (current);
			auto name = namebuf[0] == '_' ? namebuf + 1 : namebuf;
			void *sym = nullptr;
			auto err = mono_dl_symbol (current, name, &sym);
			if (!sym) {
				outs () << "R: " << namestr << "\n";
			}
			assert (sym);
			return JITSymbol{(uint64_t)(gssize)sym, flags};
		};
		auto on_error = [] (Error err) {
			outs () << "R2: " << err << "\n";
			assert (0);
		};
		auto resolver = createLegacyLookupResolver (execution_session,
			lookup_name, on_error);
		resolvers[k] = std::move (resolver);
		auto err = compile_layer.addModule (k, std::move(m));
		if (err) {
			outs () << "addModule error: " << err << "\n";
			assert (0);
		}
		return k;
	}

	std::string
	mangle (const std::string &name)
	{
		std::string ret;
		raw_string_ostream out{ret};
		Mangler::getNameWithPrefix (out, name, data_layout);
		return ret;
	}

	std::string
	mangle (const GlobalValue *gv)
	{
		std::string ret;
		raw_string_ostream out{ret};
		Mangler{}.getNameWithPrefix (out, gv, false);
		return ret;
	}

	gpointer
	compile (
		Function *func, int nvars, LLVMValueRef *callee_vars,
		gpointer *callee_addrs, gpointer *eh_frame)
	{
		auto module = func->getParent ();
		module->setDataLayout (data_layout);
		// The lifetime of this module is managed by the C API, and the
		// `unique_ptr` created here will be released in the
		// NotifyCompiled callback.
		auto k = add_module (std::unique_ptr<Module>(module));
		auto bodysym = compile_layer.findSymbolIn (k, mangle (func), false);
		auto bodyaddr = bodysym.getAddress ();
		assert (bodyaddr);
		for (int i = 0; i < nvars; ++i) {
			auto var = unwrap<GlobalVariable> (callee_vars[i]);
			auto sym = compile_layer.findSymbolIn (k, mangle (var->getName ()), true);
			auto addr = sym.getAddress ();
			g_assert ((bool)addr);
			callee_addrs[i] = (gpointer)addr.get ();
		}
		auto ehsym = compile_layer.findSymbolIn (k, "mono_eh_frame", false);
		auto ehaddr = ehsym.getAddress ();
		g_assert ((bool)ehaddr);
		*eh_frame = (gpointer)ehaddr.get ();
		return (gpointer)bodyaddr.get ();
	}
};

static void
init_mono_llvm_jit ()
{
}

static MonoLLVMJIT *
make_mono_llvm_jit (TargetMachine *target_machine)
{
	return new MonoLLVMJIT{target_machine};
}

#elif LLVM_API_VERSION > 600

class MonoLLVMJIT {
public:
	/* We use our own trampoline infrastructure instead of the Orc one */
	typedef RTDyldObjectLinkingLayer ObjLayerT;
	typedef IRCompileLayer<ObjLayerT, SimpleCompiler> CompileLayerT;
	typedef CompileLayerT::ModuleHandleT ModuleHandleT;

	MonoLLVMJIT (TargetMachine *TM, MonoJitMemoryManager *mm)
		: TM(TM), ObjectLayer([=] { return std::shared_ptr<RuntimeDyld::MemoryManager> (mm); }),
		  CompileLayer (ObjectLayer, SimpleCompiler (*TM)),
		  modules() {
	}

	ModuleHandleT addModule(Function *F, std::shared_ptr<Module> M) {
		auto Resolver = createLambdaResolver(
                      [&](const std::string &Name) {
						  const char *name = Name.c_str ();
						  JITSymbolFlags flags = JITSymbolFlags ();
						  if (!strcmp (name, "___bzero"))
							  return JITSymbol((uint64_t)(gssize)(void*)bzero, flags);

						  MonoDl *current;
						  char *err;
						  void *symbol;
						  current = mono_dl_open (NULL, 0, NULL);
						  g_assert (current);
						  if (name [0] == '_')
							  err = mono_dl_symbol (current, name + 1, &symbol);
						  else
							  err = mono_dl_symbol (current, name, &symbol);
						  mono_dl_close (current);
						  if (!symbol)
							  outs () << "R: " << Name << "\n";
						  assert (symbol);
						  return JITSymbol((uint64_t)(gssize)symbol, flags);
                      },
                      [](const std::string &S) {
						  outs () << "R2: " << S << "\n";
						  assert (0);
						  return nullptr;
					  } );

		auto m = CompileLayer.addModule(M, std::move(Resolver));
		g_assert (!!m);
		return m.get ();
	}

	std::string mangle(const std::string &Name) {
		std::string MangledName;
		{
			raw_string_ostream MangledNameStream(MangledName);
			Mangler::getNameWithPrefix(MangledNameStream, Name,
									   TM->createDataLayout());
		}
		return MangledName;
	}

	std::string mangle(const GlobalValue *GV) {
		std::string MangledName;
		{
			Mangler Mang;

			raw_string_ostream MangledNameStream(MangledName);
			Mang.getNameWithPrefix(MangledNameStream, GV, false);
		}
		return MangledName;
	}

	gpointer compile (Function *F, int nvars, LLVMValueRef *callee_vars, gpointer *callee_addrs, gpointer *eh_frame) {
		F->getParent ()->setDataLayout (TM->createDataLayout ());

		legacy::FunctionPassManager funcPassMngr(F->getParent ());
		funcPassMngr.doInitialization(); // cache per module?
		funcPassMngr.add(createSROAPass());
		funcPassMngr.add(createInstructionCombiningPass());
		// more passes? loop-vectorize, loop-unroll, GVN, etc..
		funcPassMngr.run(*F);
		funcPassMngr.doFinalization();

		// Orc uses a shared_ptr to refer to modules so we have to save them ourselves to keep a ref
		std::shared_ptr<Module> m (F->getParent ());
		modules.push_back (m);
		auto ModuleHandle = addModule (F, m);
		auto BodySym = CompileLayer.findSymbolIn(ModuleHandle, mangle (F), false);
		auto BodyAddr = BodySym.getAddress();
		assert (BodyAddr);

		for (int i = 0; i < nvars; ++i) {
			GlobalVariable *var = unwrap<GlobalVariable>(callee_vars [i]);

			auto sym = CompileLayer.findSymbolIn (ModuleHandle, mangle (var->getName ()), true);
			auto addr = sym.getAddress ();
			g_assert ((bool)addr);
			callee_addrs [i] = (gpointer)addr.get ();
		}

		auto ehsym = CompileLayer.findSymbolIn(ModuleHandle, "mono_eh_frame", false);
		auto ehaddr = ehsym.getAddress ();
		g_assert ((bool)ehaddr);
		*eh_frame = (gpointer)ehaddr.get ();
		return (gpointer)BodyAddr.get ();
	}

private:
	TargetMachine *TM;
	ObjLayerT ObjectLayer;
	CompileLayerT CompileLayer;
	std::vector<std::shared_ptr<Module>> modules;
};

static MonoJitMemoryManager *mono_mm;

static void
init_mono_llvm_jit ()
{
	mono_mm = new MonoJitMemoryManager ();
}

static MonoLLVMJIT *
make_mono_llvm_jit (TargetMachine *target_machine)
{
	return new MonoLLVMJIT(target_machine, mono_mm);
}

#endif

static MonoLLVMJIT *jit;

MonoEERef
mono_llvm_create_ee (LLVMModuleProviderRef MP, AllocCodeMemoryCb *alloc_cb, FunctionEmittedCb *emitted_cb, ExceptionTableCb *exception_cb, LLVMExecutionEngineRef *ee)
{
	alloc_code_mem_cb = alloc_cb;

	InitializeNativeTarget ();
	InitializeNativeTargetAsmPrinter();

	EnableMonoEH = true;
	MonoEHFrameSymbol = "mono_eh_frame";

	EngineBuilder EB;
#if defined(TARGET_AMD64) || defined(TARGET_X86)
	std::vector<std::string> attrs;
	// FIXME: Autodetect this
	attrs.push_back("sse3");
	attrs.push_back("sse4.1");
	EB.setMAttrs (attrs);
#endif
	auto TM = EB.selectTarget ();
	assert (TM);

	init_mono_llvm_jit ();
	jit = make_mono_llvm_jit (TM);

	return NULL;
}

/*
 * mono_llvm_compile_method:
 *
 *   Compile METHOD to native code. Compute the addresses of the variables in CALLEE_VARS and store them into
 * CALLEE_ADDRS. Return the EH frame address in EH_FRAME.
 */
gpointer
mono_llvm_compile_method (MonoEERef mono_ee, LLVMValueRef method, int nvars, LLVMValueRef *callee_vars, gpointer *callee_addrs, gpointer *eh_frame)
{
	return jit->compile (unwrap<Function> (method), nvars, callee_vars, callee_addrs, eh_frame);
}

void
mono_llvm_dispose_ee (MonoEERef *eeref)
{
}

#else /* MONO_CROSS_COMPILE or LLVM_API_VERSION < 600 */

void
mono_llvm_set_unhandled_exception_handler (void)
{
}

MonoEERef
mono_llvm_create_ee (LLVMModuleProviderRef MP, AllocCodeMemoryCb *alloc_cb, FunctionEmittedCb *emitted_cb, ExceptionTableCb *exception_cb, LLVMExecutionEngineRef *ee)
{
	g_error ("LLVM JIT not supported on this platform.");
	return NULL;
}

gpointer
mono_llvm_compile_method (MonoEERef mono_ee, LLVMValueRef method, int nvars, LLVMValueRef *callee_vars, gpointer *callee_addrs, gpointer *eh_frame)
{
	g_assert_not_reached ();
	return NULL;
}

void
mono_llvm_dispose_ee (MonoEERef *eeref)
{
	g_assert_not_reached ();
}

#endif /* !MONO_CROSS_COMPILE */
