' Author:
'   Maverson Eduardo Schulze Rosa (maverson@gmail.com)
'
' GrupoTIC - UFPR - Federal University of Paraná

REM LineNo: 16
REM ExpectedError: BC31400
REM ErrorMessage: Local variables within methods of structures cannot be declared 'Static'.

Imports System

Structure Somestruct
	Dim a1 as integer 'Just because a struct must contain at least one instance member variable or Event declaration.
	
	sub testStaticVars
		Static localStaticVariable as integer
	end sub
End Structure

Module Test
	Sub main()
	End Sub
End Module
