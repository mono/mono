using System;
using System.IO;
using System.Collections;
using System.Text;

namespace PEAPI 
{
  public class Hex {
    readonly static char[] hexDigit = {'0','1','2','3','4','5','6','7',
                                        '8','9','A','B','C','D','E','F'};
    readonly static uint[] iByteMask = {0x000000FF, 0x0000FF00, 0x00FF0000, 0xFF000000};
    readonly static ulong[] lByteMask = {0x00000000000000FF, 0x000000000000FF00, 
                                          0x0000000000FF0000, 0x00000000FF000000,
                                          0x000000FF00000000, 0x0000FF0000000000,
                                          0x00FF000000000000, 0xFF00000000000000 };
    readonly static uint nibble0Mask = 0x0000000F;
    readonly static uint nibble1Mask = 0x000000F0;

    public static String Byte(int b) {
      char[] str = new char[2];
      uint num = (uint)b;
      uint b1 = num & nibble0Mask;
      uint b2 = (num & nibble1Mask) >> 4;
      str[0] = hexDigit[b2];
      str[1] = hexDigit[b1];
      return new String(str);
    }

    public static String Short(int b) {
      char[] str = new char[4];
      uint num1 = (uint)b & iByteMask[0];
      uint num2 = ((uint)b & iByteMask[1]) >> 8;
      uint b1 = num1 & nibble0Mask;
      uint b2 = (num1 & nibble1Mask) >> 4;
      uint b3 = num2 & nibble0Mask;
      uint b4 = (num2 & nibble1Mask) >> 4;
      str[0] = hexDigit[b4];
      str[1] = hexDigit[b3];
      str[2] = hexDigit[b2];
      str[3] = hexDigit[b1];
      return new String(str);
    }

    public static String Int(int val) {
      char[] str = new char[8];
      uint num = (uint)val;
      int strIx = 7;
      for (int i=0; i < iByteMask.Length; i++) {
        uint b = num & iByteMask[i];
        b >>= (i*8);
        uint b1 = b & nibble0Mask;
        uint b2 = (b & nibble1Mask) >> 4;
        str[strIx--] = hexDigit[b1];
        str[strIx--] = hexDigit[b2];
      }
      return new String(str);
    }
 
    public static String Int(uint num) {
      char[] str = new char[8];
      int strIx = 7;
      for (int i=0; i < iByteMask.Length; i++) {
        uint b = num & iByteMask[i];
        b >>= (i*8);
        uint b1 = b & nibble0Mask;
        uint b2 = (b & nibble1Mask) >> 4;
        str[strIx--] = hexDigit[b1];
        str[strIx--] = hexDigit[b2];
      }
      return new String(str);
    }

    public static String Long(long lnum) {
      ulong num = (ulong)lnum;
      char[] str = new char[16];
      int strIx = 15;
      for (int i=0; i < lByteMask.Length; i++) {
        ulong b = num & lByteMask[i];
        b >>= (i*8);
        ulong b1 = b & nibble0Mask;
        ulong b2 = (b & nibble1Mask) >> 4;
        str[strIx--] = hexDigit[b1];
        str[strIx--] = hexDigit[b2];
      }
      return new String(str);
    }
  }

  public class NotYetImplementedException : System.Exception 
  {
     public NotYetImplementedException(string msg) : base(msg + " Not Yet Implemented") { }
  }

  public class TypeSignatureException : System.Exception {
    public TypeSignatureException(string msg) : base(msg) { }
  }

                   public class ClassRefInst : Type {

                          private Type type;
                          private bool is_value;

            public ClassRefInst (Type type, bool is_value) : base (0x12) {
                    this.type = type;
                    this.is_value = is_value;
                    if (is_value)
                            type.SetTypeIndex (0x11);
                    tabIx = MDTable.TypeSpec;
            }

            internal sealed override void TypeSig(MemoryStream str) {
                    type.TypeSig (str);
            }
    }
                  
  public class MVar : Type {

            private int index;

            public MVar (int index) : base (0x1E) {
                    this.index = index;
                    tabIx = MDTable.TypeSpec;
            }

            internal sealed override void TypeSig(MemoryStream str) {
                    str.WriteByte(typeIndex);
                    MetaData.CompressNum ((uint) index, str);
            }
    }

    public class GenericTypeSpec : Type {

            private int index;

            public GenericTypeSpec (int index) : base (0x13) {
                    this.index = index;
                    tabIx = MDTable.TypeSpec;
            }

            internal sealed override void TypeSig(MemoryStream str) {
                    str.WriteByte(typeIndex);
                    MetaData.CompressNum ((uint) index, str);
            }
    }

  public class GenericTypeInst : Type {

          private Type gen_type;
          private Type[] gen_param;

          public GenericTypeInst (Type gen_type, Type[] gen_param) : base (0x15)
          {
                  typeIndex = 0x15;
                  this.gen_type = gen_type;
                  this.gen_param = gen_param;
                  tabIx = MDTable.TypeSpec;
  }

          internal sealed override void TypeSig(MemoryStream str) {
                  str.WriteByte(typeIndex);
                  gen_type.TypeSig (str);
                  MetaData.CompressNum ((uint) gen_param.Length, str);
                  foreach (Type param in gen_param)
                          param.TypeSig (str);
            }
  }

  public class GenericMethodSig {

          private Type[] gen_param;

          public GenericMethodSig (Type[] gen_param)
          {
                  this.gen_param = gen_param;
          }

          internal void TypeSig (MemoryStream str)
          {
                  MetaData.CompressNum ((uint) gen_param.Length, str); // THIS IS NOT RIGHT, but works
                  MetaData.CompressNum ((uint) gen_param.Length, str);
                  foreach (Type param in gen_param)
                          param.TypeSig (str);
          }

          internal uint GetSigIx (MetaData md)
          {
                  MemoryStream sig = new MemoryStream();
                  TypeSig (sig);
                  return md.AddToBlobHeap (sig.ToArray());
          }
  }

        public class Sentinel : Type {

            public Sentinel () : base (0x41) { }

            internal sealed override void TypeSig(MemoryStream str) {
                    str.WriteByte(typeIndex);
            }
        }

        /// <summary>
        /// The IL Array type
        /// </summary>
        public abstract class Array : Type
        {

    protected Type elemType;
                protected MetaData metaData;
                protected string cnameSpace, cname;

    internal Array(Type eType, byte TypeId) : base(TypeId) {
      elemType = eType;
                        tabIx = MDTable.TypeSpec;
    }

        }

  /**************************************************************************/  
  
  /// <summary>
  /// Single dimensional array with zero lower bound
  /// </summary>
  public class ZeroBasedArray : Array {

    /// <summary>
    /// Create a new array  -   elementType[]
    /// </summary>
    /// <param name="elementType">the type of the array elements</param>
    public ZeroBasedArray(Type elementType) : base (elementType,0x1D) { }

    internal sealed override void TypeSig(MemoryStream str) {
      str.WriteByte(typeIndex);
      elemType.TypeSig(str); 
    }

  }


  /**************************************************************************/           

  /// <summary>
  /// Multi dimensional array with explicit bounds
  /// </summary>
  public class BoundArray : Array {
    int[] lowerBounds;
    int[] sizes;
    uint numDims;

    /// <summary>
    /// Create a new multi dimensional array type 
    /// eg. elemType[1..5,3..10,5,,] would be 
    /// new BoundArray(elemType,5,[1,3,0],[5,10,4])
    /// </summary>
    /// <param name="elementType">the type of the elements</param>
    /// <param name="dimensions">the number of dimensions</param>
    /// <param name="loBounds">lower bounds of dimensions</param>
    /// <param name="upBounds">upper bounds of dimensions</param>
    public BoundArray(Type elementType, uint dimensions, int[] loBounds, 
      int[] upBounds) : base (elementType,0x14) {
      numDims = dimensions;
      lowerBounds = loBounds;
      sizes = new int[loBounds.Length];
      for (int i=0; i < loBounds.Length; i++) {
        sizes[i] = upBounds[i] - loBounds[i] + 1;
      }
    }

    /// <summary>
    /// Create a new multi dimensional array type 
    /// eg. elemType[5,10,20] would be new BoundArray(elemType,3,[5,10,20])
    /// </summary>
    /// <param name="elementType">the type of the elements</param>
    /// <param name="dimensions">the number of dimensions</param>
    /// <param name="size">the sizes of the dimensions</param>
    public BoundArray(Type elementType, uint dimensions, int[] size) 
                                                  : base (elementType,0x14) {
      numDims = dimensions;
      sizes = size;
    }

    /// <summary>
    /// Create a new multi dimensional array type 
    /// eg. elemType[,,] would be new BoundArray(elemType,3)
    /// </summary>
    /// <param name="elementType">the type of the elements</param>
    /// <param name="dimensions">the number of dimensions</param>
    public BoundArray(Type elementType, uint dimensions)
                                                  : base (elementType,0x14) {
      numDims = dimensions;
    }

    internal sealed override void TypeSig(MemoryStream str) {
      str.WriteByte(typeIndex);
      elemType.TypeSig(str);
      MetaData.CompressNum(numDims,str);
      if ((sizes != null) && (sizes.Length > 0)) {
        MetaData.CompressNum((uint)sizes.Length,str);
        for (int i=0; i < sizes.Length; i++) {
          MetaData.CompressNum((uint)sizes[i],str);
        }
      } else str.WriteByte(0);
      if ((lowerBounds != null) && (lowerBounds.Length > 0)) {
        MetaData.CompressNum((uint)lowerBounds.Length,str);
        for (int i=0; i < lowerBounds.Length; i++) {
          MetaData.CompressNum((uint)lowerBounds[i],str);
        }
      } else str.WriteByte(0);
    }
  
  }
  /**************************************************************************/  
  /// <summary>
  /// Descriptor for THIS assembly (.assembly)
  /// </summary>
  public class Assembly : ResolutionScope 
  {
    ushort majorVer, minorVer, buildNo, revisionNo;
    uint flags;
    uint hashAlgId;
    uint keyIx = 0, cultIx = 0;
    
    internal Assembly(string name, MetaData md) : base(name,md) {
      tabIx = MDTable.Assembly;
    }

    /// <summary>
    /// Add details about THIS assembly
    /// </summary>
    /// <param name="majVer">Major Version</param>
    /// <param name="minVer">Minor Version</param>
    /// <param name="bldNo">Build Number</param>
    /// <param name="revNo">Revision Number</param>
    /// <param name="key">Hash Key</param>
    /// <param name="hash">Hash Algorithm</param>
    /// <param name="cult">Culture</param>
    public void AddAssemblyInfo(int majVer, int minVer, int bldNo, int revNo, 
                              byte[] key, uint hash, string cult) {
      majorVer = (ushort)majVer;
      minorVer = (ushort)minVer;
      buildNo = (ushort)bldNo;
      revisionNo = (ushort)revNo;
      hashAlgId = hash;
      keyIx = metaData.AddToBlobHeap(key);
      cultIx = metaData.AddToStringsHeap(cult);
    }

    /// <summary>
    /// Add an attribute to THIS assembly
    /// </summary>
    /// <param name="aa">assembly attribute</param>
    public void AddAssemblyAttr(AssemAttr aa) {
      flags |= (uint)aa;
    }

    internal sealed override uint Size(MetaData md) {
      return 16 + md.BlobIndexSize() + 2 * md.StringsIndexSize();
    }

    internal sealed override void Write(FileImage output) {
//      Console.WriteLine("Writing assembly element with nameIx of " + nameIx + " at file offset " + output.Seek(0,SeekOrigin.Current));
      output.Write((uint)hashAlgId);
      output.Write(majorVer);
      output.Write(minorVer);
      output.Write(buildNo);
      output.Write(revisionNo);
      output.Write(flags);
      output.BlobIndex(keyIx);
      output.StringsIndex(nameIx);
      output.StringsIndex(cultIx);
    }

    internal sealed override uint GetCodedIx(CIx code) {
      switch (code) {
        case (CIx.HasCustomAttr) : return 14; 
        case (CIx.HasDeclSecurity) : return 2; 
      }
      return 0;
    }

  }     
  /**************************************************************************/  

        public interface IExternRef  {
                ClassRef AddClass(string nsName, string name);
                ClassRef AddValueClass(string nsName, string name);
        }
        
        /// <summary>
        /// A reference to an external assembly (.assembly extern)
        /// </summary>
        public class AssemblyRef : ResolutionScope, IExternRef
        {
    private ushort major, minor, build, revision;
    uint flags, keyIx, hashIx, cultIx;
    bool hasVersion = false, isKeyToken = false;
    byte[] keyBytes;
    string culture;

    internal AssemblyRef(MetaData md, string name) : base(name,md) {
      tabIx = MDTable.AssemblyRef;
                }

    /// <summary>
    /// Add version information about this external assembly
    /// </summary>
    /// <param name="majVer">Major Version</param>
    /// <param name="minVer">Minor Version</param>
    /// <param name="bldNo">Build Number</param>
    /// <param name="revNo">Revision Number</param>
    public void AddVersionInfo(int majVer, int minVer, int bldNo, int revNo) {
      major = (ushort)majVer;
      minor = (ushort)minVer;
      build = (ushort)bldNo;
      revision = (ushort)revNo;
      hasVersion = true;
    }

    /// <summary>
    /// Add the hash value for this external assembly
    /// </summary>
    /// <param name="hash">bytes of the hash value</param>
    public void AddHash(byte[] hash) {
      hashIx = metaData.AddToBlobHeap(hash); 
    }

    /// <summary>
    /// Set the culture for this external assembly
    /// </summary>
    /// <param name="cult">the culture string</param>
    public void AddCulture(string cult) {
      cultIx = metaData.AddToStringsHeap(cult);
      culture = cult;
    }

    /// <summary>
    /// Add the full public key for this external assembly
    /// </summary>
    /// <param name="key">bytes of the public key</param>
    public void AddKey(byte[] key) {
      flags |= 0x0001;   // full public key
      keyBytes = key;
      keyIx = metaData.AddToBlobHeap(key); 
    }

    /// <summary>
    /// Add the public key token (low 8 bytes of the public key)
    /// </summary>
    /// <param name="key">low 8 bytes of public key</param>
    public void AddKeyToken(byte[] key) {
      keyIx = metaData.AddToBlobHeap(key); 
      keyBytes = key;
      isKeyToken = true;
    }

    /// <summary>
    /// Add a class to this external assembly
    /// </summary>
    /// <param name="nsName">name space name</param>
    /// <param name="name">class name</param>
    /// <returns></returns>
    public virtual ClassRef AddClass(string nsName, string name) {
      ClassRef aClass = new ClassRef(nsName,name,metaData);
      metaData.AddToTable(MDTable.TypeRef,aClass);
      aClass.SetParent(this);
      return aClass;
    }

    /// <summary>
    /// Add a value class to this external assembly
    /// </summary>
    /// <param name="nsName">name space name</param>
    /// <param name="name">class name</param>
    /// <returns></returns>
    public virtual ClassRef AddValueClass(string nsName, string name) {
      ClassRef aClass = new ClassRef(nsName,name,metaData);
      metaData.AddToTable(MDTable.TypeRef,aClass);
      aClass.SetParent(this);
      aClass.MakeValueClass();
      return aClass;
    }

    internal string TypeName() {
      string result = name;
      if (hasVersion) 
        result = result + ", Version=" + major + "." + minor + "." + 
                  build + "." + revision;
      if (keyBytes != null) {
        string tokenStr = "=";
        if (isKeyToken) tokenStr = "Token=";
        result = result + ", PublicKey" + tokenStr;
        for (int i=0; i < keyBytes.Length; i++) {
          result = result + Hex.Byte(keyBytes[i]);
        }
      }
      if (culture != null) 
        result = result + ", Culture=" + culture;
      return result;
    }

    internal sealed override uint Size(MetaData md) {
      return 12 + 2 * md.StringsIndexSize() + 2 * md.BlobIndexSize();
    }

    internal sealed override void Write(FileImage output) {
      output.Write(major);
      output.Write(minor);
      output.Write(build);
      output.Write(revision);
      output.Write(flags);
      output.BlobIndex(keyIx);
      output.StringsIndex(nameIx);
      output.StringsIndex(cultIx);
      output.BlobIndex(hashIx);
    }

    internal sealed override uint GetCodedIx(CIx code) {
      switch (code) {
        case (CIx.ResolutionScope) : return 2; 
        case (CIx.HasCustomAttr) : return 15; 
        case (CIx.Implementation) : return 1; 
      }
      return 0;
    }

        }
  /**************************************************************************/  

  /// <summary>
  /// flags for the assembly (.corflags)
  /// </summary>
  public enum CorFlags {CF_IL_ONLY, CF_32_BITREQUIRED, CF_STRONGNAMESIGNED, 
                        CF_TRACKDEBUGDATA}

  /// <summary>
  /// subsystem for the assembly (.subsystem)
  /// </summary>
  public enum SubSystem { Native = 1, Windows_GUI = 2, 
    Windows_CUI = 3, OS2_CUI = 5, POSIX_CUI = 7, Native_Windows = 8, 
    Windows_CE_GUI = 9}
 
  /// <summary>
  /// Hash algorithms for the assembly
  /// </summary>
  public enum HashAlgorithm { None, SHA1 }

  /// <summary>
  /// Attributes for this assembly
  /// </summary>
  public enum AssemAttr { EnableJITCompileTracking = 0x8000, 
                          DisableJITCompileOptimizer = 0x4000}

  /// <summary>
  /// Method call conventions
  /// </summary>
  public enum CallConv { Default, Cdecl, Stdcall, Thiscall, 
    Fastcall, Vararg, Instance = 0x20, Generic = 0x10, InstanceExplicit = 0x60 }

  /// <summary>
  /// Type custom modifier
  /// </summary>
  public enum CustomModifier { modreq = 0x1F, modopt };

  /// <summary>
  /// Attibutes for a class
  /// </summary>
  public enum TypeAttr {Private, Public, NestedPublic, NestedPrivate, 
    NestedFamily, NestedAssembly, NestedFamAndAssem, NestedFamOrAssem, 
    SequentialLayout, ExplicitLayout = 0x10, Interface = 0x20, 
    Abstract = 0x80, PublicAbstract = 0x81, Sealed = 0x100, 
    PublicSealed = 0x101, SpecialName = 0x400, RTSpecialName = 0x800, 
    Import = 0x1000, Serializable = 0x2000, UnicodeClass = 0x10000,
    AutoClass = 0x20000, BeforeFieldInit = 0x100000 }

  /// <summary>
  /// Attributes for a field
  /// </summary>
  public enum FieldAttr {Default, Private, FamAndAssem, Assembly, 
    Family, FamOrAssem, Public, Static = 0x10, PublicStatic = 0x16, 
    Initonly = 0x20, Literal = 0x40, Notserialized = 0x80, 
    SpecialName = 0x200, RTSpecialName = 0x400 }
  
  /// <summary>
  /// Attributes for a method
  /// </summary>
  public enum MethAttr { Default, Private, FamAndAssem, Assembly,
    Family, FamOrAssem, Public, Static = 0x0010, PublicStatic = 0x16, 
    Final = 0x0020, PublicStaticFinal = 0x36, Virtual = 0x0040, 
    PrivateVirtual, PublicVirtual = 0x0046, HideBySig = 0x0080, 
    NewSlot = 0x0100, Abstract = 0x0400, SpecialName = 0x0800,
    RTSpecialName = 0x1000, SpecialRTSpecialName = 0x1800, 
    RequireSecObject = 0x8000}

  /// <summary>
  /// Attributes for .pinvokeimpl method declarations
  /// </summary>
  public enum PInvokeAttr { nomangle = 1, ansi = 2, unicode = 4, autochar = 6,
                            lasterr = 0x0040, winapi = 0x0100, cdecl = 0x0200,
                            stdcall = 0x0300, thiscall = 0x0400, fastcall = 0x0500 }

  /// <summary>
  /// Implementation attributes for a method
  /// </summary>
  public enum ImplAttr { IL, Native, Runtime = 0x03, Unmanaged = 0x04,
    ForwardRef = 0x10, PreserveSig = 0x0080, InternalCall = 0x1000, 
    Synchronised = 0x0020, Synchronized = 0x0020, NoInLining = 0x0008, Optil = 0x0002}

  /// <summary>
  /// Modes for a parameter
  /// </summary>
  public enum ParamAttr { Default, In, Out, Opt = 4 }

  /// <summary>
  /// CIL instructions
  /// </summary>
  public enum Op { nop, breakOp, ldarg_0, ldarg_1, ldarg_2, ldarg_3,
    ldloc_0, ldloc_1, ldloc_2, ldloc_3, stloc_0, stloc_1, stloc_2, stloc_3, 
    ldnull = 0x14, ldc_i4_m1, ldc_i4_0, ldc_i4_1, ldc_i4_2, ldc_i4_3, 
    ldc_i4_4, ldc_i4_5, ldc_i4_6, ldc_i4_7, ldc_i4_8, dup = 0x25, pop, 
    ret = 0x2A, ldind_i1 = 0x46, ldind_u1, ldind_i2, ldind_u2, ldind_i4, 
    ldind_u4, ldind_i8, ldind_i,  ldind_r4, ldind_r8, ldind_ref, stind_ref, 
    stind_i1, stind_i2, stind_i4, stind_i8, stind_r4, stind_r8, add, sub, mul,
    div, div_un, rem, rem_un, and, or, xor, shl, shr, shr_un, neg, not, 
    conv_i1, conv_i2, conv_i4, conv_i8, conv_r4, conv_r8, conv_u4, conv_u8, 
    conv_r_un = 0x76, throwOp = 0x7A, conv_ovf_i1_un = 0x82, conv_ovf_i2_un,
    conv_ovf_i4_un, conv_ovf_i8_un, conf_ovf_u1_un, conv_ovf_u2_un, 
    conv_ovf_u4_un, conv_ovf_u8_un, conv_ovf_i_un, conv_ovf_u_un, 
    ldlen = 0x8E, ldelem_i1 = 0x90, ldelem_u1, ldelem_i2, ldelem_u2, 
    ldelem_i4, ldelem_u4, ldelem_i8, ldelem_i, ldelem_r4, ldelem_r8, 
    ldelem_ref, stelem_i, stelem_i1, stelem_i2, stelem_i4, stelem_i8, stelem_r4 = 0xA0, stelem_r8,
    stelem_ref, conv_ovf_i1 = 0xb3, conv_ovf_u1, conv_ovf_i2, conv_ovf_u2, 
    conv_ovf_i4, conv_ovf_u4, conv_ovf_i8, conv_ovf_u8, ckfinite = 0xC3, 
    conv_u2 = 0xD1, conv_u1, conv_i, conv_ovf_i, conv_ovf_u, add_ovf, 
    add_ovf_un, mul_ovf, mul_ovf_un, sub_ovf, sub_ovf_un, endfinally, 
    stind_i = 0xDF, conv_u, arglist = 0xFE00, ceq, cgt, cgt_un, clt, clt_un, 
    localloc = 0xFE0F, endfilter = 0xFE11, volatile_ = 0xFE13, tail_, 
    cpblk = 0xFE17, initblk, rethrow = 0xFE1A, refanytype = 0xFE1D}

  /// <summary>
  /// CIL instructions requiring an integer parameter
  /// </summary>
  public enum IntOp {ldarg_s = 0x0E, ldarga_s, starg_s, ldloc_s, ldloca_s, 
                     stloc_s, ldc_i4_s = 0x1F, ldc_i4, ldarg = 0xFE09,
                     ldarga, starg, ldloc, ldloca, stloc, unaligned = 0xFE12 }

  /// <summary>
  /// CIL instructions requiring a field parameter
  /// </summary>
  public enum FieldOp {ldfld = 0x7B, ldflda, stfld, ldsfld, ldsflda,
                       stsfld, ldtoken = 0xD0 }

  /// <summary>
  /// CIL instructions requiring a method parameter
  /// </summary>
  public enum MethodOp {jmp = 0x27, call, callvirt = 0x6F, newobj = 0x73, 
                        ldtoken = 0xD0, ldftn = 0xFE06, ldvirtfn }

  /// <summary>
  /// CIL instructions requiring a type parameter
  /// </summary>
  public enum TypeOp {cpobj = 0x70, ldobj, castclass = 0x74, isinst, 
                      unbox = 0x79, stobj = 0x81, box = 0x8C, newarr, 
                      ldelema = 0x8F, refanyval = 0xC2, mkrefany = 0xC6, 
                      ldtoken = 0xD0, initobj = 0xFE15, sizeOf = 0xFE1C,
                      ldelem = 0xA3, stelem = 0xA4, unbox_any }

  /// <summary>
  /// CIL branch instructions
  /// </summary>
  public enum BranchOp {
          // short branches
          br_s = 0x2B, brfalse_s, brtrue_s, beq_s, bge_s, bgt_s,
          ble_s, blt_s, bne_un_s, bge_un_s, bgt_un_s, ble_un_s, blt_un_s,
          // long branches
          br = 0x38, brfalse, brtrue, beq, bge, bgt, ble, blt,
          bne_un, bge_un, bgt_un, ble_un, blt_un,

          leave = 0xDD, leave_s }

  /// <summary>
  /// Index for all the tables in the meta data
  /// </summary>
  public enum MDTable { Module, TypeRef, TypeDef, Field = 0x04, Method = 0x06,
    Param = 0x08, InterfaceImpl, MemberRef, Constant, CustomAttribute, 
    FieldMarshal, DeclSecurity, ClassLayout, FieldLayout, StandAloneSig, 
    EventMap, Event = 0x14, PropertyMap, Property = 0x17, MethodSemantics, 
    MethodImpl, ModuleRef, TypeSpec, ImplMap, FieldRVA, Assembly = 0x20, 
    AssemblyProcessor, AssemblyOS, AssemblyRef, AssemblyRefProcessor, 
    AssemblyRefOS, File, ExportedType, ManifestResource, NestedClass,
    GenericParam, MethodSpec, GenericParamConstraint  }

  public enum SafeArrayType { int16 = 2, int32, float32, float64,
    currency, date, bstr, dispatch, error, boolean, variant, unknown,
    Decimal, int8 = 16, uint8, uint16, uint32, Int = 22, UInt }

  internal enum CIx { TypeDefOrRef, HasConst, HasCustomAttr, HasFieldMarshal,
    HasDeclSecurity, MemberRefParent, HasSemantics, MethodDefOrRef, 
    MemberForwarded, Implementation, CustomAttributeType, ResolutionScope,
    TypeOrMethodDef, MaxCIx }

  internal enum MapType { eventMap, propertyMap, nestedClass }

  /**************************************************************************/  
        /// <summary>
        /// The assembly for mscorlib.  
        /// </summary>
        public sealed class MSCorLib : AssemblyRef
        {
    private static readonly int valueTypeIx = 18;
    private readonly string systemName = "System";
    private ClassRef[] systemClasses = new ClassRef[valueTypeIx+2];
    private PrimitiveType[] systemTypes = new PrimitiveType[valueTypeIx];
    private TypeSpec[] specialTypeSpecs = new TypeSpec[valueTypeIx];
    private static int[] specialNames = {
      PrimitiveType.Void.GetName().GetHashCode(),
      PrimitiveType.Boolean.GetName().GetHashCode(),
      PrimitiveType.Char.GetName().GetHashCode(),
      PrimitiveType.Int8.GetName().GetHashCode(),
      PrimitiveType.UInt8.GetName().GetHashCode(),
      PrimitiveType.Int16.GetName().GetHashCode(),
      PrimitiveType.UInt16.GetName().GetHashCode(),
      PrimitiveType.Int32.GetName().GetHashCode(),
      PrimitiveType.UInt32.GetName().GetHashCode(),
      PrimitiveType.Int64.GetName().GetHashCode(),
      PrimitiveType.UInt64.GetName().GetHashCode(),
      PrimitiveType.Float32.GetName().GetHashCode(),
      PrimitiveType.Float64.GetName().GetHashCode(),
      PrimitiveType.String.GetName().GetHashCode(),
      PrimitiveType.TypedRef.GetName().GetHashCode(),
      PrimitiveType.IntPtr.GetName().GetHashCode(),
      PrimitiveType.UIntPtr.GetName().GetHashCode(),
      PrimitiveType.Object.GetName().GetHashCode(),
      "ValueType".GetHashCode(),
      "Enum".GetHashCode()
    };

