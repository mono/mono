// ILTables.cs
// Mechanically generated  - DO NOT EDIT!
//
// (C) Sergey Chaban (serge@wildwestsoftware.com)

using System;
using System.Collections;
using System.Reflection.Emit;

namespace Mono.ILASM {





        public sealed class ILTables {

                private static Hashtable keywords = null;
                private static Hashtable directives = null;
                private static readonly object mutex;


                private ILTables ()
                {
                }

                static ILTables ()
                {
                        mutex = new object ();
                }

                private static void AllocTable (ref Hashtable tbl, int size)
                {
                        lock (mutex) {
                                if (tbl == null)
                                        tbl = new Hashtable (size);
                        }
                }

                public static Hashtable Directives
                {
                        get {
                                if (directives != null) return directives;

                                AllocTable (ref directives, 300);

                                directives [".addon"] = new ILToken (Token.D_ADDON, ".addon");
                                directives [".algorithm"] = new ILToken (Token.D_ALGORITHM, ".algorithm");
                                directives [".assembly"] = new ILToken (Token.D_ASSEMBLY, ".assembly");
                                directives [".backing"] = new ILToken (Token.D_BACKING, ".backing");
                                directives [".blob"] = new ILToken (Token.D_BLOB, ".blob");
                                directives [".capability"] = new ILToken (Token.D_CAPABILITY, ".capability");
                                directives [".cctor"] = new ILToken (Token.D_CCTOR, ".cctor");
                                directives [".class"] = new ILToken (Token.D_CLASS, ".class");
                                directives [".comtype"] = new ILToken (Token.D_COMTYPE, ".comtype");
                                directives [".config"] = new ILToken (Token.D_CONFIG, ".config");
                                directives [".imagebase"] = new ILToken (Token.D_IMAGEBASE, ".imagebase");
                                directives [".corflags"] = new ILToken (Token.D_CORFLAGS, ".corflags");
                                directives [".ctor"] = new ILToken (Token.D_CTOR, ".ctor");
                                directives [".custom"] = new ILToken (Token.D_CUSTOM, ".custom");
                                directives [".data"] = new ILToken (Token.D_DATA, ".data");
                                directives [".emitbyte"] = new ILToken (Token.D_EMITBYTE, ".emitbyte");
                                directives [".entrypoint"] = new ILToken (Token.D_ENTRYPOINT, ".entrypoint");
                                directives [".event"] = new ILToken (Token.D_EVENT, ".event");
                                directives [".exeloc"] = new ILToken (Token.D_EXELOC, ".exeloc");
                                directives [".export"] = new ILToken (Token.D_EXPORT, ".export");
                                directives [".field"] = new ILToken (Token.D_FIELD, ".field");
                                directives [".file"] = new ILToken (Token.D_FILE, ".file");
                                directives [".fire"] = new ILToken (Token.D_FIRE, ".fire");
                                directives [".get"] = new ILToken (Token.D_GET, ".get");
                                directives [".hash"] = new ILToken (Token.D_HASH, ".hash");
                                directives [".implicitcom"] = new ILToken (Token.D_IMPLICITCOM, ".implicitcom");
                                directives [".language"] = new ILToken (Token.D_LANGUAGE, ".language");
                                directives [".line"] = new ILToken (Token.D_LINE, ".line");
                                directives ["#line"] = new ILToken (Token.D_XLINE, "#line");
                                directives [".locale"] = new ILToken (Token.D_LOCALE, ".locale");
                                directives [".locals"] = new ILToken (Token.D_LOCALS, ".locals");
                                directives [".manifestres"] = new ILToken (Token.D_MANIFESTRES, ".manifestres");
                                directives [".maxstack"] = new ILToken (Token.D_MAXSTACK, ".maxstack");
                                directives [".method"] = new ILToken (Token.D_METHOD, ".method");
                                directives [".mime"] = new ILToken (Token.D_MIME, ".mime");
                                directives [".module"] = new ILToken (Token.D_MODULE, ".module");
                                directives [".mresource"] = new ILToken (Token.D_MRESOURCE, ".mresource");
                                directives [".namespace"] = new ILToken (Token.D_NAMESPACE, ".namespace");
                                directives [".originator"] = new ILToken (Token.D_ORIGINATOR, ".originator");
                                directives [".os"] = new ILToken (Token.D_OS, ".os");
                                directives [".other"] = new ILToken (Token.D_OTHER, ".other");
                                directives [".override"] = new ILToken (Token.D_OVERRIDE, ".override");
                                directives [".pack"] = new ILToken (Token.D_PACK, ".pack");
                                directives [".param"] = new ILToken (Token.D_PARAM, ".param");
                                directives [".permission"] = new ILToken (Token.D_PERMISSION, ".permission");
                                directives [".permissionset"] = new ILToken (Token.D_PERMISSIONSET, ".permissionset");
                                directives [".processor"] = new ILToken (Token.D_PROCESSOR, ".processor");
                                directives [".property"] = new ILToken (Token.D_PROPERTY, ".property");
                                directives [".publickey"] = new ILToken (Token.D_PUBLICKEY, ".publickey");
                                directives [".publickeytoken"] = new ILToken (Token.D_PUBLICKEYTOKEN, ".publickeytoken");
                                directives [".removeon"] = new ILToken (Token.D_REMOVEON, ".removeon");
                                directives [".set"] = new ILToken (Token.D_SET, ".set");
                                directives [".size"] = new ILToken (Token.D_SIZE, ".size");
				directives [".stackreserve"] = new ILToken (Token.D_STACKRESERVE, ".stackreserve");
                                directives [".subsystem"] = new ILToken (Token.D_SUBSYSTEM, ".subsystem");
                                directives [".title"] = new ILToken (Token.D_TITLE, ".title");
                                directives [".try"] = new ILToken (Token.D_TRY, ".try");
                                directives [".ver"] = new ILToken (Token.D_VER, ".ver");
                                directives [".vtable"] = new ILToken (Token.D_VTABLE, ".vtable");
                                directives [".vtentry"] = new ILToken (Token.D_VTENTRY, ".vtentry");
                                directives [".vtfixup"] = new ILToken (Token.D_VTFIXUP, ".vtfixup");
                                directives [".zeroinit"] = new ILToken (Token.D_ZEROINIT, ".zeroinit");

                                return directives;
                        }
                }



