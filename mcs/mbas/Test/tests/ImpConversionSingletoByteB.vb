'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionSingletoByteC
	Sub Main()
			Dim a as Single= 111
			Dim b as Byte = 111 + a
			if b <> 222
				Throw new System.Exception("Addition of Single & Byte not working. Expected 222 but got " &b) 
			End if		
	End Sub
End Module