    internal MSCorLib(MetaData md) : base(md,"mscorlib") {
      md.AddToTable(MDTable.AssemblyRef,this);
      systemTypes[PrimitiveType.Void.GetSystemTypeIx()] = PrimitiveType.Void;
      systemTypes[PrimitiveType.Boolean.GetSystemTypeIx()] = PrimitiveType.Boolean;
      systemTypes[PrimitiveType.Char.GetSystemTypeIx()] = PrimitiveType.Char;
      systemTypes[PrimitiveType.Int8.GetSystemTypeIx()] = PrimitiveType.Int8;
      systemTypes[PrimitiveType.UInt8.GetSystemTypeIx()] = PrimitiveType.UInt8;
      systemTypes[PrimitiveType.Int16.GetSystemTypeIx()] = PrimitiveType.Int16;
      systemTypes[PrimitiveType.UInt16.GetSystemTypeIx()] = PrimitiveType.UInt16;
      systemTypes[PrimitiveType.Int32.GetSystemTypeIx()] = PrimitiveType.Int32;
      systemTypes[PrimitiveType.UInt32.GetSystemTypeIx()] = PrimitiveType.UInt32;
      systemTypes[PrimitiveType.Int64.GetSystemTypeIx()] = PrimitiveType.Int64;
      systemTypes[PrimitiveType.UInt64.GetSystemTypeIx()] = PrimitiveType.UInt64;
      systemTypes[PrimitiveType.Float32.GetSystemTypeIx()] = PrimitiveType.Float32;
      systemTypes[PrimitiveType.Float64.GetSystemTypeIx()] = PrimitiveType.Float64;
      systemTypes[PrimitiveType.IntPtr.GetSystemTypeIx()] = PrimitiveType.IntPtr;
      systemTypes[PrimitiveType.UIntPtr.GetSystemTypeIx()] = PrimitiveType.UIntPtr;
      systemTypes[PrimitiveType.String.GetSystemTypeIx()] = PrimitiveType.String;
      systemTypes[PrimitiveType.Object.GetSystemTypeIx()] = PrimitiveType.Object;
      systemTypes[PrimitiveType.TypedRef.GetSystemTypeIx()] = PrimitiveType.TypedRef;
    }

    /// <summary>
    /// Add a class to the mscorlib assembly
    /// </summary>
    /// <param name="nsName">name space name</param>
    /// <param name="name">class name</param>
    /// <returns></returns>
    public override ClassRef AddClass(string nsName, string name) {
      ClassRef aClass = GetSpecialClass(nsName,name);
      if (aClass == null) {
        aClass = new ClassRef(nsName,name,metaData);
        metaData.AddToTable(MDTable.TypeRef,aClass);
        aClass.SetParent(this);
      }
      return aClass;
    }

    private ClassRef GetSpecialClass(string nsName,string name) {
      if (nsName.CompareTo(systemName) != 0) return null;
      int hash = name.GetHashCode();
      for (int i=0; i < specialNames.Length; i++) {
        if (hash == specialNames[i]) {
          if (systemClasses[i] == null) {
            if (i < valueTypeIx) {
              systemClasses[i] = new SystemClass(systemTypes[i],this,metaData);
              if ((systemTypes[i] != PrimitiveType.Object) &&
                (systemTypes[i] != PrimitiveType.String)) {
                systemClasses[i].MakeValueClass();
              }
            } else {
              systemClasses[i] = new ClassRef(nsName,name,metaData);
              systemClasses[i].SetParent(this);
              systemClasses[i].MakeValueClass();
            }
            metaData.AddToTable(MDTable.TypeRef,systemClasses[i]);
          }
          return systemClasses[i];
        }
      }
      return null;
    }

    internal ClassRef GetSpecialSystemClass(PrimitiveType pType) {
      int ix = pType.GetSystemTypeIx();
      if (systemClasses[ix] == null) {
        systemClasses[ix] = new SystemClass(pType,this,metaData);
        metaData.AddToTable(MDTable.TypeRef,systemClasses[ix]);
      }
      return systemClasses[ix];
    }
        
    private ClassRef GetValueClass(string name, int hash) {
      int ix = valueTypeIx;
      if (hash != specialNames[valueTypeIx]) ix++;
      if (systemClasses[ix] == null) {
        systemClasses[ix] = new ClassRef(systemName,name,metaData);
        systemClasses[ix].SetParent(this);
        systemClasses[ix].MakeValueClass();
        metaData.AddToTable(MDTable.TypeRef,systemClasses[ix]);
      }
      return systemClasses[ix];
    }

    internal ClassRef ValueType() {
      if (systemClasses[valueTypeIx] == null) {
        ClassRef valType = new ClassRef("System","ValueType",metaData);
        valType.SetParent(this);
        valType.MakeValueClass();
        metaData.AddToTable(MDTable.TypeRef,valType);
        systemClasses[valueTypeIx] = valType;
      }
      return systemClasses[valueTypeIx];
    }

    /// <summary>
    /// Add a value class to this external assembly
    /// </summary>
    /// <param name="nsName">name space name</param>
    /// <param name="name">class name</param>
    /// <returns></returns>
    public override ClassRef AddValueClass(string nsName, string name) {
      if (nsName.CompareTo(systemName) == 0) {
        int hash = name.GetHashCode();
        if ((hash == specialNames[valueTypeIx]) ||
            (hash == specialNames[valueTypeIx+1])) {
          return GetValueClass(name,hash);
        }
      }
      ClassRef aClass = new ClassRef(nsName,name,metaData);
      metaData.AddToTable(MDTable.TypeRef,aClass);
      aClass.SetParent(this);
      aClass.MakeValueClass();
      return aClass;
    }

        }
  /**************************************************************************/  
        /// <summary>
        /// Signature for calli instruction
        /// </summary>
        public class CalliSig : Signature
        {
    private static readonly byte Sentinel = 0x41;
    CallConv callConv;
    Type returnType;
    Type[] parameters, optParams;
                uint numPars = 0, numOptPars = 0;

    /// <summary>
    /// Create a signature for a calli instruction
    /// </summary>
    /// <param name="cconv">calling conventions</param>
    /// <param name="retType">return type</param>
    /// <param name="pars">parameter types</param>
    public CalliSig(CallConv cconv, Type retType, Type[] pars) {
                        tabIx = MDTable.StandAloneSig;
      callConv = cconv;
      returnType = retType;
      parameters = pars;
                        if (pars != null) numPars = (uint)pars.Length;
    }

    /// <summary>
    /// Add the optional parameters to a vararg method
    /// This method sets the vararg calling convention
    /// </summary>
    /// <param name="optPars">the optional pars for the vararg call</param>
    public void AddVarArgs(Type[] optPars) {
      optParams = optPars;
      if (optPars != null) numOptPars = (uint)optPars.Length;
      callConv |= CallConv.Vararg;
    }

    /// <summary>
    /// Add extra calling conventions to this callsite signature
    /// </summary>
    /// <param name="cconv"></param>
    public void AddCallingConv(CallConv cconv) {
      callConv |= cconv;
    }

    internal sealed override void BuildTables(MetaData md) {
      if (done) return;
      MemoryStream sig = new MemoryStream();
      sig.WriteByte((byte)callConv);
      MetaData.CompressNum(numPars+numOptPars,sig);
      returnType.TypeSig(sig);
      for (int i=0; i < numPars; i++) {
        parameters[i].TypeSig(sig);
      }
      sigIx = md.AddToBlobHeap(sig.ToArray());
      if (numOptPars > 0) {
        sig.WriteByte(Sentinel);
        for (int i=0; i < numOptPars; i++) {
          optParams[i].TypeSig(sig);
        }
      }
      done = true;
    }

        }
  /**************************************************************************/  
  /// <summary>
  /// The IL instructions for a method
  /// </summary>
  public class CILInstructions 
  {

    private static readonly uint ExHeaderSize = 4;
    private static readonly uint FatExClauseSize = 24;
    private static readonly uint SmlExClauseSize = 12;
    private static readonly sbyte maxByteVal = 127;
    private static readonly sbyte minByteVal = -128;
    private static readonly byte maxUByteVal = 255;
    private static readonly int smallSize = 64;
    private static readonly ushort TinyFormat = 0x2;
    private static readonly ushort FatFormat = 0x3003;
    private static readonly ushort MoreSects = 0x8;
    private static readonly ushort InitLocals = 0x10;
    private static readonly uint FatSize = 12;
    private static readonly uint FatWords = FatSize/4;
    private static readonly byte FatExceptTable = 0x41;
    private static readonly byte SmlExceptTable = 0x01; 

    private MetaData metaData;
    private ArrayList exceptions, blockStack;
    //private bool codeChecked = false;
    private static readonly int INITSIZE = 5;
    private CILInstruction[] buffer = new CILInstruction[INITSIZE];
    private int tide = 0;
    private uint offset = 0;
    private ushort headerFlags = 0;
    private short maxStack;
    private uint paddingNeeded = 0;
    private byte exceptHeader = 0;
    uint localSigIx = 0;
    uint codeSize = 0, exceptSize = 0;
    bool tinyFormat, fatExceptionFormat = false;

    public uint Offset {
	get { return offset; }
    }	

    internal CILInstructions(MetaData md) {
      metaData = md;
    }

    private void AddToBuffer(CILInstruction inst) {
      if (tide >= buffer.Length) {
        CILInstruction[] tmp = buffer;
        buffer = new CILInstruction[tmp.Length * 2];
        for (int i=0; i < tide; i++) {
          buffer[i] = tmp[i];
        }
      }
                        //Console.WriteLine("Adding instruction at offset " + offset + " with size " + inst.size);
                        inst.offset = offset;
      offset += inst.size;
      buffer[tide++] = inst;
    }

    /// <summary>
    /// Add a simple IL instruction
    /// </summary>
    /// <param name="inst">the IL instruction</param>
    public void Inst(Op inst) {
      AddToBuffer(new Instr((int)inst));
    }

    /// <summary>
    /// Add an IL instruction with an integer parameter
    /// </summary>
    /// <param name="inst">the IL instruction</param>
    /// <param name="val">the integer parameter value</param>
    public void IntInst(IntOp inst, int val) {
      int instr = (int)inst;
      if ((inst == IntOp.ldc_i4_s) || (inst == IntOp.ldc_i4)) 
        AddToBuffer(new IntInstr(instr,val,(inst == IntOp.ldc_i4_s)));
      else
        AddToBuffer(new UIntInstr(instr,val,((inst < IntOp.ldc_i4_s) ||
                                              (inst == IntOp.unaligned))));
    }

    /// <summary>
    /// Add the load long instruction
    /// </summary>
    /// <param name="cVal">the long value</param>
    public void ldc_i8(long cVal) {
      AddToBuffer(new LongInstr(0x21,cVal));
    }

    /// <summary>
    /// Add the load float32 instruction
    /// </summary>
    /// <param name="cVal">the float value</param>
    public void ldc_r4(float cVal) {
      AddToBuffer(new FloatInstr(0x22,cVal));
    }

    /// <summary>
    /// Add the load float64 instruction
    /// </summary>
    /// <param name="cVal">the float value</param>
    public void ldc_r8(double cVal) {
      AddToBuffer(new DoubleInstr(0x23,cVal));
    }

    /// <summary>
    /// Add the load string instruction
    /// </summary>
    /// <param name="str">the string value</param>
    public void ldstr(string str) {
      AddToBuffer(new StringInstr(0x72,str));
    }

          /// <summary>
          /// Add the load string instruction
          /// </summary>
          public void ldstr (byte[] str) {
                  AddToBuffer (new StringInstr (0x72, str));
          }
          
    /// <summary>
    /// Add the calli instruction
    /// </summary>
    /// <param name="sig">the signature for the calli</param>
    public void calli(CalliSig sig) {
      AddToBuffer(new SigInstr(0x29,sig));
    }

    /// <summary>
    /// Add a label to the CIL instructions
    /// </summary>
    /// <param name="lab">the label to be added</param>
    public void CodeLabel(CILLabel lab) {
      AddToBuffer(new LabelInstr(lab));
    }

    /// <summary>
    /// Add an instruction with a field parameter
    /// </summary>
    /// <param name="inst">the CIL instruction</param>
    /// <param name="f">the field parameter</param>
    public void FieldInst(FieldOp inst, Field f) {
      AddToBuffer(new FieldInstr((int)inst,f));
    }

    /// <summary>
    /// Add an instruction with a method parameter
    /// </summary>
    /// <param name="inst">the CIL instruction</param>
    /// <param name="m">the method parameter</param>
    public void MethInst(MethodOp inst, Method m) {
      AddToBuffer(new MethInstr((int)inst,m));
    }

    /// <summary>
    /// Add an instruction with a type parameter
    /// </summary>
    /// <param name="inst">the CIL instruction</param>
    /// <param name="t">the type argument for the CIL instruction</param>
    public void TypeInst(TypeOp inst, Type aType) {
      AddToBuffer(new TypeInstr((int)inst,aType,metaData));
    }

    /// <summary>
    /// Add a branch instruction
    /// </summary>
    /// <param name="inst">the branch instruction</param>
    /// <param name="lab">the label that is the target of the branch</param>
    public void Branch(BranchOp inst,  CILLabel lab) {
      AddToBuffer(new BranchInstr((int)inst,lab));
    }

    /// <summary>
    /// Add a switch instruction
    /// </summary>
    /// <param name="labs">the target labels for the switch</param>
    public void Switch(CILLabel[] labs) {
      AddToBuffer(new SwitchInstr(0x45,labs));
    }

    /// <summary>
    /// Add a byte to the CIL instructions (.emitbyte)
    /// </summary>
    /// <param name="bVal"></param>
    public void emitbyte(byte bVal) { 
      AddToBuffer(new CILByte(bVal));
    }

    /// <summary>
    /// Add an instruction which puts an integer on TOS.  This method
    /// selects the correct instruction based on the value of the integer.
    /// </summary>
    /// <param name="i">the integer value</param>
    public void PushInt(int i) {
      if (i == -1) {
        AddToBuffer(new Instr((int)Op.ldc_i4_m1));
      } else if ((i >= 0) && (i <= 8)) {
        Op op = (Op)(Op.ldc_i4_0 + i);
        AddToBuffer(new Instr((int)op));
      } else if ((i >= minByteVal) && (i <= maxByteVal)) {
        AddToBuffer(new IntInstr((int)IntOp.ldc_i4_s,i,true));
      } else {
        AddToBuffer(new IntInstr((int)IntOp.ldc_i4,i,false)); 
      }
    }

    /// <summary>
    /// Add the instruction to load a long on TOS
    /// </summary>
    /// <param name="l">the long value</param>
    public void PushLong(long l) {
      AddToBuffer(new LongInstr(0x21,l));
    }

    /// <summary>
    /// Add an instruction to push the boolean value true on TOS
    /// </summary>
    public void PushTrue() {
      AddToBuffer(new Instr((int)Op.ldc_i4_1));
    }

    /// <summary>
    ///  Add an instruction to push the boolean value false on TOS
    /// </summary>
    public void PushFalse() {
      AddToBuffer(new Instr((int)Op.ldc_i4_0));
    }

    /// <summary>
    /// Add the instruction to load an argument on TOS.  This method
    /// selects the correct instruction based on the value of argNo
    /// </summary>
    /// <param name="argNo">the number of the argument</param>
    public void LoadArg(int argNo) {
      if (argNo < 4) {
        int op = (int)Op.ldarg_0 + argNo;
        AddToBuffer(new Instr(op));
      } else if (argNo <= maxUByteVal) {
        AddToBuffer(new UIntInstr((int)IntOp.ldarg,argNo,true));
      } else {
        AddToBuffer(new UIntInstr(0x09,argNo,false)); 
      }
   }

    /// <summary>
    /// Add the instruction to load the address of an argument on TOS.
    /// This method selects the correct instruction based on the value
    /// of argNo.
    /// </summary>
    /// <param name="argNo">the number of the argument</param>
    public void LoadArgAdr(int argNo) {
      if (argNo <= maxUByteVal) {
        AddToBuffer(new UIntInstr((int)IntOp.ldarga,argNo,true));
      } else {
        AddToBuffer(new UIntInstr(0x0A,argNo,false)); 
      }
    }

    /// <summary>
    /// Add the instruction to load a local on TOS.  This method selects
    /// the correct instruction based on the value of locNo.
    /// </summary>
    /// <param name="locNo">the number of the local to load</param>
    public void LoadLocal(int locNo) {
      if (locNo < 4) {
        int op = (int)Op.ldloc_0 + locNo;
        AddToBuffer(new Instr(op));
      } else if (locNo <= maxUByteVal) {
        AddToBuffer(new UIntInstr((int)IntOp.ldloc,locNo,true));
      } else {
        AddToBuffer(new UIntInstr(0x0C,locNo,false)); 
      }
    }

    /// <summary>
    /// Add the instruction to load the address of a local on TOS.
    /// This method selects the correct instruction based on the 
    /// value of locNo.
    /// </summary>
    /// <param name="locNo">the number of the local</param>
    public void LoadLocalAdr(int locNo) {
      if (locNo <= maxUByteVal) {
        AddToBuffer(new UIntInstr((int)IntOp.ldloca,locNo,true));
      } else {
        AddToBuffer(new UIntInstr(0x0D,locNo,false)); 
      }
    }

    /// <summary>
    /// Add the instruction to store to an argument.  This method
    /// selects the correct instruction based on the value of argNo.
    /// </summary>
    /// <param name="argNo">the argument to be stored to</param>
    public void StoreArg(int argNo) {
      if (argNo <= maxUByteVal) {
        AddToBuffer(new UIntInstr((int)IntOp.starg,argNo,true));
      } else {
        AddToBuffer(new UIntInstr(0x0B,argNo,false)); 
      }
    }

    /// <summary>
    /// Add the instruction to store to a local.  This method selects
    /// the correct instruction based on the value of locNo.
    /// </summary>
    /// <param name="locNo">the local to be stored to</param>
    public void StoreLocal(int locNo) {
      if (locNo < 4) {
        int op = (int)Op.stloc_0 + locNo;
        AddToBuffer(new Instr(op));
      } else if (locNo <= maxUByteVal) {
        AddToBuffer(new UIntInstr((int)IntOp.stloc,locNo,true));
      } else {
        AddToBuffer(new UIntInstr(0x0E,locNo,false)); 
      }
    }

    /// <summary>
    /// Create a new CIL label.  To place the label in the CIL instruction
    /// stream use CodeLabel.
    /// </summary>
    /// <returns>a new CIL label</returns>
    public CILLabel NewLabel() {
      return new CILLabel();
    }

    public void AddTryBlock(TryBlock tryBlock) {
      if (exceptions == null) 
        exceptions = new ArrayList();
      else if (exceptions.Contains(tryBlock)) return;
      exceptions.Add(tryBlock);
    }

    /// <summary>
    /// Create a new label at this position in the code buffer
    /// </summary>
    /// <returns>the label at the current position</returns>
    public CILLabel NewCodedLabel() {
      CILLabel lab = new CILLabel();
      AddToBuffer(new LabelInstr(lab));
      return lab;
    }

    /// <summary>
    /// Mark this position as the start of a new block
    /// (try, catch, filter, finally or fault)
    /// </summary>
    public void StartBlock() {
      if (blockStack == null) blockStack = new ArrayList();
      blockStack.Insert(0,NewCodedLabel());
    }

    /// <summary>
    /// Mark this position as the end of the last started block and
    /// make it a try block.  This try block is added to the current 
    /// instructions (ie do not need to call AddTryBlock)
    /// </summary>
    /// <returns>The try block just ended</returns>
    public TryBlock EndTryBlock() {
      TryBlock tBlock = new TryBlock((CILLabel)blockStack[0],NewCodedLabel());
      blockStack.RemoveAt(0);
      AddTryBlock(tBlock);
      return tBlock;
    }

    /// <summary>
    /// Mark this position as the end of the last started block and
    /// make it a catch block.  This catch block is associated with the
    /// specified try block.
    /// </summary>
    /// <param name="exceptType">the exception type to be caught</param>
    /// <param name="tryBlock">the try block associated with this catch block</param>
    public void EndCatchBlock(Class exceptType, TryBlock tryBlock) {
      Catch catchBlock = new Catch(exceptType,(CILLabel)blockStack[0],
        NewCodedLabel());
      tryBlock.AddHandler(catchBlock);
    }

    /// <summary>
    /// Mark this position as the end of the last started block and
    /// make it a filter block.  This filter block is associated with the
    /// specified try block.
    /// </summary>
    /// <param name="filterLab">the label where the filter code is</param>
    /// <param name="tryBlock">the try block associated with this filter block</param>
    public void EndFilterBlock(CILLabel filterLab, TryBlock tryBlock) {
      Filter filBlock = new Filter(filterLab,(CILLabel)blockStack[0],NewCodedLabel());
      tryBlock.AddHandler(filBlock);
    }

    /// <summary>
    /// Mark this position as the end of the last started block and
    /// make it a finally block.  This finally block is associated with the
    /// specified try block.
    /// </summary>
    /// <param name="tryBlock">the try block associated with this finally block</param>
    public void EndFinallyBlock(TryBlock tryBlock) {
      Finally finBlock= new Finally((CILLabel)blockStack[0],NewCodedLabel());
      tryBlock.AddHandler(finBlock);
    }

    /// <summary>
    /// Mark this position as the end of the last started block and
    /// make it a fault block.  This fault block is associated with the
    /// specified try block.
    /// </summary>
    /// <param name="tryBlock">the try block associated with this fault block</param>
    public void EndFaultBlock(TryBlock tryBlock) {
      Fault fBlock= new Fault((CILLabel)blockStack[0],NewCodedLabel());
      tryBlock.AddHandler(fBlock);
    }

    internal uint GetCodeSize() {
      return codeSize + paddingNeeded + exceptSize;
    }

    internal void CheckCode(uint locSigIx, bool initLocals, int maxStack) {
      if (tide == 0) return;
      bool changed = true;
      while (changed) {
        changed = false;
        for (int i=0; i < tide; i++) {
          changed = buffer[i].Check(metaData) || changed;
        }
        if (changed) {
          for (int i=1; i < tide; i++) {
            buffer[i].offset = buffer[i-1].offset + buffer[i-1].size;
          }
          offset = buffer[tide-1].offset + buffer[tide-1].size;
        }
      }
                        codeSize = offset;
                        // Console.WriteLine("codeSize before header added = " + codeSize);
      if ((offset < smallSize) && (maxStack <= 8) && (locSigIx == 0) && (exceptions == null)) {
        // can use tiny header
        //Console.WriteLine("Tiny Header");
        tinyFormat = true;
        headerFlags = (ushort)(TinyFormat | ((ushort)codeSize << 2));
        codeSize++;
        if ((codeSize % 4) != 0) { paddingNeeded = 4 - (codeSize % 4); }
      } else {
        //Console.WriteLine("Fat Header");
        tinyFormat = false;
        localSigIx = locSigIx;
        this.maxStack = (short)maxStack;
        headerFlags = FatFormat;
        if (exceptions != null) {
          // Console.WriteLine("Got exceptions");
          headerFlags |= MoreSects;
          uint numExceptClauses = 0;
          for (int i=0; i < exceptions.Count; i++) {
            TryBlock tryBlock = (TryBlock)exceptions[i];
            tryBlock.SetSize();
            numExceptClauses += (uint)tryBlock.NumHandlers();
            if (tryBlock.isFat()) fatExceptionFormat = true;
          }

          uint data_size = ExHeaderSize + numExceptClauses *
                 (fatExceptionFormat ? FatExClauseSize : SmlExClauseSize);

          if (data_size > 256)
                  fatExceptionFormat = true;
                  
          // Console.WriteLine("numexceptclauses = " + numExceptClauses);
          if (fatExceptionFormat) {
            // Console.WriteLine("Fat exception format");
            exceptHeader = FatExceptTable;
            exceptSize = ExHeaderSize + numExceptClauses * FatExClauseSize;
          } else {
            // Console.WriteLine("Tiny exception format");
            exceptHeader = SmlExceptTable;
            exceptSize = ExHeaderSize + numExceptClauses * SmlExClauseSize;
          }
          // Console.WriteLine("exceptSize = " + exceptSize);
        }
        if (initLocals) headerFlags |= InitLocals;
        if ((offset % 4) != 0) { paddingNeeded = 4 - (offset % 4); }
        codeSize += FatSize;
      }
                  // Console.WriteLine("codeSize = " + codeSize + "  headerFlags = " + 
      //                   Hex.Short(headerFlags));
    }

                internal void Write(FileImage output) {
      // Console.WriteLine("Writing header flags = " + Hex.Short(headerFlags));
                        if (tinyFormat) {
        // Console.WriteLine("Writing tiny code");
                                output.Write((byte)headerFlags);
                        } else {
        // Console.WriteLine("Writing fat code");
                                output.Write(headerFlags);
                                output.Write((ushort)maxStack);
                                output.Write(offset);
                                output.Write(localSigIx);
                        }
      // Console.WriteLine(Hex.Int(tide) + " CIL instructions");
      // Console.WriteLine("starting instructions at " + output.Seek(0,SeekOrigin.Current));
                        for (int i=0; i < tide; i++) {
                                buffer[i].Write(output);
                        }
      // Console.WriteLine("ending instructions at " + output.Seek(0,SeekOrigin.Current));
      for (int i=0; i < paddingNeeded; i++) { output.Write((byte)0); }
      if (exceptions != null) {
        // Console.WriteLine("Writing exceptions");
        // Console.WriteLine("header = " + Hex.Short(exceptHeader) + " exceptSize = " + Hex.Int(exceptSize));
        output.Write(exceptHeader);
        output.Write3Bytes((uint)exceptSize);
        for (int i=0; i < exceptions.Count; i++) {
          TryBlock tryBlock = (TryBlock)exceptions[i];
          tryBlock.Write(output,fatExceptionFormat);
        }
      }
                }

  }
  /**************************************************************************/  
        /// <summary>
        /// A label in the IL
        /// </summary>
        public class CILLabel
        {
    CILInstruction branch;
    CILInstruction[] multipleBranches;
    int tide = 0;
    CILInstruction labInstr;
    uint offset = 0;
 
    public CILLabel (uint offset) {
        this.offset = offset;
        }
        

    internal CILLabel() {
    }

    internal void AddBranch(CILInstruction instr) {
      if (branch == null) {
        branch = instr;
        return;
      }
      if (multipleBranches == null) {
        multipleBranches = new CILInstruction[2];
      } else if (tide >= multipleBranches.Length) {
        CILInstruction[] tmp = multipleBranches;
        multipleBranches = new CILInstruction[tmp.Length*2];
        for (int i=0; i < tide; i++) {
          multipleBranches[i] = tmp[i];
        }
      }
      multipleBranches[tide++] = instr;
    }

    internal void AddLabelInstr(LabelInstr lInstr) {
      labInstr = lInstr;
    }

    internal uint GetLabelOffset() {
      if (labInstr == null) return 0;
      return labInstr.offset + offset;
    }

        }
  /**************************************************************************/  
  public abstract class CodeBlock {

    private static readonly int maxCodeSize = 256;
    protected CILLabel start, end;
    protected bool small = true;

    public CodeBlock(CILLabel start, CILLabel end) {
      this.start = start;
      this.end = end;
    }

    internal virtual bool isFat() {
      // Console.WriteLine("block start = " + start.GetLabelOffset() +
      //                  "  block end = " + end.GetLabelOffset());
      return (end.GetLabelOffset() - start.GetLabelOffset()) > maxCodeSize;
    }

    internal virtual void Write(FileImage output, bool fatFormat) {
      if (fatFormat) output.Write(start.GetLabelOffset());
      else output.Write((short)start.GetLabelOffset());
      uint len = end.GetLabelOffset() - start.GetLabelOffset();
      if (fatFormat) output.Write(len);
      else output.Write((byte)len);
    }

  }

