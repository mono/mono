'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionLongtoIntegerC
	Sub Main()
			Dim a as Long= 111
			Dim b as Integer = 111 + a
			if b <> 222
				Throw new System.Exception("Addition of Long & Integer not working. Expected 222 but got " &b) 
			End if		
	End Sub
End Module