                public static Hashtable Keywords
                {
                        get {
                                if (keywords != null) return keywords;

                                AllocTable (ref keywords, 300);

                                keywords ["at"] = new ILToken (Token.K_AT, "at");
                                keywords ["as"] = new ILToken (Token.K_AS, "as");
                                keywords ["implicitcom"] = new ILToken (Token.K_IMPLICITCOM, "implicitcom");
                                keywords ["implicitres"] = new ILToken (Token.K_IMPLICITRES, "implicitres");
                                keywords ["noappdomain"] = new ILToken (Token.K_NOAPPDOMAIN, "noappdomain");
                                keywords ["noprocess"] = new ILToken (Token.K_NOPROCESS, "noprocess");
                                keywords ["nomachine"] = new ILToken (Token.K_NOMACHINE, "nomachine");
                                keywords ["extern"] = new ILToken (Token.K_EXTERN, "extern");
                                keywords ["instance"] = new ILToken (Token.K_INSTANCE, "instance");
                                keywords ["explicit"] = new ILToken (Token.K_EXPLICIT, "explicit");
                                keywords ["default"] = new ILToken (Token.K_DEFAULT, "default");
                                keywords ["vararg"] = new ILToken (Token.K_VARARG, "vararg");
                                keywords ["unmanaged"] = new ILToken (Token.K_UNMANAGED, "unmanaged");
                                keywords ["cdecl"] = new ILToken (Token.K_CDECL, "cdecl");
                                keywords ["stdcall"] = new ILToken (Token.K_STDCALL, "stdcall");
                                keywords ["thiscall"] = new ILToken (Token.K_THISCALL, "thiscall");
                                keywords ["fastcall"] = new ILToken (Token.K_FASTCALL, "fastcall");
                                keywords ["marshal"] = new ILToken (Token.K_MARSHAL, "marshal");
                                keywords ["in"] = new ILToken (Token.K_IN, "in");
                                keywords ["out"] = new ILToken (Token.K_OUT, "out");
                                keywords ["opt"] = new ILToken (Token.K_OPT, "opt");
                                // Not a keyword according to ilasm 1.1
                                // keywords ["lcid"] = new ILToken (Token.K_LCID, "lcid");
                                //keywords ["retval"] = new ILToken (Token.K_RETVAL, "retval");
                                keywords ["static"] = new ILToken (Token.K_STATIC, "static");
                                keywords ["public"] = new ILToken (Token.K_PUBLIC, "public");
                                keywords ["private"] = new ILToken (Token.K_PRIVATE, "private");
                                keywords ["family"] = new ILToken (Token.K_FAMILY, "family");
                                keywords ["initonly"] = new ILToken (Token.K_INITONLY, "initonly");
                                keywords ["rtspecialname"] = new ILToken (Token.K_RTSPECIALNAME, "rtspecialname");
                                keywords ["specialname"] = new ILToken (Token.K_SPECIALNAME, "specialname");
                                keywords ["assembly"] = new ILToken (Token.K_ASSEMBLY, "assembly");
                                keywords ["famandassem"] = new ILToken (Token.K_FAMANDASSEM, "famandassem");
                                keywords ["famorassem"] = new ILToken (Token.K_FAMORASSEM, "famorassem");
                                keywords ["privatescope"] = new ILToken (Token.K_PRIVATESCOPE, "privatescope");
                                keywords ["literal"] = new ILToken (Token.K_LITERAL, "literal");
                                keywords ["notserialized"] = new ILToken (Token.K_NOTSERIALIZED, "notserialized");
                                keywords ["value"] = new ILToken (Token.K_VALUE, "value");
                                keywords ["not_in_gc_heap"] = new ILToken (Token.K_NOT_IN_GC_HEAP, "not_in_gc_heap");
                                keywords ["interface"] = new ILToken (Token.K_INTERFACE, "interface");
                                keywords ["sealed"] = new ILToken (Token.K_SEALED, "sealed");
                                keywords ["abstract"] = new ILToken (Token.K_ABSTRACT, "abstract");
                                keywords ["auto"] = new ILToken (Token.K_AUTO, "auto");
                                keywords ["sequential"] = new ILToken (Token.K_SEQUENTIAL, "sequential");
                                keywords ["ansi"] = new ILToken (Token.K_ANSI, "ansi");
                                keywords ["unicode"] = new ILToken (Token.K_UNICODE, "unicode");
                                keywords ["autochar"] = new ILToken (Token.K_AUTOCHAR, "autochar");
                                keywords ["bestfit"] = new ILToken (Token.K_BESTFIT, "bestfit");
                                keywords ["charmaperror"] = new ILToken (Token.K_CHARMAPERROR, "charmaperror");
                                keywords ["import"] = new ILToken (Token.K_IMPORT, "import");
                                keywords ["serializable"] = new ILToken (Token.K_SERIALIZABLE, "serializable");
                                keywords ["nested"] = new ILToken (Token.K_NESTED, "nested");
                                keywords ["lateinit"] = new ILToken (Token.K_LATEINIT, "lateinit");
                                keywords ["extends"] = new ILToken (Token.K_EXTENDS, "extends");
                                keywords ["implements"] = new ILToken (Token.K_IMPLEMENTS, "implements");
                                keywords ["final"] = new ILToken (Token.K_FINAL, "final");
                                keywords ["virtual"] = new ILToken (Token.K_VIRTUAL, "virtual");
                                keywords ["hidebysig"] = new ILToken (Token.K_HIDEBYSIG, "hidebysig");
                                keywords ["newslot"] = new ILToken (Token.K_NEWSLOT, "newslot");
                                keywords ["unmanagedexp"] = new ILToken (Token.K_UNMANAGEDEXP, "unmanagedexp");
                                keywords ["pinvokeimpl"] = new ILToken (Token.K_PINVOKEIMPL, "pinvokeimpl");
                                keywords ["nomangle"] = new ILToken (Token.K_NOMANGLE, "nomangle");
                                keywords ["ole"] = new ILToken (Token.K_OLE, "ole");
                                keywords ["lasterr"] = new ILToken (Token.K_LASTERR, "lasterr");
                                keywords ["winapi"] = new ILToken (Token.K_WINAPI, "winapi");
                                keywords ["native"] = new ILToken (Token.K_NATIVE, "native");
                                keywords ["il"] = new ILToken (Token.K_IL, "il");
                                keywords ["cil"] = new ILToken (Token.K_CIL, "cil");
                                keywords ["optil"] = new ILToken (Token.K_OPTIL, "optil");
                                keywords ["managed"] = new ILToken (Token.K_MANAGED, "managed");
                                keywords ["forwardref"] = new ILToken (Token.K_FORWARDREF, "forwardref");
                                keywords ["runtime"] = new ILToken (Token.K_RUNTIME, "runtime");
                                keywords ["internalcall"] = new ILToken (Token.K_INTERNALCALL, "internalcall");
                                keywords ["synchronized"] = new ILToken (Token.K_SYNCHRONIZED, "synchronized");
                                keywords ["noinlining"] = new ILToken (Token.K_NOINLINING, "noinlining");
                                keywords ["custom"] = new ILToken (Token.K_CUSTOM, "custom");
                                keywords ["fixed"] = new ILToken (Token.K_FIXED, "fixed");
                                keywords ["sysstring"] = new ILToken (Token.K_SYSSTRING, "sysstring");
                                keywords ["array"] = new ILToken (Token.K_ARRAY, "array");
                                keywords ["variant"] = new ILToken (Token.K_VARIANT, "variant");
                                keywords ["currency"] = new ILToken (Token.K_CURRENCY, "currency");
                                keywords ["syschar"] = new ILToken (Token.K_SYSCHAR, "syschar");
                                keywords ["void"] = new ILToken (Token.K_VOID, "void");
                                keywords ["bool"] = new ILToken (Token.K_BOOL, "bool");
                                keywords ["int8"] = new ILToken (Token.K_INT8, "int8");
                                keywords ["int16"] = new ILToken (Token.K_INT16, "int16");
                                keywords ["int32"] = new ILToken (Token.K_INT32, "int32");
                                keywords ["int64"] = new ILToken (Token.K_INT64, "int64");
                                keywords ["float32"] = new ILToken (Token.K_FLOAT32, "float32");
                                keywords ["float64"] = new ILToken (Token.K_FLOAT64, "float64");
                                keywords ["error"] = new ILToken (Token.K_ERROR, "error");
                                keywords ["unsigned"] = new ILToken (Token.K_UNSIGNED, "unsigned");
                                keywords ["uint"] = new ILToken (Token.K_UINT, "uint");
                                keywords ["uint8"] = new ILToken (Token.K_UINT8, "uint8");
                                keywords ["uint16"] = new ILToken (Token.K_UINT16, "uint16");
                                keywords ["uint32"] = new ILToken (Token.K_UINT32, "uint32");
                                keywords ["uint64"] = new ILToken (Token.K_UINT64, "uint64");
                                keywords ["decimal"] = new ILToken (Token.K_DECIMAL, "decimal");
                                keywords ["date"] = new ILToken (Token.K_DATE, "date");
                                keywords ["bstr"] = new ILToken (Token.K_BSTR, "bstr");
                                keywords ["lpstr"] = new ILToken (Token.K_LPSTR, "lpstr");
                                keywords ["lpwstr"] = new ILToken (Token.K_LPWSTR, "lpwstr");
                                keywords ["lptstr"] = new ILToken (Token.K_LPTSTR, "lptstr");
                                keywords ["objectref"] = new ILToken (Token.K_OBJECTREF, "objectref");
                                keywords ["iunknown"] = new ILToken (Token.K_IUNKNOWN, "iunknown");
                                keywords ["idispatch"] = new ILToken (Token.K_IDISPATCH, "idispatch");
                                keywords ["struct"] = new ILToken (Token.K_STRUCT, "struct");
                                keywords ["safearray"] = new ILToken (Token.K_SAFEARRAY, "safearray");
                                keywords ["int"] = new ILToken (Token.K_INT, "int");
                                keywords ["byvalstr"] = new ILToken (Token.K_BYVALSTR, "byvalstr");
                                keywords ["tbstr"] = new ILToken (Token.K_TBSTR, "tbstr");
                                keywords ["lpvoid"] = new ILToken (Token.K_LPVOID, "lpvoid");
                                keywords ["any"] = new ILToken (Token.K_ANY, "any");
                                keywords ["float"] = new ILToken (Token.K_FLOAT, "float");
                                keywords ["lpstruct"] = new ILToken (Token.K_LPSTRUCT, "lpstruct");
                                keywords ["null"] = new ILToken (Token.K_NULL, "null");
                                //              keywords ["ptr"] = new ILToken (Token.K_PTR, "ptr");
                                keywords ["vector"] = new ILToken (Token.K_VECTOR, "vector");
                                keywords ["hresult"] = new ILToken (Token.K_HRESULT, "hresult");
                                keywords ["carray"] = new ILToken (Token.K_CARRAY, "carray");
                                keywords ["userdefined"] = new ILToken (Token.K_USERDEFINED, "userdefined");
                                keywords ["record"] = new ILToken (Token.K_RECORD, "record");
                                keywords ["filetime"] = new ILToken (Token.K_FILETIME, "filetime");
                                keywords ["blob"] = new ILToken (Token.K_BLOB, "blob");
                                keywords ["stream"] = new ILToken (Token.K_STREAM, "stream");
                                keywords ["storage"] = new ILToken (Token.K_STORAGE, "storage");
                                keywords ["streamed_object"] = new ILToken (Token.K_STREAMED_OBJECT, "streamed_object");
                                keywords ["stored_object"] = new ILToken (Token.K_STORED_OBJECT, "stored_object");
                                keywords ["blob_object"] = new ILToken (Token.K_BLOB_OBJECT, "blob_object");
                                keywords ["cf"] = new ILToken (Token.K_CF, "cf");
                                keywords ["clsid"] = new ILToken (Token.K_CLSID, "clsid");
                                keywords ["method"] = new ILToken (Token.K_METHOD, "method");
                                keywords ["class"] = new ILToken (Token.K_CLASS, "class");
                                keywords ["pinned"] = new ILToken (Token.K_PINNED, "pinned");
                                keywords ["modreq"] = new ILToken (Token.K_MODREQ, "modreq");
                                keywords ["modopt"] = new ILToken (Token.K_MODOPT, "modopt");
                                keywords ["typedref"] = new ILToken (Token.K_TYPEDREF, "typedref");
                                keywords ["property"] = new ILToken (Token.K_PROPERTY, "property");
#if NET_2_0 || BOOTSTRAP_NET_2_0
                                keywords ["type"] = new ILToken (Token.K_TYPE, "type");
#endif
                                keywords ["refany"] = new ILToken (Token.K_TYPEDREF, "typedref");
                                keywords ["wchar"] = new ILToken (Token.K_WCHAR, "wchar");
                                keywords ["char"] = new ILToken (Token.K_CHAR, "char");
                                keywords ["fromunmanaged"] = new ILToken (Token.K_FROMUNMANAGED, "fromunmanaged");
                                keywords ["callmostderived"] = new ILToken (Token.K_CALLMOSTDERIVED, "callmostderived");
                                keywords ["bytearray"] = new ILToken (Token.K_BYTEARRAY, "bytearray");
                                keywords ["with"] = new ILToken (Token.K_WITH, "with");
                                keywords ["init"] = new ILToken (Token.K_INIT, "init");
                                keywords ["to"] = new ILToken (Token.K_TO, "to");
                                keywords ["catch"] = new ILToken (Token.K_CATCH, "catch");
                                keywords ["filter"] = new ILToken (Token.K_FILTER, "filter");
                                keywords ["finally"] = new ILToken (Token.K_FINALLY, "finally");
                                keywords ["fault"] = new ILToken (Token.K_FAULT, "fault");
                                keywords ["handler"] = new ILToken (Token.K_HANDLER, "handler");
                                keywords ["tls"] = new ILToken (Token.K_TLS, "tls");
                                keywords ["field"] = new ILToken (Token.K_FIELD, "field");
                                keywords ["request"] = new ILToken (Token.K_REQUEST, "request");
                                keywords ["demand"] = new ILToken (Token.K_DEMAND, "demand");
                                keywords ["assert"] = new ILToken (Token.K_ASSERT, "assert");
                                keywords ["deny"] = new ILToken (Token.K_DENY, "deny");
                                keywords ["permitonly"] = new ILToken (Token.K_PERMITONLY, "permitonly");
                                keywords ["linkcheck"] = new ILToken (Token.K_LINKCHECK, "linkcheck");
                                keywords ["inheritcheck"] = new ILToken (Token.K_INHERITCHECK, "inheritcheck");
                                keywords ["reqmin"] = new ILToken (Token.K_REQMIN, "reqmin");
                                keywords ["reqopt"] = new ILToken (Token.K_REQOPT, "reqopt");
                                keywords ["reqrefuse"] = new ILToken (Token.K_REQREFUSE, "reqrefuse");
                                keywords ["prejitgrant"] = new ILToken (Token.K_PREJITGRANT, "prejitgrant");
                                keywords ["prejitdeny"] = new ILToken (Token.K_PREJITDENY, "prejitdeny");
                                keywords ["noncasdemand"] = new ILToken (Token.K_NONCASDEMAND, "noncasdemand");
                                keywords ["noncaslinkdemand"] = new ILToken (Token.K_NONCASLINKDEMAND, "noncaslinkdemand");
                                keywords ["noncasinheritance"] = new ILToken (Token.K_NONCASINHERITANCE, "noncasinheritance");
                                keywords ["readonly"] = new ILToken (Token.K_READONLY, "readonly");
                                keywords ["nometadata"] = new ILToken (Token.K_NOMETADATA, "nometadata");
                                keywords ["algorithm"] = new ILToken (Token.K_ALGORITHM, "algorithm");
                                keywords ["fullorigin"] = new ILToken (Token.K_FULLORIGIN, "fullorigin");
                                // keywords ["nan"] = new ILToken (Token.K_NAN, "nan");
                                // keywords ["inf"] = new ILToken (Token.K_INF, "inf");
                                // keywords ["publickey"] = new ILToken (Token.K_PUBLICKEY, "publickey");
                                keywords ["enablejittracking"] = new ILToken (Token.K_ENABLEJITTRACKING, "enablejittracking");
                                keywords ["disablejitoptimizer"] = new ILToken (Token.K_DISABLEJITOPTIMIZER, "disablejitoptimizer");
                                keywords ["retargetable"] = new ILToken (Token.K_RETARGETABLE, "retargetable");
                                keywords ["preservesig"] = new ILToken (Token.K_PRESERVESIG, "preservesig");
                                keywords ["beforefieldinit"] = new ILToken (Token.K_BEFOREFIELDINIT, "beforefieldinit");
                                keywords ["alignment"] = new ILToken (Token.K_ALIGNMENT, "alignment");
                                keywords ["nullref"] = new ILToken (Token.K_NULLREF, "nullref");
                                keywords ["valuetype"] = new ILToken (Token.K_VALUETYPE, "valuetype");
                                keywords ["compilercontrolled"] = new ILToken (Token.K_COMPILERCONTROLLED, "compilercontrolled");
                                keywords ["reqsecobj"] = new ILToken (Token.K_REQSECOBJ, "reqsecobj");
                                keywords ["enum"] = new ILToken (Token.K_ENUM, "enum");
                                keywords ["object"] = new ILToken (Token.K_OBJECT, "object");
                                keywords ["string"] = new ILToken (Token.K_STRING, "string");
                                keywords ["true"] = new ILToken (Token.K_TRUE, "true");
                                keywords ["false"] = new ILToken (Token.K_FALSE, "false");
                                keywords ["is"] = new ILToken (Token.K_IS, "is");
                                keywords ["on"] = new ILToken (Token.K_ON, "on");
                                keywords ["off"] = new ILToken (Token.K_OFF, "off");
				keywords ["strict"] = new ILToken (Token.K_STRICT, "strict");

                                return keywords;
                        }
                }




        } // class ILTables




} // namespace Mono.ILASM