  /// <summary>
  /// The descriptor for a guarded block (.try)
  /// </summary>
  public class TryBlock : CodeBlock {
    protected bool fatFormat = false;
    protected int flags = 0;
    ArrayList handlers = new ArrayList();

    /// <summary>
    /// Create a new try block
    /// </summary>
    /// <param name="start">start label for the try block</param>
    /// <param name="end">end label for the try block</param>
    public TryBlock(CILLabel start, CILLabel end) : base(start,end) { }

    /// <summary>
    /// Add a handler to this try block
    /// </summary>
    /// <param name="handler">a handler to be added to the try block</param>
    public void AddHandler(HandlerBlock handler) {
      flags = handler.GetFlag();
      handlers.Add(handler);
    }

    internal void SetSize() {
      fatFormat = base.isFat();
      if (fatFormat) return;
      for (int i=0; i < handlers.Count; i++) {
        HandlerBlock handler = (HandlerBlock)handlers[i];
        if (handler.isFat()) {
          fatFormat = true;
          return;
        }
      }
    }

    internal int NumHandlers() {
      return handlers.Count;
  }

    internal override bool isFat() {
      return fatFormat;
    }

    internal override void Write(FileImage output, bool fatFormat) {
      // Console.WriteLine("writing exception details");
      for (int i=0; i < handlers.Count; i++) {
        // Console.WriteLine("Except block " + i);
        HandlerBlock handler = (HandlerBlock)handlers[i];
        if (fatFormat) output.Write(flags);
        else output.Write((short)flags);
        // Console.WriteLine("flags = " + Hex.Short(flags));
        base.Write(output,fatFormat);
        handler.Write(output,fatFormat);
      }
    }
  }

  public abstract class HandlerBlock : CodeBlock 
  {
    protected static readonly short ExceptionFlag = 0;
    protected static readonly short FilterFlag = 0x01;
    protected static readonly short FinallyFlag = 0x02;
    protected static readonly short FaultFlag = 0x04;

    public HandlerBlock(CILLabel start, CILLabel end) : base(start,end) { }

    internal virtual short GetFlag() { return ExceptionFlag; }

    internal override void Write(FileImage output, bool fatFormat) {
      base.Write(output,fatFormat);
    }

  }

  /// <summary>
  /// The descriptor for a catch clause (.catch)
  /// </summary>
  public class Catch : HandlerBlock 
  {
    Class exceptType;
    
    /// <summary>
    /// Create a new catch clause
    /// </summary>
    /// <param name="except">the exception to be caught</param>
    /// <param name="handlerStart">start of the handler code</param>
    /// <param name="handlerEnd">end of the handler code</param>
    public Catch(Class except, CILLabel handlerStart, CILLabel handlerEnd) 
                                            : base(handlerStart,handlerEnd) {
      exceptType = except;
    }

    internal override void Write(FileImage output, bool fatFormat) {
      base.Write(output,fatFormat);
      output.Write(exceptType.Token());
    }
  }

  /// <summary>
  /// The descriptor for a filter clause (.filter)
  /// </summary>
  public class Filter : HandlerBlock 
  {
    CILLabel filterLabel;

    /// <summary>
    /// Create a new filter clause
    /// </summary>
    /// <param name="filterLabel">the label where the filter code starts</param>
    /// <param name="handlerStart">the start of the handler code</param>
    /// <param name="handlerEnd">the end of the handler code</param>
    public Filter(CILLabel filterLabel, CILLabel handlerStart, 
                        CILLabel handlerEnd) : base(handlerStart,handlerEnd) {
      this.filterLabel = filterLabel;
    }

    internal override short GetFlag() { 
      return FilterFlag; 
    }

    internal override void Write(FileImage output, bool fatFormat) {
      base.Write(output,fatFormat);
      output.Write(filterLabel.GetLabelOffset());
    }

  }

  /// <summary>
  /// Descriptor for a finally block (.finally)
  /// </summary>
  public class Finally : HandlerBlock 
  {
    /// <summary>
    /// Create a new finally clause
    /// </summary>
    /// <param name="finallyStart">start of finally code</param>
    /// <param name="finallyEnd">end of finally code</param>
    public Finally(CILLabel finallyStart, CILLabel finallyEnd)
      : base(finallyStart,finallyEnd) { }

    internal override short GetFlag() { 
      return FinallyFlag; 
    }

    internal override void Write(FileImage output, bool fatFormat) {
      base.Write(output,fatFormat);
      output.Write((int)0);
    }

  }

  /// <summary>
  /// Descriptor for a fault block (.fault)
  /// </summary>
  public class Fault : HandlerBlock 
  {
    /// <summary>
    /// Create a new fault clause
    /// </summary>
    /// <param name="faultStart">start of the fault code</param>
    /// <param name="faultEnd">end of the fault code</param>
    public Fault(CILLabel faultStart, CILLabel faultEnd)
                                              : base(faultStart,faultEnd) { }

    internal override short GetFlag() { 
      return FaultFlag; 
    }

    internal override void Write(FileImage output, bool fatFormat) {
      base.Write(output,fatFormat);
      output.Write((int)0);
      
    }
  }

  /**************************************************************************/  
  /// <summary>
        /// The base descriptor for a class 
        /// </summary>
        public abstract class Class : Type
        {
    protected int row = 0;
    protected string name, nameSpace;
    protected uint nameIx, nameSpaceIx;
                protected MetaData _metaData;
                internal Class(string nameSpaceName, string className, MetaData md)
                                                              : base(0x12) {
      nameSpace = nameSpaceName;
      name = className;
      nameIx = md.AddToStringsHeap(name);
      nameSpaceIx = md.AddToStringsHeap(nameSpace);
      _metaData = md;
    }

    internal Class(uint nsIx, uint nIx) : base(0x12) {
      nameSpaceIx = nsIx;
      nameIx = nIx;
    }

    internal virtual uint TypeDefOrRefToken() { return 0; }

    internal virtual void MakeValueClass() {
      typeIndex = 0x11;
    }
        
    internal virtual string TypeName() {
      return (nameSpace + "." + name);
    }

    internal override MetaDataElement GetTypeSpec(MetaData md) {
      return this;
    }
        }
  /**************************************************************************/  
  // This Class produces entries in the TypeDef table of the MetaData 
  // in the PE meta data.

  // NOTE:  Entry 0 in TypeDef table is always the pseudo class <module> 
  // which is the parent for functions and variables declared a module level

  /// <summary>
        /// The descriptor for a class defined in the IL (.class) in the current assembly/module
        /// </summary>
        /// 
        public class ClassDef : Class
        {
    private static readonly uint HasSecurity = 0x00040000;
    private static readonly byte ElementType_Class = 0x12;

    Class superType;
    ArrayList fields = new ArrayList();
    ArrayList methods = new ArrayList();
    ArrayList events;
    ArrayList properties;
    bool typeIndexChecked = true;
    uint fieldIx = 0, methodIx = 0;
    byte[] securityActions;
    uint flags;
    ClassLayout layout;
    ClassDef parentClass;
    MetaData metaData;
    
    internal ClassDef(TypeAttr attrSet, string nsName, string name, 
                      MetaData md) : base(nsName, name, md) {
                        metaData = md;
      superType = metaData.mscorlib.GetSpecialSystemClass(PrimitiveType.Object);
      flags = (uint)attrSet;
      tabIx = MDTable.TypeDef;
    }

    internal void SetSuper(Class sClass) {
      superType = sClass;
      if (sClass is ClassRef)
        typeIndex = superType.GetTypeIndex();
      else
        typeIndexChecked = false;
    }

    internal override void MakeValueClass() {
      superType = metaData.mscorlib.ValueType();
      typeIndex = superType.GetTypeIndex();
    }

    public void SpecialNoSuper() {
      superType = null;
    }

    /// <summary>
    /// Add an attribute to this class
    /// </summary>
    /// <param name="ta">the attribute to be added</param>
    public void AddAttribute(TypeAttr ta) { 
      flags |= (uint)ta;
    }

    /// <summary>
    /// Add an interface that is implemented by this class
    /// </summary>
    /// <param name="iFace">the interface that is implemented</param>
    public void AddImplementedInterface(Class iFace) {
      metaData.AddToTable(MDTable.InterfaceImpl,new InterfaceImpl(this,iFace));
    }

    /// <summary>
    ///  Add a named generic type parameter
    /// </summary>
    public GenericParameter AddGenericParameter (short index, string name) {
            GenericParameter gp = new GenericParameter (this, metaData, index, name);
            metaData.AddToTable (MDTable.GenericParam, gp);
            return gp;
    }

    /// <summary>
    /// Add a field to this class
    /// </summary>
    /// <param name="name">field name</param>
    /// <param name="fType">field type</param>
    /// <returns>a descriptor for this new field</returns>
    public FieldDef AddField(string name, Type fType) {
      FieldDef field = new FieldDef(name,fType);
      fields.Add(field);
      return field;
    }

    /// <summary>
    /// Add a field to this class
    /// </summary>
    /// <param name="fAtts">attributes for this field</param>
    /// <param name="name">field name</param>
    /// <param name="fType">field type</param>
    /// <returns>a descriptor for this new field</returns>
    public FieldDef AddField(FieldAttr fAtts, string name, Type fType) {
      FieldDef field = new FieldDef(fAtts,name,fType);
      fields.Add(field);
      return field;
    }

    public void SetFieldOrder (ArrayList fields)
    {
            this.fields = fields;
    }

    /// <summary>
    /// Add a method to this class
    /// </summary>
    /// <param name="name">method name</param>
    /// <param name="retType">return type</param>
    /// <param name="pars">parameters</param>
    /// <returns>a descriptor for this new method</returns>
    public MethodDef AddMethod(string name, Type retType, Param[] pars) {
      // Console.WriteLine("Adding method " + name + " to class " + this.name);
      MethodDef meth = new MethodDef(metaData,name,retType, pars);
      methods.Add(meth);
      return meth;
    }

    /// <summary>
    /// Add a method to this class
    /// </summary>
    /// <param name="mAtts">attributes for this method</param>
    /// <param name="iAtts">implementation attributes for this method</param>
    /// <param name="name">method name</param>
    /// <param name="retType">return type</param>
    /// <param name="pars">parameters</param>
    /// <returns>a descriptor for this new method</returns>
    public MethodDef AddMethod(MethAttr mAtts, ImplAttr iAtts, string name, 
                               Type retType, Param[] pars) {
      // Console.WriteLine("Adding method " + name + " to class " + this.name);
      MethodDef meth = new MethodDef(metaData,mAtts,iAtts,name,retType,pars);
      methods.Add(meth);
      return meth;
    }

    /// <summary>
    /// Add an event to this class
    /// </summary>
    /// <param name="name">event name</param>
    /// <param name="eType">event type</param>
    /// <returns>a descriptor for this new event</returns>
    public Event AddEvent(string name, Type eType) {
      Event e = new Event(name,eType,this);
      if (events == null) events = new ArrayList();
      events.Add(e);
      return e;
    }

    /// <summary>
    /// Add a property to this class
    /// </summary>
    /// <param name="name">property name</param>
    /// <param name="propType">property type</param>
    /// <returns>a descriptor for this new property</returns>
    public Property AddProperty(string name, Type retType, Type[] pars) {
      Property p = new Property(name, retType, pars, this);
      if (properties == null) properties = new ArrayList();
      properties.Add(p);
      return p;
    }


    /// <summary>
    /// Add a nested class to this class
    /// </summary>
    /// <param name="attrSet">attributes for this nested class</param>
    /// <param name="nsName">nested name space name</param>
    /// <param name="name">nested class name</param>
    /// <returns>a descriptor for this new nested class</returns>
    public ClassDef AddNestedClass(TypeAttr attrSet, string nsName, 
                                   string name) {
      ClassDef nClass = new ClassDef(attrSet,nsName,name,metaData);
      metaData.AddToTable(MDTable.TypeDef,nClass);
      metaData.AddToTable(MDTable.NestedClass,new MapElem(nClass,Row,MDTable.TypeDef));
      nClass.parentClass = this;
      return (nClass);
     }

    /// <summary>
    /// Add a nested class to this class
    /// </summary>
    /// <param name="attrSet">attributes for this nested class</param>
    /// <param name="nsName">nested name space name</param>
    /// <param name="name">nested class name</param>
    /// <param name="sType">super type of this nested class</param>
    /// <returns>a descriptor for this new nested class</returns>
    public ClassDef AddNestedClass(TypeAttr attrSet, string nsName, 
                                   string name, Class sType) {
      ClassDef nClass = new ClassDef(attrSet,nsName,name,metaData);
      nClass.SetSuper(sType);
      metaData.AddToTable(MDTable.TypeDef,nClass);
      metaData.AddToTable(MDTable.NestedClass,
                          new MapElem(nClass,Row,MDTable.TypeDef));
      nClass.parentClass = this;
      return (nClass);
    }

    /// <summary>
    /// Add layout information for this class.  This class must have the
    /// sequential or explicit attribute.
    /// </summary>
    /// <param name="packSize">packing size (.pack)</param>
    /// <param name="classSize">class size (.size)</param>
    public void AddLayoutInfo (int packSize, int classSize) {
      layout = new ClassLayout(packSize,classSize,this);
    }

    /// <summary>
    /// Use a method as the implementation for another method (.override)
    /// </summary>
    /// <param name="decl">the method to be overridden</param>
    /// <param name="body">the implementation to be used</param>
    public void AddMethodOverride(Method decl, Method body) {
      metaData.AddToTable(MDTable.MethodImpl,new MethodImpl(this,decl,body));
    }

    /// <summary>
    /// Add security to this class NOT YET IMPLEMENTED
    /// </summary>
    /// <param name="permissionSet"></param>
    public void AddSecurity(byte[] permissionSet) {
      throw(new NotYetImplementedException("Class security "));
      //flags |= HasSecurity;
     // securityActions = permissionSet;
    }

    //public void AddLineInfo(int row, int col) { }

    internal void CheckTypeIndex() {
      if (typeIndexChecked) return;
      if (!(superType is ClassRef)) 
        ((ClassDef)superType).CheckTypeIndex();
      typeIndex = superType.GetTypeIndex();
      typeIndexChecked = true;
    }

    internal sealed override void BuildTables(MetaData md) {
      if (done) return;
      if ((flags & (uint)TypeAttr.Interface) != 0) { superType = null; }
      // Console.WriteLine("Building tables for " + name);
      if (layout != null) md.AddToTable(MDTable.ClassLayout,layout);
      // Console.WriteLine("adding methods " + methods.Count);
      methodIx = md.TableIndex(MDTable.Method);
      for (int i=0; i < methods.Count; i++) {
        md.AddToTable(MDTable.Method,(MetaDataElement)methods[i]);
        ((MethodDef)methods[i]).BuildTables(md);
      }
      // Console.WriteLine("adding fields");
      fieldIx = md.TableIndex(MDTable.Field);
      for (int i=0; i < fields.Count; i++) {
        md.AddToTable(MDTable.Field,(MetaDataElement)fields[i]);
        ((FieldDef)fields[i]).BuildTables(md);
      }
      // Console.WriteLine("adding events and properties");
      if (events != null) { 
        for (int i=0; i < events.Count; i++) {
          md.AddToTable(MDTable.Event,(Event)events[i]);
          ((Event)events[i]).BuildTables(md);
        }
        md.AddToTable(MDTable.EventMap,
          new MapElem(this,((Event)events[0]).Row,MDTable.Event));
      }
      if (properties != null) { 
        for (int i=0; i < properties.Count; i++) {
          md.AddToTable(MDTable.Property,(Property)properties[i]);
          ((Property)properties[i]).BuildTables(md);
        }
        md.AddToTable(MDTable.PropertyMap,new MapElem(this,
                          ((Property)properties[0]).Row,MDTable.Property));
      }
      // Console.WriteLine("End of building tables");
      done = true;
    }

    internal sealed override uint Size(MetaData md) {
      return 4 + 2 * md.StringsIndexSize() + 
        md.CodedIndexSize(CIx.TypeDefOrRef) +
        md.TableIndexSize(MDTable.Field) + 
        md.TableIndexSize(MDTable.Method);
    }

    internal sealed override void Write(FileImage output) {
      output.Write(flags);
      output.StringsIndex(nameIx);
      output.StringsIndex(nameSpaceIx);
      //if (superType != null) 
      // Console.WriteLine("getting coded index for superType of " + name + " = " + superType.GetCodedIx(CIx.TypeDefOrRef));
      output.WriteCodedIndex(CIx.TypeDefOrRef,superType);
      output.WriteIndex(MDTable.Field,fieldIx);
      output.WriteIndex(MDTable.Method,methodIx);
    }

    internal sealed override uint TypeDefOrRefToken() {
      uint cIx = Row;
      cIx = cIx << 2;
      return cIx;
    }

    internal sealed override void TypeSig(MemoryStream sig) {
      if (!typeIndexChecked) CheckTypeIndex();
      sig.WriteByte(GetTypeIndex());
      MetaData.CompressNum(TypeDefOrRefToken(),sig);
    }

    internal sealed override uint GetCodedIx(CIx code) {
      switch (code) {
        case (CIx.TypeDefOrRef) : return 0; 
        case (CIx.HasCustomAttr) : return 3; 
        case (CIx.HasDeclSecurity) : return 0; 
        case (CIx.TypeOrMethodDef) : return 0; 
      }
      return 0;
    }
 
        }
  /**************************************************************************/  
        /// <summary>
        /// Layout information for a class (.class [sequential | explicit])
        /// </summary>
        internal class ClassLayout : MetaDataElement
        {
    ClassDef parent;
    ushort packSize = 0;
    uint classSize = 0;

                internal ClassLayout(int pack, int cSize, ClassDef par) {
      packSize = (ushort)pack;
      classSize = (uint)cSize;
      parent = par;
      tabIx = MDTable.ClassLayout;
                }

    internal sealed override uint Size(MetaData md) {
      return 6 + md.TableIndexSize(MDTable.TypeDef);
    }

    internal sealed override void Write(FileImage output) {
      output.Write(packSize);
      output.Write(classSize);
      output.WriteIndex(MDTable.TypeDef,parent.Row);
    }

        }
  /**************************************************************************/  
        /// <summary>
        /// Descriptor for a class/interface declared in another module of THIS 
        /// assembly, or in another assembly.
        /// </summary>
        public class ClassRef : Class
        {
    protected ResolutionScope parent;
    ExternClass externClass;
    protected MetaData metaData;

                internal ClassRef(string nsName, string name, MetaData md) : base(nsName, name, md) {
      metaData = md;
      tabIx = MDTable.TypeRef;
    }

    /// <summary>
    /// Add a method to this class
    /// </summary>
    /// <param name="name">method name</param>
    /// <param name="retType">return type</param>
    /// <param name="pars">parameter types</param>
    /// <returns>a descriptor for this method</returns>
    public MethodRef AddMethod(string name, Type retType, Type[] pars) {
      MethodRef meth = new MethodRef(this,name,retType,pars,false,null);
      metaData.AddToTable(MDTable.MemberRef,meth);
      return meth;
    }

    /// <summary>
    /// Add a method to this class
    /// </summary>
    /// <param name="name">method name</param>
    /// <param name="retType">return type</param>
    /// <param name="pars">parameter types</param>
    /// <returns>a descriptor for this method</returns>
    public MethodRef AddVarArgMethod(string name, Type retType, 
                                     Type[] pars, Type[] optPars) {
      MethodRef meth = new MethodRef(this,name,retType,pars,true,optPars);
      metaData.AddToTable(MDTable.MemberRef,meth);
      return meth;
    }

    /// <summary>
    /// Add a field to this class
    /// </summary>
    /// <param name="name">field name</param>
    /// <param name="fType">field type</param>
    /// <returns>a descriptor for this field</returns>
    public FieldRef AddField(string name, Type fType) {
      FieldRef field = new FieldRef(this,name,fType);
      metaData.AddToTable(MDTable.MemberRef,field);
      return field;
    }

    internal void SetParent(ResolutionScope par) {
      parent = par;
    }

    internal override string TypeName() {
      if ((parent != null) && (parent is AssemblyRef))
        return (nameSpace + "." + name + ", " + ((AssemblyRef)parent).TypeName());
      else 
        return (nameSpace + name);
    }

    internal sealed override uint Size(MetaData md) {
      return md.CodedIndexSize(CIx.ResolutionScope) + 2 * 
             md.StringsIndexSize();
    }

    internal sealed override void Write(FileImage output) {
      output.WriteCodedIndex(CIx.ResolutionScope,parent);
      output.StringsIndex(nameIx);
      output.StringsIndex(nameSpaceIx);
    }

    internal override sealed uint TypeDefOrRefToken() {
      uint cIx = Row;
      cIx = (cIx << 2) | 0x1;
      return cIx;
    }

    internal override void TypeSig(MemoryStream sig) {
      sig.WriteByte(GetTypeIndex());
      MetaData.CompressNum(TypeDefOrRefToken(),sig);
    }

    internal sealed override uint GetCodedIx(CIx code) {
      switch (code) {
        case (CIx.TypeDefOrRef) : return 1; 
        case (CIx.HasCustomAttr) : return 2; 
        case (CIx.MemberRefParent) : return 1; 
        case (CIx.ResolutionScope) : return 3; 
      }
      return 0;
    }
 
        }
  /**************************************************************************/  

  public class ExternClassRef : ClassRef {

    ExternClass externClass;

    internal ExternClassRef(TypeAttr attrs, string nsName, string name,
                          FileRef declFile, MetaData md) : base(nsName,name,md) {
      externClass = new ExternClass(attrs,nameSpaceIx,nameIx,declFile);
      metaData.AddToTable(MDTable.ExportedType,externClass);
    }

    internal ExternClassRef(string name, MetaData md) : base(null,name,md) {
    }

    public ClassRef AddNestedClass(TypeAttr attrs, string name) {
      ExternClassRef nestedClass = new ExternClassRef(name,metaData);
      externClass = new ExternClass(attrs,0,nameIx,this.externClass);
      metaData.AddToTable(MDTable.ExportedType,externClass);
      return nestedClass;
    }
 
  }
  /**************************************************************************/  
  /// <summary>
  /// Descriptor for a constant value
  /// </summary>
  public abstract class Constant {
    protected uint size = 0;
    protected Type type;
    protected uint blobIndex;
    protected bool addedToBlobHeap = false;

                internal Constant()     {       }

    internal virtual uint GetBlobIndex(MetaData md) { return 0; }

    internal uint GetSize() { return size; }

    internal byte GetTypeIndex() { return type.GetTypeIndex(); }

    internal virtual void Write(BinaryWriter bw) {  }

        }
  /// <summary>
  /// Descriptor for a constant value
  /// </summary>
  public abstract class DataConstant : Constant {
    private uint dataOffset = 0;

    internal DataConstant() { }

    public uint DataOffset {
      get { return dataOffset; }
      set { dataOffset = value; }
    }

  }

  /// <summary>
  /// Boolean constant
  /// </summary>
  public class BoolConst : Constant {
    bool val;

    /// <summary>
    /// Create a new boolean constant with the value "val"
    /// </summary>
    /// <param name="val">value of this boolean constant</param>
    public BoolConst(bool val) {
      this.val = val;
      size = 1;
      type = PrimitiveType.Boolean;
    }

    internal sealed override uint GetBlobIndex(MetaData md) {
      if (!addedToBlobHeap) {
        if (val) blobIndex = md.AddToBlobHeap((sbyte)1);
        else blobIndex = md.AddToBlobHeap((sbyte)0);
        addedToBlobHeap = true;
      }
      return blobIndex;
    }

    internal sealed override void Write(BinaryWriter bw) {
      if (val) bw.Write((sbyte)1);
      else bw.Write((sbyte)0);
    }

  }

  public class ByteArrConst : DataConstant {
    byte[] val;

    public ByteArrConst(byte[] val) {
      this.val = val;
      size = (uint)val.Length;
    }

    public Type Type {
            get { return type; }
            set { type = value; }
    }

    internal sealed override uint GetBlobIndex(MetaData md) {
      if (!addedToBlobHeap) {
        blobIndex = md.AddToBlobHeap(val);
        addedToBlobHeap = true;
      }
      return blobIndex;
    }

    internal sealed override void Write(BinaryWriter bw) {
      bw.Write(val);
    }

  }

  public class CharConst : Constant {
    char val;

    public CharConst(char val) {
      this.val = val;
      size = 2;
      type = PrimitiveType.Char;
    }

    internal sealed override uint GetBlobIndex(MetaData md) {
      if (!addedToBlobHeap) {
        blobIndex = md.AddToBlobHeap(val);
        addedToBlobHeap = true;
      }
      return blobIndex;
    }

    internal sealed override void Write(BinaryWriter bw) {
      bw.Write(val);
    }

  }

  public class FloatConst : DataConstant {
    float val;

    public FloatConst(float val) {
      this.val = val;
      size = 4;
      type = PrimitiveType.Float32;
    }

    internal sealed override uint GetBlobIndex(MetaData md) {
      if (!addedToBlobHeap) {
        blobIndex = md.AddToBlobHeap(val);
        addedToBlobHeap = true;
      }
      return blobIndex;
    }

    internal sealed override void Write(BinaryWriter bw) {
      bw.Write(val);
    }

  }

  public class DoubleConst : DataConstant {
    double val;

    public DoubleConst(double val) {
      this.val = val;
      size = 8;
      type = PrimitiveType.Float64;
    }

    internal sealed override uint GetBlobIndex(MetaData md) {
      if (!addedToBlobHeap) {
        blobIndex = md.AddToBlobHeap(val);
        addedToBlobHeap = true;
      }
      return blobIndex;
    }

    internal sealed override void Write(BinaryWriter bw) {
      bw.Write(val);
    }

  }

  public class IntConst : DataConstant {
    long val;

    public IntConst(sbyte val) {
      this.val = val;
      size = 1;
      type = PrimitiveType.Int8;
    }

    public IntConst(short val) {
      this.val = val;
      size = 2;
      type = PrimitiveType.Int16;
    }

    public IntConst(int val) {
      this.val = val;
      size = 4;
      type = PrimitiveType.Int32;
    }

    public IntConst(long val) {
      this.val = val;
      size = 8;
      type = PrimitiveType.Int64;
    }

    internal sealed override uint GetBlobIndex(MetaData md) {
      if (!addedToBlobHeap) {
        switch (size) {
          case (1) : blobIndex = md.AddToBlobHeap((sbyte)val); break;
          case (2) : blobIndex = md.AddToBlobHeap((short)val); break;
          case (4) : blobIndex = md.AddToBlobHeap((int)val); break;
          default : blobIndex = md.AddToBlobHeap(val); break; 
        }
        addedToBlobHeap = true;
      }
      return blobIndex;
    }

    internal sealed override void Write(BinaryWriter bw) {
      switch (size) {
        case (1) : bw.Write((sbyte)val); break;
        case (2) : bw.Write((short)val); break;
        case (4) : bw.Write((int)val); break;
        default : bw.Write(val); break; 
      }
    }

  }

  public class UIntConst : Constant {
    long val;

    public UIntConst(sbyte val) {
      this.val = val;
      size = 1;
      type = PrimitiveType.UInt8;
    }
    public UIntConst(short val) {
      this.val = val;
      size = 2;
      type = PrimitiveType.UInt16;
    }
    public UIntConst(int val) {
      this.val = val;
      size = 4;
      type = PrimitiveType.UInt32;
    }
    public UIntConst(long val) {
      this.val = val;
      size = 8;
      type = PrimitiveType.UInt64;
    }

    internal sealed override uint GetBlobIndex(MetaData md) {
      if (!addedToBlobHeap) {
        switch (size) {
          case (1) : blobIndex = md.AddToBlobHeap((sbyte)val); break;
          case (2) : blobIndex = md.AddToBlobHeap((short)val); break;
          case (4) : blobIndex = md.AddToBlobHeap((int)val); break;
          default : blobIndex = md.AddToBlobHeap(val); break;
        }
        addedToBlobHeap = true;
      }
      return blobIndex;
    }

    internal sealed override void Write(BinaryWriter bw) {
      switch (size) {
        case (1) : bw.Write((sbyte)val); break;
        case (2) : bw.Write((short)val); break;
        case (4) : bw.Write((int)val); break;
        default : bw.Write(val); break;
      }
    }

  }

