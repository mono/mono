Imports System
Module ConditionalCompilation
	Sub Main()
		Dim value As Integer
		Try
			'Testing #If Then and #End If Block
			
			#If True Then
			       value=10 
			#End If
			If value<>10 Then
				Throw New Exception("#A1-Conditional Compilation:Failed ")
			End If
			
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try
	End Sub
End Module

