'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)
'TypeOf

Imports System

	Class NewClass
		Public i As Integer
		Public Overridable Sub MyMethod(i)
		End Sub
	End Class

	Class NewClass2
		Public i As Integer
		Public Overridable Sub MyMethod(i)
		End Sub
	End Class


Module Test 
	Sub Main() 
		Dim TestObj2 As NewClass2 = New NewClass2()
		Dim TestObj As NewClass = New NewClass()
		If TypeOf Testobj Is NewClass Then
		Else 
			Throw New Exception ("Unexpected Behavior S should be 10 ")
		End If
	End Sub
End Module