  public class StringConst : DataConstant {
    string val;

    public StringConst(string val) {
      this.val = val;
      size = (uint)val.Length;  // need to add null ??
      type = PrimitiveType.String;
    }

    internal sealed override uint GetBlobIndex(MetaData md) {
      if (!addedToBlobHeap) {
        byte [] b = Encoding.Unicode.GetBytes (val);
        blobIndex = md.AddToBlobHeap(b);
        addedToBlobHeap = true;
      }
      return blobIndex;
    }

    internal sealed override void Write(BinaryWriter bw) {
      bw.Write(val);
    }

  }

  public class NullConst : Constant {
 
    public NullConst() { 
      size = 4;
      type = PrimitiveType.Class;
    }

    internal sealed override uint GetBlobIndex(MetaData md) {
      if (!addedToBlobHeap) {
        blobIndex = md.AddToBlobHeap((int)0);
        addedToBlobHeap = true;
      }
      return blobIndex;
    }

    internal sealed override void Write(BinaryWriter bw) {
      bw.Write((int)0); 
    }

  }

  public class AddressConstant : DataConstant {
    DataConstant data;

    public AddressConstant(DataConstant dConst) {
      data = dConst;
      size = 4;
      type = PrimitiveType.TypedRef;
    }

    internal sealed override void Write(BinaryWriter bw) {
      ((FileImage)bw).WriteDataRVA(data.DataOffset);
    }

  }

  public class RepeatedConstant : DataConstant {
    DataConstant data;
    uint repCount;

    public RepeatedConstant(DataConstant dConst, int repeatCount) {
      data = dConst;
      repCount = (uint)repeatCount;
      int[] sizes = new int[1];
      sizes[0] = repeatCount;
      type = new BoundArray(type,1,sizes);
      size = data.GetSize() * repCount;
    }

    internal sealed override void Write(BinaryWriter bw) {
      for (int i=0; i < repCount; i++) {
        data.Write(bw);
      }
    }

  }

  public class ArrayConstant : DataConstant {
    DataConstant[] dataVals;

    public ArrayConstant(DataConstant[] dVals) {
      dataVals = dVals;
      for (int i=0; i < dataVals.Length; i++) {
        size += dataVals[i].GetSize();
      }
    }

    internal sealed override void Write(BinaryWriter bw) {
      for (int i=0; i < dataVals.Length; i++) {
        dataVals[i].Write(bw);
      }
    }

  }

  public class ClassType : Constant {
    string name;
    Class desc;

    public ClassType(string className) {
      name = className;
      type = PrimitiveType.ClassType;
    }

    public ClassType(Class classDesc) {
      desc = classDesc;
      type = PrimitiveType.ClassType;
    }

    internal override void Write(BinaryWriter bw) {
      if (name == null)  name = desc.TypeName();
      bw.Write(name);
    }

  }



  /**************************************************************************/  
        /// <summary>
        /// Summary description for ConstantElem.
        /// </summary>
        internal class ConstantElem : MetaDataElement
        {
    MetaDataElement parent;
    Constant cValue;
    uint valIx = 0;

                internal ConstantElem(MetaDataElement parent, Constant val) {
      this.parent = parent;
      cValue = val;
                        tabIx = MDTable.Constant;
                }

    internal sealed override void BuildTables(MetaData md) {
      if (done) return;
      valIx = cValue.GetBlobIndex(md);
      done = true;
    }

    internal void AddToBlob(BinaryWriter bw) {
      cValue.Write(bw);
    }

    internal sealed override uint Size(MetaData md) {
      return 2 + md.CodedIndexSize(CIx.HasConst) + md.BlobIndexSize();
    }

    internal sealed override void Write(FileImage output) {
      output.Write(cValue.GetTypeIndex());
      output.Write((byte)0);
      output.WriteCodedIndex(CIx.HasConst,parent);
      output.BlobIndex(valIx);
    }

        }
  /**************************************************************************/  
        /// <summary>
        /// Descriptor for a Custom Attribute (.custom) 
        /// </summary>

        public class CustomAttribute : MetaDataElement
        {
    private static readonly ushort prolog = 0x0001;
    MetaDataElement parent;
    Method type;
    uint valIx;
    Constant cVal;
    byte[] byteVal;
    ushort numNamed = 0;
    ArrayList names, vals;

                internal CustomAttribute(MetaDataElement paren, Method constrType, 
                                                          Constant val) {
      parent = paren;
      type = constrType;
      cVal = val;
      tabIx = MDTable.CustomAttribute;
      throw(new NotYetImplementedException("Custom Attributes "));
                }

    internal CustomAttribute(MetaDataElement paren, Method constrType,
                                                          byte[] val) {
      parent = paren;
      type = constrType;
      tabIx = MDTable.CustomAttribute;
      byteVal = val;
    }

    public void AddFieldOrProp(string name, Constant val) {
      if (numNamed == 0) {
        names = new ArrayList();
        vals = new ArrayList();
      }
      names.Add(name);
      vals.Add(val);
    }

    internal sealed override void BuildTables(MetaData md) {
      BinaryWriter bw = new BinaryWriter(new MemoryStream());
      bw.Write(byteVal);
      md.AddToTable(MDTable.CustomAttribute, this);
      MemoryStream str = (MemoryStream)bw.BaseStream;
      valIx = md.AddToBlobHeap(str.ToArray());
    }

    internal sealed override uint Size(MetaData md) {
      return md.CodedIndexSize(CIx.HasCustomAttr) + md.CodedIndexSize(CIx.CustomAttributeType) + md.BlobIndexSize();
    }

    internal sealed override void Write(FileImage output) {
      output.WriteCodedIndex(CIx.HasCustomAttr,parent);
      output.WriteCodedIndex(CIx.CustomAttributeType,type);
      output.BlobIndex(valIx);
    }

        }
  /**************************************************************************/  
        /// <summary>
        /// Descriptor for a custom modifier of a type (modopt or modreq)
        /// </summary>

        public class CustomModifiedType : Type
        {
    Type type;
    Class cmodType;

    /// <summary>
    /// Create a new custom modifier for a type
    /// </summary>
    /// <param name="type">the type to be modified</param>
    /// <param name="cmod">the modifier</param>
    /// <param name="cmodType">the type reference to be associated with the type</param>
                public CustomModifiedType(Type type, CustomModifier cmod, Class cmodType)
                                                          : base((byte)cmod) {
      this.type = type;
      this.cmodType = cmodType;
                }

    internal sealed override void TypeSig(MemoryStream str) {
      str.WriteByte(typeIndex);
      MetaData.CompressNum(cmodType.TypeDefOrRefToken(),str);
      type.TypeSig(str);
    }
  
  }
  /**************************************************************************/  
  /// <summary>
  /// Descriptor for security permissions for a class or a method NOT YET IMPLEMENTED
  /// </summary>
        
        public class DeclSecurity : MetaDataElement
        {
    ushort action;
    MetaDataElement parent;
    uint permissionIx;

                internal DeclSecurity(MetaDataElement paren, ushort act)        {
      parent = paren;
      action = act;
      tabIx = MDTable.DeclSecurity;
      throw(new NotYetImplementedException("Security "));
                }

    internal sealed override uint Size(MetaData md) {
      return 2 + md.CodedIndexSize(CIx.HasDeclSecurity) + md.BlobIndexSize();
    }

    internal sealed override void BuildTables(MetaData md) {
      if (done) return;
// add permission to blob heap
      done = true;
    }

    internal sealed override void Write(FileImage output) {
      output.Write(action);
      output.WriteCodedIndex(CIx.HasDeclSecurity,parent);
      output.BlobIndex(permissionIx);
    }
 
        }
  /**************************************************************************/  
        /// <summary>
        /// Descriptor for an event
        /// </summary>
  public class Event : Feature
        {
    Type eventType;

    internal Event(string name, Type eType, ClassDef parent) 
                                            : base(name, parent) {
      eventType = eType;
                        tabIx = MDTable.Event;
    }

    /// <summary>
    /// Add the addon method to this event
    /// </summary>
    /// <param name="addon">the addon method</param>
    public void AddAddon(MethodDef addon) {
      AddMethod(addon,MethodType.AddOn);
    }

    /// <summary>
    /// Add the removeon method to this event
    /// </summary>
    /// <param name="removeOn">the removeon method</param>
    public void AddRemoveOn(MethodDef removeOn) {
      AddMethod(removeOn,MethodType.RemoveOn);
    }

    /// <summary>
    /// Add the fire method to this event
    /// </summary>
    /// <param name="fire">the fire method</param>
    public void AddFire(MethodDef fire) {
      AddMethod(fire,MethodType.Fire);
    }

    /// <summary>
    /// Add another method to this event
    /// </summary>
    /// <param name="other">the method to be added</param>
    public void AddOther(MethodDef other) {
      AddMethod(other,MethodType.Other);
    }

    internal sealed override void BuildTables(MetaData md) {
      if (done) return;
      nameIx = md.AddToStringsHeap(name);
      for (int i=0; i < tide; i++) {
        md.AddToTable(MDTable.MethodSemantics,methods[i]);
      }
      done = true;
    }

    internal sealed override uint Size(MetaData md) {
      return 2 + md.StringsIndexSize() + md.CodedIndexSize(CIx.TypeDefOrRef);
    }

    internal sealed override void Write(FileImage output) {
      output.Write(flags);
      output.StringsIndex(nameIx);
      output.WriteCodedIndex(CIx.TypeDefOrRef,eventType);
    }

    internal sealed override uint GetCodedIx(CIx code) {
      switch (code) {
        case (CIx.HasCustomAttr) : return 10; 
        case (CIx.HasSemantics) : return 0; 
      }
      return 0;
    }
   
        }
  /**************************************************************************/  
        /// <summary>
        /// Descriptor for a class defined in another module of THIS assembly 
        /// and exported (.class extern)
        /// </summary>

  internal class ExternClass : Class
        {
                MetaDataElement parent;
    uint flags;

    internal ExternClass(TypeAttr attr, uint nsIx, uint nIx, 
                         MetaDataElement paren) : base(nsIx,nIx) {
      flags = (uint)attr;
            parent = paren;
                        tabIx = MDTable.ExportedType;
    }

    internal sealed override uint Size(MetaData md) {
      return 8 + 2* md.StringsIndexSize() + md.CodedIndexSize(CIx.Implementation);
    }

    internal sealed override void Write(FileImage output) {
      output.Write(flags);
      output.Write(0);
      output.StringsIndex(nameIx);
      output.StringsIndex(nameSpaceIx);
      output.WriteCodedIndex(CIx.Implementation,parent);
    }

    internal sealed override uint GetCodedIx(CIx code) {
      switch (code) {
        case (CIx.HasCustomAttr) : return 17; 
        case (CIx.Implementation) : return 2; 
      }
      return 0;
    }

        }
  /**************************************************************************/  
        /// <summary>
        /// Base class for Event and Property descriptors
        /// </summary>

  public class Feature : MetaDataElement
        {
    internal enum MethodType : ushort { Setter = 0x01, Getter, Other = 0x04, AddOn = 0x08, 
      RemoveOn = 0x10, Fire = 0x20 }

    private static readonly int INITSIZE = 5;
    private static readonly ushort specialName = 0x200;
    private static readonly ushort rtSpecialName = 0x400;

    protected ClassDef parent;
    protected ushort flags = 0;
    protected string name;
    protected int tide = 0;
    protected uint nameIx;
    protected MethodSemantics[] methods = new MethodSemantics[INITSIZE];
    
    internal Feature(string name, ClassDef par) {
      parent = par;
      this.name = name;
    }

    internal void AddMethod(MethodDef meth, MethodType mType) {
      if (tide >= methods.Length) { 
        int len = methods.Length;
        MethodSemantics[] mTmp = methods;
        methods = new MethodSemantics[len * 2];
        for (int i=0; i < len; i++) {
          methods[i] = mTmp[i];
        }
      }
      methods[tide++] = new MethodSemantics(mType,meth,this);
    }

    /// <summary>
    /// Set the specialName attribute for this Event or Property
    /// </summary>
    public void SetSpecialName() {
      flags |= specialName;
    }

    /// <summary>
    /// Set the RTSpecialName attribute for this Event or Property
    /// </summary>
    public void SetRTSpecialName() {
      flags |= rtSpecialName;
    }
  
        }
  /*****************************************************************************/  
        /// <summary>
        /// Descriptor for a field of a class
        /// </summary>

  public abstract class Field : Member
        {
    protected static readonly byte FieldSig = 0x6;

    protected Type type;

                internal Field(string pfName, Type pfType) : base(pfName)
                {
      type = pfType;
                }

        }
  /**************************************************************************/  
        /// <summary>
        /// Descriptor for a field defined in a class of THIS assembly/module
        /// </summary>
  public class FieldDef : Field
        {
    //private static readonly uint PInvokeImpl = 0x2000;
    private static readonly ushort HasFieldMarshal = 0x1000;
    private static readonly ushort HasFieldRVA = 0x100;
    private static readonly ushort HasDefault = 0x8000;

    FieldRVA rva;
    ConstantElem constVal;
    FieldLayout layout;
    FieldMarshal marshalInfo;
    ushort flags;

    internal FieldDef(string name, Type fType) : base(name,fType) {
      tabIx = MDTable.Field;
    }

    internal FieldDef(FieldAttr attrSet, string name, Type fType) : base(name, fType) { 
      flags = (ushort)attrSet;
      tabIx = MDTable.Field;
    }

    /// <summary>
    /// Add an attribute(s) to this field
    /// </summary>
    /// <param name="fa">the attribute(s) to be added</param>
    public void AddFieldAttr(FieldAttr fa) {
      flags |= (ushort)fa;
    }
 
    /// <summary>
    /// Add a value for this field
    /// </summary>
    /// <param name="val">the value for the field</param>
    public void AddValue(Constant val) {
      constVal = new ConstantElem(this,val);
      flags |= HasDefault;
    }

    /// <summary>
    /// Add an initial value for this field (at dataLabel) (.data)
    /// </summary>
    /// <param name="val">the value for the field</param>
    /// <param name="repeatVal">the number of repetitions of this value</param>
    public void AddDataValue(DataConstant val) {
      flags |= HasFieldRVA;
      rva = new FieldRVA(this,val);
    }

    /// <summary>
    /// Set the offset of the field.  Used for sequential or explicit classes.
    /// (.field [offs])
    /// </summary>
    /// <param name="offs">field offset</param>
    public void SetOffset(uint offs) {
      layout = new FieldLayout(this,offs);
    }

    /// <summary>
    /// Set the marshalling info for a field
    /// </summary>
    /// <param name="mInf"></param>
    public void SetMarshalInfo(NativeType marshallType) {
      flags |= HasFieldMarshal;
      marshalInfo = new FieldMarshal(this,marshallType);
    }

    internal sealed override void BuildTables(MetaData md) {
      if (done) return;
      nameIx = md.AddToStringsHeap(name);
      MemoryStream sig = new MemoryStream();
      sig.WriteByte(FieldSig);
      type.TypeSig(sig);
      sigIx = md.AddToBlobHeap(sig.ToArray());
      if (rva != null) {
        md.AddToTable(MDTable.FieldRVA,rva);
        rva.BuildTables(md);
      } else if (constVal != null) {
        md.AddToTable(MDTable.Constant,constVal);
        constVal.BuildTables(md);
      }
      if (layout != null) md.AddToTable(MDTable.FieldLayout,layout);
      if (marshalInfo != null) {
        md.AddToTable(MDTable.FieldMarshal,marshalInfo);
        marshalInfo.BuildTables(md);
      }
      done = true;
    }

    internal sealed override uint Size(MetaData md) {
      return 2 + md.StringsIndexSize() + md.BlobIndexSize();
    }

    internal sealed override void Write(FileImage output) {
      output.Write(flags);
      output.StringsIndex(nameIx);
      output.BlobIndex(sigIx);
    }

    internal sealed override uint GetCodedIx(CIx code) {
      switch (code) {
        case (CIx.HasConst) : return 0; 
        case (CIx.HasCustomAttr) : return 1; 
        case (CIx.HasFieldMarshal) : return 0; 
        case (CIx.MemberForwarded) : return 0; 
      }
      return 0;
    }

        }
  /**************************************************************************/  
        /// <summary>
        /// Descriptor for layout information for a field
        /// </summary>
        
        public class FieldLayout : MetaDataElement
        {
    Field field;
    uint offset;

    internal FieldLayout(Field field, uint offset)      {
      this.field = field;
      this.offset = offset;
                        tabIx = MDTable.FieldLayout;
    }

    internal sealed override uint Size(MetaData md) {
      return 4 + md.TableIndexSize(MDTable.Field);
    }

    internal sealed override void Write(FileImage output) {
      output.Write(offset);
      output.WriteIndex(MDTable.Field,field.Row);
    }

        }
  /*****************************************************************************/  
        /// <summary>
        /// Marshalling information for a field or param
        /// </summary>
  public class FieldMarshal : MetaDataElement
        {
    MetaDataElement field;
    NativeType nt;
    uint ntIx;

    internal FieldMarshal(MetaDataElement field, NativeType nType)      {
      this.field = field;
      this.nt = nType;
                        tabIx = MDTable.FieldMarshal;
                }

    internal sealed override void BuildTables(MetaData md) {
      if (done) return;
      ntIx = md.AddToBlobHeap(nt.ToBlob());
      done = true;
    }

    internal sealed override uint Size(MetaData md) {
      return md.CodedIndexSize(CIx.HasFieldMarshal) + md.BlobIndexSize();
    }

    internal sealed override void Write(FileImage output) {
      output.WriteCodedIndex(CIx.HasFieldMarshal,field);
      output.BlobIndex(ntIx);
    }

        }
  /**************************************************************************/  
        /// <summary>
        /// Descriptor for a field of a class defined in another assembly/module
        /// </summary>
  public class FieldRef : Field
        {
    MetaDataElement parent;

                internal FieldRef(MetaDataElement paren, string name, Type fType) : base(name, fType)   {       
      parent = paren;
    }

    internal sealed override void BuildTables(MetaData md) {
      if (done) return;
      nameIx = md.AddToStringsHeap(name);
      MemoryStream sig = new MemoryStream();
      sig.WriteByte(FieldSig);
      type.TypeSig(sig);
      sigIx = md.AddToBlobHeap(sig.ToArray());
      done = true;
    }

    internal sealed override uint Size(MetaData md) {
      return md.CodedIndexSize(CIx.MemberRefParent) + md.StringsIndexSize() + md.BlobIndexSize();
    }

    internal sealed override void Write(FileImage output) {
      output.WriteCodedIndex(CIx.MemberRefParent,parent);
      output.StringsIndex(nameIx);
      output.BlobIndex(sigIx);
    }

    internal sealed override uint GetCodedIx(CIx code) { return 6; }

        }
  /**************************************************************************/  
        /// <summary>
        /// Descriptor for the address of a field's value in the PE file
        /// </summary>
  public class FieldRVA : MetaDataElement
        {
    Field field;
    DataConstant data;

    internal FieldRVA(Field field, DataConstant data)   {
      this.field = field;
      this.data = data;
      tabIx = MDTable.FieldRVA;
                }

    internal sealed override void BuildTables(MetaData md) {
      if (done) return;
      md.AddData(data);
      done = true;
    }

    internal sealed override uint Size(MetaData md) {
      return 4 + md.TableIndexSize(MDTable.Field);
    }

    internal sealed override void Write(FileImage output) {
      output.WriteDataRVA(data.DataOffset);
      output.WriteIndex(MDTable.Field,field.Row);
    }

        }
  /**************************************************************************/  
        /// <summary>
        /// Image for a PEFile
        /// File Structure
  ///     DOS Header (128 bytes) 
  ///     PE Signature ("PE\0\0") 
  ///     PEFileHeader (20 bytes)
  ///     PEOptionalHeader (224 bytes) 
  ///     SectionHeaders (40 bytes * NumSections)
  ///
  ///     Sections .text (always present - contains metadata)
  ///              .sdata (contains any initialised data in the file - may not be present)
  ///                     (for ilams /debug this contains the Debug table)
  ///              .reloc (always present - in pure CIL only has one fixup)
  ///               others???  c# produces .rsrc section containing a Resource Table
  ///
  /// .text layout
  ///     IAT (single entry 8 bytes for pure CIL)
  ///     CLIHeader (72 bytes)
  ///     CIL instructions for all methods (variable size)
  ///     MetaData 
  ///       Root (20 bytes + UTF-8 Version String + quad align padding)
  ///       StreamHeaders (8 bytes + null terminated name string + quad align padding)
  ///       Streams 
  ///         #~        (always present - holds metadata tables)
  ///         #Strings  (always present - holds identifier strings)
  ///         #US       (Userstring heap)
  ///         #Blob     (signature blobs)
  ///         #GUID     (guids for assemblies or Modules)
  ///    ImportTable (40 bytes)
  ///    ImportLookupTable(8 bytes) (same as IAT for standard CIL files)
  ///    Hint/Name Tables with entry "_CorExeMain" for .exe file and "_CorDllMain" for .dll (14 bytes)
  ///    ASCII string "mscoree.dll" referenced in ImportTable (+ padding = 16 bytes)
  ///    Entry Point  (0xFF25 followed by 4 bytes 0x400000 + RVA of .text)
  ///
  ///  #~ stream structure
  ///    Header (24 bytes)
  ///    Rows   (4 bytes * numTables)
  ///    Tables
  /// </summary>
        internal class FileImage : BinaryWriter
        {
    internal readonly static uint[] iByteMask = {0x000000FF, 0x0000FF00, 0x00FF0000, 0xFF000000};
    internal readonly static ulong[] lByteMask = {0x00000000000000FF, 0x000000000000FF00,
                                                  0x0000000000FF0000, 0x00000000FF000000,
                                                  0x000000FF00000000, 0x0000FF0000000000,
                                                  0x00FF000000000000, 0xFF00000000000000 };
    internal readonly static uint nibble0Mask = 0x0000000F;
    internal readonly static uint nibble1Mask = 0x000000F0;

    private static readonly byte[] DOSHeader = { 0x4d,0x5a,0x90,0x00,0x03,0x00,0x00,0x00,
                                                 0x04,0x00,0x00,0x00,0xff,0xff,0x00,0x00,
                                                 0xb8,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                                                 0x40,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                                                 0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                                                 0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                                                 0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                                                 0x00,0x00,0x00,0x00,0x80,0x00,0x00,0x00,
                                                 0x0e,0x1f,0xba,0x0e,0x00,0xb4,0x09,0xcd,
                                                 0x21,0xb8,0x01,0x4c,0xcd,0x21,0x54,0x68,
                                                 0x69,0x73,0x20,0x70,0x72,0x6f,0x67,0x72,
                                                 0x61,0x6d,0x20,0x63,0x61,0x6e,0x6e,0x6f,
                                                 0x74,0x20,0x62,0x65,0x20,0x72,0x75,0x6e,
                                                 0x20,0x69,0x6e,0x20,0x44,0x4f,0x53,0x20,
                                                 0x6d,0x6f,0x64,0x65,0x2e,0x0d,0x0d,0x0a,
                                                 0x24,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                                                 0x50,0x45,0x00,0x00};
    private static byte[] PEHeader = { 0x4c, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
                                       0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                       0xE0, 0x00, 0x0E, 0x01, // PE Header Standard Fields
                                       0x0B, 0x01, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 
                                       0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
                                       0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                                     };

    private static readonly uint minFileAlign = 0x200;
    private static readonly uint maxFileAlign = 0x1000;
    private static readonly uint fileHeaderSize = 0x178;
    private static readonly uint sectionHeaderSize = 40;
    private static readonly uint SectionAlignment = 0x2000;
    private static readonly uint ImageBase = 0x400000;
    private static readonly uint ImportTableSize = 40;
    private static readonly uint IATSize = 8;
    private static readonly uint CLIHeaderSize = 72;
    private uint runtimeFlags = 0x01;  // COMIMAGE_FLAGS_ILONLY
    // 32BITREQUIRED 0x02, STRONGNAMESIGNED 0x08, TRACKDEBUGDATA 0x10000
                private static readonly uint relocFlags = 0x42000040;
                private static readonly ushort exeCharacteristics = 0x010E;
                private static readonly ushort dllCharacteristics = 0x210E;
    // section names are all 8 bytes
    private static readonly string textName = ".text\0\0\0";
    private static readonly string sdataName = ".sdata\0\0";
    private static readonly string relocName = ".reloc\0\0";
    private static readonly string rsrcName = ".rsrc\0\0\0";
    private static readonly string exeHintNameTable = "\0\0_CorExeMain\0";
    private static readonly string dllHintNameTable = "\0\0_CorDllMain\0";
    private static readonly string runtimeEngineName = "mscoree.dll\0\0";

                private Section text, sdata, rsrc;
                ArrayList data;
    BinaryWriter reloc = new BinaryWriter(new MemoryStream());
    uint dateStamp = 0;
    DateTime origin = new DateTime(1970,1,1);
    uint numSections = 2; // always have .text  and .reloc sections
    internal SubSystem subSys = SubSystem.Windows_CUI;  // default is Windows Console mode
    internal uint fileAlign = minFileAlign;
    uint entryPointOffset, entryPointPadding, imageSize, headerSize, headerPadding, entryPointToken = 0;
    uint relocOffset, relocRVA, relocSize, relocPadding, relocTide, hintNameTableOffset;
    uint metaDataOffset, runtimeEngineOffset, initDataSize = 0, importTablePadding;
    uint importTableOffset, importLookupTableOffset, totalImportTableSize;
    MetaData metaData;
    char[] runtimeEngine = runtimeEngineName.ToCharArray(), hintNameTable;
                bool doDLL, largeStrings, largeGUID, largeUS, largeBlob;
                ushort characteristics;

    internal FileImage(bool makeDLL, string fileName) : base(new FileStream(fileName,FileMode.Create)) {
      InitFileImage(makeDLL);
      TimeSpan tmp = System.IO.File.GetCreationTime(fileName).Subtract(origin);
      dateStamp = Convert.ToUInt32(tmp.TotalSeconds);
    }

    internal FileImage(bool makeDLL, Stream str) : base(str) {
      InitFileImage(makeDLL);
      TimeSpan tmp = DateTime.Now.Subtract(origin);
      dateStamp = Convert.ToUInt32(tmp.TotalSeconds);
    }

    private void InitFileImage(bool makeDLL) {
      doDLL = makeDLL;
                        if (doDLL) {
                                hintNameTable = dllHintNameTable.ToCharArray();
                                characteristics = dllCharacteristics;
                        } else {
                                hintNameTable = exeHintNameTable.ToCharArray();
                                characteristics = exeCharacteristics;
                        }
      text = new Section(textName,0x60000020);     // IMAGE_SCN_CNT  CODE, EXECUTE, READ
//                      rsrc = new Section(rsrcName,0x40000040);     // IMAGE_SCN_CNT  INITIALIZED_DATA, READ
      metaData = new MetaData(this);
    }

                internal MetaData GetMetaData() {
                        return metaData;
                }

    private uint GetNextSectStart(uint rva, uint tide) {
      if (tide < SectionAlignment) return rva + SectionAlignment;
      return rva + ((tide / SectionAlignment) + 1) * SectionAlignment;
    }

