// Copyright (c) 2004 Francisco T. Martinez <paco@mfcon.com>
// All rights reserved.
//
using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Mfconsulting.General.Prj2Make.Schema.Prjx;
using Mfconsulting.General.Prj2Make.Schema.Csproj;

namespace Mfconsulting.General.Prj2Make
{
    public class Maker
    {
    	public enum TypeOfFile {
    		unknown,
			sln,
			csproj,
			cmbx,
			prjx			 
		}
		
		// Helper function to determine the type of
		// file being parsed based on its extension
		protected TypeOfFile DetermineFileType(string inFileName)
		{
			string ExtentionString = System.IO.Path.GetExtension (inFileName);
			
			switch(ExtentionString)
			{
			case ".sln":
				return TypeOfFile.sln;
			case ".csproj":
				return TypeOfFile.csproj;
			case ".cmbx":
				return TypeOfFile.cmbx;
			case ".prjx":
				return TypeOfFile.prjx;
			}		
			
			return TypeOfFile.unknown;
		}

		public bool CreateCombineFromSln(string slnFileName)
		{
			SlnMaker slnMkObj = new SlnMaker();

			// Load the sln and parse it
			slnMkObj.MsSlnToCmbxHelper(slnFileName);

			return false;
		}
     
		public bool CreatePrjxFromCsproj(string csprojFileName)
		{
			SlnMaker slnMkObj = new SlnMaker();

			// Load the csproj and parse it
			slnMkObj.CreatePrjxFromCsproj(csprojFileName);

			return false;
		}
     
     	// Main entry point for Makefile generation
		public string MakerMain(bool isUnixMode, bool isMcsMode, string slnFile)
    	{
    		SlnMaker mk1Obj = null;
    		CmbxMaker mk2Obj = null;
            // Test to see what kind if file we got
            // sln, csproj, cmbx, prjx
            switch(DetermineFileType(slnFile))
            {
            case TypeOfFile.sln:
            	mk1Obj = new SlnMaker();
            	return mk1Obj.MsSlnHelper (isUnixMode, isMcsMode, true, slnFile);
            case TypeOfFile.csproj:
            	mk1Obj = new SlnMaker();
            	return mk1Obj.MsSlnHelper (isUnixMode, isMcsMode, false, slnFile);
            case TypeOfFile.cmbx:
            	mk2Obj = new CmbxMaker();
            	return mk2Obj.MdCmbxHelper (isUnixMode, isMcsMode, true, slnFile);
            case TypeOfFile.prjx:
            	mk2Obj = new CmbxMaker();
            	return mk2Obj.MdCmbxHelper (isUnixMode, isMcsMode, false, slnFile);
            }
           	return "Error: unknown file type.";
		}
    }    
}
