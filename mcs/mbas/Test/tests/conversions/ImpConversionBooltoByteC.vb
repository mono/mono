'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionBooleantoByteC
	Sub Main()
			Dim a as Boolean = True
			Dim b as Byte = 111 + a
			if b <> 110
				Throw new System.Exception("Addition of Boolean & Byte not working. Expected 110 but got " &b) 
			End if		
	End Sub
End Module

