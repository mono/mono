#
# Scripts that we install
#
# Starting with Mono 2.0, the majority of the tools default to the 
# 2.0 profile.  Commands that support the 1.0 toolchain and must
# run under 1.0 have the number 1 appended to their command names
# (for example mcs1 is the C# 1.0 compiler) and are listed in the
# scripts_1_0 compat variable.
# 
# To preserve compatibility with old Makefiles and tools we keep the
# command names that had the suffix 2 (like wsdl2)
#
if(USE_BATCH_FILES)
message(FATAL_ERROR FIXME)
set(SCRIPT_IN script.bat.in)
set(FILTER sed -e 's,\.bat\.exe,\.exe,g' -e 's,/,\\,g')
set(SCRIPT_SUFFIX .bat)
else()
set(SCRIPT_IN script.in)
set(FILTER cat)
set(SCRIPT_SUFFIX)
endif()

set(COMPAT_1_0_SUFFIX 1${SCRIPT_SUFFIX})
set(COMPAT_2_0_SUFFIX 2${SCRIPT_SUFFIX})

set(scripts_1_0_compat
	al1${SCRIPT_SUFFIX}
	genxs1${SCRIPT_SUFFIX}
	ilasm1${SCRIPT_SUFFIX}
	mcs1${SCRIPT_SUFFIX}
	mkbundle1${SCRIPT_SUFFIX}
	gacutil1${SCRIPT_SUFFIX}
	monop1${SCRIPT_SUFFIX}
	resgen1${SCRIPT_SUFFIX}
	wsdl1${SCRIPT_SUFFIX})

set(scripts_1_0
	caspol${SCRIPT_SUFFIX}
	cert2spc${SCRIPT_SUFFIX}
	certmgr${SCRIPT_SUFFIX}
	chktrust${SCRIPT_SUFFIX}
	cilc${SCRIPT_SUFFIX}
	disco${SCRIPT_SUFFIX}
	dtd2xsd${SCRIPT_SUFFIX}
	dtd2rng${SCRIPT_SUFFIX}
	installvst${SCRIPT_SUFFIX}
	genxs${SCRIPT_SUFFIX}
	macpack${SCRIPT_SUFFIX}
	mcs${SCRIPT_SUFFIX}
	mjs${SCRIPT_SUFFIX}
	monolinker${SCRIPT_SUFFIX}
	mono-cil-strip${SCRIPT_SUFFIX}
	mozroots${SCRIPT_SUFFIX}
	permview${SCRIPT_SUFFIX}
	prj2make${SCRIPT_SUFFIX}
	secutil${SCRIPT_SUFFIX}
	setreg${SCRIPT_SUFFIX}
	signcode${SCRIPT_SUFFIX}
	soapsuds${SCRIPT_SUFFIX}
	xsd${SCRIPT_SUFFIX})

set(scripts_1_0_umask
	makecert${SCRIPT_SUFFIX}
	sn${SCRIPT_SUFFIX})

set(scripts_2_0
	al2${SCRIPT_SUFFIX}
	csharp${SCRIPT_SUFFIX}
	gacutil2${SCRIPT_SUFFIX}
	gmcs${SCRIPT_SUFFIX}
	httpcfg${SCRIPT_SUFFIX}
	ilasm2${SCRIPT_SUFFIX}
	mconfig${SCRIPT_SUFFIX}
	mkbundle2${SCRIPT_SUFFIX}
	mdoc${SCRIPT_SUFFIX}              
	mono-api-info${SCRIPT_SUFFIX}
	mono-shlib-cop${SCRIPT_SUFFIX}
	monop2${SCRIPT_SUFFIX} 
	resgen2${SCRIPT_SUFFIX}
	pdb2mdb${SCRIPT_SUFFIX}
	wsdl2${SCRIPT_SUFFIX}
	xsd2${SCRIPT_SUFFIX}
	mono-xmltool${SCRIPT_SUFFIX}
	sgen${SCRIPT_SUFFIX}
	sqlmetal${SCRIPT_SUFFIX}
	sqlsharp${SCRIPT_SUFFIX}
	svcutil${SCRIPT_SUFFIX}
	xbuild${SCRIPT_SUFFIX})

set(MDOC_SUBCOMMANDS
	mdoc-assemble
	mdoc-export-html
	mdoc-export-msxdoc
	mdoc-update
	mdoc-validate)

set(MDOC_COMPAT
	mdassembler	
	mdvalidater	
	monodocer	
	monodocs2html	
	monodocs2slashdoc)

set(bin_SCRIPTS
  ${scripts_1_0}
  ${scripts_1_0_umask}
  ${scripts_1_0_compat}
  ${scripts_defaults}
  ${scripts_2_0}
  ${scripts_service}
  ${scripts_nunit}
  ${scripts_rpmhelpers}
  ${MDOC_SUBCOMMANDS}
  ${MDOC_COMPAT}
  mod
  mono-test-install)