    private void BuildTextSection() {
      // .text layout
      //    IAT (single entry 8 bytes for pure CIL)
      //    CLIHeader (72 bytes)
      //    CIL instructions for all methods (variable size)
      //    MetaData 
      //    ImportTable (40 bytes)
      //    ImportLookupTable(8 bytes) (same as IAT for standard CIL files)
      //    Hint/Name Tables with entry "_CorExeMain" for .exe file and "_CorDllMain" for .dll (14 bytes)
      //    ASCII string "mscoree.dll" referenced in ImportTable (+ padding = 16 bytes)
      //    Entry Point  (0xFF25 followed by 4 bytes 0x400000 + RVA of .text)
      metaData.BuildMetaData(IATSize + CLIHeaderSize);
      metaDataOffset = IATSize + CLIHeaderSize;
      // Console.WriteLine("Code starts at " + metaDataOffset);
      metaDataOffset += metaData.CodeSize();
      // resourcesStart =
      // strongNameSig = metaData.GetStrongNameSig();
      // fixUps = RVA for vtable
      importTableOffset = metaDataOffset + metaData.Size();
      importTablePadding = NumToAlign(importTableOffset,16);
      importTableOffset += importTablePadding;
      importLookupTableOffset = importTableOffset + ImportTableSize;
      hintNameTableOffset = importLookupTableOffset + IATSize;
      runtimeEngineOffset = hintNameTableOffset + (uint)hintNameTable.Length;
      entryPointOffset = runtimeEngineOffset + (uint)runtimeEngine.Length;
      totalImportTableSize = entryPointOffset - importTableOffset;
      // Console.WriteLine("total import table size = " + totalImportTableSize);
      // Console.WriteLine("entrypoint offset = " + entryPointOffset);
      entryPointPadding = NumToAlign(entryPointOffset,4) + 2;
      entryPointOffset += entryPointPadding;
      text.AddReloc(entryPointOffset+2);
      text.IncTide(entryPointOffset + 6);
      //if (text.Tide() < fileAlign) fileAlign = minFileAlign;
                        text.SetSize(NumToAlign(text.Tide(),fileAlign));
      // Console.WriteLine("text size = " + text.Size() + " text tide = " + text.Tide() + " text padding = " + text.Padding());
                        // Console.WriteLine("metaDataOffset = " + Hex.Int(metaDataOffset));
                        // Console.WriteLine("importTableOffset = " + Hex.Int(importTableOffset));
                        // Console.WriteLine("importLookupTableOffset = " + Hex.Int(importLookupTableOffset));
                        // Console.WriteLine("hintNameTableOffset = " + Hex.Int(hintNameTableOffset));
                        // Console.WriteLine("runtimeEngineOffset = " + Hex.Int(runtimeEngineOffset));
                        // Console.WriteLine("entryPointOffset = " + Hex.Int(entryPointOffset));
                        // Console.WriteLine("entryPointPadding = " + Hex.Int(entryPointPadding));

    }

    internal void BuildRelocSection() {
                        text.DoRelocs(reloc);
                        if (sdata != null) sdata.DoRelocs(reloc);
                        if (rsrc != null) rsrc.DoRelocs(reloc);
      relocTide = (uint)reloc.Seek(0,SeekOrigin.Current);
      relocPadding = NumToAlign(relocTide,fileAlign);
      relocSize = relocTide + relocPadding;
                        imageSize = relocRVA + SectionAlignment;
      initDataSize += relocSize;
    }

    private void CalcOffsets() {
if (sdata != null)
        numSections++;
if (rsrc != null)
        numSections++;
      headerSize = fileHeaderSize + (numSections * sectionHeaderSize);
      headerPadding = NumToAlign(headerSize,fileAlign);
      headerSize += headerPadding;
      uint offset = headerSize;
      uint rva = SectionAlignment;
      text.SetOffset(offset);
      text.SetRVA(rva);
      offset += text.Size();
      rva  = GetNextSectStart(rva,text.Tide());
                        // Console.WriteLine("headerSize = " + headerSize);
                        // Console.WriteLine("headerPadding = " + headerPadding);
                        // Console.WriteLine("textOffset = " + Hex.Int(text.Offset()));
                        if (sdata != null) { 
                                sdata.SetSize(NumToAlign(sdata.Tide(),fileAlign));
                                sdata.SetOffset(offset);
        sdata.SetRVA(rva);
        offset += sdata.Size();
        rva = GetNextSectStart(rva,sdata.Tide());
                                initDataSize += sdata.Size();
      }
      if (rsrc != null) { 
                     rsrc.SetSize(NumToAlign(rsrc.Tide(),fileAlign));
                                rsrc.SetOffset(offset);
        rsrc.SetRVA(rva);
        offset += rsrc.Size();
        rva = GetNextSectStart(rva,rsrc.Tide());
                                initDataSize += rsrc.Size();
      }
      relocOffset = offset;
      relocRVA = rva;
    }

    internal void MakeFile() {
      if (doDLL) hintNameTable = dllHintNameTable.ToCharArray();
      else hintNameTable = exeHintNameTable.ToCharArray();
      BuildTextSection();
      CalcOffsets();
      BuildRelocSection();
      // now write it out
      WriteHeader();
      WriteSections();
      Flush();
      Close();
    }

    private void WriteHeader() {
      Write(DOSHeader);
                        // Console.WriteLine("Writing PEHeader at offset " + Seek(0,SeekOrigin.Current));
                        WritePEHeader();
                         // Console.WriteLine("Writing text section header at offset " + Hex.Long(Seek(0,SeekOrigin.Current)));
                        text.WriteHeader(this,relocRVA);
      if (sdata != null) sdata.WriteHeader(this,relocRVA);
      if (rsrc != null) rsrc.WriteHeader(this,relocRVA);
                        // Console.WriteLine("Writing reloc section header at offset " + Seek(0,SeekOrigin.Current));
                        WriteRelocSectionHeader();
                        // Console.WriteLine("Writing padding at offset " + Seek(0,SeekOrigin.Current));
                        WriteZeros(headerPadding);
    }

    private void WriteSections() {
                        // Console.WriteLine("Writing text section at offset " + Seek(0,SeekOrigin.Current));
      WriteTextSection();
      if (sdata != null) WriteSDataSection();
      if (rsrc != null) WriteRsrcSection();
      WriteRelocSection();
    }

     private void WriteIAT() {
      Write(text.RVA() + hintNameTableOffset);
      Write(0);
    }

                private void WriteImportTables() {
                        // Import Table
      WriteZeros(importTablePadding);
      // Console.WriteLine("Writing import tables at offset " + Hex.Long(Seek(0,SeekOrigin.Current)));
                        Write(importLookupTableOffset + text.RVA());
                        WriteZeros(8); 
                        Write(runtimeEngineOffset + text.RVA());
                        Write(text.RVA());    // IAT is at the beginning of the text section
                        WriteZeros(20);
                        // Import Lookup Table
                        WriteIAT();                // lookup table and IAT are the same
                        // Hint/Name Table
       // Console.WriteLine("Writing hintname table at " + Hex.Long(Seek(0,SeekOrigin.Current)));
                        Write(hintNameTable);
      Write(runtimeEngineName.ToCharArray());
                }

    private void WriteTextSection() {
      WriteIAT();
      WriteCLIHeader();
      // Console.WriteLine("Writing code at " + Hex.Long(Seek(0,SeekOrigin.Current)));
      metaData.WriteByteCodes(this);
      // Console.WriteLine("Finished writing code at " + Hex.Long(Seek(0,SeekOrigin.Current)));
      largeStrings = metaData.LargeStringsIndex();
      largeGUID = metaData.LargeGUIDIndex();
      largeUS = metaData.LargeUSIndex();
      largeBlob = metaData.LargeBlobIndex();
      metaData.WriteMetaData(this);
      WriteImportTables();
                        WriteZeros(entryPointPadding);
                        Write((ushort)0x25FF);
                        Write(ImageBase + text.RVA());
                        WriteZeros(text.Padding());
    }

    private void WriteCLIHeader() {
      Write(CLIHeaderSize);       // Cb
      Write((short)2);            // Major runtime version
      Write((short)0);            // Minor runtime version
      Write(text.RVA() + metaDataOffset);
      Write(metaData.Size());
      Write(runtimeFlags);
      Write(entryPointToken);
      WriteZeros(8);                     // Resources - used by Manifest Resources NYI
      WriteZeros(8);                     // Strong Name stuff here!! NYI
      WriteZeros(8);                     // CodeManagerTable
      WriteZeros(8);                     // VTableFixups NYI
      WriteZeros(16);                    // ExportAddressTableJumps, ManagedNativeHeader
     }

    private void WriteSDataSection() {
      long size = sdata.Size ();
      long start = BaseStream.Position;
      for (int i=0; i < data.Count; i++) {
        ((DataConstant)data[i]).Write(this);
      }
      while (BaseStream.Position < (start + size))
              Write ((byte) 0);
    }

                private void WriteRsrcSection() {
                }

    private void WriteRelocSection() {
     // Console.WriteLine("Writing reloc section at " + Seek(0,SeekOrigin.Current) + " = " + relocOffset);
      MemoryStream str = (MemoryStream)reloc.BaseStream;
      Write(str.ToArray());
      WriteZeros(NumToAlign((uint)str.Position,fileAlign));
    }

    internal void SetEntryPoint(uint entryPoint) {
      entryPointToken = entryPoint;
    }

    internal void AddInitData(DataConstant cVal) {
                        if (sdata == null) {                    
                                sdata = new Section(sdataName,0xC0000040);   // IMAGE_SCN_CNT  INITIALIZED_DATA, READ, WRITE
                                data = new ArrayList(); 
                        }
      data.Add(cVal);
      cVal.DataOffset = sdata.Tide();
      sdata.IncTide(cVal.GetSize());
    }

    internal void WriteZeros(uint numZeros) {
      for (int i=0; i < numZeros; i++) {
        Write((byte)0);
      }
    }

    internal void WritePEHeader() {
      Write((ushort)0x014C);  // Machine - always 0x14C for Managed PE Files (allow others??)
      Write((ushort)numSections);
      Write(dateStamp);
      WriteZeros(8); // Pointer to Symbol Table and Number of Symbols (always zero for ECMA CLI files)
      Write((ushort)0x00E0);  // Size of Optional Header
      Write(characteristics);
      // PE Optional Header
      Write((ushort)0x010B);   // Magic
      Write((byte)0x6);        // LMajor pure-IL = 6   C++ = 7
      Write((byte)0x0);        // LMinor
      Write(text.Size());
      Write(initDataSize);
      Write(0);                // Check other sections here!!
      Write(text.RVA() + entryPointOffset);
      Write(text.RVA());
                        uint dataBase = 0;
                        if (sdata != null) dataBase = sdata.RVA();
                        else if (rsrc != null) dataBase = rsrc.RVA();
      else dataBase = relocRVA;
                        Write(dataBase);
      Write(ImageBase);
      Write(SectionAlignment);
      Write(fileAlign);
      Write((ushort)0x04);     // OS Major
      WriteZeros(6);                  // OS Minor, User Major, User Minor
      Write((ushort)0x04);     // SubSys Major
      WriteZeros(6);           // SybSys Minor, Reserved
      Write(imageSize);
      Write(headerSize);
      Write((int)0);           // File Checksum
      Write((ushort)subSys);
      Write((short)0);         // DLL Flags
      Write((uint)0x100000);   // Stack Reserve Size
      Write((uint)0x1000);     // Stack Commit Size
      Write((uint)0x100000);   // Heap Reserve Size
      Write((uint)0x1000);     // Heap Commit Size
      Write(0);                // Loader Flags
      Write(0x10);             // Number of Data Directories
      WriteZeros(8);                  // Export Table
      Write(importTableOffset + text.RVA());
      Write(totalImportTableSize);
      WriteZeros(24);            // Resource, Exception and Certificate Tables
      Write(relocRVA);
      Write(relocTide);
      WriteZeros(48);            // Debug, Copyright, Global Ptr, TLS, Load Config and Bound Import Tables
      Write(text.RVA());         // IATRVA - IAT is at start of .text Section
      Write(IATSize);
      WriteZeros(8);             // Delay Import Descriptor
      Write(text.RVA()+IATSize); // CLIHeader immediately follows IAT
      Write(CLIHeaderSize);    
      WriteZeros(8);             // Reserved
    }

    internal void WriteRelocSectionHeader() {
      Write(relocName.ToCharArray());
      Write(relocTide);
      Write(relocRVA);
      Write(relocSize);
      Write(relocOffset);
      WriteZeros(12);
      Write(relocFlags);
    }

    private void Align (MemoryStream str, int val) {
      if ((str.Position % val) != 0) {
        for (int i=val - (int)(str.Position % val); i > 0; i--) {
          str.WriteByte(0);
        }
      }
    }

    private uint Align(uint val, uint alignVal) {
      if ((val % alignVal) != 0) {
        val += alignVal - (val % alignVal);
      }
      return val;
    }

    private uint NumToAlign(uint val, uint alignVal) {
      if ((val % alignVal) == 0) return 0;
      return alignVal - (val % alignVal);
    }

    internal void StringsIndex(uint ix) {
      if (largeStrings) Write(ix);
      else Write((ushort)ix);
    }

    internal void GUIDIndex(uint ix) {
      if (largeGUID) Write(ix);
      else Write((ushort)ix);
    }

    internal void USIndex(uint ix) {
      if (largeUS) Write(ix);
      else Write((ushort)ix);
    }

    internal void BlobIndex(uint ix) {
      if (largeBlob) Write(ix);
      else Write((ushort)ix);
    }

    internal void WriteIndex(MDTable tabIx,uint ix) {
      if (metaData.LargeIx(tabIx)) Write(ix);
      else Write((ushort)ix);
    }

    internal void WriteCodedIndex(CIx code, MetaDataElement elem) {
      metaData.WriteCodedIndex(code,elem,this);
    }
    
    internal void WriteCodeRVA(uint offs) {
      Write(text.RVA() + offs);
    }

    internal void WriteDataRVA(uint offs) {
      Write(sdata.RVA() + offs);
    }

    internal void Write3Bytes(uint val) {
      byte b3 = (byte)((val & FileImage.iByteMask[2]) >> 16);
      byte b2 = (byte)((val & FileImage.iByteMask[1]) >> 8);;
      byte b1 = (byte)(val & FileImage.iByteMask[0]);
      Write(b1);
      Write(b2);
      Write(b3);
    }

        }
  /**************************************************************************/  
        /// <summary>
        /// Descriptor for a file referenced in THIS assembly/module (.file)
        /// </summary>
        public class FileRef : MetaDataElement
        {
    private static readonly uint NoMetaData = 0x1;
    uint nameIx = 0, hashIx = 0;
    uint flags = 0;

    internal FileRef(string name, byte[] hashBytes, bool metaData,
                      bool entryPoint, MetaData md) {
      if (!metaData) flags = NoMetaData;
      if (entryPoint) md.SetEntryPoint(this);
      nameIx = md.AddToStringsHeap(name);
      hashIx = md.AddToBlobHeap(hashBytes);
      tabIx = MDTable.File;
                }

    internal FileRef(uint nameIx, byte[] hashBytes, bool metaData,
                      bool entryPoint, MetaData md) {
      if (!metaData) flags = NoMetaData;
      if (entryPoint) md.SetEntryPoint(this);
      this.nameIx = nameIx;
      hashIx = md.AddToBlobHeap(hashBytes);
      tabIx = MDTable.File;
    }

    internal sealed override uint Size(MetaData md) {
      return 4 + md.StringsIndexSize() + md.BlobIndexSize();
    }

    internal sealed override void Write(FileImage output) {
      output.Write(flags);
      output.StringsIndex(nameIx);
      output.BlobIndex(hashIx);
    }

    internal sealed override uint GetCodedIx(CIx code) {
      switch (code) {
        case (CIx.HasCustomAttr) : return 16; 
        case (CIx.Implementation) : return 0;
      }
      return 0;
    }

        }
  /**************************************************************************/  
        /// <summary>
        /// Descriptor for pinvoke information for a method NOT YET IMPLEMENTED
        /// </summary>
        public class ImplMap : MetaDataElement
        {
    private static readonly ushort NoMangle = 0x01;
    ushort flags;
    Method meth;
    string importName;
    uint iNameIx;
    ModuleRef importScope;

                internal ImplMap(ushort flag, Method implMeth, string iName, ModuleRef mScope) {
      flags = flag;
      meth = implMeth;
      importName = iName;
      importScope = mScope;
      tabIx = MDTable.ImplMap;
      if (iName == null) flags |= NoMangle;
      //throw(new NotYetImplementedException("PInvoke "));
                }

    internal sealed override void BuildTables(MetaData md) {
      if (done) return;
      iNameIx = md.AddToStringsHeap(importName);
      done = true;
    }

    internal sealed override uint Size(MetaData md) {
      return 2+ md.CodedIndexSize(CIx.MemberForwarded) + 
        md.StringsIndexSize() +  md.TableIndexSize(MDTable.ModuleRef);
    }

    internal sealed override void Write(FileImage output) {
      output.Write(flags);
      output.WriteCodedIndex(CIx.MemberForwarded,meth);
      output.StringsIndex(iNameIx);
      output.WriteIndex(MDTable.ModuleRef,importScope.Row);
    }

        }

  /**************************************************************************/  
  /// <summary>
  /// Descriptor for an IL instruction
  /// </summary>
  internal abstract class CILInstruction {
    protected static readonly sbyte maxByteVal = 127;
    protected static readonly sbyte minByteVal = -128;
    protected static readonly byte leadByte = 0xFE;
                protected static readonly uint USHeapIndex = 0x70000000;
    protected static readonly int longInstrStart = (int)Op.arglist;
    public bool twoByteInstr = false;
    public uint size = 0;
    public uint offset;

    internal virtual bool Check(MetaData md) {
      return false;
    }

                internal virtual void Write(FileImage output) { }

  }

  internal class CILByte : CILInstruction {
    byte byteVal;

    internal CILByte(byte bVal) {
      byteVal = bVal;
      size = 1;
    }

    internal override void Write(FileImage output) {
      output.Write(byteVal);
    }

  }

  
  internal class Instr : CILInstruction {
    protected int instr;

    internal Instr(int inst) {
      if (inst >= longInstrStart) {
        instr = inst - longInstrStart;
        twoByteInstr = true;
        size = 2;
      } else {
        instr = inst;
        size = 1;
      }
    }

                internal override void Write(FileImage output) {
      //Console.WriteLine("Writing instruction " + instr + " with size " + size);
      if (twoByteInstr) output.Write(leadByte);
      output.Write((byte)instr);
                }

  }

  internal class IntInstr : Instr {
    int val;
    bool byteNum;

    internal IntInstr(int inst, int num, bool byteSize) : base(inst) {
      val = num;
      byteNum = byteSize;
      if (byteNum) size++;
      else size += 4;
    }

                internal sealed override void Write(FileImage output) {
                        base.Write(output);
      if (byteNum) 
        output.Write((sbyte)val);
      else 
                                output.Write(val); 
                }

  }

  internal class UIntInstr : Instr {
    int val;
    bool byteNum;

                internal UIntInstr(int inst, int num, bool byteSize) : base(inst) {
                        val = num;
      byteNum = byteSize;
      if (byteNum) size++;
      else size += 2;
                }

                internal sealed override void Write(FileImage output) {
                        base.Write(output);
      if (byteNum)
                          output.Write((byte)val);
      else
                                output.Write((ushort)val); 
                }
        
  }

        internal class LongInstr : Instr {
                long val;

                internal LongInstr(int inst, long l) : base(inst) {
                        val = l;
                        size += 8;
                }

                internal sealed override void Write(FileImage output) {
                        base.Write(output);
                        output.Write(val);
                }

        }

  internal class FloatInstr : Instr {
    float fVal;

    internal FloatInstr(int inst, float f) : base(inst) {
      fVal = f;
      size += 4;
    }

                internal sealed override void Write(FileImage output) {
                        base.Write(output);
                        output.Write(fVal);
                }

        }

  internal class DoubleInstr : Instr {
    double val;

    internal DoubleInstr(int inst, double d) : base(inst) {
      val = d;
      size += 8;
    }

                internal sealed override void Write(FileImage output) {
                        base.Write(output);
                        output.Write(val);
                }

        }

  internal class StringInstr : Instr {
    string val;
          byte[] bval;                                                  
    uint strIndex;

    internal StringInstr(int inst, string str) : base(inst) {
      val = str;  
      size += 4;
    }

          internal StringInstr (int inst, byte[] str) : base (inst) {
                  bval = str;
                  size += 4;
          }
                   
    internal sealed override bool Check(MetaData md) {
            if (val != null)
                    strIndex = md.AddToUSHeap(val);
            else
                    strIndex = md.AddToUSHeap (bval);
      return false;
    }

                internal sealed override void Write(FileImage output) {
                        base.Write(output);
                        output.Write(USHeapIndex  | strIndex);
                }

        }

  internal class LabelInstr : CILInstruction {
    CILLabel label;

    internal LabelInstr(CILLabel lab) {
      label = lab;
      label.AddLabelInstr(this);
    }
  }

  internal class FieldInstr : Instr {
    Field field;

    internal FieldInstr(int inst, Field f) : base(inst) {
      field = f;
      size += 4;
    }

                internal sealed override void Write(FileImage output) {
                        base.Write(output);
                        output.Write(field.Token());
                }

        }

  internal class MethInstr : Instr {
    Method meth;

    internal MethInstr(int inst, Method m) : base(inst) {
      meth = m;
      size += 4;
    }

                internal sealed override void Write(FileImage output) {
                        base.Write(output);
                        output.Write(meth.Token());
                }

  }

  internal class SigInstr : Instr {
    CalliSig signature;

    internal SigInstr(int inst, CalliSig sig) : base(inst) {
      signature = sig;
      size += 4;
    }

    internal sealed override bool Check(MetaData md) {
      md.AddToTable(MDTable.StandAloneSig,signature);
      signature.BuildTables(md);
      return false;
    }

                internal sealed override void Write(FileImage output) {
                        base.Write(output);
                        output.Write(signature.Token());
                }
  }

  internal class TypeInstr : Instr {
    MetaDataElement theType;

    internal TypeInstr(int inst, Type aType, MetaData md) : base(inst) {
      theType = aType.GetTypeSpec(md);
      size += 4;
    }

    internal sealed override void Write(FileImage output) {
      base.Write(output);
      output.Write(theType.Token());
                }

  }

  internal class BranchInstr : Instr {
    CILLabel dest;
    private bool shortVer = true;
    private static readonly byte longInstrOffset = 13;
    private int target = 0;

    internal BranchInstr(int inst, CILLabel dst) : base(inst) {
      dest = dst;
      dest.AddBranch(this);
      size++;

      if (inst >= (int) BranchOp.br && inst != (int) BranchOp.leave_s) {
              shortVer = false;
              size += 3;
      }
    }

    internal sealed override bool Check(MetaData md) {
      target = (int)dest.GetLabelOffset() - (int)(offset + size);
      return false;
    }

                internal sealed override void Write(FileImage output) {
                        base.Write(output);
                        if (shortVer)
                                output.Write((sbyte)target);
                        else
                                output.Write(target);
                }

  }

  internal class SwitchInstr : Instr {
    CILLabel[] cases;
                uint numCases = 0;

    internal SwitchInstr(int inst, CILLabel[] dsts) : base(inst) {
      cases = dsts;
                        if (cases != null) numCases = (uint)cases.Length;
      size += 4 + (numCases * 4);
      for (int i=0; i < numCases; i++) {
        cases[i].AddBranch(this);
      }
    }

                internal sealed override void Write(FileImage output) {
                        base.Write(output);
                        output.Write(numCases);
                        for (int i=0; i < numCases; i++) {
                                int target = (int)cases[i].GetLabelOffset() - (int)(offset + size);
                                output.Write(target);
                        }
                }

  }
  /**************************************************************************/  

        public class GenericParameter : MetaDataElement
        {
                MetaDataElement owner;
                MetaData metadata;
                string name;
                uint nameIx;
                short index;

                internal GenericParameter (ClassDef owner, MetaData metadata,
                                short index, string name) : this (owner, metadata, index, name, true)
                {
                }

                internal GenericParameter (MethodDef owner, MetaData metadata,
                                short index, string name) : this (owner, metadata, index, name, true)
                {
                }

                private GenericParameter (MetaDataElement owner, MetaData metadata,
                                short index, string name, bool nadda)
                {
                        this.owner = owner;
                        this.metadata = metadata;
                        this.index = index;
                        tabIx = MDTable.GenericParam;
                        this.name = name;
                }

                public void AddConstraint  (Type constraint) {
                        metadata.AddToTable (MDTable.GenericParamConstraint,
                                        new GenericParamConstraint (this, constraint));
                }

                internal sealed override uint Size(MetaData md) {
                        return (uint) (4 +
                               md.CodedIndexSize(CIx.TypeOrMethodDef) + 
                               4 +
                               md.TableIndexSize(MDTable.TypeDef));
                }

                internal sealed override void BuildTables(MetaData md) {
                        if (done) return;
                        nameIx = md.AddToStringsHeap(name);
                        done = true;
                }

                internal sealed override void Write(FileImage output) {
                        output.Write ((short) index);
                        output.Write ((short) 0);
                        output.WriteCodedIndex(CIx.TypeOrMethodDef, owner);
                        output.Write ((uint) nameIx);
                        output.WriteIndex(MDTable.TypeDef,owner.Row);
                }

    
        }

        internal class GenericParamConstraint : MetaDataElement
        {
                GenericParameter param;
                Type type;

                public GenericParamConstraint (GenericParameter param, Type type) {
                        this.param = param;
                        this.type = type;
                        tabIx = MDTable.GenericParamConstraint;
                }

                internal sealed override uint Size(MetaData md) {
                        return (uint) (md.TableIndexSize(MDTable.GenericParam) +
                                       md.CodedIndexSize(CIx.TypeDefOrRef));
                }

                internal sealed override void Write(FileImage output) {
                        output.WriteIndex(MDTable.GenericParam, param.Row);
                        output.WriteCodedIndex(CIx.TypeDefOrRef, type);
                }


        }

        internal class MethodSpec : MetaDataElement
        {
                Method meth;
                GenericMethodSig g_sig;
                uint sidx;

                internal MethodSpec (Method meth, GenericMethodSig g_sig) {
                        this.meth = meth;
                        this.g_sig = g_sig;
                        tabIx = MDTable.MethodSpec;
                }

                internal sealed override void BuildTables (MetaData md) {
                        if (done) return;
                        sidx = g_sig.GetSigIx (md);
                        done = true;
                }

                internal sealed override uint Size (MetaData md) {
                        return (uint) (md.CodedIndexSize(CIx.MethodDefOrRef) +
                                       md.BlobIndexSize ());
                }

                internal sealed override void Write (FileImage output) {
                    output.WriteCodedIndex (CIx.MethodDefOrRef, meth);
                    output.BlobIndex (sidx);
                }
        }

  /**************************************************************************/
        /// <summary>
        /// Descriptor for interface implemented by a class
        /// </summary>
        public class InterfaceImpl: MetaDataElement
        { 
    ClassDef theClass;
    Class theInterface;

                internal InterfaceImpl(ClassDef theClass, Class theInterface) {
      this.theClass = theClass;
      this.theInterface = theInterface;
                        tabIx = MDTable.InterfaceImpl;
                }

    internal sealed override uint Size(MetaData md) {
      return md.TableIndexSize(MDTable.TypeDef) + 
             md.CodedIndexSize(CIx.TypeDefOrRef);
    }

    internal sealed override void Write(FileImage output) {
      output.WriteIndex(MDTable.TypeDef,theClass.Row);
      output.WriteCodedIndex(CIx.TypeDefOrRef,theInterface);
    }

    internal sealed override uint GetCodedIx(CIx code) { return 5; }

        }
  /**************************************************************************/  
        /// <summary>
        /// Descriptor for a local of a method
        /// </summary>
  public class Local
        {
    private static readonly byte Pinned = 0x45;
    string name;
    Type type;
    bool pinned = false, byref = false;

    /// <summary>
    /// Create a new local variable 
    /// </summary>
    /// <param name="lName">name of the local variable</param>
    /// <param name="lType">type of the local variable</param>
    public Local(string lName, Type lType) {
      name = lName;
      type = lType;
    }

    /// <summary>
    /// Create a new local variable that is byref and/or pinned
    /// </summary>
    /// <param name="lName">local name</param>
    /// <param name="lType">local type</param>
    /// <param name="byRef">is byref</param>
    /// <param name="isPinned">has pinned attribute</param>
                public Local(string lName, Type lType, bool byRef, bool isPinned)
                {
      name = lName;
      type = lType;
      byref = byRef;
      pinned = isPinned;
                }

    internal void TypeSig(MemoryStream str) {
      if (pinned) str.WriteByte(Pinned);
      type.TypeSig(str);
    }

        }
  /**************************************************************************/  
        /// <summary>
        /// Descriptor for the locals for a method
        /// </summary>

