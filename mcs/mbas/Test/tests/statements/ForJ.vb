'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module M
	Sub Main()
		dim count as integer
		dim ErrMsg As string = ""
		for i as Decimal  = 1 to 10.49 step .51
			count = count+1 
		next i
		if count<>19
			ErrMsg = "#A1 For not working"
		end if

		count = 0 
		for i  as Single = 1 to 10.49 step .51
			count = count+1 
		next i
		if count<>19
			ErrMsg = ErrMsg & vbCrLf & "#A2 For not working"
		end if
	
		count = 0 
		for i  as Double = 1 to 10.49 step .51
			count = count+1 
		next i
		if count<>19
			ErrMsg = ErrMsg & vbCrLf & "#A3 For not working"
		end if
		if (ErrMsg <> "")
			throw new System.Exception (ErrMsg)
		End IF
	end sub
End Module
