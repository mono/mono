Imports System

Module ConditionalStatementsA

	Sub Main()

		Dim i As Integer = 0

		if i = 0 then i = 1

		if i <> 1 then throw new exception("#CSA1") else i = 2
		
		if i = 1 then else i = 3

		if i <> 3 then i = 2 else    ' Should give compile time error

		if i <> 2
			i = 3
		end if
				
		if i = 3
		end if
				
		if i <> 3
			throw new exception("#CSA2")   
		else
			i = 4
		end if

		if i <> 4 then
			throw new exception("#CSA3")
		elseif i = 4
			i = 5
		end if

		if i <> 5
			throw new exception("#CSA4") 
		elseif i = 6
			throw new exception("#CSA5")
		elseif i = 5 then
			i = 6
		else
			throw new exception("#CSA6")
		end if
		
	End Sub
	
End Module