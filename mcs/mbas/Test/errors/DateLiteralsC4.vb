REM LineNo: 9
REM ExpectedError: BC31085
REM ErrorMessage: Date constant is not valid.

Module DateLiterals
    Sub Main()
        Dim d As Date
	
	d = # 12/31/02 #
   End Sub
End Module

	
	
