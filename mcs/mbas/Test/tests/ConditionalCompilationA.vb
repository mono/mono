Imports System
Module ConditionalCompilation
	Sub Main()
		Dim value As Integer
		Try
			'Testing #If and #End If Block - Variation 1
			
			#If True
			       value=10 
			#End If
			If value<>10 Then
				Throw New Exception("#A11-Conditional Compilation:Failed ")
			End If
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try

		Try
			'Testing #If and #End If Block - Variation 2
			#If False
				Throw New Exception("#A21-Conditional Compilation:Failed")
                	#End If
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try

		Try
			'Testing #If, #Else and #End If Block - Variation 3
			#If True
				value=30
			#Else
				Throw New Exception("#A31-Conditional Compilation:Failed")
                	#End If

			If value<>30
				Throw New Exception("#A32-Conditional Compilation:Failed")
			End If
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try

		Try
			'Testing #If, #Else and #End If Block - Variation 4
			#If False
				Throw New Exception("#A41-Conditional Compilation:Failed")
			#Else
				value=40
                	#End If

			If value<>40
				Throw New Exception("#A42-Conditional Compilation:Failed")
			End If
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try

	End Sub
End Module
