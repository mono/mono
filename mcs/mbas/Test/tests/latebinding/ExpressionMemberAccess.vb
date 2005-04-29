Imports System
Class C
	Public F As Integer = 10
End Class

Module Test
	Public Function ReturnC() As Object
		Console.WriteLine("Returning a new instance of C.")
		Return New C()
	End Function

	Public Sub Main()
		if Returnc().F <> 10 Then 
			Throw New Exception ("Unexpected Behavior Returnc().F should be 10")	
		End If
	End Sub
End Module
