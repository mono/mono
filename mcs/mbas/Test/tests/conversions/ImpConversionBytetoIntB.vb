'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionBytetoIntegerC
	Sub Main()
			Dim a as Byte = 111
			Dim b as Integer = 111 + a
			if b <> 222
				Throw new System.Exception("Addition of Byte & Integer not working. Expected 222 but got " &b) 
			End if		
	End Sub
End Module



