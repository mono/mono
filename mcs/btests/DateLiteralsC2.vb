REM LineNo: 13
REM ExpectedError: BC31085
REM ErrorMessage: Date constant is not valid.

REM LineNo: 15
REM ExpectedError: BC31085
REM ErrorMessage: Date constant is not valid.

Module DateLiterals
    Sub Main()
        Dim d As Date
	
	d = # 12/40/2002 #

	d = # 11/31/2002 #
   End Sub
End Module

	
	
