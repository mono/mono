Imports System
Module ConditionalCompilation
	Sub Main()
                'Testing line continuation within conditional compilation statement
		Dim value As Integer
                #If _
                True 
			value=10
                #Else _
		      _

			Throw New Exception("#D11-Conditional Compilation: Failed")
		#End If

		If value<>10 Then
			Throw New Exception("#D12-Conditional Compilation: Failed")
		End If
        End Sub
End Module

