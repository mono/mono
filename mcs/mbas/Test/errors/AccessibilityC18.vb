REM LineNo: 11
REM ExpectedError: BC30456
REM ErrorMessage: 'C' is not a member of 'C1'.

Class C1
End Class

Class C3
	Public Sub S()
		Dim myC As New C1()
		myC.C = 20
	End Sub
End Class

Module Accessibility
	Sub Main()
	End Sub
End Module