#
# these are the new defaults, 2.0 profile-based commands
#
# we can move scripts_1_0 scripts here as we see fit, if(we need to
# keep a 1.0 command available, we should additionally put it on
# the scripts_1_0_compat list
#
set(scripts_defaults
  al${SCRIPT_SUFFIX}
  ilasm${SCRIPT_SUFFIX}
  gacutil${SCRIPT_SUFFIX}
  mkbundle${SCRIPT_SUFFIX}
  monop${SCRIPT_SUFFIX}
  resgen${SCRIPT_SUFFIX}
  wsdl${SCRIPT_SUFFIX})

###scripts_service = mono-service mono-service2
###scripts_nunit = nunit-console${SCRIPT_SUFFIX} nunit-console2${SCRIPT_SUFFIX}
###scripts_rpmhelpers = mono-find-provides mono-find-requires
###
###CLEANFILES = ${scripts_1_0} ${scripts_1_0_umask} ${scripts_2_0} ${scripts_defaults} ${scripts_1_0_compat} mono-service mono-service2 nunit-console nunit-console2 mono-find-provides mono-find-requires mod ${MDOC_SUBCOMMANDS}
###DISTCLEANFILES = ${pkgconfig_DATA} ${scripts_rpmhelpers}
###
###EXTRA_DIST =			\
###	script.in		\
###	script.bat.in		\
###	script_umask.in		\
###	script_umask.bat.in	\
###	mod.in			\
###	mono-service.in		\
###	mono-find-provides.in	\
###	mono-find-requires.in	\
###	mono-test-install	\
###	${MDOC_COMPAT}		\
###	patch-quiet.sh
###
if(USE_JIT)
set(mono_interp mono)
else()
set(mono_interp mint)
endif()

if(PLATFORM_WIN32)
if(CROSS_COMPILING)
set(plat_bindir ${bindir})
set(mono_instdir ${prefix}/lib/mono)
else()
set(plat_bindir ${shell cygpath -m ${libdir}})
set(mono_instdir ${shell cygpath -m ${libdir}}/mono)
endif()
else()
set(plat_bindir ${bindir})
set(mono_instdir ${prefix}/lib/mono)
endif()

function(rewrite input output exe version)
  file(READ ${input} s)
  string(REPLACE "\@bindir\@" "${bindir_full}" s ${s})
  string(REPLACE "\@mono_instdir\@" "${mono_instdir}" s ${s})
  string(REPLACE "\@mono_interp\@" "${mono_interp}" s ${s})
  string(REPLACE "\@framework_version\@" "${version}" s ${s})
  string(REPLACE "\@exe_name\@" "${exe}" s ${s})
  file(WRITE ${output} ${s})
endfunction()

function(rewrite_debug input output exe version)
  file(READ ${input} s)
  string(REPLACE "\@bindir\@" "${bindir_full}" s ${s})
  string(REPLACE "\@mono_instdir\@" "${mono_instdir}" s ${s})
  string(REPLACE "\@mono_interp\@" "${mono_interp} --debug" s ${s})
  string(REPLACE "\@framework_version\@" "${version}" s ${s})
  string(REPLACE "\@exe_name\@" "${exe}" s ${s})
  file(WRITE ${output} ${s})
endfunction()

set(srcdir ${top_srcdir}/scripts)

# FIXME: Make these into targets
rewrite(mono-service.in mono-service mono-service 1.0)
rewrite(mono-service.in mono-service2 mono-service2 2.0)
rewrite_debug(${SCRIPT_IN} nunit-console${SCRIPT_SUFFIX} nunit-console 1.0)
rewrite_debug(${SCRIPT_IN} nunit-console2${SCRIPT_SUFFIX} nunit-console 2.0)
rewrite(mod.in mod${SCRIPT_SUFFIX} mod 1.0)

foreach(script ${scripts_1_0})
  rewrite(${srcdir}/script.in ${script} ${script} 1.0)
endforeach()

foreach(script ${scripts_1_0_umask})
  rewrite(${srcdir}/script_umask.in ${script} ${script} 1.0)
endforeach()

foreach(script ${scripts_1_0_compat})
  string(REPLACE ${COMPAT_1_0_SUFFIX} "" exe ${script})
  rewrite(${srcdir}/script_umask.in ${script} ${exe} 1.0)
endforeach()

foreach(script ${scripts_defaults})
  rewrite(${srcdir}/script.in ${script} ${script} 2.0)
endforeach()

foreach(script ${scripts_2_0})
  string(REPLACE ${COMPAT_2_0_SUFFIX} "" exe ${script})
  rewrite(${srcdir}/script.in ${script} ${exe} 2.0)
endforeach()

foreach(script ${MDOC_SUBCOMMANDS})
  string(REPLACE "mdoc-" "" cmd ${script})
  file(WRITE ${script} "#!/bin/sh\nexec mdoc ${cmd} \"$@\"\n")
endforeach()

install(PROGRAMS ${bin_SCRIPTS} DESTINATION bin)
