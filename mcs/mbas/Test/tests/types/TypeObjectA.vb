'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System

	Public Class ValueReference
		Public AgeClass As Integer
	End Class

	Structure SomeStruct
		Public AgeStruct As Integer
	End Structure

Module Test
	Sub Main()
		Dim objVal1 As Object = New SomeStruct()
		objval1.AgeStruct = 50
		Dim objval2 As Object = objval1
		objval2.AgeStruct = 100
		if (objval1.AgeStruct <> 50 or objval2.AgeStruct <> 100) then
			Throw New Exception ("objval1.AgeStruct should be 50, but got " & objval1.AgeStruct & " and objval2.AgeStruct should be , but got " & objval2.AgeStruct)
		End if
		
		Dim Objref1 As Object = New ValueReference()
		objref1.AgeClass = 50
		Dim objref2 As Object = objref1
		objref2.AgeClass = 100
		if (objref2.AgeClass <>objref2.AgeClass or objref2.AgeClass <> 100) then
			Throw New Exception ("objref1.AgeClass should be 100, but got " & objref1.AgeClass & "and objref2.AgeClass should be , but got " & objref2.AgeClass)
		End if
	End Sub
End Module
