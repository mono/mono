Imports System
Module ConditionalConstant
	Sub Main()
	   Dim value As Integer
	   'Testing the scope of the conditional constant
	   #Const a=True
	   #If a
		   #Const a=False 
		   value=10
	   #End If
	   #If a
		value=20
	   #End If
	   If value<>10 Then
		Throw New Exception("ConditionalConstantA:Failed-conditional constant should have global scope")
	   End If

	  'Redefining a conditional constant
	  #Const a=5
	End Sub
End Module
