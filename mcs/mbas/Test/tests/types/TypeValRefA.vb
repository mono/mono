'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module TypeValRef

	Public Class ValueReference
		Public Age As Short
	End Class

	Structure MyStruct
		Public Age As Short
	End Structure

	Sub Main()
		Dim objRef1 As ValueReference
		Dim objRef2 As ValueReference

		Dim objValue1 As MyStruct
		Dim objValue2 As MyStruct

		objRef1 = New ValueReference( )
		objRef1.Age = 20
		objRef2 = objRef1
		objRef2.Age = 30

		If ((objRef1.Age <> objRef2.Age) or (objRef1.Age <> 30) )
			Throw New Exception ("Unexpected behavior objRef1.Age and objRef2.Age should return the same value Expected 30 but got = " & objRef2.Age)
		End if 

		objValue1 = New MyStruct( )
		objValue1.Age = 20
		objValue2 = objValue1
		objValue2.Age = 30
		If (objValue1.Age <> 20) or (objValue2.Age <> 30) then
			Throw New Exception ("Unexpected behavior. Expected objValue1.Age = 20 and objValue2.Age = 30 but got  objValue1.Age = " & objValue1.Age & " objValue2.Age = " & objValue2.Age)
		End if 
	End Sub
End Module
