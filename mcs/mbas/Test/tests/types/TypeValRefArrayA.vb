'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module TypeValRefArray

	Sub Main()
		Dim SomeArray1( ) As Integer = {1, 2, 3}
		Dim SomeArray2( ) As Integer
		SomeArray2 = SomeArray1
		SomeArray1(0) = 100
		if SomeArray2(0) <> 100 then 
			Throw	New Exception ("Unexpected Behavior SomeArray2(0) refers to SomeArray1(0) value should be 100 but got SomeArray2(0) = " & SomeArray2(0))
		End if
	End Sub
End Module