  public class LocalSig : Signature
        {
                private static readonly byte LocalSigByte = 0x7;
    Local[] locals;

                public LocalSig(Local[] locals)         {
      this.locals = locals;
      tabIx = MDTable.StandAloneSig;
                }

    internal sealed override void BuildTables(MetaData md) {
      if (done) return;
      MemoryStream sig = new MemoryStream();
      sig.WriteByte(LocalSigByte);
      MetaData.CompressNum((uint)locals.Length,sig);
      for (int i=0; i < locals.Length; i++) {
        ((Local)locals[i]).TypeSig(sig);
      }
      sigIx = md.AddToBlobHeap(sig.ToArray());
      done = true;
    }

  }

  /**************************************************************************/  
        /// <summary>
        /// Descriptor for resources used in this PE file NOT YET IMPLEMENTED
        /// </summary>

  public class ManifestResource : MetaDataElement
        {
    private static readonly uint PublicResource = 0x1;
    private static readonly uint PrivateResource = 0x2;

    string mrName;
    MetaDataElement rRef;
    int fileOffset;
    uint nameIx = 0;
    uint flags = 0;

    public ManifestResource(string name, bool isPub, FileRef fileRef) {
      mrName = name;
      if (isPub) flags = PublicResource;
      else flags = PrivateResource;
      rRef = fileRef;
      tabIx = MDTable.ManifestResource;
      throw(new NotYetImplementedException("Manifest Resources "));
    }

    public ManifestResource(string name, bool isPub, FileRef fileRef, 
                                                            int fileIx) {
      mrName = name;
      if (isPub) flags = PublicResource;
      else flags = PrivateResource;
      rRef = fileRef;
      fileOffset = fileIx;
    }

    public ManifestResource(string name, bool isPub, AssemblyRef assemRef) {
      mrName = name;
      if (isPub) flags = PublicResource;
      else flags = PrivateResource;
      rRef = assemRef;
    }

    internal sealed override void BuildTables(MetaData md) {
      if (done) return;
      nameIx = md.AddToStringsHeap(mrName);
      done = true;
    }

    internal sealed override uint Size(MetaData md) {
      return 8 + md.StringsIndexSize() + 
                 md.CodedIndexSize(CIx.Implementation);
    }

    internal sealed override void Write(FileImage output) {
      output.Write(fileOffset);
      output.Write(flags);
      output.StringsIndex(nameIx);
      output.WriteCodedIndex(CIx.Implementation,rRef);
    }

    internal sealed override uint GetCodedIx(CIx code) { return 18; }

        }
  /**************************************************************************/  
        /// <summary>
        /// Base class for elements in the PropertyMap, EventMap and 
        /// NestedClass MetaData tables
        /// </summary>
  public class MapElem : MetaDataElement
        {
    ClassDef parent;
    uint elemIx;
    MDTable elemTable;

                internal MapElem(ClassDef par, uint elIx, MDTable elemTab) {
      parent = par;
      elemIx = elIx;
      elemTable = elemTab;
                }

    internal sealed override uint Size(MetaData md) {
      return md.TableIndexSize(MDTable.TypeDef) + md.TableIndexSize(elemTable);
    }

    internal sealed override void Write(FileImage output) {
      output.WriteIndex(MDTable.TypeDef,parent.Row);
      output.WriteIndex(elemTable,elemIx);
    }
        }
  /**************************************************************************/  
        /// <summary>
        /// Base class for field/methods (member of a class)
        /// </summary>
        public abstract class Member : MetaDataElement
        {
    protected string name;
    protected uint nameIx = 0, sigIx = 0;
    
                internal Member(string memName)
                {
      name = memName;
                        tabIx = MDTable.MemberRef;
                }

        }
  /**************************************************************************/  
        /// <summary>
  /// MetaData 
  ///   Root (20 bytes + UTF-8 Version String + quad align padding)
  ///   StreamHeaders (8 bytes + null terminated name string + quad align padding)
  ///   Streams 
  ///     #~        (always present - holds metadata tables)
  ///     #Strings  (always present - holds identifier strings)
  ///     #US       (Userstring heap)
  ///     #Blob     (signature blobs)
  ///     #GUID     (guids for assemblies or Modules)
  /// </summary>

  public class MetaData 
        {
                private static readonly int[] CIxShiftMap = {2,2,5,1,2,3,1,1,1,2,3,2,1};
                private static readonly byte StringsHeapMask = 0x1;
                private static readonly byte GUIDHeapMask = 0x2;
                private static readonly byte BlobHeapMask = 0x4;
    private static readonly uint MetaDataSignature = 0x424A5342;
    private static readonly uint maxSmlIxSize = 0xFFFF;
    private static readonly uint max1BitSmlIx = 0x7FFF;
    private static readonly uint max2BitSmlIx = 0x3FFF;
    private static readonly uint max3BitSmlIx = 0x1FFF;
    private static readonly uint max5BitSmlIx = 0x7FF;
    // NOTE: version and stream name strings MUST always be quad padded
    private static readonly string version = "v1.0.3705\0\0\0";
    private static readonly char[] tildeName = {'#','~','\0','\0'};
    private static readonly char[] stringsName = {'#','S','t','r','i','n','g','s','\0','\0','\0','\0'};
    private static readonly char[] usName = {'#','U','S','\0'};
    private static readonly char[] guidName = {'#','G','U','I','D','\0','\0','\0'};
    private static readonly char[] blobName = {'#','B','l','o','b','\0','\0','\0'};
                private static readonly uint MetaDataHeaderSize = 20 + (uint)version.Length;
    private static readonly uint TildeHeaderSize = 24;
    private static readonly uint StreamHeaderSize = 8;
    private static readonly uint numMetaDataTables = (int)MDTable.GenericParamConstraint + 1;
    private static readonly uint tildeHeaderSize = 8 + (uint)tildeName.Length;

    MetaDataStream strings, us, guid, blob;

    MetaDataStream[] streams = new MetaDataStream[5];
    uint numStreams = 5;
    uint tildeTide = 0, tildePadding = 0, tildeStart = 0;
    uint numTables = 0;
    ArrayList[] metaDataTables = new ArrayList[numMetaDataTables];
    ArrayList byteCodes = new ArrayList();
    uint codeSize = 0, codeStart, byteCodePadding = 0, metaDataSize = 0;
                ulong valid = 0, /*sorted = 0x000002003301FA00;*/ sorted = 0;
    bool[] largeIx = new bool[numMetaDataTables];
    bool[] lgeCIx = new bool[(int)CIx.MaxCIx];
                bool largeStrings = false, largeUS = false, largeGUID = false, largeBlob = false;
                private FileImage file;
    private byte heapSizes = 0;
                MetaDataElement entryPoint;
                BinaryWriter output;
    public MSCorLib mscorlib;
    private TypeSpec[] systemTypeSpecs = new TypeSpec[PrimitiveType.NumSystemTypes];
    long mdStart;
                private ArrayList cattr_list;
                
    internal MetaData(FileImage file) {
      // tilde = new MetaDataStream(tildeName,false,0);
      this.file = file;
                        strings = new MetaDataStream(stringsName,new UTF8Encoding(),true);
      us = new MetaDataStream(usName,new UnicodeEncoding(),true);
      guid = new MetaDataStream(guidName,false);
      blob = new MetaDataStream(blobName,true);
      streams[1] = strings;
      streams[2] = us;
      streams[3] = guid;
      streams[4] = blob;
      for (int i=0; i < numMetaDataTables; i++) {
        largeIx[i] = false;
      }
      for (int i=0; i < lgeCIx.Length; i++) {
        lgeCIx[i] = false;
      }
      mscorlib = new MSCorLib(this);
                }
 
    internal TypeSpec GetPrimitiveTypeSpec(int ix) {
      return systemTypeSpecs[ix];
    }

    internal void SetPrimitiveTypeSpec(int ix, TypeSpec typeSpec) {
      systemTypeSpecs[ix] = typeSpec;
    }

    internal uint Size() {
      return metaDataSize;
    }

    
    private void CalcHeapSizes ()
    {
            if (strings.LargeIx()) {
                    largeStrings = true;
                    heapSizes |= StringsHeapMask;
            }
            if (guid.LargeIx()) {
                    largeGUID = true;
                    heapSizes |= GUIDHeapMask;
            }
            if (blob.LargeIx()) {
                    largeBlob = true;
                    heapSizes |= BlobHeapMask;
            }

            largeUS = us.LargeIx();
   }

                internal void StreamSize(byte mask) {
                        heapSizes |= mask;
                }

    internal uint AddToUSHeap(string str) {
      if (str == null) return 0;
      return us.Add(str,true);
   }

                internal uint AddToUSHeap(byte[] str) {
                        if (str == null) return 0;
                        return us.Add (str, true);
                }
                                
    internal uint AddToStringsHeap(string str) {
      if ((str == null) || (str.CompareTo("") == 0)) return 0;
      return strings.Add(str,false);
    }

    internal uint AddToGUIDHeap(Guid guidNum) {
      return guid.Add(guidNum, false);
    }

    internal uint AddToBlobHeap(byte[] blobBytes) {
      if (blobBytes == null) return 0;
      return blob.Add(blobBytes, true);
    }

    internal uint AddToBlobHeap(byte val) {
      return blob.Add(val, true);
    }

    internal uint AddToBlobHeap(sbyte val) {
      return blob.Add(val, true);
    }

    internal uint AddToBlobHeap(ushort val) {
      return blob.Add(val, true);
    }

    internal uint AddToBlobHeap(short val) {
      return blob.Add(val, true);
    }

    internal uint AddToBlobHeap(uint val) {
      return blob.Add(val, true);
    }

    internal uint AddToBlobHeap(int val) {
      return blob.Add(val, true);
    }

    internal uint AddToBlobHeap(ulong val) {
      return blob.Add(val, true);
    }

    internal uint AddToBlobHeap(long val) {
      return blob.Add(val, true);
    }

    internal uint AddToBlobHeap(float val) {
      return blob.Add(val, true);
    }

    internal uint AddToBlobHeap(double val) {
      return blob.Add(val, true);
    }

    internal uint AddToBlobHeap(string val) {
      return blob.Add(val,true);
    }

                internal void AddCustomAttribute (CustomAttribute cattr)
                {
                        if (cattr_list == null)
                                cattr_list = new ArrayList ();
                        cattr_list.Add (cattr);
                }

    private ArrayList GetTable(MDTable tableIx) {
      int tabIx = (int)tableIx;
      if (metaDataTables[tabIx] == null) {
        metaDataTables[tabIx] = new ArrayList();
        valid |= ((ulong)0x1 << tabIx);
        // Console.WriteLine("after creating table " + tableIx + "(" + tabIx + ") valid = " + valid);
        numTables++;
      }
      return metaDataTables[tabIx];
    }
 
    internal void AddToTable(MDTable tableIx, MetaDataElement elem) {
      if (elem.Row > 0) {
        // Console.Out.WriteLine("ERROR - element already in table " + tableIx);
        return;
      }
      // updates Row field of the element
      // Console.WriteLine("Adding element to table " + (uint)tableIx);
      ArrayList table = GetTable(tableIx);
      elem.Row = (uint)table.Count + 1;
      table.Add(elem);
    }

    internal uint TableIndex(MDTable tableIx) {
      if (metaDataTables[(int)tableIx] == null) return 1;
      return (uint)metaDataTables[(int)tableIx].Count+1;
    }

    internal uint AddCode(CILInstructions byteCode) {
      byteCodes.Add(byteCode);
      uint offset = codeSize + codeStart;
      codeSize += byteCode.GetCodeSize();
      return offset;
    }

    internal void SetEntryPoint(MetaDataElement ep) {
      entryPoint = ep;
    }

    internal void AddData(DataConstant cVal) {
      file.AddInitData(cVal);
    }

                internal static void CompressNum(uint val, MemoryStream sig) {
                        if (val < 0x7F) {
                                sig.WriteByte((byte)val);
                        } else if (val < 0x3FFF) {
                                byte b1 = (byte)((val >> 8) | 0x80);
                                byte b2 = (byte)(val & FileImage.iByteMask[0]);
                                sig.WriteByte(b1);
                                sig.WriteByte(b2);
                        } else {
                                byte b1 = (byte)((val >> 24) | 0xC0);
                                byte b2 = (byte)((val & FileImage.iByteMask[2]) >> 16);
                                byte b3 = (byte)((val & FileImage.iByteMask[1]) >> 8);;
                                byte b4 = (byte)(val & FileImage.iByteMask[0]);
                                sig.WriteByte(b1);
                                sig.WriteByte(b2);
                                sig.WriteByte(b3);
                                sig.WriteByte(b4);
                        }
                }

    internal uint CodeSize() {
      return codeSize + byteCodePadding;
    }

    internal uint StringsIndexSize() {
      if (largeStrings) return 4;
      return 2;
    }

    internal uint GUIDIndexSize() {
      if (largeGUID) return 4;
      return 2;
    }

    internal uint USIndexSize() {
      if (largeUS) return 4;
      return 2;
    }

    internal uint BlobIndexSize() {
      if (largeBlob) return 4;
      return 2;
    }

    internal uint CodedIndexSize(CIx code) {
      if (lgeCIx[(uint)code]) return 4;
      return 2;
    }

    internal uint TableIndexSize(MDTable tabIx) {
      if (largeIx[(uint)tabIx]) return 4;
      return 2;
    }
    
    private void SetIndexSizes() {
      for (int i=0; i < numMetaDataTables; i++) {
          if (metaDataTables[i] == null)
                  continue;

          uint count = (uint)metaDataTables[i].Count;
          if (count > maxSmlIxSize)
                  largeIx[i] = true;

            MDTable tabIx = (MDTable)i;
            if (count > max5BitSmlIx) {
              lgeCIx[(int)CIx.HasCustomAttr] = true;
            }
            if (count > max3BitSmlIx) {
              if ((tabIx == MDTable.TypeRef) || (tabIx == MDTable.ModuleRef) || (tabIx == MDTable.Method) || (tabIx == MDTable.TypeSpec) || (tabIx == MDTable.Field)) 
                lgeCIx[(int)CIx.CustomAttributeType] = true;
              if ((tabIx == MDTable.Method) || (tabIx == MDTable.MemberRef)) 
                lgeCIx[(int)CIx.MemberRefParent] = true;
            }
            if (count > max2BitSmlIx) {
              if ((tabIx == MDTable.Field) || (tabIx == MDTable.Param) || (tabIx == MDTable.Property)) 
                lgeCIx[(int)CIx.HasConst] = true;
              if ((tabIx == MDTable.TypeDef) || (tabIx == MDTable.TypeRef) || (tabIx == MDTable.TypeSpec))
                lgeCIx[(int)CIx.TypeDefOrRef] = true;
              if ((tabIx == MDTable.TypeDef) || (tabIx == MDTable.Method) || (tabIx == MDTable.Assembly))
                lgeCIx[(int)CIx.HasDeclSecurity] = true;
              if ((tabIx == MDTable.File) || (tabIx == MDTable.AssemblyRef) || (tabIx == MDTable.ExportedType))
                lgeCIx[(int)CIx.Implementation] = true;
              if ((tabIx == MDTable.Module) || (tabIx == MDTable.ModuleRef) || (tabIx == MDTable.AssemblyRef) || (tabIx == MDTable.TypeRef))
                lgeCIx[(int)CIx.ResolutionScope] = true;
            }
            if (count > max1BitSmlIx) {
              if ((tabIx == MDTable.Field) || (tabIx == MDTable.Param)) 
                lgeCIx[(int)CIx.HasFieldMarshal] = true;
              if ((tabIx == MDTable.Event) || (tabIx == MDTable.Property)) 
                lgeCIx[(int)CIx.HasSemantics] = true;
              if ((tabIx == MDTable.Method) || (tabIx == MDTable.MemberRef)) 
                lgeCIx[(int)CIx.MethodDefOrRef] = true;
              if ((tabIx == MDTable.Field) || (tabIx == MDTable.Method)) 
                lgeCIx[(int)CIx.MemberForwarded] = true; 
              if ((tabIx == MDTable.TypeDef) || (tabIx == MDTable.Method)) 
                lgeCIx[(int)CIx.TypeOrMethodDef] = true; 
            }
      }
    }

    private void SetStreamOffsets() {
      uint sizeOfHeaders = StreamHeaderSize + (uint)tildeName.Length;
      for (int i=1; i < numStreams; i++) {
        sizeOfHeaders += streams[i].headerSize();
      }
      metaDataSize = MetaDataHeaderSize + sizeOfHeaders;
      tildeStart = metaDataSize;
      metaDataSize += tildeTide + tildePadding;
      for (int i=1; i < numStreams; i++) {
        streams[i].Start = metaDataSize;
        metaDataSize += streams[i].Size();
        streams[i].WriteDetails();
      }
    }

    internal void CalcTildeStreamSize() {
CalcHeapSizes ();
      //tilde.SetIndexSizes(strings.LargeIx(),us.LargeIx(),guid.LargeIx(),blob.LargeIx());
      tildeTide = TildeHeaderSize;
      tildeTide += 4 * numTables;
      //Console.WriteLine("Tilde header + sizes = " + tildeTide);
      for (int i=0; i < numMetaDataTables; i++) {
        if (metaDataTables[i] != null) {
          ArrayList table = metaDataTables[i];
          // Console.WriteLine("Meta data table " + i + " at offset " + tildeTide);
          tildeTide += (uint)table.Count * ((MetaDataElement)table[0]).Size(this);
          // Console.WriteLine("Metadata table " + i + " has size " + table.Count);
          // Console.WriteLine("tildeTide = " + tildeTide);
        }
      }
      if ((tildeTide % 4) != 0) tildePadding = 4 - (tildeTide % 4);
      //Console.WriteLine("tildePadding = " + tildePadding);
    }

    internal void WriteTildeStream(FileImage output) {
      long startTilde = output.Seek(0,SeekOrigin.Current);
                        output.Write((uint)0); // Reserved
                        output.Write((byte)1); // MajorVersion
                        output.Write((byte)0); // MinorVersion
                        output.Write(heapSizes);
                        output.Write((byte)1); // Reserved
                        output.Write(valid);
                        output.Write(sorted);
                        for (int i=0; i < numMetaDataTables; i++) {
                                if (metaDataTables[i] != null) {
                                        uint count = (uint)metaDataTables[i].Count;
                                        output.Write(count);
                                }
                        }
      long tabStart = output.Seek(0,SeekOrigin.Current);
      // Console.WriteLine("Starting metaData tables at " + tabStart);
                        for (int i=0; i < numMetaDataTables; i++) {
                                if (metaDataTables[i] != null) {
          // Console.WriteLine("Starting metaData table " + i + " at " + (output.Seek(0,SeekOrigin.Current) - startTilde));
          ArrayList table = metaDataTables[i];
                                        for (int j=0; j < table.Count; j++) {
             ((MetaDataElement)table[j]).Write(output);
                                        }
                                }
                        }
      // Console.WriteLine("Writing padding at " + output.Seek(0,SeekOrigin.Current));
      for (int i=0; i < tildePadding; i++) output.Write((byte)0);
                }

    private void BuildTable(ArrayList table) {
      if (table == null) return;
      for (int j=0; j < table.Count; j++) {
        ((MetaDataElement)table[j]).BuildTables(this);
      }
    }

    internal void BuildMetaData(uint codeStartOffset) {
      codeStart = codeStartOffset;
      BuildTable(metaDataTables[(int)MDTable.TypeDef]);
      BuildTable(metaDataTables[(int)MDTable.MemberRef]);
      BuildTable(metaDataTables[(int)MDTable.GenericParam]);
      BuildTable(metaDataTables[(int)MDTable.MethodSpec]);
      BuildTable(metaDataTables[(int)MDTable.GenericParamConstraint]);

      if (cattr_list != null) {
              foreach (CustomAttribute cattr in cattr_list)
                      cattr.BuildTables (this);
      }

/*      for (int i=0; i < metaDataTables.Length; i++) {
        ArrayList table = metaDataTables[i];
        if (table != null) {
          for (int j=0; j < table.Count; j++) {
            ((MetaDataElement)table[j]).BuildTables(this);
          }
        }
      }
      */

                        SetIndexSizes();
                        for (int i=1; i < numStreams; i++) {
                                streams[i].EndStream();
                        }
                        CalcTildeStreamSize();
                        SetStreamOffsets();
      byteCodePadding = NumToAlign(codeSize,4);
      if (entryPoint != null) file.SetEntryPoint(entryPoint.Token());
    }

    internal void WriteByteCodes(FileImage output) {
      for (int i=0; i < byteCodes.Count; i++) {
        ((CILInstructions)byteCodes[i]).Write(output);
      }
      for (int i=0; i < byteCodePadding; i++) {
        output.Write((byte)0);
      }
    }

    internal void WriteMetaData(FileImage output) {
                        this.output = output;
      mdStart = output.Seek(0,SeekOrigin.Current);
      // Console.WriteLine("Writing metaData at " + Hex.Long(mdStart));
      output.Write(MetaDataSignature);
      output.Write((short)1);  // Major Version
      output.Write((short)1);  // Minor Version  ECMA = 0, PEFiles = 1
      output.Write(0);         // Reserved
      output.Write(version.Length);
      output.Write(version.ToCharArray());   // version string is already zero padded
      output.Write((short)0);
      output.Write((ushort)numStreams);
      // write tilde header
      output.Write(tildeStart);
      output.Write(tildeTide + tildePadding);
      output.Write(tildeName);
      for (int i=1; i < numStreams; i++) streams[i].WriteHeader(output);
      // Console.WriteLine("Writing tilde stream at " + output.Seek(0,SeekOrigin.Current) + " = " + tildeStart);
      WriteTildeStream(output);
      for (int i=1; i < numStreams; i++) streams[i].Write(output);
      // Console.WriteLine("Finished Writing metaData at " + output.Seek(0,SeekOrigin.Current));
    }

                internal bool LargeStringsIndex() { return strings.LargeIx(); }
                internal bool LargeGUIDIndex() { return guid.LargeIx(); }
                internal bool LargeUSIndex() { return us.LargeIx(); }
                internal bool LargeBlobIndex() { return blob.LargeIx(); }

    internal bool LargeIx(MDTable tabIx) { return largeIx[(uint)tabIx]; }


                private uint NumToAlign(uint val, uint alignVal) {
                        if ((val % alignVal) == 0) return 0;
                        return alignVal - (val % alignVal);
                }

    internal void WriteCodedIndex(CIx code, MetaDataElement elem, FileImage output) {
      uint ix = 0;
      if (elem != null) { 
        ix = (elem.Row << CIxShiftMap[(uint)code]) | elem.GetCodedIx(code);
        // Console.WriteLine("coded index = " + ix + " row = " + elem.Row);
      //} else {
        // Console.WriteLine("elem for coded index is null");
      }
      if (lgeCIx[(uint)code]) 
        output.Write(ix);
      else
        output.Write((ushort)ix);
    }

        }
  /**************************************************************************/  
        /// <summary>
        /// Base class for all Meta Data table elements
        /// </summary>

  public abstract class MetaDataElement
        {

    protected ArrayList customAttributes;
    private uint row = 0;
    protected bool done = false;
                protected MDTable tabIx;

    internal MetaDataElement() { }

    public uint Row {
      get {
        return row;
      }
      set {
        if (row == 0) row = value;
      }
    }

    internal virtual uint GetCodedIx(CIx code) { return 0; }

    /// <summary>
    /// Add a custom attribute to this item
    /// </summary>
    /// <param name="ctorMeth">the constructor method for this attribute</param>
    /// <param name="val">the byte value of the parameters</param>
    public void AddCustomAttribute(Method ctorMeth, byte[] val) {
      if (customAttributes == null) {
        customAttributes = new ArrayList();
      } 
      customAttributes.Add(new CustomAttribute(this,ctorMeth,val));
    }

    /// <summary>
    /// Add a custom attribute to this item
    /// </summary>
    /// <param name="ctorMeth">the constructor method for this attribute</param>
    /// <param name="val">the constant values of the parameters</param>
    public void AddCustomAttribute(Method ctorMeth, Constant[] cVals) {
      if (customAttributes == null) {
        customAttributes = new ArrayList();
      } 
//      customAttributes.Add(new CustomAttribute(this,ctorMeth,cVals));
    }

    internal uint Token() {
      return (((uint)tabIx << 24) | row);
    }

    internal virtual void BuildTables(MetaData md) {
      done = true;
    }

    internal virtual uint Size(MetaData md) { 
      return 0;
    }

    internal virtual void Write(FileImage output) {   }

        }
  /**************************************************************************/  
        /// <summary>
        /// Stream in the Meta Data  (#Strings, #US, #Blob and #GUID)
        /// </summary>

  internal class MetaDataStream : BinaryWriter 
        {
                private static readonly uint StreamHeaderSize = 8;
                private static uint maxSmlIxSize = 0xFFFF;
                
    private uint start = 0; 
    uint size = 0, tide = 1;
    bool largeIx = false;
    uint sizeOfHeader;
    char[] name;
    Hashtable htable = new Hashtable();
    Hashtable btable = new Hashtable (new ByteArrayHashCodeProvider (), new ByteArrayComparer ());

    internal MetaDataStream(char[] name, bool addInitByte) : base(new MemoryStream()) {
      if (addInitByte) { Write((byte)0); size = 1; }
      this.name = name;
      sizeOfHeader = StreamHeaderSize + (uint)name.Length;
    }

    internal MetaDataStream(char[] name, System.Text.Encoding enc, bool addInitByte) : base(new MemoryStream(),enc) {
      if (addInitByte) { Write((byte)0); size = 1; }
      this.name = name;
      sizeOfHeader = StreamHeaderSize + (uint)name.Length;
                }

    public uint Start {
      get {
        return start;
      }
      set {
        start = value;
      }
    }
  
    internal uint headerSize() {
      // Console.WriteLine(name + " stream has headersize of " + sizeOfHeader);
      return sizeOfHeader;
    }

    internal void SetSize(uint siz) {
      size = siz;
    }

    internal uint Size() {
      return size;
    }

    internal bool LargeIx() {
      return largeIx;
    }

    internal void WriteDetails() {
      // Console.WriteLine(name + " - size = " + size);
    }

                internal uint Add(string str, bool prependSize) {
      Object val = htable[str];
      uint index = 0;
      if (val == null) { 
        index = size;
        htable[str] = index;
        char[] arr = str.ToCharArray();
        if (prependSize) CompressNum((uint)arr.Length*2+1);
        Write(arr);
        Write((byte)0);
        size = (uint)Seek(0,SeekOrigin.Current);
      } else {
        index = (uint)val;
      }
                        return index;
                }
                internal uint Add (byte[] str, bool prependSize) {
                        Object val = btable [str];
                        uint index = 0;
                        if (val == null) {
                                index = size;
                                btable [str] = index;
                                if (prependSize) CompressNum ((uint) str.Length);
                                Write (str);
                                size = (uint) Seek (0, SeekOrigin.Current);
                        } else {
                                index = (uint) val;
                        }
                        return index;
                }

                    
                internal uint Add(Guid guid, bool prependSize) {
                        byte [] b = guid.ToByteArray ();
                        if (prependSize) CompressNum ((uint) b.Length);
                        Write(guid.ToByteArray());
                        size =(uint)Seek(0,SeekOrigin.Current);
                        return tide++;
                }

                internal uint Add(byte[] blob) {
                        uint ix = size;
                        CompressNum((uint)blob.Length);
                        Write(blob);
                        size = (uint)Seek(0,SeekOrigin.Current);
                        return ix;
                }

    internal uint Add(byte val, bool prependSize) {
      uint ix = size;
      if (prependSize) CompressNum (1);
      Write(val);
      size = (uint)Seek(0,SeekOrigin.Current);
      return ix;
    }

    internal uint Add(sbyte val, bool prependSize) {
      uint ix = size;
      if (prependSize) CompressNum (1);
      Write(val);
      size = (uint)Seek(0,SeekOrigin.Current);
      return ix;
    }

    internal uint Add(ushort val, bool prependSize) {
      uint ix = size;
      if (prependSize) CompressNum (2);
      Write(val);
      size = (uint)Seek(0,SeekOrigin.Current);
      return ix;
    }

