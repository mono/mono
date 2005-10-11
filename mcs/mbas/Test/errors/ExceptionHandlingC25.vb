' Author:
'   Maverson Eduardo Schulze Rosa (maverson@gmail.com)
'
' GrupoTIC - UFPR - Federal University of Paraná

REM LineNo: 18
REM ExpectedError: BC30203
REM ErrorMessage: Identifier expected

Imports System
Imports Microsoft.VisualBasic

Module On_Error
	
	private i as integer = 10

	Sub Main()
                On Error Goto -2
		
		On Error Goto OnErrorLabel1
                
		i /=  0
		
		Exit Sub		

		OnErrorLabel1:
		Console.WriteLine("1 Error Number: " & Err.Number & " , Error Description: " & Err.Description)
		
	End Sub

End Module
