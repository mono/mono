' Author:
'   Maverson Eduardo Schulze Rosa (maverson@gmail.com)
'
' GrupoTIC - UFPR - Federal University of Paraná

Imports System

Module LocalDeclarationC
	sub main()
		Dim i As Integer = 0
		
		'Declaring a Local Static Variable in a Method Child Block.
		If i = 0 Then
			'this is declared as ".field  private static  specialname".
                	static Dim stVarModule as integer

			'this uses stsfld.
        	        stVarModule = 10

			'this uses ldsfld.
			if stVarModule <> 10 then
				Throw New Exception("#LDC1 - Load  Local Static Variable Failed")
			end if
		End If
	end sub
End Module
