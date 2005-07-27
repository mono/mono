' Author:
'   Maverson Eduardo Schulze Rosa (maverson@gmail.com)
'
' GrupoTIC - UFPR - Federal University of Paraná

Imports System

Module LocalDeclarationB
	
	sub main()
		'this is declared as ".field  private static  specialname".
                static stVarModule as integer

		'this uses stsfld.
                stVarModule = 10

		'this uses ldsfld.
		if stVarModule <> 10 then
			Throw New Exception("#LDB1 - Load  Local Static Variable Failed")
		end if

		Dim C as NonStaticField = new NonStaticField()
		c.test_class()
	end sub

        class NonStaticField
	
	    sub test_class()
	    	'this is declared as ".field  private specialname" without static.
                static stVarClass as integer
                
		'this uses stfld.
		stVarClass = 10

		'this uses ldfld.
		if stVarClass <> 10 then
			Throw New Exception("#LDB2 - Load  Local Static Variable Failed")
		end if
	    end sub
	end class
End Module
