'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 12
REM ExpectedError: BC30506
REM ErrorMessage:  Handles clause requires a WithEvents variable.

Class AA
	Protected Event A
	Sub EH1() Handles AA.A
	End Sub	
End Class

Module A
	Sub Main()
	End Sub
End Module