    internal uint Add(short val, bool prependSize) {
      uint ix = size;
      if (prependSize) CompressNum (2);
      Write(val);
      size = (uint)Seek(0,SeekOrigin.Current);
      return ix;
    }

    internal uint Add(uint val, bool prependSize) {
      uint ix = size;
      if (prependSize) CompressNum (4);
      Write(val);
      size = (uint)Seek(0,SeekOrigin.Current);
      return ix;
    }

    internal uint Add(int val, bool prependSize) {
      uint ix = size;
      if (prependSize) CompressNum (4);
      Write (val);
      size = (uint)Seek(0,SeekOrigin.Current);
      return ix;
    }

    internal uint Add(ulong val, bool prependSize) {
      uint ix = size;
      if (prependSize) CompressNum (8);
      Write(val);
      size = (uint)Seek(0,SeekOrigin.Current);
      return ix;
    }

    internal uint Add(long val, bool prependSize) {
      uint ix = size;
      if (prependSize) CompressNum (8);
      Write(val);
      size = (uint)Seek(0,SeekOrigin.Current);
      return ix;
    }

    internal uint Add(float val, bool prependSize) {
      uint ix = size;
      if (prependSize) CompressNum (4);
      Write(val);
      size = (uint)Seek(0,SeekOrigin.Current);
      return ix;
    }

    internal uint Add(double val, bool prependSize) {
      uint ix = size;
      if (prependSize) CompressNum (8);
      Write(val);
      size = (uint)Seek(0,SeekOrigin.Current);
      return ix;
    }

                private void CompressNum(uint val) {
      if (val < 0x7F) {
        Write((byte)val);
      } else if (val < 0x3FFF) {
        byte b1 = (byte)((val >> 8) | 0x80);
        byte b2 = (byte)(val & FileImage.iByteMask[0]);
        Write(b1);
        Write(b2);
      } else {
        byte b1 = (byte)((val >> 24) | 0xC0);
        byte b2 = (byte)((val & FileImage.iByteMask[2]) >> 16);
        byte b3 = (byte)((val & FileImage.iByteMask[1]) >> 8);;
        byte b4 = (byte)(val & FileImage.iByteMask[0]);
        Write(b1);
        Write(b2);
        Write(b3);
        Write(b4);
      }
    }

    private void QuadAlign() {
      if ((size % 4) != 0) {
        uint pad = 4 - (size % 4);
        size += pad;
        for (int i=0; i < pad; i++) {
          Write((byte)0);
        }
      }
    }

                internal void EndStream() {
                        QuadAlign();
                        if (size > maxSmlIxSize) {
                                largeIx = true;
                        }
                }

    internal void WriteHeader(BinaryWriter output) {
      output.Write(start);
      output.Write(size);
      output.Write(name);
    }

                internal virtual void Write(BinaryWriter output) {
       // Console.WriteLine("Writing " + name + " stream at " + output.Seek(0,SeekOrigin.Current) + " = " + start);
      MemoryStream str = (MemoryStream)BaseStream;
                        output.Write(str.ToArray());
                }

        }
  /**************************************************************************/  
        /// <summary>
        /// Base class for Method Descriptors
        /// </summary>

  public abstract class Method : Member
        {
    protected CallConv callConv = CallConv.Default;
    protected Type retType;

    internal Method(string methName, Type rType) : base(methName)
                {
      retType = rType;
                }

    /// <summary>
    /// Add calling conventions to this method descriptor
    /// </summary>
    /// <param name="cconv"></param>
    public void AddCallConv(CallConv cconv) {
      callConv |= cconv;
    }

                internal abstract void TypeSig(MemoryStream sig);

    internal uint GetSigIx(MetaData md) {
      MemoryStream sig = new MemoryStream();
                        TypeSig(sig);
      return md.AddToBlobHeap(sig.ToArray());
    }

    internal Type GetRetType() {
      return retType;
    }

        }
  /**************************************************************************/  
        /// <summary>
        /// Descriptor for a method defined in THIS assembly/module
        /// IL     .method
        /// </summary>

  public class MethodDef : Method
        {
    private static readonly ushort PInvokeImpl = 0x2000;
    //private static readonly uint UnmanagedExport = 0x0008;
   // private static readonly byte LocalSigByte = 0x7;
    uint parIx = 0, textOffset = 0;

    MetaData metaData;
    CILInstructions code;
    ArrayList securityActions = new ArrayList();
                Param[] parList;
    Local[] locals;
    bool initLocals;
    ushort methFlags = 0, implFlags = 0;
    int maxStack = 0, numPars = 0;
    bool entryPoint = false;
    LocalSig localSig;
                ArrayList varArgSigList;
    ImplMap pinvokeImpl;


                internal MethodDef(MetaData md, string name, Type retType, Param[] pars) : base(name,retType) {
      metaData = md;
                        parList = pars;
                        if (parList != null) numPars = parList.Length;
      tabIx = MDTable.Method;
                }

    internal MethodDef(MetaData md, MethAttr mAttrSet, ImplAttr iAttrSet, string name, Type retType, Param[] pars) : base(name,retType) {
      metaData = md;
                        parList = pars;
                        if (parList != null) numPars = parList.Length;
      // Console.WriteLine("Creating method " + name + " with " + numPars + " parameters");
                        methFlags = (ushort)mAttrSet;
      implFlags = (ushort)iAttrSet;
      tabIx = MDTable.Method;
    }

                internal Param[] GetPars() {
                        return parList;
                }

    /// <summary>
    /// Add some attributes to this method descriptor
    /// </summary>
    /// <param name="ma">the attributes to be added</param>
    public void AddMethAttribute(MethAttr ma) {
      methFlags |= (ushort)ma;
    }

    /// <summary>
    /// Add some implementation attributes to this method descriptor
    /// </summary>
    /// <param name="ia">the attributes to be added</param>
    public void AddImplAttribute(ImplAttr ia) {
      implFlags |= (ushort)ia;
    }

    public void AddPInvokeInfo(ModuleRef scope, string methName,
                               PInvokeAttr callAttr) {
      pinvokeImpl = new ImplMap((ushort)callAttr,this,methName,scope);
      methFlags |= PInvokeImpl;
    }

    /// <summary>
    ///  Add a named generic type parameter
    /// </summary>
    public GenericParameter AddGenericParameter (short index, string name) {
            GenericParameter gp = new GenericParameter (this, metaData, index, name);
            metaData.AddToTable (MDTable.GenericParam, gp);
            return gp;
    }

    /// <summary>
    /// Set the maximum stack height for this method
    /// </summary>
    /// <param name="maxStack">the maximum height of the stack</param>
    public void SetMaxStack(int maxStack) {
      this.maxStack = maxStack; 
    }

    /// <summary>
    /// Add local variables to this method
    /// </summary>
    /// <param name="locals">the locals to be added</param>
    /// <param name="initLocals">are locals initialised to default values</param>
    public void AddLocals(Local[] locals, bool initLocals) {
      this.locals = locals;
      this.initLocals = initLocals;
    }

    /// <summary>
    /// Mark this method as having an entry point
    /// </summary>
    public void DeclareEntryPoint() {
      entryPoint = true;
    }

    /// <summary>
    /// Create a code buffer for this method to add the IL instructions to
    /// </summary>
    /// <returns>a buffer for this method's IL instructions</returns>
    public CILInstructions CreateCodeBuffer() {
      code = new CILInstructions(metaData);
      return code;
    }

    /// <summary>
    /// Make a method reference descriptor for this method to be used 
    /// as a callsite signature for this vararg method
    /// </summary>
    /// <param name="optPars">the optional pars for the vararg method call</param>
    /// <returns></returns>
    public MethodRef MakeVarArgSignature(Type[] optPars) {
      Type[] pars = new Type[numPars];
      MethodRef varArgSig;
      for (int i=0; i < numPars; i++) {
        pars[i] = parList[i].GetParType();
      }
      varArgSig = new MethodRef(this,name,retType,pars,true,optPars);

      if (varArgSigList == null)
              varArgSigList = new ArrayList ();
      varArgSigList.Add (varArgSig);
      return varArgSig;
    }

                internal sealed override void TypeSig(MemoryStream sig) {
                        sig.WriteByte((byte)callConv);
                        MetaData.CompressNum((uint)numPars,sig);
                        retType.TypeSig(sig);
                        for (ushort i=0; i < numPars; i++) {
                                parList[i].seqNo = (ushort)(i+1);
                                parList[i].TypeSig(sig);
                        }
                }

    internal sealed override void BuildTables(MetaData md) {
      if (done) return;
      if (pinvokeImpl != null) {
        md.AddToTable(MDTable.ImplMap,pinvokeImpl);
        pinvokeImpl.BuildTables(md);
      }
      if (entryPoint) md.SetEntryPoint(this);
      uint locToken = 0;
      if (locals != null) {
        localSig = new LocalSig(locals);
        md.AddToTable(MDTable.StandAloneSig,localSig);
        localSig.BuildTables(md);
        locToken = localSig.Token();
      }
                        if (code != null) {
                                code.CheckCode(locToken,initLocals,maxStack);
                                textOffset = md.AddCode(code);
                        }
      nameIx = md.AddToStringsHeap(name);
      sigIx = GetSigIx(md);
      parIx = md.TableIndex(MDTable.Param);
                        for (int i=0; i < numPars; i++) {
                                md.AddToTable(MDTable.Param,parList[i]);
                                parList[i].BuildTables(md);
                        }
      if (varArgSigList != null) {
              foreach (MethodRef varArgSig in varArgSigList) {
        md.AddToTable(MDTable.MemberRef,varArgSig);
        varArgSig.BuildTables(md);
      }
      }
      // Console.WriteLine("method has " + numPars + " parameters");
      done = true;
    }

    internal sealed override uint Size(MetaData md) {
      return 8 + md.StringsIndexSize() + md.BlobIndexSize() + md.TableIndexSize(MDTable.Param);
    }

    internal sealed override void Write(FileImage output) {
      if (ZeroRva ()) output.Write(0);
      else output.WriteCodeRVA(textOffset);
      output.Write(implFlags);
      output.Write(methFlags);
      output.StringsIndex(nameIx);
      output.BlobIndex(sigIx);
      output.WriteIndex(MDTable.Param,parIx);
    }

    internal bool ZeroRva () {
        return (((methFlags & (ushort)MethAttr.Abstract) != 0) ||
                        ((implFlags & (ushort)ImplAttr.Runtime) != 0) ||
                        ((implFlags & (ushort)ImplAttr.InternalCall) != 0) || 
                        (pinvokeImpl != null)); // TODO: Not entirely true but works for now
    }

    internal sealed override uint GetCodedIx(CIx code) {
                        switch (code) {
                                case (CIx.HasCustomAttr) : return 0; 
                                case (CIx.HasDeclSecurity) : return 1; 
                                case (CIx.MemberRefParent) : return 3; 
                                case (CIx.MethodDefOrRef) : return 0; 
                                case (CIx.MemberForwarded) : return 1; 
                                case (CIx.CustomAttributeType) : return 2; 
                                case (CIx.TypeOrMethodDef) : return 1; 
                        }
                        return 0;
    }

        }
  /**************************************************************************/  
        /// <summary>
        /// Descriptor for an overriding method (.override)
        /// </summary>
  public class MethodImpl : MetaDataElement
        {
    ClassDef parent;
    Method header, body;

                internal MethodImpl(ClassDef par, Method decl, Method bod)      {
      parent = par;
      header = decl;
      body = bod;
                        tabIx = MDTable.MethodImpl;
                }

    internal sealed override uint Size(MetaData md) {
      return md.TableIndexSize(MDTable.TypeDef) + 2 * md.CodedIndexSize(CIx.MethodDefOrRef);
    }

    internal sealed override void Write(FileImage output) {
      output.WriteIndex(MDTable.TypeDef,parent.Row);
      output.WriteCodedIndex(CIx.MethodDefOrRef,body);
      output.WriteCodedIndex(CIx.MethodDefOrRef,header);
    }

        }
  /**************************************************************************/  
        /// <summary>
        /// Descriptor for a method defined in another assembly/module
        /// </summary>
  public class MethodRef : Method
  {
    private static readonly byte Sentinel = 0x41;
                Type[] parList, optParList;
    MetaDataElement parent;
    uint numPars = 0, numOptPars = 0;

    internal MethodRef(MetaDataElement paren, string name, Type retType,
        Type[] pars, bool varArgMeth, Type[] optPars) : base(name,retType) {
      parent = paren;
      parList = pars;
      if (parList != null) numPars = (uint)parList.Length;
      if (varArgMeth) {
        optParList = optPars;
        if (optParList != null) numOptPars = (uint)optParList.Length;
        callConv = CallConv.Vararg;
      }
    }

                internal sealed override void TypeSig(MemoryStream sig) {
                        sig.WriteByte((byte)callConv);
                        MetaData.CompressNum(numPars+numOptPars,sig);
                        retType.TypeSig(sig);
                        for (int i=0; i < numPars; i++) {
                                parList[i].TypeSig(sig);
                        }
      if (numOptPars > 0) {
        sig.WriteByte(Sentinel);
        for (int i=0; i < numOptPars; i++) {
          optParList[i].TypeSig(sig);
        }
      }
                }

    internal sealed override void BuildTables(MetaData md) {
      if (done) return;
      nameIx = md.AddToStringsHeap(name);
      sigIx = GetSigIx(md);
      done = true;
    }

    internal sealed override uint Size(MetaData md) {
      return md.CodedIndexSize(CIx.MemberRefParent) + md.StringsIndexSize() + md.BlobIndexSize();
    }

    internal sealed override void Write(FileImage output) {
      output.WriteCodedIndex(CIx.MemberRefParent,parent);
      output.StringsIndex(nameIx);
      output.BlobIndex(sigIx);
    }

    internal sealed override uint GetCodedIx(CIx code) {
      switch (code) {
        case (CIx.HasCustomAttr) : return 6; 
        case (CIx.MethodDefOrRef) : return 1; 
        case (CIx.CustomAttributeType) : return 3; 
      }
                        return 0;
    }

        }
  /**************************************************************************/  
        /// <summary>
        /// Descriptor for Property and Event methods
        /// </summary>
  public class MethodSemantics : MetaDataElement {

    Feature.MethodType type;
    MethodDef meth;
    Feature eventOrProp;

    internal MethodSemantics(Feature.MethodType mType, MethodDef method, Feature feature) {
      type = mType;
      meth = method;
      eventOrProp = feature;
                        tabIx = MDTable.MethodSemantics;
                }

    internal sealed override uint Size(MetaData md) {
      return 2 + md.TableIndexSize(MDTable.Method) + md.CodedIndexSize(CIx.HasSemantics);
    }

    internal sealed override void Write(FileImage output) {
      output.Write((ushort)type);
      output.WriteIndex(MDTable.Method,meth.Row);
      output.WriteCodedIndex(CIx.HasSemantics,eventOrProp);
    }

        }
  /**************************************************************************/  
        /// <summary>
        /// Descriptor for a FunctionPointer type
        /// </summary>
        /// 
        public class MethPtrType : Type
        {
                bool varArgMeth;
                Type retType;
                Type [] parList;
                Type [] optParList;
                CallConv callConv;
                uint numPars;
                uint numOptPars;
                uint sigIx = 0;

    /// <summary>
    /// Create a new function pointer type
    /// </summary>
    /// <param name="meth">the function to be referenced</param>
    public MethPtrType (CallConv callconv, Type retType, Type[] pars,
                    bool varArgMeth, Type[] optPars) : base(0x1B) {
      this.retType = retType;
      callConv = callconv;
      parList = pars;
      this.varArgMeth = varArgMeth;
      if (parList != null) numPars = (uint)parList.Length;
      if (varArgMeth) {
        optParList = optPars;
        if (optParList != null) numOptPars = (uint)optParList.Length;
        callConv |= CallConv.Vararg;
      }
      tabIx = MDTable.TypeSpec;
    }
  
    internal sealed override void TypeSig(MemoryStream sig) {
      sig.WriteByte(typeIndex);
      // Bootlegged from method ref
      sig.WriteByte((byte)callConv);
      MetaData.CompressNum (numPars + numOptPars, sig);
      retType.TypeSig (sig);
      for (int i=0; i < numPars; i++) {
              parList[i].TypeSig (sig);
      }
      if (varArgMeth) {
              sig.WriteByte (0x41); // Write the sentinel
        for (int i=0; i < numOptPars; i++) {
          optParList[i].TypeSig (sig);
        }
      }
    }

    internal sealed override void BuildTables(MetaData md) {
      if (done) return;
      MemoryStream sig = new MemoryStream();
      TypeSig(sig);
      sigIx = md.AddToBlobHeap(sig.ToArray());
      done = true;
    }

                internal sealed override uint Size(MetaData md) {
                        return md.BlobIndexSize();
                }

                internal sealed override void Write(FileImage output) {
                        output.BlobIndex(sigIx);
                }

                internal sealed override uint GetCodedIx(CIx code) { return 0x1B; }

        }
  /**************************************************************************/  
        /// <summary>
        /// Descriptor for THIS module
        /// </summary>
  public class Module : ResolutionScope
        {
    Guid mvid;
    uint mvidIx = 0;

                internal Module(string name, MetaData md) : base(name,md)       {
      mvid = Guid.NewGuid();
      mvidIx = md.AddToGUIDHeap(mvid);
      tabIx = MDTable.Module;
    }

    public Guid Guid {
      get { return mvid; }
    }

    internal sealed override uint Size(MetaData md) {
      return 2 + md.StringsIndexSize() + 3 * md.GUIDIndexSize();
    }

    internal sealed override void Write(FileImage output) {
      output.Write((short)0);
      output.StringsIndex(nameIx);
      output.GUIDIndex(mvidIx);
      output.GUIDIndex(0);
      output.GUIDIndex(0);
    }

    internal sealed override uint GetCodedIx(CIx code) {
                        switch (code) {
                                case (CIx.HasCustomAttr) : return 7; 
                                case (CIx.ResolutionScope) : return 0;
                        }
                        return 0;
    }
  }
  /**************************************************************************/  
        /// <summary>
        /// Descriptor for another module in THIS assembly
        /// </summary>
        public class ModuleRef : ResolutionScope, IExternRef
        {

                internal ModuleRef(MetaData md, string name) : base(name,md) {
      tabIx = MDTable.ModuleRef;
                }

    /// <summary>
    /// Add a class to this external module.  This is a class declared in
    /// another module of THIS assembly.
    /// </summary>
    /// <param name="nsName">name space name</param>
    /// <param name="name">class name</param>
    /// <returns>a descriptor for this class in another module</returns>
    public ClassRef AddClass(string nsName, string name) {
      ClassRef aClass = new ClassRef(nsName,name,metaData);
      metaData.AddToTable(MDTable.TypeRef,aClass);
      aClass.SetParent(this);
      return aClass;
    }

    /// <summary>
    /// Make a file descriptor to correspond to this module.  The file
    /// descriptor will have the same name as the module descriptor
    /// </summary>
    /// <param name="hashBytes">the hash of the file</param>
    /// <param name="hasMetaData">the file contains metadata</param>
    /// <param name="entryPoint">the program entry point is in this file</param>
    /// <returns>a descriptor for the file which contains this module</returns>
    public FileRef MakeFile(byte[] hashBytes, bool hasMetaData, bool entryPoint) {
      FileRef file = new FileRef(nameIx,hashBytes,hasMetaData,entryPoint,metaData);
      metaData.AddToTable(MDTable.File,file);
      return file;
    }

    /// <summary>
    /// Add a value class to this module.  This is a class declared in
    /// another module of THIS assembly.
    /// </summary>
    /// <param name="nsName">name space name</param>
    /// <param name="name">class name</param>
    /// <returns></returns>
    public ClassRef AddValueClass(string nsName, string name) {
      ClassRef aClass = new ClassRef(nsName,name,metaData);
      metaData.AddToTable(MDTable.TypeRef,aClass);
      aClass.SetParent(this);
      aClass.MakeValueClass();
      return aClass;
    }

    /// <summary>
    /// Add a class which is declared public in this external module of
    /// THIS assembly.  This class will be exported from this assembly.
    /// The ilasm syntax for this is .extern class
    /// </summary>
    /// <param name="attrSet">attributes of the class to be exported</param>
    /// <param name="nsName">name space name</param>
    /// <param name="name">external class name</param>
    /// <param name="declFile">the file where the class is declared</param>
    /// <param name="isValueClass">is this class a value type?</param>
    /// <returns>a descriptor for this external class</returns>
    public ExternClassRef AddExternClass(TypeAttr attrSet, string nsName, 
                                         string name, FileRef declFile, 
                                         bool isValueClass) {
      ExternClassRef cRef = new ExternClassRef(attrSet,nsName,name,declFile,metaData);
      metaData.AddToTable(MDTable.TypeRef,cRef);
      cRef.SetParent(this);
      if (isValueClass) cRef.MakeValueClass();
      return cRef;
    }

    /// <summary>
    /// Add a "global" method in another module
    /// </summary>
    /// <param name="name">method name</param>
    /// <param name="retType">return type</param>
    /// <param name="pars">method parameter types</param>
    /// <returns>a descriptor for this method in anther module</returns>
    public MethodRef AddMethod(string name, Type retType, Type[] pars) {
      MethodRef meth = new MethodRef(this,name,retType,pars,false,null);
      metaData.AddToTable(MDTable.MemberRef,meth);
      return meth;
    }

    /// <summary>
    /// Add a vararg method to this class
    /// </summary>
    /// <param name="name">method name</param>
    /// <param name="retType">return type</param>
    /// <param name="pars">parameter types</param>
    /// <param name="optPars">optional param types for this vararg method</param>
    /// <returns>a descriptor for this method</returns>
    public MethodRef AddVarArgMethod(string name, Type retType, 
      Type[] pars, Type[] optPars) {
      MethodRef meth = new MethodRef(this,name,retType,pars,true,optPars);
      metaData.AddToTable(MDTable.MemberRef,meth);
      return meth;
    }

    /// <summary>
    /// Add a field in another module
    /// </summary>
    /// <param name="name">field name</param>
    /// <param name="fType">field type</param>
    /// <returns>a descriptor for this field in another module</returns>
    public FieldRef AddField(string name, Type fType) {
      FieldRef field = new FieldRef(this,name,fType);
      metaData.AddToTable(MDTable.MemberRef,field);
      return field;
    }

    internal sealed override uint Size(MetaData md) {
      return md.StringsIndexSize();
    }

    internal sealed override void Write(FileImage output) {
      output.StringsIndex(nameIx);
    }

    internal sealed override uint GetCodedIx(CIx code) {
      switch (code) {
        case (CIx.HasCustomAttr) : return 12; 
        case (CIx.MemberRefParent) : return 2; 
        case (CIx.ResolutionScope) : return 1; 
      }
                        return 0;
    }
 
  }
  /**************************************************************************/  
        /// <summary>
        /// Descriptors for native types used for marshalling
        /// </summary>
  public class NativeType {
    public static readonly NativeType Void = new NativeType(0x01);
    public static readonly NativeType Boolean = new NativeType(0x02);
    public static readonly NativeType Int8 = new NativeType(0x03);
    public static readonly NativeType UInt8 = new NativeType(0x04);
    public static readonly NativeType Int16 = new NativeType(0x05);
    public static readonly NativeType UInt16 = new NativeType(0x06);
    public static readonly NativeType Int32 = new NativeType(0x07);
    public static readonly NativeType UInt32 = new NativeType(0x08);
    public static readonly NativeType Int64 = new NativeType(0x09);
    public static readonly NativeType UInt64 = new NativeType(0x0A);
    public static readonly NativeType Float32 = new NativeType(0x0B);
    public static readonly NativeType Float64 = new NativeType(0x0C);
    public static readonly NativeType Currency = new NativeType(0x0F);
    public static readonly NativeType BStr = new NativeType(0x13);
    public static readonly NativeType LPStr = new NativeType(0x14);
    public static readonly NativeType LPWStr = new NativeType(0x15);
    public static readonly NativeType LPTStr = new NativeType(0x16);
    public static readonly NativeType FixedSysString = new NativeType(0x17);
    public static readonly NativeType IUnknown = new NativeType(0x19);
    public static readonly NativeType IDispatch = new NativeType(0x1A);
    public static readonly NativeType Struct = new NativeType(0x1B);
    public static readonly NativeType Interface = new NativeType(0x1C);
    public static readonly NativeType Int = new NativeType(0x1F);
    public static readonly NativeType UInt = new NativeType(0x20);
    public static readonly NativeType ByValStr = new NativeType(0x22);
    public static readonly NativeType AnsiBStr = new NativeType(0x23);
    public static readonly NativeType TBstr = new NativeType(0x24);
    public static readonly NativeType VariantBool = new NativeType(0x25);
    public static readonly NativeType FuncPtr = new NativeType(0x26);
    public static readonly NativeType AsAny = new NativeType(0x28);

    protected byte typeIndex;

    internal NativeType(byte tyIx) { typeIndex = tyIx; }

    internal byte GetTypeIndex() { return typeIndex; }

    internal virtual byte[] ToBlob() {
      byte[] bytes = new byte[1];
      bytes[0] = GetTypeIndex();
      return bytes;
    }

   }

  public class NativeArray : NativeType 
  {
    NativeType elemType;
    uint len = 0, parNum = 0;

    /*
    public NativeArray(NativeType elemType) : base(0x2A) {
      this.elemType = elemType;
    }

    public NativeArray(NativeType elemType, int len) : base(0x2A) {
      this.elemType = elemType;
      this.len = len;
    }
*/
    public NativeArray(NativeType elemType, int numElem, int parNumForLen) : base(0x2A) {
      this.elemType = elemType;
      len = (uint)numElem;
      parNum = (uint)parNumForLen;
    }

    internal override byte[] ToBlob() {
      MemoryStream str = new MemoryStream();
      str.WriteByte(GetTypeIndex());
      if (elemType == null) str.WriteByte(0x50);  // no info (MAX)
      else str.WriteByte(elemType.GetTypeIndex());
      MetaData.CompressNum(parNum,str);
      str.WriteByte(1);
      MetaData.CompressNum(len,str);
      return str.ToArray();
    }

  }

  public class SafeArray : NativeType 
  {
    SafeArrayType elemType;

    public SafeArray(SafeArrayType elemType) : base(0x1D) {
      this.elemType = elemType;
    }

    internal override byte[] ToBlob() {
      byte[] bytes = new byte[2];
      bytes[0] = GetTypeIndex();
      bytes[1] = (byte)elemType;
      return bytes;
    }

  }

  public class FixedArray : NativeType 
  {
    NativeType elemType;
    uint numElem;

    public FixedArray(NativeType elemType, int numElems) : base(0x1E) {
      this.elemType = elemType;
      numElem = (uint)numElems;
    }

    internal override byte[] ToBlob() {
      MemoryStream str = new MemoryStream();
      str.WriteByte(GetTypeIndex());
      MetaData.CompressNum(numElem,str);
      if (elemType == null) str.WriteByte(0x50);  // no info (MAX)
      else str.WriteByte(elemType.GetTypeIndex());
      return str.ToArray();
    }

  }

  public class CustomMarshaller : NativeType 
  {
    string typeName;
    string marshallerName;
    string cookie;

    public CustomMarshaller(string typeNameOrGUID, string marshallerName, 
                string optCookie) : base(0x2C) {
      typeName = typeNameOrGUID;
      this.marshallerName = marshallerName;
      cookie = optCookie;
    }

    internal override byte[] ToBlob() {
      MemoryStream str = new MemoryStream();
      BinaryWriter bw = new BinaryWriter(str,new UTF8Encoding());
      bw.Write(GetTypeIndex());
      bw.Write(typeName.ToCharArray());
      bw.Write((byte)0);
      bw.Write(marshallerName.ToCharArray());
      bw.Write((byte)0);
      if (cookie != null) bw.Write(cookie.ToCharArray());
      bw.Write((byte)0);
      bw.Flush();
      return str.ToArray();
    }
  }

