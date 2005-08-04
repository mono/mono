Option Explicit
Option Strict Off
Option Compare Text

Imports System

Module RedimPreserve

    Sub DoTest ()
		Dim A(10) As Integer
		Dim B(10) As Integer
		Dim I As Integer
	
		Console.WriteLine("Array Size = 10 (Dim)")
	
		For I = 0 To 10
			A(I) = 10 - I 
			B(I) = I*3 
		Next I

		PrintArrays(A, B)
	
		Console.WriteLine("Array Size = 15 (ReDim Preserve)")
		
		ReDim Preserve A(15), B(15)

		PrintArrays(A, B)
		
		
		Console.WriteLine("Array Size = 5 (ReDim Preserve)")
	
		ReDim Preserve A(5), B(5)		

		PrintArrays(A, B)

		Console.WriteLine("Array Size = 3 (ReDim)")
	
		ReDim A(3), B(3)		

		PrintArrays(A, B)
    End Sub

	Sub PrintArrays(ArrayA() as Integer, ArrayB() as Integer)
		Dim I As Integer
		For I = 0 To ArrayA.Length - 1
			Console.WriteLine("{0} : {1} {2}", I, ArrayA(I), ArrayB(I) )
		Next I
	End Sub

End Module
