REM LineNo: 11
REM ExpectedError: BC31085
REM ErrorMessage: Date constant is not valid.

Module DateLiterals
    Sub Main()
        Dim d As Date
	
	' if minuts and seconds are not specified
	' AM or PM must be specified
	d = # 1 #
   End Sub
End Module

	
	
