// Simplified version of ../../../external/corefx/src/System.Runtime.CompilerServices.Unsafe/src/System.Runtime.CompilerServices.Unsafe.il

.class public abstract auto ansi sealed beforefieldinit System.Runtime.CompilerServices.Unsafe
{
  .method public hidebysig static !!T Read<T>(void* source) cil managed aggressiveinlining
  {

        .maxstack 1
        ldarg.0
        ldobj !!T
        ret
  }

  .method public hidebysig static !!T ReadUnaligned<T>(void* source) cil managed aggressiveinlining
  {

        .maxstack 1
        ldarg.0
        unaligned. 0x1
        ldobj !!T
        ret
  }
  
  .method public hidebysig static !!T ReadUnaligned<T>(uint8& source) cil managed aggressiveinlining
  {

        .maxstack 1
        ldarg.0
        unaligned. 0x1
        ldobj !!T
        ret
  }
  
  .method public hidebysig static void Write<T>(void* destination,
                                                 !!T 'value') cil managed aggressiveinlining
  {

        .maxstack 2
        ldarg.0
        ldarg.1
        stobj !!T
        ret
  }

  .method public hidebysig static void WriteUnaligned<T>(void* destination,
                                                 !!T 'value') cil managed aggressiveinlining
  {

        .maxstack 2
        ldarg.0
        ldarg.1
        unaligned. 0x01
        stobj !!T
        ret
  }

  .method public hidebysig static void WriteUnaligned<T>(uint8& destination,
                                                 !!T 'value') cil managed aggressiveinlining
  {

        .maxstack 2
        ldarg.0
        ldarg.1
        unaligned. 0x01
        stobj !!T
        ret
  }

  .method public hidebysig static void Copy<T>(void* destination,
                                                !!T& source) cil managed aggressiveinlining
  {

        .maxstack 2
        ldarg.0
        ldarg.1
        ldobj !!T
        stobj !!T
        ret
  }

  .method public hidebysig static void Copy<T>(!!T& destination,
                                                void* source) cil managed aggressiveinlining
  {

        .maxstack 2
        ldarg.0
        ldarg.1
        ldobj !!T
        stobj !!T
        ret
  }

  .method public hidebysig static void* AsPointer<T>(!!T& 'value') cil managed aggressiveinlining
  {

        .maxstack 1
        ldarg.0
        conv.u
        ret
  }

  .method public hidebysig static int32 SizeOf<T>() cil managed aggressiveinlining
  {

        .maxstack 1
        sizeof !!T
        ret
  }

  .method public hidebysig static void CopyBlock(void* destination, void* source, uint32 byteCount) cil managed aggressiveinlining
  {

        .maxstack 3
        ldarg.0
        ldarg.1
        ldarg.2
        cpblk
        ret
  }

  .method public hidebysig static void CopyBlock(uint8& destination, uint8& source, uint32 byteCount) cil managed aggressiveinlining
  {

        .maxstack 3
        ldarg.0
        ldarg.1
        ldarg.2
        cpblk
        ret
  }

  .method public hidebysig static void CopyBlockUnaligned(void* destination, void* source, uint32 byteCount) cil managed aggressiveinlining
  {

        .maxstack 3
        ldarg.0
        ldarg.1
        ldarg.2
        unaligned. 0x1
        cpblk
        ret
  }

  .method public hidebysig static void CopyBlockUnaligned(uint8& destination, uint8& source, uint32 byteCount) cil managed aggressiveinlining
  {

        .maxstack 3
        ldarg.0
        ldarg.1
        ldarg.2
        unaligned. 0x1
        cpblk
        ret
  }

  .method public hidebysig static void InitBlock(void* startAddress, uint8 'value', uint32 byteCount) cil managed aggressiveinlining
  {

        .maxstack 3
        ldarg.0
        ldarg.1
        ldarg.2
        initblk
        ret
  }

  .method public hidebysig static void InitBlock(uint8& startAddress, uint8 'value', uint32 byteCount) cil managed aggressiveinlining
  {

        .maxstack 3
        ldarg.0
        ldarg.1
        ldarg.2
        initblk
        ret
  }

  .method public hidebysig static void InitBlockUnaligned(void* startAddress, uint8 'value', uint32 byteCount) cil managed aggressiveinlining
  {

        .maxstack 3
        ldarg.0
        ldarg.1
        ldarg.2
        unaligned. 0x1
        initblk
        ret
  }

  .method public hidebysig static void InitBlockUnaligned(uint8& startAddress, uint8 'value', uint32 byteCount) cil managed aggressiveinlining
  {

        .maxstack 3
        ldarg.0
        ldarg.1
        ldarg.2
        unaligned. 0x1
        initblk
        ret
  }

  .method public hidebysig static !!T As<class T>(object o) cil managed aggressiveinlining
  {

        .maxstack 1
        ldarg.0
        ret
  }

  .method public hidebysig static !!T& AsRef<T>(void* source) cil managed aggressiveinlining
  {

        .maxstack 1
        ldarg.0
        ret
  }

  .method public hidebysig static !!T& AsRef<T>(!!T& source) cil managed aggressiveinlining
  {
        .param [1]
        .custom instance void [mscorlib]System.Runtime.CompilerServices.IsReadOnlyAttribute::.ctor() = ( 01 00 00 00 )
        .maxstack 1
        ldarg.0
        ret
  }

  .method public hidebysig static !!TTo& As<TFrom, TTo>(!!TFrom& source) cil managed aggressiveinlining
  {

        .maxstack 1
        ldarg.0
        ret
  }

  .method public hidebysig static !!T& Add<T>(!!T& source, int32 elementOffset) cil managed aggressiveinlining
  {

        .maxstack 3
        ldarg.0
        ldarg.1
        sizeof !!T
        conv.i
        mul
        add
        ret
  }

  .method public hidebysig static void* Add<T>(void* source, int32 elementOffset) cil managed aggressiveinlining
  {

        .maxstack 3
        ldarg.0
        ldarg.1
        sizeof !!T
        conv.i
        mul
        add
        ret
  }

  .method public hidebysig static !!T& Add<T>(!!T& source, native int elementOffset) cil managed aggressiveinlining
  {

        .maxstack 3
        ldarg.0
        ldarg.1
        sizeof !!T
        mul
        add
        ret
  }

  .method public hidebysig static !!T& AddByteOffset<T>(!!T& source, native int byteOffset) cil managed aggressiveinlining
  {

        .maxstack 2
        ldarg.0
        ldarg.1
        add
        ret
  }

  .method public hidebysig static !!T& Subtract<T>(!!T& source, int32 elementOffset) cil managed aggressiveinlining
  {

        .maxstack 3
        ldarg.0
        ldarg.1
        sizeof !!T
        conv.i
        mul
        sub
        ret
  }

  .method public hidebysig static void* Subtract<T>(void* source, int32 elementOffset) cil managed aggressiveinlining
  {

        .maxstack 3
        ldarg.0
        ldarg.1
        sizeof !!T
        conv.i
        mul
        sub
        ret
  }

  .method public hidebysig static !!T& Subtract<T>(!!T& source, native int elementOffset) cil managed aggressiveinlining
  {

        .maxstack 3
        ldarg.0
        ldarg.1
        sizeof !!T
        mul
        sub
        ret
  }

  .method public hidebysig static !!T& SubtractByteOffset<T>(!!T& source, native int byteOffset) cil managed aggressiveinlining
  {

        .maxstack 2
        ldarg.0
        ldarg.1
        sub
        ret
  }

  .method public hidebysig static native int ByteOffset<T>(!!T& origin, !!T& target) cil managed aggressiveinlining
  {

        .maxstack 2
        ldarg.1
        ldarg.0
        sub
        ret
  }

  .method public hidebysig static bool AreSame<T>(!!T& left, !!T& right) cil managed aggressiveinlining
  {

        .maxstack 2
        ldarg.0
        ldarg.1
        ceq
        ret
  }

  .method public hidebysig static bool IsAddressGreaterThan<T>(!!T& left, !!T& right) cil managed aggressiveinlining
  {
        .maxstack 2
        ldarg.0
        ldarg.1
        cgt.un
        ret
  }

  .method public hidebysig static bool IsAddressLessThan<T>(!!T& left, !!T& right) cil managed aggressiveinlining
  {
        .maxstack 2
        ldarg.0
        ldarg.1
        clt.un
        ret
  }

  .method public hidebysig static !!T& Unbox<valuetype .ctor ([mscorlib]System.ValueType) T> (object 'box') cil managed aggressiveinlining
  {
        .maxstack 1
        ldarg.0
        unbox !!T
        ret
  }
}