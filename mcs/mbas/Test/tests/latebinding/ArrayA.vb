Imports System

Module VariableC
	Dim a() As Integer = {1, 2, 3, 4, 5}
	Dim b(3) As Integer
	Dim e as Integer() = {1, 2, 3}
	Dim g as Integer(,) = { {1,1}, {2,2}, {3,3} }
	
	Sub Main()
		Dim obj As Object = a
		
		If obj(2) <> 3 Then
			Throw New Exception("#A1, value mismatch")
		End If
		
		obj = b
		obj(0) = 2
		obj(1) = 5
		obj(2) = 10
		If obj(1) <> 5 Then
			Throw New Exception("#A2, value mismatch")
		End If
		
		obj = e
		If obj(1) <> 2 Then
			Throw New Exception("#A3, value mismatch")
		End If
		
		obj = g
		If obj(2,1) <> 3 Then
			Throw New Exception("#A4, value mismatch")
		End If
	End Sub
End Module
