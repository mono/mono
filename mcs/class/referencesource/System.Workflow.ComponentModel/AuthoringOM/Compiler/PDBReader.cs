namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Runtime.InteropServices;

    internal sealed class PDBReader : IDisposable
    {
        #region Data Members

        private const string IMetaDataImportGuid = "7DAC8207-D3AE-4c75-9B67-92801A497D44";
        private ISymUnmanagedReader symReader;

        #endregion

        #region Constructor and Dispose

        public PDBReader(string assemblyPath)
        {
            object metaDataImport = null;
            IMetaDataDispenser dispenser = null;

            try
            {
                Guid metaDataImportIID = new Guid(IMetaDataImportGuid);

                dispenser = (IMetaDataDispenser)(new MetaDataDispenser());
                dispenser.OpenScope(assemblyPath, 0, ref metaDataImportIID, out metaDataImport);

                this.symReader = (ISymUnmanagedReader)(new CorSymReader_SxS());
                this.symReader.Initialize(metaDataImport, assemblyPath, null, null);
            }
            finally
            {
                // Release COM objects so that files don't remain locked.
                if (metaDataImport != null)
                    Marshal.ReleaseComObject(metaDataImport);

                if (dispenser != null)
                    Marshal.ReleaseComObject(dispenser);
            }
        }

        ~PDBReader()
        {
            Dispose();
        }

        void IDisposable.Dispose()
        {
            Dispose();
            GC.SuppressFinalize(this);
        }

        private void Dispose()
        {
            if (this.symReader != null)
            {
                Marshal.ReleaseComObject(this.symReader);
                this.symReader = null;
            }
        }

        #endregion

        #region Public methods

        public void GetSourceLocationForOffset(uint methodDef, uint offset, out string fileLocation, out uint line, out uint column)
        {
            fileLocation = null;
            line = 0;
            column = 0;

            ISymUnmanagedMethod symMethod = null;
            ISymUnmanagedDocument[] documents = null;
            uint sequencePointCount = 0;

            try
            {
                symMethod = this.symReader.GetMethod(methodDef);
                sequencePointCount = symMethod.GetSequencePointCount();

                documents = new ISymUnmanagedDocument[sequencePointCount];
                uint[] offsets = new uint[sequencePointCount];
                uint[] lines = new uint[sequencePointCount];
                uint[] columns = new uint[sequencePointCount];
                uint[] endLines = new uint[sequencePointCount];
                uint[] endColumns = new uint[sequencePointCount];

                symMethod.GetSequencePoints(sequencePointCount, out sequencePointCount, offsets, documents, lines, columns, endLines, endColumns);

                uint index = 1;
                for (; index < sequencePointCount; index++)
                { if (offsets[index] > offset) break; }

                index = index - 1;

                // Work Around: AkashS - The SymReader returns bad line-column data for unconditional branch
                // instructions. The line number is whacky and the column number is 0. Need to verify why this is so.
                // We just look for a good sequence point data, it should be close enough in the source code.
                while (columns[index] == 0 && index > 0)
                    index--;

                while (index < sequencePointCount && columns[index] == 0)
                    index++;

                // What more can we do?
                if (index >= sequencePointCount || columns[index] == 0)
                    index = 0;

                // End Work around


                line = lines[index];
                column = columns[index];

                ISymUnmanagedDocument document = documents[index];
                uint urlLength = 261;
                string url = new string('\0', (int)urlLength);

                document.GetURL(urlLength, out urlLength, url);
                fileLocation = url.Substring(0, (int)urlLength - 1);
            }
            finally
            {
                // Release COM objects so that files don't remain locked.
                for (uint i = 0; i < sequencePointCount; i++)
                    if (documents[i] != null)
                        Marshal.ReleaseComObject(documents[i]);

                if (symMethod != null)
                    Marshal.ReleaseComObject(symMethod);
            }
        }

        #endregion
    }

    #region Interop declarations

    //
    // Note: 
    // These interop declaration are sufficient for our purposes of reading the symbol information from
    // the PDB. They are not complete otherwise!
    //
    [ComImport, Guid("0A3976C5-4529-4ef8-B0B0-42EED37082CD")]
    internal class CorSymReader_SxS
    { }


    [ComImport,
    CoClass(typeof(CorSymReader_SxS)),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("B4CE6286-2A6B-3712-A3B7-1EE1DAD467B5")]
    internal interface ISymUnmanagedReader
    {
        // NYI.
        void GetDocument();
        void GetDocuments();
        void GetUserEntryPoint();

        ISymUnmanagedMethod GetMethod(uint methodDef);

        // NYI.
        void GetMethodByVersion();
        void GetVariables();
        void GetGlobalVariables();
        void GetMethodFromDocumentPosition();
        void GetSymAttribute();
        void GetNamespaces();

        // Incomplete - We don't use the Stream
        void Initialize([In, MarshalAs(UnmanagedType.IUnknown)] object metaDataImport, [In, MarshalAs(UnmanagedType.LPWStr)] string pdbPath, [In, MarshalAs(UnmanagedType.LPWStr)] string searchPath, [In, MarshalAs(UnmanagedType.IUnknown)] object stream);

        // NYI.
        void UpdateSymbolStore();
        void ReplaceSymbolStore();
        void GetSymbolStoreFileName();
        void GetMethodsFromDocumentPosition();
        void GetDocumentVersion();
        void GetMethodVersion();
    }


    [ComImport,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("B62B923C-B500-3158-A543-24F307A8B7E1")]
    internal interface ISymUnmanagedMethod
    {
        uint GetToken();
        uint GetSequencePointCount();

        // Incomplete - Don't need to define ISymUnmanagedScope.
        object GetRootScope();

        // Incomplete - Don't need to define ISymUnmanagedScope.
        object GetScopeFromOffset(uint offset);

        uint GetOffset([In, MarshalAs(UnmanagedType.IUnknown)] ISymUnmanagedDocument document, uint line, uint column);
        void GetRanges([In, MarshalAs(UnmanagedType.IUnknown)] ISymUnmanagedDocument document, uint line, uint column, uint rangeCount, [Out] out uint actualRangeCount, [In, Out, MarshalAs(UnmanagedType.LPArray)] uint[] ranges);

        // NYI.
        void GetParameters();

        // NYI.
        void GetNamespace();

        void GetSourceStartEnd([In, Out, MarshalAs(UnmanagedType.LPArray)] ISymUnmanagedDocument[] documents, [In, Out, MarshalAs(UnmanagedType.LPArray)] uint[] lines, [In, Out, MarshalAs(UnmanagedType.LPArray)] uint[] columns, [Out, MarshalAs(UnmanagedType.Bool)] out bool positionsDefined);
        void GetSequencePoints(uint pointsCount, [Out] out uint actualPointsCount, [In, Out, MarshalAs(UnmanagedType.LPArray)] uint[] offsets, [In, Out, MarshalAs(UnmanagedType.LPArray)] ISymUnmanagedDocument[] documents, [In, Out, MarshalAs(UnmanagedType.LPArray)] uint[] lines, [In, Out, MarshalAs(UnmanagedType.LPArray)] uint[] columns, [In, Out, MarshalAs(UnmanagedType.LPArray)] uint[] endLines, [In, Out, MarshalAs(UnmanagedType.LPArray)] uint[] endColumns);
    }


    [ComImport,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("40DE4037-7C81-3E1E-B022-AE1ABFF2CA08")]
    internal interface ISymUnmanagedDocument
    {
        void GetURL(uint urlLength, [Out] out uint actualUrlLength, [In, Out, MarshalAs(UnmanagedType.LPWStr)] string url);

        // The rest are NYI.
        void GetDocumentType();
        void GetLanguage();
        void GetLanguageVendor();
        void GetCheckSumAlgorithmId();
        void GetCheckSum();
        void FindClosestLine();
        void HasEmbeddedSource();
        void GetSourceLength();
        void GetSourceRange();
    }


    [ComImport,
    Guid("E5CB7A31-7512-11d2-89CE-0080C792E5D8")]
    internal class MetaDataDispenser
    {
    }


    [ComImport,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    CoClass(typeof(MetaDataDispenser)),
    Guid("809C652E-7396-11d2-9771-00A0C9B4D50C")]
    internal interface IMetaDataDispenser
    {
        // NYI
        void DefineScope();

        // Incomplete - I don't really need to define IMetaDataImport.
        void OpenScope([In, MarshalAs(UnmanagedType.LPWStr)] string scope, uint flags, [In] ref Guid riid, [Out, MarshalAs(UnmanagedType.IUnknown)] out object unknown);

        // NYI
        void OpenScopeOnMemory();
    }

    #endregion
}
