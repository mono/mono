REM LineNo: 9
REM ExpectedError: BC31085
REM ErrorMessage: Date constant is not valid.

Module DateLiterals
    Sub Main()
        Dim d As Date
	
	d = # 11/31/2002 #
   End Sub
End Module

	
	
