'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Class C
	Public Shared F As Integer = 10
End Class

Module Test
	Public Function ReturnC() As C
		Console.WriteLine("Returning a new instance of C.")
		Return New C()
	End Function

	Public Sub Main()
		if Returnc().F <> 10 Then 
			Throw New Exception ("Unexpected Behavior Returnc().F should be 10")	
		End If
	End Sub
End Module
