Imports System

Module ConditionalStatementsB

    Sub Main()

        Dim i As Integer = 0
	
	' With the single-line form, it is possible to have multiple 
	' statements executed as the result of an If...Then decision.

        If i = 0 Then i += 1 : i += 2 : i += 3
	

	If i <> 6 Then throw new exception("#CSB1 - LineIfThenStatement failed")  _
		else i += 6 : i += 12   
	
	
	If i <> 24 Then 
		throw new exception("#CSB2 - LineIfThenStatement failed")
	End If
	
	' Execution of a Case block is not permitted to "fall through" to 
	' next switch section

	Dim j As Integer = 0
	for i = 0 To 3
		Select Case i
			Case 0
			Case 2
				j += 2	
			Case 1
			Case 3
				j += 3
		End Select
	next

	if j <> 5 then
		throw new exception("#CSB3 - Switch Case Statement failed")
	end if
	
    End Sub

End Module