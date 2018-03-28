//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    // this is a heavily modified version of the Win32ManifestGenerator found in the CLR    
    class ComIntegrationManifestGenerator : MarshalByRefObject
    {
        internal static void GenerateManifestCollectionFile(Guid[] manifests, String strAssemblyManifestFileName, String assemblyName)
        {
            String title = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";
            String asmTitle = "<assembly xmlns=\"urn:schemas-microsoft-com:asm.v1\" manifestVersion=\"1.0\">";
            String asmEnd = "</assembly>";

            String path = Path.GetDirectoryName(strAssemblyManifestFileName);
            if (!String.IsNullOrEmpty(path) && !Directory.Exists(path))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(System.ServiceModel.ComIntegration.Error.DirectoryNotFound(path));

            }

            Stream s = null;

            try
            {
                // manifest title
                s = File.Create(strAssemblyManifestFileName);
                WriteUTFChars(s, title + Environment.NewLine);
                WriteUTFChars(s, asmTitle + Environment.NewLine);

                WriteUTFChars(s, "<assemblyIdentity" + Environment.NewLine, 4);
                WriteUTFChars(s, "name=\"" + assemblyName + "\"" + Environment.NewLine, 8);
                WriteUTFChars(s, "version=\"1.0.0.0\"/>" + Environment.NewLine, 8);

                for (int i = 0; i < manifests.Length; i++)
                {

                    WriteUTFChars(s, "<dependency>" + Environment.NewLine, 4);
                    
                    WriteUTFChars(s, "<dependentAssembly>" + Environment.NewLine, 8);
                    WriteUTFChars(s, "<assemblyIdentity" + Environment.NewLine, 12);
                    
                    WriteUTFChars(s, "name=\"" + manifests[i].ToString() + "\"" + Environment.NewLine, 16);
                    WriteUTFChars(s, "version=\"1.0.0.0\"/>" + Environment.NewLine, 16);

                    WriteUTFChars(s, "</dependentAssembly>" + Environment.NewLine, 8);
                    WriteUTFChars(s, "</dependency>" + Environment.NewLine, 4);
                }

                WriteUTFChars(s, asmEnd);

            }
            catch (Exception e)
            {
                if (e is NullReferenceException || e is SEHException)
                {
                    throw;
                }

                s.Close();
                File.Delete(strAssemblyManifestFileName);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(System.ServiceModel.ComIntegration.Error.ManifestCreationFailed(strAssemblyManifestFileName, e.Message));
            }

            s.Close();
        }

        internal static void GenerateWin32ManifestFile(Type[] aTypes, String strAssemblyManifestFileName, String assemblyName)
        {
            String title = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";
            String asmTitle = "<assembly xmlns=\"urn:schemas-microsoft-com:asm.v1\" manifestVersion=\"1.0\">";

            String path = Path.GetDirectoryName(strAssemblyManifestFileName);
            if (!String.IsNullOrEmpty(path) && !Directory.Exists(path))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(System.ServiceModel.ComIntegration.Error.DirectoryNotFound(path));
                
            }

            Stream s = null;  

            try
            {
                // manifest title
                s = File.Create(strAssemblyManifestFileName);
                WriteUTFChars(s, title + Environment.NewLine);
                WriteUTFChars(s, asmTitle + Environment.NewLine);
                
                WriteUTFChars(s, "<assemblyIdentity" + Environment.NewLine, 4);
                WriteUTFChars(s, "name=\"" + assemblyName + "\"" + Environment.NewLine, 8);
                WriteUTFChars(s, "version=\"1.0.0.0\"/>" + Environment.NewLine, 8);

                AsmCreateWin32ManifestFile(s, aTypes);

            }
            catch (Exception e)
            {
                if (e is NullReferenceException || e is SEHException)
                {
                    throw;
                }

                s.Close();
                File.Delete(strAssemblyManifestFileName);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(System.ServiceModel.ComIntegration.Error.ManifestCreationFailed(strAssemblyManifestFileName, e.Message));
            }

            s.Close();
        }

        static void AsmCreateWin32ManifestFile(Stream s, Type[] aTypes)
        {            
            String asmEnd = "</assembly>";
            
            WriteTypes(s, aTypes, 4);
            WriteUTFChars(s, asmEnd);
        }

        static void WriteTypes(Stream s, Type[] aTypes, int offset)
        {
            RegistrationServices regServices = new RegistrationServices();
            String name = null;

            Assembly asm = Assembly.GetExecutingAssembly();
            string asmver = asm.ImageRuntimeVersion;


            foreach (Type t in aTypes)
            {
                // only registrable managed types will show up in the manifest file
                if (!regServices.TypeRequiresRegistration(t))
                {
                    throw Fx.AssertAndThrow("User defined types must be registrable");
                }
                
                String strClsId = "{" + Marshal.GenerateGuidForType(t).ToString().ToUpperInvariant() + "}";
                name = t.FullName;

                // this type is a com imported type or Record
                if (regServices.TypeRepresentsComType(t) || t.IsValueType)
                {
                    WriteUTFChars(s, "<clrSurrogate" + Environment.NewLine, offset);
                    // attribute clsid
                    WriteUTFChars(s, "    clsid=\"" + strClsId + "\"" + Environment.NewLine, offset);    
                    
                    // attribute class
                    WriteUTFChars(s, "    name=\"" + name + "\"" + Environment.NewLine, offset);
                    // clr version
                    WriteUTFChars(s, "    runtimeVersion=\"" + asmver + "\">" + Environment.NewLine, offset);

                    WriteUTFChars(s, "</clrSurrogate>" + Environment.NewLine, offset);
                }                
            }
        }

        static void WriteUTFChars(Stream s, String value, int offset)
        {
            for (int i = 0; i < offset; i++)
            {
                WriteUTFChars(s, " ");
            }
            WriteUTFChars(s, value);
        }

        static void WriteUTFChars(Stream s, String value)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value);
            s.Write(bytes, 0, bytes.Length);
        }
    }
}
