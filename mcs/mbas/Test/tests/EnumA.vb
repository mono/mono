Imports System

Module M
	Enum E
		A
		B
		C
		D
		E = 10
		F
		G = 5
		H
		I = D
		J = -10
		K 
	End Enum


	Public Enum E1 As Long
		A = 10
		B = 20
	End Enum

   Sub Main()
	dim i as integer
	i = E.A
	If i <> 0 Then
		Throw new Exception("#A1, unexpected result")
	End If

	i = E.B
	If i <> 1 Then
		Throw new Exception("#A2, unexpected result")
	End If

	i = E.C
	If i <> 2 Then
		Throw new Exception("#A3, unexpected result")
	End If
	i = E.D
	If i <> 3 Then
		Throw new Exception("#A4, unexpected result")
	End If
	i = E.E
	If i <> 10 Then
		Throw new Exception("#A5, unexpected result")
	End If
	i = E.F
	If i <> 11 Then
		Throw new Exception("#A6, unexpected result")
	End If
	i = E.G
	If i <> 5 Then
		Throw new Exception("#A7, unexpected result")
	End If
	i = E.H
	If i <> 6 Then
		Throw new Exception("#A8, unexpected result")
	End If
	i = E.I
	If i <> 3 Then
		Throw new Exception("#A9, unexpected result")
	End If
	i = E.J
	If i <> -10 Then
		Throw new Exception("#A10, unexpected result")
	End If
	i = E.K
	If i <> -9 Then
		Throw new Exception("#A11, unexpected result")
	End If

'        Console.WriteLine(E.A)
'        Console.WriteLine(E.B)
'        Console.WriteLine(E.C)
'        Console.WriteLine(E.D)
'        Console.WriteLine(E.E)
'        Console.WriteLine(E.F)
'        Console.WriteLine(E.G)
'        Console.WriteLine(E.H)
'        Console.WriteLine(E.I)
'        Console.WriteLine(E.J)
'        Console.WriteLine(E.K)
    End Sub

	

End Module