  /**************************************************************************/  
        /// <summary>
        /// Descriptor for a parameter of a method defined in this assembly/module
        /// </summary>
  public class Param : MetaDataElement
        {
    private static readonly ushort hasDefault = 0x1000;
    private static readonly ushort hasFieldMarshal = 0x2000;

    Type pType;
    string pName;
    internal ushort seqNo = 0;
    ushort parMode;
    ConstantElem defaultVal;
    uint nameIx = 0;
    FieldMarshal marshalInfo;

    /// <summary>
    /// Create a new parameter for a method
    /// </summary>
    /// <param name="mode">param mode (in, out, opt)</param>
    /// <param name="parName">parameter name</param>
    /// <param name="parType">parameter type</param>
    public Param(ParamAttr mode, string parName, Type parType) {
      pName = parName;
      pType = parType;
      parMode = (ushort)mode;
                        tabIx = MDTable.Param;
                }

    /// <summary>
    /// Add a default value to this parameter
    /// </summary>
    /// <param name="c">the default value for the parameter</param>
    public void AddDefaultValue(Constant cVal) {
      defaultVal = new ConstantElem(this,cVal);
      parMode |= hasDefault;
    }

    /// <summary>
    /// Add marshalling information about this parameter
    /// </summary>
    public void AddMarshallInfo(NativeType marshallType) {
      parMode |= hasFieldMarshal;
      marshalInfo = new FieldMarshal(this,marshallType);
    }

    internal Type GetParType() { return pType; }

    internal sealed override void BuildTables(MetaData md) {
      if (done) return;
      nameIx = md.AddToStringsHeap(pName);
      if (defaultVal != null) {
        md.AddToTable(MDTable.Constant,defaultVal);
        defaultVal.BuildTables(md);
      }
      if (marshalInfo != null) {
        md.AddToTable(MDTable.FieldMarshal,marshalInfo);
        marshalInfo.BuildTables(md);
      }
      done = true;
    }

    internal void TypeSig(MemoryStream str) {
      pType.TypeSig(str);
    }

    internal sealed override uint Size(MetaData md) {
      return 4 + md.StringsIndexSize();
    }

    internal sealed override void Write(FileImage output) {
      output.Write(parMode);
      output.Write(seqNo);
      output.StringsIndex(nameIx);
    }

    internal sealed override uint GetCodedIx(CIx code) {
      switch (code) {
        case (CIx.HasCustomAttr) : return 4; 
        case (CIx.HasConst) : return 1; 
        case (CIx.HasFieldMarshal) : return 1; 
      }
                        return 0;
    }

        }
  /**************************************************************************/  
        /// <summary>
        /// Base class for the PEFile (starting point)
        /// </summary>
  public class PEFile
        {
    private static readonly string mscorlibName = "mscorlib";
    private Module thisMod;
    private ClassDef moduleClass;
    private ArrayList classRefList = new ArrayList();
    private ArrayList classDefList = new ArrayList();
    private Assembly thisAssembly;
    private int corFlags = 1;
    FileImage fileImage;
                MetaData metaData;

    /// <summary>
    /// Create a new PEFile.  Each PEFile is a module.
    /// </summary>
    /// <param name="name">module name, also used for the file name</param>
    /// <param name="isDLL">create a .dll or .exe file</param>
    /// <param name="hasAssembly">this file is an assembly and 
    /// will contain the assembly manifest.  The assembly name is the 
    /// same as the module name</param>
    public PEFile(string name, bool isDLL, bool hasAssembly) {
      // Console.WriteLine(Hex.Byte(0x12));
      // Console.WriteLine(Hex.Short(0x1234));
      // Console.WriteLine(Hex.Int(0x12345678));
      string fName = MakeFileName(null,name,isDLL);
      fileImage = new FileImage(isDLL,fName);
      InitPEFile(name, fName, hasAssembly);
    }

    /// <summary>
    /// Create a new PEFile.  Each PEFile is a module.
    /// </summary>
    /// <param name="name">module name, also used for the file name</param>
    /// <param name="isDLL">create a .dll or .exe file</param>
    /// <param name="hasAssembly">this file is an assembly and 
    /// will contain the assembly manifest.  The assembly name is the 
    /// same as the module name</param>
    /// <param name="outputDir">write the PEFile to this directory.  If this
    /// string is null then the output will be to the current directory</param>
    public PEFile(string name, bool isDLL, bool hasAssembly, string outputDir) {
      // Console.WriteLine(Hex.Byte(0x12));
      // Console.WriteLine(Hex.Short(0x1234));
      // Console.WriteLine(Hex.Int(0x12345678));
      string fName = MakeFileName(outputDir,name,isDLL);
                        fileImage = new FileImage(isDLL,fName);
      InitPEFile(name, fName, hasAssembly);
    }

    /// <summary>
    /// Create a new PEFile
    /// </summary>
    /// <param name="name">module name</param>
    /// <param name="isDLL">create a .dll or .exe</param>
    /// <param name="hasAssembly">this PEfile is an assembly and
    /// will contain the assemly manifest.  The assembly name is the
    /// same as the module name</param>
    /// <param name="outStream">write the PEFile to this stream instead
    /// of to a new file</param>
    public PEFile(string name, bool isDLL, bool hasAssembly, Stream outStream) {
      fileImage = new FileImage(isDLL,outStream);
      InitPEFile(name, MakeFileName(null,name,isDLL), hasAssembly);
    }

    public PEFile(string name, string module_name, bool isDLL, bool hasAssembly, Stream outStream) {
      fileImage = new FileImage(isDLL,outStream);
      InitPEFile(name, (module_name == null ? MakeFileName(null,name,isDLL) : module_name), hasAssembly);
    }

    private void InitPEFile(string name, string fName, bool hasAssembly) {
      metaData = fileImage.GetMetaData();
      thisMod = new Module(fName,metaData);
      if (hasAssembly) {
        thisAssembly = new Assembly(name,metaData);
        metaData.AddToTable(MDTable.Assembly,thisAssembly);      
      }
      moduleClass = AddClass(TypeAttr.Private,"","<Module>");
      moduleClass.SpecialNoSuper();
      metaData.AddToTable(MDTable.Module,thisMod);
    }
 

    public ClassDef ModuleClass {
            get { return moduleClass; }
    }

    /// <summary>
    /// Set the subsystem (.subsystem) (Default is Windows Console mode)
    /// </summary>
    /// <param name="subS">subsystem value</param>
    public void SetSubSystem(SubSystem subS) {
      fileImage.subSys = subS;
    }

    /// <summary>
    /// Set the flags (.corflags)
    /// </summary>
    /// <param name="flags">the flags value</param>
    public void SetCorFlags(int flags) {
      corFlags = flags;
    }

    private string MakeFileName(string dirName, string name, bool isDLL) {
      string result = "";
      if ((dirName != null) && (dirName.CompareTo("") != 0)) {
        result = dirName;
        if (!dirName.EndsWith("\\")) result += "\\";
      }
      result += name;
       
      // if (isDLL) result += ".dll";  else result += ".exe";
      
      return result;
    }

    /// <summary>
    /// Add an external assembly to this PEFile (.assembly extern)
    /// </summary>
    /// <param name="assemName">the external assembly name</param>
    /// <returns>a descriptor for this external assembly</returns>
    public AssemblyRef AddExternAssembly(string assemName) {
      if (assemName.CompareTo(mscorlibName) == 0) return metaData.mscorlib;
      AssemblyRef anAssem = new AssemblyRef(metaData,assemName);
      metaData.AddToTable(MDTable.AssemblyRef,anAssem);
      // Console.WriteLine("Adding assembly " + assemName);
      return anAssem;
    }

    /// <summary>
    /// Add an external module to this PEFile (.module extern)
    /// </summary>
    /// <param name="name">the external module name</param>
    /// <returns>a descriptor for this external module</returns>
    public ModuleRef AddExternModule(string name) {
      ModuleRef modRef = new ModuleRef(metaData,name);
      metaData.AddToTable(MDTable.ModuleRef,modRef);
      return modRef;
    }

    /// <summary>
    /// Add a "global" method to this module
    /// </summary>
    /// <param name="name">method name</param>
    /// <param name="retType">return type</param>
    /// <param name="pars">method parameters</param>
    /// <returns>a descriptor for this new "global" method</returns>
    public MethodDef AddMethod(string name, Type retType, Param[] pars) {
      return moduleClass.AddMethod(name,retType,pars);
    }

    /// <summary>
    /// Add a "global" method to this module
    /// </summary>
    /// <param name="mAtts">method attributes</param>
    /// <param name="iAtts">method implementation attributes</param>
    /// <param name="name">method name</param>
    /// <param name="retType">return type</param>
    /// <param name="pars">method parameters</param>
    /// <returns>a descriptor for this new "global" method</returns>
    public MethodDef AddMethod(MethAttr mAtts, ImplAttr iAtts, string name, Type retType, Param[] pars) {
      return moduleClass.AddMethod(mAtts,iAtts,name,retType,pars);
    }

    public MethodRef AddMethodToTypeSpec (Type item, string name, Type retType, Type[] pars) {
            MethodRef meth = new MethodRef (item.GetTypeSpec (metaData), name, retType, pars, false, null);
            metaData.AddToTable (MDTable.MemberRef,meth);
            return meth;
    }

    public MethodRef AddVarArgMethodToTypeSpec (Type item, string name, Type retType,
                    Type[] pars, Type[] optPars) {
            MethodRef meth = new MethodRef(item.GetTypeSpec (metaData), name,retType,pars,true,optPars);
            metaData.AddToTable(MDTable.MemberRef,meth);
            return meth;
    }

    public FieldRef AddFieldToTypeSpec (Type item, string name, Type fType) {
            FieldRef field = new FieldRef (item.GetTypeSpec (metaData), name,fType);
            metaData.AddToTable (MDTable.MemberRef,field);
            return field;
    }

    public void AddMethodSpec (Method m, GenericMethodSig g_sig)
    {
            MethodSpec ms = new MethodSpec (m, g_sig);
            metaData.AddToTable (MDTable.MethodSpec, ms);
    }

    /// <summary>
    /// Add a "global" field to this module
    /// </summary>
    /// <param name="name">field name</param>
    /// <param name="fType">field type</param>
    /// <returns>a descriptor for this new "global" field</returns>
    public FieldDef AddField(string name, Type fType) {
      return moduleClass.AddField(name,fType);
    }

    /// <summary>
    /// Add a "global" field to this module
    /// </summary>
    /// <param name="attrSet">attributes of this field</param>
    /// <param name="name">field name</param>
    /// <param name="fType">field type</param>
    /// <returns>a descriptor for this new "global" field</returns>
    public FieldDef AddField(FieldAttr attrSet, string name, Type fType) {
      return moduleClass.AddField(attrSet,name,fType);
    }

    /// <summary>
    /// Add a class to this module
    /// </summary>
    /// <param name="attrSet">attributes of this class</param>
    /// <param name="nsName">name space name</param>
    /// <param name="name">class name</param>
    /// <returns>a descriptor for this new class</returns>
    public ClassDef AddClass(TypeAttr attrSet, string nsName, string name) {
      ClassDef aClass = new ClassDef(attrSet,nsName,name,metaData);
      metaData.AddToTable(MDTable.TypeDef,aClass);
      return aClass;
    }

    /// <summary>
    /// Add a class which extends System.ValueType to this module
    /// </summary>
    /// <param name="attrSet">attributes of this class</param>
    /// <param name="nsName">name space name</param>
    /// <param name="name">class name</param>
    /// <returns>a descriptor for this new class</returns>
    public ClassDef AddValueClass(TypeAttr attrSet, string nsName, string name) {
      ClassDef aClass = new ClassDef(attrSet,nsName,name,metaData);
      aClass.MakeValueClass();
      metaData.AddToTable(MDTable.TypeDef,aClass);
      return aClass;
    }

    /// <summary>
    /// Add a class to this module
    /// </summary>
    /// <param name="attrSet">attributes of this class</param>
    /// <param name="nsName">name space name</param>
    /// <param name="name">class name</param>
    /// <param name="superType">super type of this class (extends)</param>
    /// <returns>a descriptor for this new class</returns>
    public ClassDef AddClass(TypeAttr attrSet, string nsName, string name, Class superType) {
      ClassDef aClass = new ClassDef(attrSet,nsName,name,metaData);
      aClass.SetSuper(superType);
      metaData.AddToTable(MDTable.TypeDef,aClass);
      return aClass;
    }

    public FileRef AddFile(string fName, byte[] hashBytes, bool hasMetaData, bool entryPoint) {
      FileRef file = new FileRef(fName,hashBytes,hasMetaData,entryPoint,metaData);
      metaData.AddToTable(MDTable.File,file);
      return file;
    }

    /// <summary>
    /// Add a manifest resource to this PEFile NOT YET IMPLEMENTED
    /// </summary>
    /// <param name="mr"></param>
    public void AddManifestResource(ManifestResource mr) {
      metaData.AddToTable(MDTable.ManifestResource,mr);
      //mr.FixName(metaData);
    }

    public void AddCustomAttribute (Method meth, byte [] data, MetaDataElement element)
    {
            metaData.AddCustomAttribute (new CustomAttribute (element, meth, data));
    }

    /// <summary>
    /// Write out the PEFile (the "bake" function)
    /// </summary>
    public void WritePEFile() { /* the "bake" function */
      fileImage.MakeFile();
    }

    /// <summary>
    /// Get the descriptor of this module
    /// </summary>
    /// <returns>the descriptor for this module</returns>
    public Module GetThisModule() {
      return thisMod;
    }

    /// <summary>
    /// Get the descriptor for this assembly.  The PEFile must have been
    /// created with hasAssembly = true
    /// </summary>
    /// <returns>the descriptor for this assembly</returns>
    public Assembly GetThisAssembly() {
      return thisAssembly;
    }

        }

  /**************************************************************************/  
        /// <summary>
        /// Descriptor for the Primitive types defined in IL
        /// </summary>
  public class PrimitiveType : Type
        {
    private string name;
    private int systemTypeIndex;
    public static int NumSystemTypes = 18;

    public static readonly PrimitiveType Void = new PrimitiveType(0x01,"Void",0);
    public static readonly PrimitiveType Boolean = new PrimitiveType(0x02,"Boolean",1);
    public static readonly PrimitiveType Char = new PrimitiveType(0x03,"Char",2);
    public static readonly PrimitiveType Int8 = new PrimitiveType(0x04,"SByte",3);
    public static readonly PrimitiveType UInt8 = new PrimitiveType(0x05,"Byte",4);
    public static readonly PrimitiveType Int16 = new PrimitiveType(0x06,"Int16",5);
    public static readonly PrimitiveType UInt16 = new PrimitiveType(0x07,"UInt16",6);
    public static readonly PrimitiveType Int32 = new PrimitiveType(0x08,"Int32",7);
    public static readonly PrimitiveType UInt32 = new PrimitiveType(0x09,"UInt32",8);
    public static readonly PrimitiveType Int64 = new PrimitiveType(0x0A,"Int64",9);
    public static readonly PrimitiveType UInt64 = new PrimitiveType(0x0B,"UInt64",10);
    public static readonly PrimitiveType Float32 = new PrimitiveType(0x0C,"Single",11);
    public static readonly PrimitiveType Float64 = new PrimitiveType(0x0D,"Double",12);
    public static readonly PrimitiveType String = new PrimitiveType(0x0E,"String",13);
                internal static readonly PrimitiveType Class = new PrimitiveType(0x12);
    public static readonly PrimitiveType TypedRef = new PrimitiveType(0x16,"TypedReference",14);
    public static readonly PrimitiveType IntPtr = new PrimitiveType(0x18,"IntPtr",15);
    public static readonly PrimitiveType UIntPtr = new PrimitiveType(0x19,"UIntPtr",16);
    public static readonly PrimitiveType Object = new PrimitiveType(0x1C,"Object",17);
    internal static readonly PrimitiveType ClassType = new PrimitiveType(0x50);
    public static readonly PrimitiveType NativeInt = IntPtr;
    public static readonly PrimitiveType NativeUInt = UIntPtr;

    internal PrimitiveType(byte typeIx) : base(typeIx) { }

                internal PrimitiveType(byte typeIx, string name, int STIx) : base(typeIx) {
      this.name = name;
      this.systemTypeIndex = STIx;
    }

    internal string GetName() { return name; }

    internal int GetSystemTypeIx() { return systemTypeIndex; }

    internal sealed override void TypeSig(MemoryStream str) {
      str.WriteByte(typeIndex);
    }

    internal override MetaDataElement GetTypeSpec(MetaData md) {
      TypeSpec tS = md.GetPrimitiveTypeSpec(systemTypeIndex);
      if (tS == null) {
        tS = new TypeSpec(this,md);
        md.SetPrimitiveTypeSpec(systemTypeIndex,tS);
        md.AddToTable(MDTable.TypeSpec,tS);
      }
      return tS;
    }

        }

  /**************************************************************************/  
        /// <summary>
        /// Descriptor for the Property of a class
        /// </summary>
  public class Property : Feature
        {
    private static readonly byte PropertyTag = 0x8;
    MethodDef getterMeth;
    ConstantElem constVal;
    uint typeBlobIx = 0;
    Type[] parList;
    Type returnType;
    uint numPars = 0;

    internal Property(string name, Type retType, Type[] pars, ClassDef parent) : base(name, parent) {
      returnType = retType;
      parList = pars;
      if (pars != null) numPars = (uint)pars.Length;
                        tabIx = MDTable.Property;
                }

    /// <summary>
    /// Add a set method to this property
    /// </summary>
    /// <param name="setter">the set method</param>
    public void AddSetter(MethodDef setter) {
      AddMethod(setter,MethodType.Setter);
    }

    /// <summary>
    /// Add a get method to this property
    /// </summary>
    /// <param name="getter">the get method</param>
    public void AddGetter(MethodDef getter) {
      AddMethod(getter,MethodType.Getter);
      getterMeth = getter;
    }

    /// <summary>
    /// Add another method to this property
    /// </summary>
    /// <param name="other">the method</param>
    public void AddOther(MethodDef other) {
      AddMethod(other,MethodType.Other);
    }

    /// <summary>
    /// Add an initial value for this property
    /// </summary>
    /// <param name="constVal">the initial value for this property</param>
    public void AddInitValue(Constant constVal) {
      this.constVal = new ConstantElem(this,constVal);
    }

    internal sealed override void BuildTables(MetaData md) {
      if (done) return;
      nameIx = md.AddToStringsHeap(name);
      MemoryStream sig = new MemoryStream();
      sig.WriteByte(PropertyTag);
      MetaData.CompressNum(numPars,sig);
      returnType.TypeSig(sig);
      for (int i=0; i < numPars; i++) {
        parList[i].TypeSig(sig);
      }
      typeBlobIx = md.AddToBlobHeap(sig.ToArray());
      for (int i=0; i < tide; i++) {
        md.AddToTable(MDTable.MethodSemantics,methods[i]);
      }
      if (constVal != null) {
        md.AddToTable(MDTable.Constant,constVal);
        constVal.BuildTables(md);
      }
      done = true;
    }

    internal sealed override uint Size(MetaData md) {
      return 2 + md.StringsIndexSize() + md.BlobIndexSize();
    }

    internal sealed override void Write(FileImage output) {
      output.Write(flags);
      output.StringsIndex(nameIx);
      output.BlobIndex(typeBlobIx);
    }

    internal sealed override uint GetCodedIx(CIx code) {
                        switch (code) {
                                case (CIx.HasCustomAttr) : return 9; 
                                case (CIx.HasConst) : return 2; 
                                case (CIx.HasSemantics) : return 1; 
                        }
                        return 0;
    }

        }
  /**************************************************************************/  
        /// <summary>
        /// Descriptor for an pointer (type * or type &)
        /// </summary>
  public abstract class PtrType : Type
        {
    Type baseType;

                internal PtrType(Type bType, byte typeIx) : base(typeIx)
                {
      baseType = bType;
      tabIx = MDTable.TypeSpec;
                }

    internal sealed override void TypeSig(MemoryStream str) {
      str.WriteByte(typeIndex);
      baseType.TypeSig(str);
    }

        }
  /**************************************************************************/  
  /// <summary>
  /// Descriptor for a managed pointer (type &  or byref)
  /// </summary>

  public class ManagedPointer : PtrType  // <type> & (BYREF)  
  {

    /// <summary>
    /// Create new managed pointer to baseType
    /// </summary>
    /// <param name="bType">the base type of the pointer</param>
    public ManagedPointer(Type baseType) : base(baseType,0x10) { }
 
  }
  /**************************************************************************/  
  /// <summary>
  /// Descriptor for an unmanaged pointer (type *)
  /// </summary>
  public class UnmanagedPointer : PtrType // PTR
  {
    /// <summary>
    /// Create a new unmanaged pointer to baseType
    /// </summary>
    /// <param name="baseType">the base type of the pointer</param>
    public UnmanagedPointer(Type baseType) : base(baseType, 0x0F) { }

  }
  /**************************************************************************/  
        /// <summary>
        /// Base class for scopes (extended by Module, ModuleRef, Assembly, AssemblyRef)
        /// </summary>
  public abstract class ResolutionScope : MetaDataElement
        {
    protected uint nameIx = 0;
    protected MetaData metaData;
    protected string name;

                internal ResolutionScope(string name, MetaData md)
                {
      metaData = md;
      this.name = name;
      nameIx = md.AddToStringsHeap(name);
                }

    internal string GetName() { return name; }

        }
  /**************************************************************************/  
        /// <summary>
        /// Descriptor for a Section in a PEFile  eg .text, .sdata
        /// </summary>
  internal class Section {
                private static readonly uint relocPageSize = 4096;  // 4K pages for fixups

                char[] name; 
                uint offset = 0, tide = 0, size = 0, rva = 0, relocTide = 0;
                //uint relocOff = 0;
    uint flags = 0, padding = 0;
                uint[] relocs; 

                internal Section(string sName, uint sFlags) {
                  name = sName.ToCharArray();
                  flags = sFlags;
                }

                internal uint Tide() { return tide; }

                internal void IncTide(uint incVal) { tide += incVal; }

                internal uint Padding() { return padding; }

                internal uint Size() { return size; }

                internal void SetSize(uint pad) {
                        padding = pad;
                        size = tide + padding;
                }

                internal uint RVA() { return rva; }

                internal void SetRVA(uint rva) { this.rva = rva; }

                internal uint Offset() { return offset; }

                internal void SetOffset(uint offs) { offset = offs; }

    internal void DoBlock(BinaryWriter reloc, uint page, int start, int end) {
      //Console.WriteLine("rva = " + rva + "  page = " + page);
      reloc.Write(rva + page);
      reloc.Write((uint)(((end-start+1)*2) + 8));
      for (int j=start; j < end; j++) {
        //Console.WriteLine("reloc offset = " + relocs[j]);
        reloc.Write((ushort)((0x3 << 12) | (relocs[j] - page)));
      }
      reloc.Write((ushort)0);
    }

                internal void DoRelocs(BinaryWriter reloc) {
      if (relocTide > 0) {
        //relocOff = (uint)reloc.Seek(0,SeekOrigin.Current);
        uint block = (relocs[0]/relocPageSize + 1) * relocPageSize;
        int start = 0;
        for (int i=1; i < relocTide; i++) {
          if (relocs[i] >= block) {
            DoBlock(reloc,block-relocPageSize,start,i);
            start = i;
            block = (relocs[i]/relocPageSize + 1) * relocPageSize;
          }
        }
        DoBlock(reloc,block-relocPageSize,start,(int)relocTide);
      }
                }

                internal void AddReloc(uint offs) {
                        int pos = 0;
                        if (relocs == null) {
                                relocs = new uint[5];
                        } else {
                                if (relocTide >= relocs.Length) {
                                        uint[] tmp = relocs;
                                        relocs = new uint[tmp.Length + 5];
                                        for (int i=0; i < relocTide; i++) {
                                                relocs[i] = tmp[i];
                                        }
                                }
                                while ((pos < relocTide) && (relocs[pos] < offs)) pos++;
                                for (int i=pos; i < relocTide; i++) {
                                        relocs[i+1] = relocs[i];
                                }
                        }
                        relocs[pos] = offs;
                        relocTide++;    
                }
      
                internal void WriteHeader(BinaryWriter output, uint relocRVA) {
                        output.Write(name);
                        output.Write(tide);
                        output.Write(rva);
                        output.Write(size);
                        output.Write(offset);
                        output.Write(0);
                        //output.Write(relocRVA + relocOff);
                        output.Write(0);
      output.Write(0);
                        //output.Write((ushort)relocTide);
                        //output.Write((ushort)0);
                        output.Write(flags);
                }

        }
  /**************************************************************************/  
  public abstract class Signature : MetaDataElement 
  {
    protected uint sigIx;

    internal Signature() {
      tabIx = MDTable.StandAloneSig;
    }

    internal sealed override uint Size(MetaData md) {
      return md.BlobIndexSize();
    }

    internal sealed override void Write(FileImage output) {
      output.BlobIndex(sigIx);
    }

    internal sealed override uint GetCodedIx(CIx code) { return (uint)tabIx; }

  }
  /**************************************************************************/  
        /// <summary>
        /// Descriptor for a class defined in System (mscorlib)
        /// </summary>
  internal class SystemClass : ClassRef
        {
    PrimitiveType elemType; 

                internal SystemClass(PrimitiveType eType, AssemblyRef paren, MetaData md)
                                    : base("System",eType.GetName(),md) {
      elemType = eType;
      parent = paren;
                }

    internal override sealed MetaDataElement GetTypeSpec(MetaData md) {
      if (typeSpec == null) typeSpec = (TypeSpec)elemType.GetTypeSpec(md);
      return typeSpec;
    }


    internal sealed override void TypeSig(MemoryStream str) {
        str.WriteByte(elemType.GetTypeIndex());
    }

        }
  /**************************************************************************/  
        /// <summary>
        /// Base class for all IL types
        /// </summary>
  public abstract class Type : MetaDataElement {
    protected byte typeIndex;
    protected TypeSpec typeSpec;

    internal Type(byte tyIx) { typeIndex = tyIx; }

    internal byte GetTypeIndex() { return typeIndex; }
    internal void SetTypeIndex (byte b) { typeIndex = b; }
          
    internal virtual MetaDataElement GetTypeSpec(MetaData md) {
      if (typeSpec == null) {
        typeSpec = new TypeSpec(this,md);
        md.AddToTable(MDTable.TypeSpec,typeSpec);
      }
      return typeSpec;
    }
 
    internal virtual void TypeSig(MemoryStream str) {
      throw(new TypeSignatureException(this.GetType().AssemblyQualifiedName +
        " doesn't have a type signature!!"));   
    }

  }

  /**************************************************************************/  

  public class TypeSpec : MetaDataElement {
    uint sigIx = 0;

    internal TypeSpec(Type aType, MetaData md) {
      MemoryStream sig = new MemoryStream();
      aType.TypeSig(sig);
      sigIx = md.AddToBlobHeap(sig.ToArray());
      tabIx = MDTable.TypeSpec;
    }

    internal sealed override uint GetCodedIx(CIx code) {
      switch (code) {
        case (CIx.TypeDefOrRef) : return 2; 
        case (CIx.HasCustomAttr) : return 13; 
        case (CIx.MemberRefParent) : return 4; 
      }
      return 0;
    }

    internal override uint Size(MetaData md) { 
      return md.BlobIndexSize();
    }

    internal sealed override void Write(FileImage output) {
      //Console.WriteLine("Writing the blob index for a TypeSpec");
      output.BlobIndex(sigIx);
    }

  }

          class ByteArrayComparer : IComparer {

                public int Compare (object x, object y)
                {
                        byte [] a = (byte []) x;
                        byte [] b = (byte []) y;
                        int len = a.Length;

                        if (b.Length != len)
                                return 1;

                        for (int i = 0; i < len; ++i)
                                if (a [i] != b [i])
                                        return 1;
                        return 0;
                }
        }

        class ByteArrayHashCodeProvider : IHashCodeProvider {

		public int GetHashCode (Object key)
		{
			byte [] arr = (byte []) key;
                        int len = arr.Length;
			int h = 0;

			for (int i = 0; i < len; ++i)
				h = (h << 5) - h + arr [i];

			return h;
		}

	}
}


