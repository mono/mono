'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module M
	Sub Main()
		Dim ErrMsg As String = ""
		dim count as integer
		for i as Long = 16 to 6 step -1
			count = count+1 
		next i
		if count<>11
			ErrMsg = "#A1 For not working"
		end if

		count=0
		for i as Short = 16 to 6 step -1
			count = count+1 
		next i
		if count<>11
			ErrMsg = ErrMsg & vbCrLf & "#A2 For not working"
		end if

		count=0
		for i as Object = 16 to 6 step -1
			count = count+1 
		next i
		if count<>11
			ErrMsg = ErrMsg & vbCrLf & "#A3 For not working"
		end if

		count=0
		for i as Double = 16 to 6 step -0.1
			count = count+1 
		next i
		if count<>101
			ErrMsg = ErrMsg & vbCrLf & "#A4 For not working"
		end if

		count=0
		for i as Single = 16 to 6 step -0.1
			count = count+1 
		next i
		if count<>100
			ErrMsg = ErrMsg & vbCrLf & "#A5 For not working"
		end if

		count=0
		for i as Decimal = 16 to 6 step -0.1
			count = count+1 
		next i
		if count<>101
			ErrMsg = ErrMsg & vbCrLf & "#A6 For not working"
		end if		
		if (ErrMsg <> "")
			throw new System.Exception (ErrMsg)
		End IF
	end sub
End Module
