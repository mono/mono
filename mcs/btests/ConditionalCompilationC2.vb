REM LineNo: 15
REM ExpectedError: BC30459
REM ErrorMessage: 'Nothing' is not valid in this context.



Imports System
Module ConditionalCompilation
	Sub Main()
		Dim value As Integer
		Dim a,b As Integer
		Try
			'Testing #If and #End If Block
			
			#If a+b
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

