'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)


Imports System

Class Keys
	Public ReadOnly Default Property Item(ByVal s As String) As Integer
		Get
			Return 10
		End Get
	End Property 
End Class

Module Test
	Sub Main()
        	Dim x As Keys = new Keys()
		Dim y As Integer
        	Dim z As Integer
		y = x!zzz 
		z = x("abc")
		if y <> 10 OR Z<> 10 then 
			Throw New Exception ("Unexpected Behavior . As y should be equal to 10 but got y =" &y)
		End If
	End Sub
End Module
