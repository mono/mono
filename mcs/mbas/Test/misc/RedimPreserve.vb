Option Explicit
Option Strict Off
Option Compare Text

Imports System

Module RedimPreserve

    Sub DoTest ()
		Dim IntArray(10) As Integer
		Dim I As Integer
	
		Console.WriteLine("Array Size = 10 (Dim)")
	
		For I = 0 To 10
			IntArray(I) = 10 - I   
			Console.Write(I)
			Console.Write(" : ")
			Console.WriteLine(IntArray(I))
		Next I
	
		Console.WriteLine("Array Size = 15 (ReDim Preserve)")
		
		ReDim Preserve IntArray(15)
		
		For I = 0 To 15
			Console.Write(I)
			Console.Write(" : ")
			Console.WriteLine(IntArray(I))
		Next I
		
		Console.WriteLine("Array Size = 5 (ReDim Preserve")
	
		ReDim Preserve IntArray(5)
		
		For I = 0 To 5
			Console.Write(I)
			Console.Write(" : ")
			Console.WriteLine(IntArray(I))
		Next I
    End Sub

End Module
