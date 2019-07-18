#!/usr/bin/env python

from __future__ import print_function
import os
import sys
import argparse
import clang.cindex

IOS_DEFINES = ["HOST_DARWIN", "TARGET_MACH", "MONO_CROSS_COMPILE", "USE_MONO_CTX", "_XOPEN_SOURCE"]

class Target:
	def __init__(self, arch, platform, others):
		self.arch_define = arch
		self.platform_define = platform
		self.defines = others

	def get_clang_args(self):
		ret = []
		if self.arch_define:
			ret.append (self.arch_define)
		if self.platform_define:
			ret.append (self.platform_define)
		if self.defines:
			ret.extend (self.defines)
		return ret

class TypeInfo:
	def __init__(self, name, is_jit):
		self.name = name
		self.is_jit = is_jit
		self.size = -1
		self.fields = []

class FieldInfo:
	def __init__(self, name, offset):
		self.name = name
		self.offset = offset

class OffsetsTool:
	def __init__(self):
		pass

	def parse_args(self):
		parser = argparse.ArgumentParser ()
		parser.add_argument ('--xcode-path', dest='xcode_path', help='path to Xcode.app')
		parser.add_argument ('--emscripten-sdk', dest='emscripten_path', help='path to emscripten sdk')
		parser.add_argument ('--outfile', dest='outfile', help='path to output file', required=True)
		parser.add_argument ('--monodir', dest='mono_path', help='path to mono source tree', required=True)
		parser.add_argument ('--targetdir', dest='target_path', help='path to mono tree configured for target', required=True)
		parser.add_argument ('--abi=', dest='abi', help='ABI triple to generate', required=True)
		args = parser.parse_args ()

		if args.xcode_path == None:
			args.xcode_path = "/Applications/Xcode.app/Contents/Developer"

		if not os.path.isdir (args.xcode_path):
			print ("Xcode directory '" + args.xcode_path + "' doesn't exist.", file=sys.stderr)
			sys.exit (1)
		if not os.path.isdir (args.mono_path):
			print ("Mono directory '" + args.mono_path + "' doesn't exist.", file=sys.stderr)
			sys.exit (1)
		if not os.path.isfile (args.target_path + "/config.h"):
			print ("File '" + args.target_path + "/config.h' doesn't exist.", file=sys.stderr)
			sys.exit (1)
			
		self.sys_includes=[]
		self.target = None
		self.target_args = []

		sysroot_ios = "/Platforms/iPhoneOS.platform/Developer/SDKs/iPhoneOS.sdk"
		sysroot_watchos = "/Platforms/WatchOS.platform/Developer/SDKs/WatchOS.sdk"

		if "wasm" in args.abi:
			if args.emscripten_path == None:
				print ("Emscripten sdk dir not set.", file=sys.stderr)
				sys.exit (1)
			self.sys_includes = [args.emscripten_path + "/system/include/libc"]
			self.target = Target ("TARGET_WASM", None, [])
			self.target_args += ["-target", args.abi]

		elif "arm-apple-darwin10" == args.abi:
			self.target = Target ("TARGET_ARM", "TARGET_IOS", ["ARM_FPU_VFP", "HAVE_ARMV5"] + IOS_DEFINES)
			self.target_args += ["-arch", "arm"]
			self.target_args += ["-isysroot", args.xcode_path + sysroot_ios]

		elif "aarch64-apple-darwin10" == args.abi:
			self.target = Target ("TARGET_ARM64", "TARGET_IOS", IOS_DEFINES)
			self.target_args += ["-arch", "arm64"]
			self.target_args += ["-isysroot", args.xcode_path + sysroot_ios]

		elif "armv7k-apple-darwin" == args.abi:
			self.target = Target ("TARGET_ARM", "TARGET_WATCHOS", ["ARM_FPU_VFP", "HAVE_ARMV5"] + IOS_DEFINES)
			self.target_args += ["-arch", "armv7k"]
			self.target_args += ["-isysroot", args.xcode_path + sysroot_watchos]

		elif "aarch64-apple-darwin10_ilp32" == args.abi:
			self.target = Target ("TARGET_ARM64", "TARGET_WATCHOS", ["MONO_ARCH_ILP32"] + IOS_DEFINES)
			self.target_args += ["-arch", "arm64_32"]
			self.target_args += ["-isysroot", args.xcode_path + sysroot_watchos]


		if not self.target:
			print ("ABI '" + args.abi + "' is not supported.", file=sys.stderr)
			sys.exit (1)

		self.args = args

	#
	# Collect size/alignment/offset information by running clang on files from the runtime
	#
	def run_clang(self):
		args = self.args

		self.runtime_types = {}

		mono_includes = [
			args.mono_path,
			args.mono_path + "/mono",
			args.mono_path + "/mono/eglib",
			args.target_path,
			args.target_path + "/mono/eglib"
			]
		
		self.basic_types = ["gint8", "gint16", "gint32", "gint64", "float", "double", "gpointer"]
		self.runtime_type_names = [
			"MonoObject",
			"MonoClass",
			"MonoVTable",
			"MonoDelegate",
			"MonoInternalThread",
			"MonoMulticastDelegate",
			"MonoTransparentProxy",
			"MonoRealProxy",
			"MonoRemoteClass",
			"MonoArray",
			"MonoArrayBounds",
			"MonoSafeHandle",
			"MonoHandleRef",
			"MonoComInteropProxy",
			"MonoString",
			"MonoException",
			"MonoTypedRef",
			"MonoThreadsSync",
			"SgenThreadInfo",
			"SgenClientThreadInfo",
			"MonoProfilerCallContext"
		]
		self.jit_type_names = [
			"MonoLMF",
			"MonoMethodRuntimeGenericContext",
			"MonoJitTlsData",
			"MonoGSharedVtMethodRuntimeInfo",
			"MonoContinuation",
			"MonoContext",
			"MonoDelegateTrampInfo",
			"GSharedVtCallInfo",
			"SeqPointInfo",
			"DynCallArgs", 
			"MonoLMFTramp",
			"CallContext",
			"MonoFtnDesc"
		]
		for name in self.runtime_type_names:
			self.runtime_types [name] = TypeInfo (name, False)
		for name in self.jit_type_names:
			self.runtime_types [name] = TypeInfo (name, True)
		
		self.basic_type_size = {}
		self.basic_type_align = {}

		srcfiles = ['mono/metadata/metadata-cross-helpers.c', 'mono/mini/mini-cross-helpers.c']

		clang_args = []
		clang_args += self.target_args
		clang_args += ['-std=gnu99', '-DMONO_GENERATING_OFFSETS']
		for include in self.sys_includes:
			clang_args.append ("-I")
			clang_args.append (include)
		for include in mono_includes:
			clang_args.append ("-I")
			clang_args.append (include)
		for define in self.target.get_clang_args ():
			clang_args.append ("-D" + define)
		
		clang.cindex.Config.set_library_path (args.xcode_path + "/Toolchains/XcodeDefault.xctoolchain/usr/lib/")
		
		for srcfile in srcfiles:
			src = args.mono_path + "/" + srcfile
			file_args = clang_args[:]
			if not 'mini' in src:
				file_args.append ('-DHAVE_SGEN_GC')
				file_args.append ('-DHAVE_MOVING_COLLECTOR')
				is_jit = False
			else:
				is_jit = True
			index = clang.cindex.Index.create()
			print ("Running clang: " + ' '.join (file_args) + ' ' + src + '\n')
			tu = index.parse (src, args = file_args)
			for d in tu.diagnostics:
				print (d)
				if d.severity > 2:
					sys.exit (1)
			for c in tu.cursor.walk_preorder():
				if c.kind != clang.cindex.CursorKind.STRUCT_DECL and c.kind != clang.cindex.CursorKind.TYPEDEF_DECL:
					continue
				name = c.spelling
				if c.kind == clang.cindex.CursorKind.TYPEDEF_DECL:
					for c2 in c.get_children ():
						if c2.kind == clang.cindex.CursorKind.STRUCT_DECL:
							c = c2
				type = c.type
				if "struct _" in name:
					name = name [8:]
				if len (name) > 0 and name [0] == '_':
					name = name [1:]
				if name in self.runtime_types:
					rtype = self.runtime_types [name]
					if rtype.is_jit != is_jit:
						continue
					if type.get_size () < 0:
						continue
					rtype.size = type.get_size ()
					for child in c.get_children ():
						if child.kind != clang.cindex.CursorKind.FIELD_DECL:
							continue
						if child.is_bitfield ():
							continue
						rtype.fields.append (FieldInfo (child.spelling, int (child.get_field_offsetof () / 8)))
				if c.spelling == "basic_types_struct":
					for field in c.get_children ():
						btype = field.spelling.replace ("_f", "")
						self.basic_type_size [btype] = field.type.get_size ()
						self.basic_type_align [btype] = field.type.get_align ()

	def gen (self):
		outfile = self.args.outfile
		target = self.target
		f = open (outfile, 'w')
		f.write ("#ifndef USED_CROSS_COMPILER_OFFSETS\n")
		if target.arch_define:
			f.write ("#ifdef " + target.arch_define + "\n")
		if target.platform_define:
			f.write ("#ifdef " + target.platform_define + "\n")
		f.write ("#ifndef HAVE_BOEHM_GC\n")
		f.write ("#define HAS_CROSS_COMPILER_OFFSETS\n")
		f.write ("#if defined (USE_CROSS_COMPILE_OFFSETS) || defined (MONO_CROSS_COMPILE)\n")

		f.write ("#if !defined (DISABLE_METADATA_OFFSETS)\n")
		f.write ("#define USED_CROSS_COMPILER_OFFSETS\n")
		for btype in self.basic_types:
			f.write ("DECL_ALIGN2(%s,%s)\n" % (btype, self.basic_type_align [btype]))
		for btype in self.basic_types:
			f.write ("DECL_SIZE2(%s,%s)\n" % (btype, self.basic_type_size [btype]))
		for type_name in self.runtime_type_names:
			type = self.runtime_types [type_name]
			if type.size == -1:
				continue
			f.write ("DECL_SIZE2(%s,%s)\n" % (type.name, type.size))
			for field in type.fields:
				f.write ("DECL_OFFSET2(%s,%s,%s)\n" % (type.name, field.name, field.offset))
		f.write ("#endif //disable metadata check\n")
		
		f.write ("#ifndef DISABLE_JIT_OFFSETS\n")
		f.write ("#define USED_CROSS_COMPILER_OFFSETS\n")
		for type_name in self.jit_type_names:
			type = self.runtime_types [type_name]
			if type.size == -1:
				continue
			f.write ("DECL_SIZE2(%s,%s)\n" % (type.name, type.size))
			for field in type.fields:
				f.write ("DECL_OFFSET2(%s,%s,%s)\n" % (type.name, field.name, field.offset))
		f.write ("#endif //disable jit check\n")
					
		f.write ("#endif //cross compiler checks\n")
		f.write ("#endif //gc check\n")
		if target.arch_define:
			f.write ("#endif //" + target.arch_define + "\n")
		if target.platform_define:
			f.write ("#endif //" + target.platform_define + "\n")
		f.write ("#endif //USED_CROSS_COMPILER_OFFSETS check\n")

tool = OffsetsTool ()
tool.parse_args ()
tool.run_clang ()
tool.gen ()

# Local Variables:
# indent-tabs-mode: 1
# tab-width: 4
# End:
