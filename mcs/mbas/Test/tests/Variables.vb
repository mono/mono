Imports System

Module Variables
	Sub Main()
		Dim a As Integer
		If a<>0 Then
			Throw New Exception("Variables : Failed-Error assigning default value to variables")
		End If
		Dim b 'Default type is Object
		If b<>"" Then
			Throw New Exception("Variables : Failed-Error in implicit conversion of Object to string")
		End If
		If b<>0 Then
			Throw New Exception("Variables : Failed-Error in implicit conversion of Object to integer")
		End If
		If b<>0.0 Then
			Throw New Exception("Variables : Failed-Error in implicit conversion of Object to double")
		End If
	End Sub
End Module
