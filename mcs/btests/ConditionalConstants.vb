REM LineNo: 23
REM ExpectedError: BC30249
REM ErrorMessage: '=' expected.

Imports System
Module ConditionalConstants
	Sub Main()
		Dim value As Integer
		#Const A=True
		#If A
			value=10
		#End If
		
		#Const B=False
		#If B
			value=20
		#End If
		If value<>10 Then
			Throw New Exception("Conditional Constant: Failed")
		End If

		'Testing the default value assigned to a conditional constant
		#Const C
		#If C
			Console.WriteLine("Default value is True")
		#Else 
			Console.WriteLine("Default value is Nothing")
		#End If
	End Sub
End Module
