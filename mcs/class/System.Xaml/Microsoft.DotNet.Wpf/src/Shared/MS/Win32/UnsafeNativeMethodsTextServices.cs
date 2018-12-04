//------------------------------------------------------------------------------
// <copyright file="UnsafeNativeMethods.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Threading;
using System.ComponentModel;
using System.Diagnostics;

// The SecurityHelper class differs between assemblies and could not actually be
//  shared, so it is duplicated across namespaces to prevent name collision.
#if WINDOWS_BASE
    using MS.Internal.WindowsBase;
#elif PRESENTATION_CORE
    using MS.Internal.PresentationCore;
#elif PRESENTATIONFRAMEWORK
    using MS.Internal.PresentationFramework;
#elif DRT
    using MS.Internal.Drt;
#else
#error Attempt to use a class (duplicated across multiple namespaces) from an unknown assembly.
#endif
namespace MS.Win32
{

    using Accessibility;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System;
    using System.Security;
    using System.Security.Permissions;
    using System.Collections;
    using System.IO;
    using System.Text;
    using Microsoft.Win32.SafeHandles;

    
    //[SuppressUnmanagedCodeSecurity()]
    internal partial class UnsafeNativeMethods {

        //------------------------------------------------------
        //
        //  public Methods
        //
        //------------------------------------------------------

        #region public Methods

        /// <SecurityNote>
        /// Critical - calls unmanaged code
        /// </SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport("msctf.dll")]
        internal static extern int TF_CreateThreadMgr(out ITfThreadMgr threadManager);

        /// <summary></summary>
        /// <SecurityNote>
        /// Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("msctf.dll")]
        public static extern int TF_CreateInputProcessorProfiles(out ITfInputProcessorProfiles profiles);

        /// <summary></summary>
        /// <SecurityNote>
        /// Critical - calls unmanaged code
        /// </SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]

        [DllImport("msctf.dll")]
        public static extern int TF_CreateDisplayAttributeMgr(out ITfDisplayAttributeMgr dam);

        /// <summary></summary>
        /// <SecurityNote>
        /// Critical - calls unmanaged code
        /// </SecurityNote>
        [SuppressUnmanagedCodeSecurity, SecurityCritical]
        [DllImport("msctf.dll")]
        public static extern int TF_CreateCategoryMgr(out ITfCategoryMgr catmgr);

        #endregion public Methods

        //------------------------------------------------------
        //
        //  Constants
        //
        //------------------------------------------------------

        #region Constants

        /// <summary></summary>
        public const int TF_CLIENTID_NULL = 0;

        /// <summary></summary>
        public const char TS_CHAR_EMBEDDED     = (char)0xfffc; // unicode 2.1 object replacement character

        /// <summary></summary>
        public const char TS_CHAR_REGION       = (char)0x0000; // region boundary

        /// <summary></summary>
        public const char TS_CHAR_REPLACEMENT  = (char)0xfffd; // hidden text placeholder char, Unicode replacement character

        /// <summary></summary>
        public const int TS_DEFAULT_SELECTION = -1;

        /// <summary></summary>
        public const int TS_S_ASYNC       = 0x00040300;

        /// <summary></summary>
        public const int TS_E_NOSELECTION = unchecked((int)0x80040205);

        /// <summary></summary>
        public const int TS_E_NOLAYOUT = unchecked((int)0x80040206);

        /// <summary></summary>
        public const int TS_E_INVALIDPOINT = unchecked((int)0x80040207);

        /// <summary></summary>
        public const int TS_E_SYNCHRONOUS = unchecked((int)0x80040208);

        /// <summary></summary>
        public const int TS_E_READONLY = unchecked((int)0x80040209);

        /// <summary></summary>
        public const int TS_E_FORMAT = unchecked((int)0x8004020a);

        /// <summary></summary>
        public const int TF_INVALID_COOKIE = -1;

        /// <summary></summary>
        public const int TF_DICTATION_ON          = 0x00000001;

        /// <summary></summary>
        public const int TF_COMMANDING_ON         = 0x00000008;

        /// <summary></summary>
        public static readonly Guid IID_ITextStoreACPSink = new Guid(0x22d44c94, 0xa419, 0x4542, 0xa2, 0x72, 0xae, 0x26, 0x09, 0x3e, 0xce, 0xcf);

        /// <summary></summary>
        public static readonly Guid IID_ITfThreadFocusSink = new Guid(0xc0f1db0c, 0x3a20, 0x405c, 0xa3, 0x03, 0x96, 0xb6, 0x01, 0x0a, 0x88, 0x5f);

        /// <summary></summary>
        public static readonly Guid IID_ITfTextEditSink = new Guid(0x8127d409, 0xccd3, 0x4683, 0x96, 0x7a, 0xb4, 0x3d, 0x5b, 0x48, 0x2b, 0xf7);

        /// <summary></summary>
        public static readonly Guid IID_ITfLanguageProfileNotifySink = new Guid(0x43c9fe15, 0xf494, 0x4c17, 0x9d, 0xe2, 0xb8, 0xa4, 0xac, 0x35, 0x0a, 0xa8);

        /// <summary></summary>
        public static readonly Guid IID_ITfCompartmentEventSink = new Guid(0x743abd5f, 0xf26d, 0x48df, 0x8c, 0xc5, 0x23, 0x84, 0x92, 0x41, 0x9b, 0x64);

        /// <summary></summary>
        public static readonly Guid IID_ITfTransitoryExtensionSink = new Guid(0xa615096f, 0x1c57, 0x4813, 0x8a, 0x15, 0x55, 0xee, 0x6e, 0x5a, 0x83, 0x9c);

        /// <summary></summary>
        public static readonly Guid GUID_TFCAT_TIP_KEYBOARD = new Guid(0x34745c63, 0xb2f0, 0x4784, 0x8b, 0x67, 0x5e, 0x12, 0xc8, 0x70, 0x1a, 0x31);
/*
        /// <summary></summary>
        public static readonly Guid GUID_TFCAT_TIP_SPEECH = new Guid("b5a73cd1-8355-426b-a161-259808f26b14");

        /// <summary></summary>
        public static readonly Guid GUID_TFCAT_TIP_HANDWRITING = new Guid("246ecb87-c2f2-4abe-905b-c8b38add2c43");
*/


        /// <summary></summary>
        public static readonly Guid GUID_PROP_ATTRIBUTE = new Guid(0x34b45670, 0x7526, 0x11d2, 0xa1, 0x47, 0x00, 0x10, 0x5a, 0x27, 0x99, 0xb5);

        /// <summary></summary>
        public static readonly Guid GUID_PROP_LANGID =  new Guid(0x3280ce20, 0x8032, 0x11d2, 0xb6, 0x03, 0x00, 0x10, 0x5a, 0x27, 0x99, 0xb5);

        /// <summary></summary>
        public static readonly Guid GUID_PROP_READING = new Guid(0x5463f7c0, 0x8e31, 0x11d2, 0xbf, 0x46, 0x00, 0x10, 0x5a, 0x27, 0x99, 0xb5);


        /// <summary></summary>
        public static readonly Guid GUID_PROP_INPUTSCOPE = new Guid(0x1713dd5a, 0x68e7, 0x4a5b, 0x9a, 0xf6, 0x59, 0x2a, 0x59, 0x5c, 0x77, 0x8d);

        /// <summary></summary>
        public static readonly Guid GUID_COMPARTMENT_KEYBOARD_DISABLED = new Guid( 0x71a5b253, 0x1951, 0x466b, 0x9f, 0xbc, 0x9c, 0x88, 0x08, 0xfa, 0x84, 0xf2);

        /// <summary></summary>
        public static Guid GUID_COMPARTMENT_KEYBOARD_OPENCLOSE = new Guid( 0x58273aad, 0x01bb, 0x4164, 0x95, 0xc6, 0x75, 0x5b, 0xa0, 0xb5, 0x16, 0x2d);

        /// <summary></summary>
        public static readonly Guid GUID_COMPARTMENT_HANDWRITING_OPENCLOSE = new Guid( 0xf9ae2c6b, 0x1866, 0x4361, 0xaf, 0x72, 0x7a, 0xa3, 0x09, 0x48, 0x89, 0x0e);

        /// <summary></summary>
        public static readonly Guid GUID_COMPARTMENT_SPEECH_DISABLED = new Guid( 0x56c5c607, 0x0703, 0x4e59, 0x8e, 0x52, 0xcb, 0xc8, 0x4e, 0x8b, 0xbe, 0x35);

        /// <summary></summary>
        public static readonly Guid GUID_COMPARTMENT_SPEECH_OPENCLOSE = new Guid( 0x544d6a63, 0xe2e8, 0x4752, 0xbb, 0xd1, 0x00, 0x09, 0x60, 0xbc, 0xa0, 0x83);

        /// <summary></summary>
        public static readonly Guid GUID_COMPARTMENT_SPEECH_GLOBALSTATE = new Guid( 0x2a54fe8e, 0x0d08, 0x460c, 0xa7, 0x5d, 0x87, 0x03, 0x5f, 0xf4, 0x36, 0xc5);

        /// <summary></summary>
        public static readonly Guid GUID_COMPARTMENT_KEYBOARD_INPUTMODE_CONVERSION = new Guid( 0xccf05dd8, 0x4a87, 0x11d7, 0xa6, 0xe2, 0x00, 0x06, 0x5b, 0x84, 0x43, 0x5c);

        /// <summary></summary>
        public static readonly Guid GUID_COMPARTMENT_KEYBOARD_INPUTMODE_SENTENCE = new Guid( 0xccf05dd9, 0x4a87, 0x11d7, 0xa6, 0xe2, 0x00, 0x06, 0x5b, 0x84, 0x43, 0x5c);

        /// <summary></summary>
        public static readonly Guid GUID_COMPARTMENT_TRANSITORYEXTENSION = new Guid( 0x8be347f5, 0xc7a0, 0x11d7, 0xb4, 0x08, 0x00, 0x06, 0x5b, 0x84, 0x43, 0x5c);

        /// <summary></summary>
        public static readonly Guid GUID_COMPARTMENT_TRANSITORYEXTENSION_DOCUMENTMANAGER = new Guid( 0x8be347f7, 0xc7a0, 0x11d7, 0xb4, 0x08, 0x00, 0x06, 0x5b, 0x84, 0x43, 0x5c);

        /// <summary></summary>
        public static readonly Guid GUID_COMPARTMENT_TRANSITORYEXTENSION_PARENT = new Guid( 0x8be347f8, 0xc7a0, 0x11d7, 0xb4, 0x08, 0x00, 0x06, 0x5b, 0x84, 0x43, 0x5c);

        /// <summary></summary>
        public static readonly Guid Clsid_SpeechTip = new Guid(0xdcbd6fa8, 0x032f, 0x11d3, 0xb5, 0xb1, 0x00, 0xc0, 0x4f, 0xc3, 0x24, 0xa1);

        /// <summary></summary>
        public static readonly Guid Guid_Null = new Guid(0,0,0,0,0,0,0,0,0,0,0);

        /// <summary></summary>
        public static readonly Guid IID_ITfFnCustomSpeechCommand = new Guid(0xfca6c349, 0xa12f, 0x43a3, 0x8d, 0xd6, 0x5a, 0x5a, 0x42, 0x82, 0x57, 0x7b);

        /// <summary></summary>
        public static readonly Guid IID_ITfFnReconversion = new Guid("4cea93c0-0a58-11d3-8df0-00105a2799b5");

        /// <summary></summary>
        public static readonly Guid IID_ITfFnConfigure = new Guid(0x88f567c6, 0x1757, 0x49f8, 0xa1, 0xb2, 0x89, 0x23, 0x4c, 0x1e, 0xef, 0xf9);

        /// <summary></summary>
        public static readonly Guid IID_ITfFnConfigureRegisterWord = new Guid(0xbb95808a, 0x6d8f, 0x4bca, 0x84, 0x00, 0x53, 0x90, 0xb5, 0x86, 0xae, 0xdf);

/*

        /// <summary></summary>
        public static readonly Guid TSATTRID_OTHERS = new Guid(0xb3c32af9,0x57d0,0x46a9,0xbc,0xa8,0xda,0xc2,0x38,0xa1,0x30,0x57);

*/

/*
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font = new Guid(0x573ea825,0x749b,0x4f8a,0x9c,0xfd,0x21,0xc3,0x60,0x5c,0xa8,0x28);
*/
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_FaceName = new Guid(0xb536aeb6,0x053b,0x4eb8,0xb6,0x5a,0x50,0xda,0x1e,0x81,0xe7,0x2e);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_SizePts = new Guid(0xc8493302,0xa5e9,0x456d,0xaf,0x04,0x80,0x05,0xe4,0x13,0x0f,0x03);
/*
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style = new Guid(0x68b2a77f,0x6b0e,0x4f28,0x81,0x77,0x57,0x1c,0x2f,0x3a,0x42,0xb1);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Bold = new Guid(0x48813a43,0x8a20,0x4940,0x8e,0x58,0x97,0x82,0x3f,0x7b,0x26,0x8a);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Italic = new Guid(0x8740682a,0xa765,0x48e1,0xac,0xfc,0xd2,0x22,0x22,0xb2,0xf8,0x10);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_SmallCaps = new Guid(0xfacb6bc6,0x9100,0x4cc6,0xb9,0x69,0x11,0xee,0xa4,0x5a,0x86,0xb4);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Capitalize = new Guid(0x7d85a3ba, 0xb4fd, 0x43b3, 0xbe, 0xfc, 0x6b, 0x98, 0x5c, 0x84, 0x31, 0x41);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Uppercase = new Guid(0x33a300e8, 0xe340, 0x4937, 0xb6, 0x97, 0x8f, 0x23, 0x40, 0x45, 0xcd, 0x9a);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Lowercase = new Guid(0x76d8ccb5, 0xca7b, 0x4498, 0x8e, 0xe9, 0xd5, 0xc4, 0xf6, 0xf7, 0x4c, 0x60);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Animation = new Guid(0xdcf73d22, 0xe029, 0x47b7, 0xbb, 0x36, 0xf2, 0x63, 0xa3, 0xd0, 0x04, 0xcc);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Animation_LasVegasLights = new Guid(0xf40423d5, 0xf87, 0x4f8f, 0xba, 0xda, 0xe6, 0xd6, 0xc, 0x25, 0xe1, 0x52);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Animation_BlinkingBackground = new Guid(0x86e5b104, 0x0104, 0x4b10, 0xb5, 0x85, 0x00, 0xf2, 0x52, 0x75, 0x22, 0xb5);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Animation_SparkleText = new Guid(0x533aad20, 0x962c, 0x4e9f, 0x8c, 0x09, 0xb4, 0x2e, 0xa4, 0x74, 0x97, 0x11);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Animation_MarchingBlackAnts = new Guid(0x7644e067, 0xf186, 0x4902, 0xbf, 0xc6, 0xec, 0x81, 0x5a, 0xa2, 0x0e, 0x9d);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Animation_MarchingRedAnts = new Guid(0x78368dad, 0x50fb, 0x4c6f, 0x84, 0x0b, 0xd4, 0x86, 0xbb, 0x6c, 0xf7, 0x81);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Animation_Shimmer = new Guid(0x2ce31b58, 0x5293, 0x4c36, 0x88, 0x09, 0xbf, 0x8b, 0xb5, 0x1a, 0x27, 0xb3);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Animation_WipeDown = new Guid(0x5872e874, 0x367b, 0x4803, 0xb1, 0x60, 0xc9, 0x0f, 0xf6, 0x25, 0x69, 0xd0);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Animation_WipeRight = new Guid(0xb855cbe3, 0x3d2c, 0x4600, 0xb1, 0xe9, 0xe1, 0xc9, 0xce, 0x02, 0xf8, 0x42);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Emboss = new Guid(0xbd8ed742, 0x349e, 0x4e37, 0x82, 0xfb, 0x43, 0x79, 0x79, 0xcb, 0x53, 0xa7);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Engrave = new Guid(0x9c3371de, 0x8332, 0x4897, 0xbe, 0x5d, 0x89, 0x23, 0x32, 0x23, 0x17, 0x9a);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Hidden = new Guid(0xb1e28770, 0x881c, 0x475f, 0x86, 0x3f, 0x88, 0x7a, 0x64, 0x7b, 0x10, 0x90);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Kerning = new Guid(0xcc26e1b4, 0x2f9a, 0x47c8, 0x8b, 0xff, 0xbf, 0x1e, 0xb7, 0xcc, 0xe0, 0xdd);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Outlined = new Guid(0x10e6db31, 0xdb0d, 0x4ac6, 0xa7, 0xf5, 0x9c, 0x9c, 0xff, 0x6f, 0x2a, 0xb4);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Position = new Guid(0x15cd26ab, 0xf2fb, 0x4062, 0xb5, 0xa6, 0x9a, 0x49, 0xe1, 0xa5, 0xcc, 0x0b);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Protected = new Guid(0x1c557cb2, 0x14cf, 0x4554, 0xa5, 0x74, 0xec, 0xb2, 0xf7, 0xe7, 0xef, 0xd4);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Shadow = new Guid(0x5f686d2f, 0xc6cd, 0x4c56, 0x8a, 0x1a, 0x99, 0x4a, 0x4b, 0x97, 0x66, 0xbe);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Spacing = new Guid(0x98c1200d, 0x8f06, 0x409a, 0x8e, 0x49, 0x6a, 0x55, 0x4b, 0xf7, 0xc1, 0x53);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Weight = new Guid(0x12f3189c, 0x8bb0, 0x461b, 0xb1, 0xfa, 0xea, 0xf9, 0x07, 0x04, 0x7f, 0xe0);
*/
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Height = new Guid(0x7e937477, 0x12e6, 0x458b, 0x92, 0x6a, 0x1f, 0xa4, 0x4e, 0xe8, 0xf3, 0x91);
/*
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Underline = new Guid(0xc3c9c9f3,0x7902,0x444b,0x9a,0x7b,0x48,0xe7,0x0f,0x4b,0x50,0xf7);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Underline_Single = new Guid(0x1b6720e5,0x0f73,0x4951,0xa6,0xb3,0x6f,0x19,0xe4,0x3c,0x94,0x61);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Underline_Double = new Guid(0x74d24aa6, 0x1db3, 0x4c69, 0xa1, 0x76, 0x31, 0x12, 0x0e, 0x75, 0x86, 0xd5);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Strikethrough = new Guid(0x0c562193,0x2d08,0x4668,0x96,0x01,0xce,0xd4,0x13,0x09,0xd7,0xaf);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Strikethrough_Single = new Guid(0x75d736b6,0x3c8f,0x4b97,0xab,0x78,0x18,0x77,0xcb,0x99,0x0d,0x31);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Strikethrough_Double = new Guid(0x62489b31, 0xa3e7, 0x4f94, 0xac, 0x43, 0xeb, 0xaf, 0x8f, 0xcc, 0x7a, 0x9f);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Overline = new Guid(0xe3989f4a,0x992b,0x4301,0x8c,0xe1,0xa5,0xb7,0xc6,0xd1,0xf3,0xc8);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Overline_Single = new Guid(0x8440d94c,0x51ce,0x47b2,0x8d,0x4c,0x15,0x75,0x1e,0x5f,0x72,0x1b);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Overline_Double = new Guid(0xdc46063a, 0xe115, 0x46e3, 0xbc, 0xd8, 0xca, 0x67, 0x72, 0xaa, 0x95, 0xb4);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Blink = new Guid(0xbfb2c036, 0x7acf, 0x4532, 0xb7, 0x20, 0xb4, 0x16, 0xdd, 0x77, 0x65, 0xa8);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Subscript = new Guid(0x5774fb84,0x389b,0x43bc,0xa7,0x4b,0x15,0x68,0x34,0x7c,0xf0,0xf4);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Superscript = new Guid(0x2ea4993c,0x563c,0x49aa,0x93,0x72,0x0b,0xef,0x09,0xa9,0x25,0x5b);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_Color = new Guid(0x857a7a37,0xb8af,0x4e9a,0x81,0xb4,0xac,0xf7,0x00,0xc8,0x41,0x1b);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Font_Style_BackgroundColor = new Guid(0xb50eaa4e, 0x3091, 0x4468, 0x81, 0xdb, 0xd7, 0x9e, 0xa1, 0x90, 0xc7, 0xc7);

        /// <summary></summary>
        public static readonly Guid TSATTRID_Text = new Guid(0x7edb8e68, 0x81f9, 0x449d, 0xa1, 0x5a, 0x87, 0xa8, 0x38, 0x8f, 0xaa, 0xc0);
*/
        /// <summary></summary>
        public static readonly Guid TSATTRID_Text_VerticalWriting = new Guid(0x6bba8195,0x046f,0x4ea9,0xb3,0x11,0x97,0xfd,0x66,0xc4,0x27,0x4b);
/*
        /// <summary></summary>
        public static readonly Guid TSATTRID_Text_RightToLeft = new Guid(0xca666e71,0x1b08,0x453d,0xbf,0xdd,0x28,0xe0,0x8c,0x8a,0xaf,0x7a);
*/
        /// <summary></summary>
        public static readonly Guid TSATTRID_Text_Orientation = new Guid(0x6bab707f,0x8785,0x4c39,0x8b,0x52,0x96,0xf8,0x78,0x30,0x3f,0xfb);
/*
        /// <summary></summary>
        public static readonly Guid TSATTRID_Text_Language = new Guid(0xd8c04ef1,0x5753,0x4c25,0x88,0x87,0x85,0x44,0x3f,0xe5,0xf8,0x19);
*/
        /// <summary></summary>
        public static readonly Guid TSATTRID_Text_ReadOnly = new Guid(0x85836617,0xde32,0x4afd,0xa5,0x0f,0xa2,0xdb,0x11,0x0e,0x6e,0x4d);
/*
        /// <summary></summary>
        public static readonly Guid TSATTRID_Text_EmbeddedObject = new Guid(0x7edb8e68, 0x81f9, 0x449d, 0xa1, 0x5a, 0x87, 0xa8, 0x38, 0x8f, 0xaa, 0xc0);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Text_Alignment = new Guid(0x139941e6, 0x1767, 0x456d, 0x93, 0x8e, 0x35, 0xba, 0x56, 0x8b, 0x5c, 0xd4);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Text_Alignment_Left = new Guid(0x16ae95d3, 0x6361, 0x43a2, 0x84, 0x95, 0xd0, 0x0f, 0x39, 0x7f, 0x16, 0x93);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Text_Alignment_Right = new Guid(0xb36f0f98, 0x1b9e, 0x4360, 0x86, 0x16, 0x03, 0xfb, 0x08, 0xa7, 0x84, 0x56);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Text_Alignment_Center = new Guid(0xa4a95c16, 0x53bf, 0x4d55, 0x8b, 0x87, 0x4b, 0xdd, 0x8d, 0x42, 0x75, 0xfc);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Text_Alignment_Justify = new Guid(0xed350740, 0xa0f7, 0x42d3, 0x8e, 0xa8, 0xf8, 0x1b, 0x64, 0x88, 0xfa, 0xf0);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Text_Link = new Guid(0x47cd9051, 0x3722, 0x4cd8, 0xb7, 0xc8, 0x4e, 0x17, 0xca, 0x17, 0x59, 0xf5);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Text_Hyphenation = new Guid(0xdadf4525, 0x618e, 0x49eb, 0xb1, 0xa8, 0x3b, 0x68, 0xbd, 0x76, 0x48, 0xe3);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Text_Para = new Guid(0x5edc5822, 0x99dc, 0x4dd6, 0xae, 0xc3, 0xb6, 0x2b, 0xaa, 0x5b, 0x2e, 0x7c);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Text_Para_FirstLineIndent = new Guid(0x07c97a13, 0x7472, 0x4dd8, 0x90, 0xa9, 0x91, 0xe3, 0xd7, 0xe4, 0xf2, 0x9c);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Text_Para_LeftIndent = new Guid(0xfb2848e9, 0x7471, 0x41c9, 0xb6, 0xb3, 0x8a, 0x14, 0x50, 0xe0, 0x18, 0x97);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Text_Para_RightIndent = new Guid(0x2c7f26f9, 0xa5e2, 0x48da, 0xb9, 0x8a, 0x52, 0x0c, 0xb1, 0x65, 0x13, 0xbf);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Text_Para_SpaceAfter = new Guid(0x7b0a3f55, 0x22dc, 0x425f, 0xa4, 0x11, 0x93, 0xda, 0x1d, 0x8f, 0x9b, 0xaa);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Text_Para_SpaceBefore = new Guid(0x8df98589, 0x194a, 0x4601, 0xb2, 0x51, 0x98, 0x65, 0xa3, 0xe9, 0x06, 0xdd);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Text_Para_LineSpacing = new Guid(0x699b380d, 0x7f8c, 0x46d6, 0xa7, 0x3b, 0xdf, 0xe3, 0xd1, 0x53, 0x8d, 0xf3);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Text_Para_LineSpacing_Single = new Guid(0xed350740, 0xa0f7, 0x42d3, 0x8e, 0xa8, 0xf8, 0x1b, 0x64, 0x88, 0xfa, 0xf0);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Text_Para_LineSpacing_OnePtFive = new Guid(0x0428a021, 0x0397, 0x4b57, 0x9a, 0x17, 0x07, 0x95, 0x99, 0x4c, 0xd3, 0xc5);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Text_Para_LineSpacing_Double = new Guid(0x82fb1805, 0xa6c4, 0x4231, 0xac, 0x12, 0x62, 0x60, 0xaf, 0x2a, 0xba, 0x28);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Text_Para_LineSpacing_AtLeast = new Guid(0xadfedf31, 0x2d44, 0x4434, 0xa5, 0xff, 0x7f, 0x4c, 0x49, 0x90, 0xa9, 0x05);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Text_Para_LineSpacing_Exactly = new Guid(0x3d45ad40, 0x23de, 0x48d7, 0xa6, 0xb3, 0x76, 0x54, 0x20, 0xc6, 0x20, 0xcc);
        /// <summary></summary>
        public static readonly Guid TSATTRID_Text_Para_LineSpacing_Multiple = new Guid(0x910f1e3c, 0xd6d0, 0x4f65, 0x8a, 0x3c, 0x42, 0xb4, 0xb3, 0x18, 0x68, 0xc5);

        /// <summary></summary>
        public static readonly Guid TSATTRID_List = new Guid(0x436d673b, 0x26f1, 0x4aee, 0x9e, 0x65, 0x8f, 0x83, 0xa4, 0xed, 0x48, 0x84);
        /// <summary></summary>
        public static readonly Guid TSATTRID_List_LevelIndel = new Guid(0x7f7cc899, 0x311f, 0x487b, 0xad, 0x5d, 0xe2, 0xa4, 0x59, 0xe1, 0x2d, 0x42);
        /// <summary></summary>
        public static readonly Guid TSATTRID_List_Type = new Guid(0xae3e665e, 0x4bce, 0x49e3, 0xa0, 0xfe, 0x2d, 0xb4, 0x7d, 0x3a, 0x17, 0xae);
        /// <summary></summary>
        public static readonly Guid TSATTRID_List_Type_Bullet = new Guid(0xbccd77c5, 0x4c4d, 0x4ce2, 0xb1, 0x02, 0x55, 0x9f, 0x3b, 0x2b, 0xfc, 0xea);
        /// <summary></summary>
        public static readonly Guid TSATTRID_List_Type_Arabic = new Guid(0x1338c5d6, 0x98a3, 0x4fa3, 0x9b, 0xd1, 0x7a, 0x60, 0xee, 0xf8, 0xe9, 0xe0);
        /// <summary></summary>
        public static readonly Guid TSATTRID_List_Type_LowerLetter = new Guid(0x96372285, 0xf3cf, 0x491e, 0xa9, 0x25, 0x38, 0x32, 0x34, 0x7f, 0xd2, 0x37);
        /// <summary></summary>
        public static readonly Guid TSATTRID_List_Type_UpperLetter = new Guid(0x7987b7cd, 0xce52, 0x428b, 0x9b, 0x95, 0xa3, 0x57, 0xf6, 0xf1, 0x0c, 0x45);
        /// <summary></summary>
        public static readonly Guid TSATTRID_List_Type_LowerRoman = new Guid(0x90466262, 0x3980, 0x4b8e, 0x93, 0x68, 0x91, 0x8b, 0xd1, 0x21, 0x8a, 0x41);
        /// <summary></summary>
        public static readonly Guid TSATTRID_List_Type_UpperRoman = new Guid(0x0f6ab552, 0x4a80, 0x467f, 0xb2, 0xf1, 0x12, 0x7e, 0x2a, 0xa3, 0xba, 0x9e);

        /// <summary></summary>
        public static readonly Guid TSATTRID_App = new Guid(0xa80f77df,0x4237,0x40e5,0x84,0x9c,0xb5,0xfa,0x51,0xc1,0x3a,0xc7);
        /// <summary></summary>
        public static readonly Guid TSATTRID_App_IncorrectSpelling = new Guid(0xf42de43c,0xef12,0x430d,0x94,0x4c,0x9a,0x08,0x97,0x0a,0x25,0xd2);
        /// <summary></summary>
        public static readonly Guid TSATTRID_App_IncorrectGrammar = new Guid(0xbd54e398,0xad03,0x4b74,0xb6,0xb3,0x5e,0xdb,0x19,0x99,0x63,0x88);
*/
        public static readonly Guid GUID_SYSTEM_FUNCTIONPROVIDER = new Guid("9a698bb0-0f21-11d3-8df1-00105a2799b5");


        #endregion Constants

        //------------------------------------------------------
        //
        //  Enums
        //
        //------------------------------------------------------

        #region Enums

        /// <summary></summary>
        [Flags]
        public enum PopFlags
        {
            /// <summary></summary>
            TF_POPF_ALL = 0x0001,
        }

        /// <summary></summary>
        [Flags]
        public enum CreateContextFlags
        {
            // TF_PLAINTEXTTSI is undocumented
        }

        /// <summary></summary>
        public enum TsGravity
        {
            /// <summary></summary>
            TS_GR_BACKWARD = 0,
            /// <summary></summary>
            TS_GR_FORWARD = 1,
        };

        /// <summary></summary>
        public enum TsShiftDir
        {
            /// <summary></summary>
            TS_SD_BACKWARD = 0,
            /// <summary></summary>
            TS_SD_FORWARD = 1,
        };

        /// <summary></summary>
        [Flags]
        public enum SetTextFlags
        {
            /// <summary></summary>
            TS_ST_CORRECTION      = 0x1,
        }

        /// <summary></summary>
        [Flags]
        public enum InsertEmbeddedFlags
        {
            /// <summary></summary>
            TS_IE_CORRECTION      = 0x1,
        }

        /// <summary></summary>
        [Flags]
        public enum InsertAtSelectionFlags
        {
            /// <summary></summary>
            TS_IAS_NOQUERY        = 0x1,
            /// <summary></summary>
            TS_IAS_QUERYONLY      = 0x2,
        }

        /// <summary></summary>
        [Flags]
        public enum AdviseFlags
        {
            /// <summary></summary>
            TS_AS_TEXT_CHANGE      = 0x01,
            /// <summary></summary>
            TS_AS_SEL_CHANGE       = 0x02,
            /// <summary></summary>
            TS_AS_LAYOUT_CHANGE    = 0x04,
            /// <summary></summary>
            TS_AS_ATTR_CHANGE      = 0x08,
            /// <summary></summary>
            TS_AS_STATUS_CHANGE    = 0x10,
        }

        /// <summary></summary>
        [Flags]
        public enum LockFlags
        {
            /// <summary></summary>
            TS_LF_SYNC            = 0x1,
            /// <summary></summary>
            TS_LF_READ            = 0x2,
            /// <summary></summary>
            TS_LF_WRITE           = 0x4,
            /// <summary></summary>
            TS_LF_READWRITE       = 0x6,
        }

        /// <summary></summary>
        [Flags]
        public enum DynamicStatusFlags
        {
            /// <summary></summary>
            TS_SD_READONLY        = 0x001,
            /// <summary></summary>
            TS_SD_LOADING         = 0x002,
        }

        /// <summary></summary>
        [Flags]
        public enum StaticStatusFlags
        {
            /// <summary></summary>
            TS_SS_DISJOINTSEL     = 0x001,
            /// <summary></summary>
            TS_SS_REGIONS         = 0x002,
            /// <summary></summary>
            TS_SS_TRANSITORY      = 0x004,
            /// <summary></summary>
            TS_SS_NOHIDDENTEXT    = 0x008,
        }

        /// <summary></summary>
        [Flags]
        public enum AttributeFlags
        {
            /// <summary></summary>
            TS_ATTR_FIND_BACKWARDS      =   0x0001,
            /// <summary></summary>
            TS_ATTR_FIND_WANT_OFFSET    =   0x0002,
            /// <summary></summary>
            TS_ATTR_FIND_UPDATESTART    =   0x0004,
            /// <summary></summary>
            TS_ATTR_FIND_WANT_VALUE     =   0x0008,
            /// <summary></summary>
            TS_ATTR_FIND_WANT_END       =   0x0010,
            /// <summary></summary>
            TS_ATTR_FIND_HIDDEN         =   0x0020,
        }

        /// <summary></summary>
        [Flags]
        public enum GetPositionFromPointFlags
        {
            /// <summary></summary>
            GXFPF_ROUND_NEAREST = 0x1,
            /// <summary></summary>
            GXFPF_NEAREST       = 0x2,
        }

        /// <summary></summary>
        public enum TsActiveSelEnd
        {
            /// <summary></summary>
            TS_AE_NONE = 0,
            /// <summary></summary>
            TS_AE_START = 1,
            /// <summary></summary>
            TS_AE_END = 2,
        }

        /// <summary></summary>
        public enum TsRunType
        {
            /// <summary></summary>
            TS_RT_PLAIN = 0,
            /// <summary></summary>
            TS_RT_HIDDEN = 1,
            /// <summary></summary>
            TS_RT_OPAQUE = 2,
        }

        /// <summary></summary>
        [Flags]
        public enum OnTextChangeFlags
        {
            /// <summary></summary>
            TS_TC_CORRECTION      = 0x1,
        }

        /// <summary></summary>
        public enum TsLayoutCode
        { 
            /// <summary></summary>
            TS_LC_CREATE = 0,
            /// <summary></summary>
            TS_LC_CHANGE = 1,
            /// <summary></summary>
            TS_LC_DESTROY = 2
        }

        /// <summary></summary>
        public enum TfGravity
        {
            /// <summary></summary>
            TF_GR_BACKWARD = 0,
            /// <summary></summary>
            TF_GR_FORWARD = 1,
        };

        /// <summary></summary>
        public enum TfShiftDir
        {
            /// <summary></summary>
            TF_SD_BACKWARD = 0,
            /// <summary></summary>
            TF_SD_FORWARD = 1,
        };

        /// <summary></summary>
        public enum TfAnchor
        {
            /// <summary></summary>
            TF_ANCHOR_START = 0,
            /// <summary></summary>
            TF_ANCHOR_END = 1,
        }

        /// <summary></summary>
        public enum TF_DA_COLORTYPE
        {
            /// <summary></summary>
            TF_CT_NONE     = 0,
            /// <summary></summary>
            TF_CT_SYSCOLOR = 1,
            /// <summary></summary>
            TF_CT_COLORREF = 2
        }

        /// <summary></summary>
        public enum TF_DA_LINESTYLE
        {
            /// <summary></summary>
            TF_LS_NONE     = 0,
            /// <summary></summary>
            TF_LS_SOLID    = 1,
            /// <summary></summary>
            TF_LS_DOT      = 2,
            /// <summary></summary>
            TF_LS_DASH     = 3,
            /// <summary></summary>
            TF_LS_SQUIGGLE = 4
        }
        
        /// <summary></summary>
        public enum TF_DA_ATTR_INFO
        {
            /// <summary></summary>
            TF_ATTR_INPUT                 =  0,
            /// <summary></summary>
            TF_ATTR_TARGET_CONVERTED      =  1,
            /// <summary></summary>
            TF_ATTR_CONVERTED             =  2,
            /// <summary></summary>
            TF_ATTR_TARGET_NOTCONVERTED   =  3,
            /// <summary></summary>
            TF_ATTR_INPUT_ERROR           =  4,
            /// <summary></summary>
            TF_ATTR_FIXEDCONVERTED        =  5,
            /// <summary></summary>
            TF_ATTR_OTHER                 =  -1
        }
        
        /// <summary></summary>
        [Flags]
        public enum GetRenderingMarkupFlags
        {
            /// <summary></summary>
            TF_GRM_INCLUDE_PROPERTY = 1
        }

        /// <summary></summary>
        [Flags]
        public enum FindRenderingMarkupFlags
        {
            /// <summary></summary>
            TF_FRM_INCLUDE_PROPERTY = 0x1,
            /// <summary></summary>
            TF_FRM_BACKWARD         = 0x2,
            /// <summary></summary>
            TF_FRM_NO_CONTAINED     = 0x4,
            /// <summary></summary>
            TF_FRM_NO_RANGE         = 0x8
        }

        /// <summary></summary>
        [Flags]
        public enum ConversionModeFlags
        {
            /// <summary></summary>
            TF_CONVERSIONMODE_ALPHANUMERIC        = 0x0000,
            /// <summary></summary>
            TF_CONVERSIONMODE_NATIVE              = 0x0001,
            /// <summary></summary>
            TF_CONVERSIONMODE_KATAKANA            = 0x0002,
            /// <summary></summary>
            TF_CONVERSIONMODE_FULLSHAPE           = 0x0008,
            /// <summary></summary>
            TF_CONVERSIONMODE_ROMAN               = 0x0010,
            /// <summary></summary>
            TF_CONVERSIONMODE_CHARCODE            = 0x0020,
            /// <summary></summary>
            TF_CONVERSIONMODE_NOCONVERSION        = 0x0100,
            /// <summary></summary>
            TF_CONVERSIONMODE_EUDC                = 0x0200,
            /// <summary></summary>
            TF_CONVERSIONMODE_SYMBOL              = 0x0400,
            /// <summary></summary>
            TF_CONVERSIONMODE_FIXED               = 0x0800,
        }

        /// <summary></summary>
        [Flags]
        public enum SentenceModeFlags
        {
            /// <summary></summary>
            TF_SENTENCEMODE_NONE                  = 0x0000,
            /// <summary></summary>
            TF_SENTENCEMODE_PLAURALCLAUSE         = 0x0001,
            /// <summary></summary>
            TF_SENTENCEMODE_SINGLECONVERT         = 0x0002,
            /// <summary></summary>
            TF_SENTENCEMODE_AUTOMATIC             = 0x0004,
            /// <summary></summary>
            TF_SENTENCEMODE_PHRASEPREDICT         = 0x0008,
            /// <summary></summary>
            TF_SENTENCEMODE_CONVERSATION          = 0x0010,
        }

        /// <summary></summary>
        public enum  TfCandidateResult
        {
            CAND_FINALIZED = 0x0,
            CAND_SELECTED  = 0x1,
            CAND_CANCELED  = 0x2,
        }

        #endregion Enums

        //------------------------------------------------------
        //
        //  Structs
        //
        //------------------------------------------------------

        #region Structs

        /// <summary></summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            /// <summary>
            /// 
            /// </summary>
            public int x;
            /// <summary>
            /// 
            /// </summary>
            public int y;
        }

        /// <summary></summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT 
        {
            /// <summary></summary>
            public int left;
            /// <summary></summary>
            public int top;
            /// <summary></summary>
            public int right;
            /// <summary></summary>
            public int bottom;

/*
            /// <summary></summary>
            public static RECT FromXYWH(int x, int y, int width, int height) 
            {
                return new RECT(x, y, x + width, y + height);
            }
            
            /// <summary></summary>
            public bool IsEmpty
            {
                get
                {
                    return left >= right || top >= bottom;
                }
            }
*/
        }

        /// <summary></summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct TS_STATUS
        {
            /// <summary></summary>
            public DynamicStatusFlags dynamicFlags;
            /// <summary></summary>
            public StaticStatusFlags staticFlags;
        }

        /// <summary></summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct TS_SELECTIONSTYLE
        {
            /// <summary></summary>
            public TsActiveSelEnd ase;
            /// <summary></summary>
            [MarshalAs(UnmanagedType.Bool)] 
            public bool interimChar;
        }

        /// <summary></summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct TS_SELECTION_ACP
        {
            /// <summary></summary>
            public int start;
            /// <summary></summary>
            public int end;
            /// <summary></summary>
            public TS_SELECTIONSTYLE style;
        }

        /// <summary></summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct TS_RUNINFO
        {
            /// <summary></summary>
            public int count;
            /// <summary></summary>
            public TsRunType type;
        }

        /// <summary></summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct TS_TEXTCHANGE
        {
            /// <summary></summary>
            public int start;
            /// <summary></summary>
            public int oldEnd;
            /// <summary></summary>
            public int newEnd;
        }

        /// <summary></summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct TS_ATTRVAL
        {
            /// <summary></summary>
            public Guid attributeId;

            /// <summary></summary>
            public Int32 overlappedId;

            // Let val's offset 0x18. Though default pack is 8...
            /// <summary></summary>
            public Int32 reserved; 

            /// <summary> </summary>
            [MarshalAs(UnmanagedType.Struct)]
            public NativeMethods.VARIANT val;
        }

        /// <summary></summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct TF_PRESERVEDKEY
        {
            /// <summary></summary>
            public int vKey;
            /// <summary></summary>
            public int modifiers;
        }

        /// <summary></summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct TF_DA_COLOR
        {
            /// <summary></summary>
            public TF_DA_COLORTYPE type;
            /// <summary></summary>
            public Int32 indexOrColorRef; // TF_CT_SYSCOLOR/TF_CT_COLORREF union
        }
        
        /// <summary></summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct TF_DISPLAYATTRIBUTE
        {
            /// <summary></summary>
            public TF_DA_COLOR     crText;
            /// <summary></summary>
            public TF_DA_COLOR     crBk;
            /// <summary></summary>
            public TF_DA_LINESTYLE lsStyle;
            /// <summary></summary>
            [MarshalAs(UnmanagedType.Bool)]
            public bool            fBoldLine;
            /// <summary></summary>
            public TF_DA_COLOR     crLine;
            /// <summary></summary>
            public TF_DA_ATTR_INFO bAttr;     
        }

        /// <summary></summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct TF_RENDERINGMARKUP
        {
            /// <summary></summary>
            ///<SecurityNote>
            /// Critical:  Field to critical type ITfRange
            ///</SecurityNote>
            [SecurityCritical]
            public ITfRange range;
            /// <summary></summary>
            public TF_DISPLAYATTRIBUTE tfDisplayAttr;
        }

        /// <summary></summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct TF_LANGUAGEPROFILE
        {
            internal Guid clsid;        // CLSID of tip
            internal short langid;      // language id
            internal Guid catid;         // category of tip
            [MarshalAs(UnmanagedType.Bool)]
            internal bool fActive;       // activated profile
            internal Guid guidProfile;   // profile description
        }


        #endregion Structs

        //------------------------------------------------------
        //
        //  Interfaces
        //
        //------------------------------------------------------

        #region Interfaces

        ///<SecurityNote>
        ///     Critical - performs an elevation.
        ///</SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("8f1b8ad8-0b6b-4874-90c5-bd76011e8f7c")]
        [System.Security.SuppressUnmanagedCodeSecurity]
        internal interface ITfMessagePump
        {
            //HRESULT PeekMessageA([out] LPMSG pMsg,
            //                     [in] HWND hwnd,
            //                     [in] UINT wMsgFilterMin,
            //                     [in] UINT wMsgFilterMax,
            //                     [in] UINT wRemoveMsg,
            //                     [out] BOOL *pfResult);
            /// <summary></summary>
            /// <SecurityNote>
            ///    Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void PeekMessageA(ref System.Windows.Interop.MSG msg,
                IntPtr hwnd,
                int msgFilterMin,
                int msgFilterMax,
                int removeMsg,
                out int result);

            //HRESULT GetMessageA([out] LPMSG pMsg,
            //                    [in] HWND hwnd,
            //                    [in] UINT wMsgFilterMin,
            //                    [in] UINT wMsgFilterMax,
            //                    [out] BOOL *pfResult);
            /// <summary></summary>
            /// <SecurityNote>
            ///    Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void GetMessageA(ref System.Windows.Interop.MSG msg,
                IntPtr hwnd,
                int msgFilterMin,
                int msgFilterMax,
                out int result);

            //HRESULT PeekMessageW([out] LPMSG pMsg,
            //                     [in] HWND hwnd,
            //                     [in] UINT wMsgFilterMin,
            //                     [in] UINT wMsgFilterMax,
            //                     [in] UINT wRemoveMsg,
            //                     [out] BOOL *pfResult);
            /// <summary></summary>
            /// <SecurityNote>
            ///    Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void PeekMessageW(ref System.Windows.Interop.MSG msg,
                IntPtr hwnd,
                int msgFilterMin,
                int msgFilterMax,
                int removeMsg,
                out int result);

            //HRESULT GetMessageW([out] LPMSG pMsg,
            //                    [in] HWND hwnd,
            //                    [in] UINT wMsgFilterMin,
            //                    [in] UINT wMsgFilterMax,
            //                    [out] BOOL *pfResult);
            /// <summary></summary>
            /// <SecurityNote>
            ///    Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void GetMessageW(ref System.Windows.Interop.MSG msg,
                IntPtr hwnd,
                int msgFilterMin,
                int msgFilterMax,
                out int result);
        };

        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical: This code calls into an unmanaged COM function which is not
        ///     safe since it elevates
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [SuppressUnmanagedCodeSecurity]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("e2449660-9542-11d2-bf46-00105a2799b5")]
        public interface ITfProperty /* : ITfReadOnlyProperty */
        {
            /// <summary></summary>
            //HRESULT GetType([out] GUID *pguid);
            void GetType(out Guid type);

            /// <summary></summary>
            //HRESULT EnumRanges([in] TfEditCookie ec,
            //                [out] IEnumTfRanges **ppEnum,
            //                [in] ITfRange *pTargetRange);
            [PreserveSig]
            int EnumRanges(int editcookie, out IEnumTfRanges ranges, ITfRange targetRange);

            /// <summary></summary>
            //HRESULT GetValue([in] TfEditCookie ec,
            //                [in] ITfRange *pRange,
            //                [out] VARIANT *pvarValue);
            void GetValue(int editCookie, ITfRange range, out object value);

            /// <summary></summary>
            //HRESULT GetContext([out] ITfContext **ppContext);
            void GetContext(out ITfContext context);

            /// <summary></summary>
            //HRESULT FindRange([in] TfEditCookie ec,
            //                [in] ITfRange *pRange,
            //                [out] ITfRange **ppRange,
            //                [in] TfAnchor aPos);
            void FindRange(int editCookie, ITfRange inRange, out ITfRange outRange, TfAnchor position);

            /// <summary></summary>
            //HRESULT SetValueStore([in] TfEditCookie ec,
            //                    [in] ITfRange *pRange,
            //                    [in] ITfPropertyStore *pPropStore);
            void stub_SetValueStore();

            /// <summary></summary>
            //HRESULT SetValue([in] TfEditCookie ec,
            //                [in] ITfRange *pRange,
            //                [in] const VARIANT *pvarValue);
            void SetValue(int editCookie, ITfRange range, object value);

            /// <summary></summary>
            //HRESULT Clear([in] TfEditCookie ec,
            //            [in] ITfRange *pRange);
            void Clear(int editCookie, ITfRange range);
        }

        /// <summary></summary>
        ///<SecurityNote>
        ///     Critical - performs an elevation.
        ///</SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [SuppressUnmanagedCodeSecurity]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("aa80e7fd-2021-11d2-93e0-0060b067b86e")]
        public interface ITfContext
        {
            //const DWORD TF_ES_ASYNCDONTCARE   = 0x0;
            //const DWORD TF_ES_SYNC            = 0x1;
            //const DWORD TF_ES_READ            = 0x2;
            //const DWORD TF_ES_READWRITE       = 0x6;
            //const DWORD TF_ES_ASYNC           = 0x8;

            /// <summary></summary>
            //HRESULT RequestEditSession([in] TfClientId tid,
            //                        [in] ITfEditSession *pes,
            //                        [in] DWORD dwFlags,
            //                        [out] HRESULT *phrSession);
            int stub_RequestEditSession();

            /// <summary></summary>
            //HRESULT InWriteSession([in] TfClientId tid,
            //                    [out] BOOL *pfWriteSession);
            void InWriteSession(int clientId, [MarshalAs(UnmanagedType.Bool)] out bool inWriteSession);

            //typedef [uuid(1690be9b-d3e9-49f6-8d8b-51b905af4c43)] enum { TF_AE_NONE = 0, TF_AE_START = 1, TF_AE_END = 2 } TfActiveSelEnd;

            //typedef [uuid(36ae42a4-6989-4bdc-b48a-6137b7bf2e42)] struct TF_SELECTIONSTYLE
            //{
            //    TfActiveSelEnd ase;
            //    BOOL fInterimChar;
            //} TF_SELECTIONSTYLE;

            //typedef [uuid(75eb22f2-b0bf-46a8-8006-975a3b6efcf1)] struct TF_SELECTION
            //{
            //    ITfRange *range;
            //    TF_SELECTIONSTYLE style;
            //} TF_SELECTION;

            //const ULONG TF_DEFAULT_SELECTION = TS_DEFAULT_SELECTION;

            /// <summary></summary>
            //HRESULT GetSelection([in] TfEditCookie ec,
            //                    [in] ULONG ulIndex,
            //                    [in] ULONG ulCount,
            //                    [out, size_is(ulCount), length_is(*pcFetched)] TF_SELECTION *pSelection,
            //                    [out] ULONG *pcFetched);
            void stub_GetSelection();

            /// <summary></summary>
            //HRESULT SetSelection([in] TfEditCookie ec, 
            //                    [in] ULONG ulCount,
            //                    [in, size_is(ulCount)] const TF_SELECTION *pSelection);
            void stub_SetSelection();

            //HRESULT GetStart([in] TfEditCookie ec,
            //                [out] ITfRange **ppStart);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            void GetStart(int ec, out ITfRange range);

            /// <summary></summary>
            //HRESULT GetEnd([in] TfEditCookie ec,
            //            [out] ITfRange **ppEnd);
            void stub_GetEnd();

            // bit values for TF_STATUS's dwDynamicFlags field
            //const DWORD TF_SD_READONLY        = TS_SD_READONLY;       // if set, document is read only; writes will fail
            //const DWORD TF_SD_LOADING         = TS_SD_LOADING;        // if set, document is loading, expect additional inserts
            // bit values for TF_STATUS's dwStaticFlags field
            //const DWORD TF_SS_DISJOINTSEL     = TS_SS_DISJOINTSEL;    // if set, the document supports multiple selections
            //const DWORD TF_SS_REGIONS         = TS_SS_REGIONS;        // if clear, the document will never contain multiple regions
            //const DWORD TF_SS_TRANSITORY      = TS_SS_TRANSITORY;     // if set, the document is expected to have a short lifespan

            //typedef [uuid(bc7d979a-846a-444d-afef-0a9bfa82b961)] TS_STATUS TF_STATUS;

            /// <summary></summary>
            //HRESULT GetActiveView([out] ITfContextView **ppView);
            void stub_GetActiveView();

            /// <summary></summary>
            //HRESULT EnumViews([out] IEnumTfContextViews **ppEnum);
            void stub_EnumViews();
            
            /// <summary></summary>
            //HRESULT GetStatus([out] TF_STATUS *pdcs);
            void stub_GetStatus();

            //HRESULT GetProperty([in] REFGUID guidProp,
            //                    [out] ITfProperty **ppProp);
            /// <SecurityNote>
            ///     Critical: COM interop call
            /// </SecurityNote>
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            void GetProperty(ref Guid guid, out ITfProperty property);

            /// <summary></summary>
            //HRESULT GetAppProperty([in] REFGUID guidProp,
            //                    [out] ITfReadOnlyProperty **ppProp);
            void stub_GetAppProperty();

            /// <summary></summary>
            //HRESULT TrackProperties([in, size_is(cProp)] const GUID **prgProp,
            //                        [in] ULONG cProp,
            //                        [in, size_is(cAppProp)] const GUID **prgAppProp,
            //                        [in] ULONG cAppProp,   
            //                        [out] ITfReadOnlyProperty **ppProperty);
            void stub_TrackProperties();

            /// <summary></summary>
            //HRESULT EnumProperties([out] IEnumTfProperties **ppEnum);
            void stub_EnumProperties();

            /// <summary></summary>
            //HRESULT GetDocumentMgr([out] ITfDocumentMgr **ppDm);
            void stub_GetDocumentMgr();

            /// <summary></summary>
            //HRESULT CreateRangeBackup([in] TfEditCookie ec,
            //                        [in] ITfRange *pRange,
            //                        [out] ITfRangeBackup **ppBackup);
            void stub_CreateRangeBackup();
        }

        /// <summary></summary>
        ///<SecurityNote>
        ///     Critical - performs an elevation.
        ///</SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("aa80e7f4-2021-11d2-93e0-0060b067b86e")]
        [SuppressUnmanagedCodeSecurity]
        public interface ITfDocumentMgr
        {
            // <summary></summary>
            //HRESULT CreateContext([in] TfClientId tidOwner,
            //                      [in] DWORD dwFlags,
            //                      [in, unique] IUnknown *punk,
            //                      [out] ITfContext **ppic,
            //                      [out] TfEditCookie *pecTextStore);
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            void CreateContext(int clientId, CreateContextFlags flags, [MarshalAs(UnmanagedType.Interface)] object obj, out ITfContext context, out int editCookie);

            // <summary></summary>
            //HRESULT Push([in] ITfContext *pic);
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            void Push(ITfContext context);

            // <summary></summary>
            //HRESULT Pop([in] DWORD dwFlags);
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            void Pop(PopFlags flags);

            /// <summary></summary>
            //HRESULT GetTop([out] ITfContext **ppic);
            void GetTop(out ITfContext context);

            //HRESULT GetBase([out] ITfContext **ppic);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            void GetBase(out ITfContext context);

            /// <summary></summary>
            //HRESULT EnumContexts([out] IEnumTfContexts **ppEnum);
            void EnumContexts([MarshalAs(UnmanagedType.Interface)] out /*IEnumTfContexts*/ object enumContexts);
        }

        /// <summary></summary>
        ///<SecurityNote>
        ///     Critical - performs an elevation.
        ///</SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("aa80e808-2021-11d2-93e0-0060b067b86e")]
        [SuppressUnmanagedCodeSecurity]
        public interface IEnumTfDocumentMgrs
        {
        }

        /// <summary></summary>
        ///<SecurityNote>
        ///     Critical - performs an elevation.
        ///</SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("101d6610-0990-11d3-8df0-00105a2799b5")]
        [SuppressUnmanagedCodeSecurity]
        public interface ITfFunctionProvider
        {
            /// <summary></summary>
            //HRESULT GetType([out] GUID *pguid);
            void GetType(out Guid guid);

            /// <summary></summary>
            //HRESULT GetDescription([out] BSTR *pbstrDesc);
            void GetDescription([MarshalAs(UnmanagedType.BStr)] out string desc);

            // HRESULT GetFunction([in] REFGUID rguid,
            //                    [in] REFIID riid,
            //                    [out, iid_is(riid)] IUnknown **ppunk);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            [PreserveSig]
            int GetFunction(ref Guid guid, ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object obj);
        }

        /// <summary></summary>
        ///<SecurityNote>
        ///     Critical - performs an elevation.
        ///</SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("db593490-098f-11d3-8df0-00105a2799b5")]
        [SuppressUnmanagedCodeSecurity]
        public interface ITfFunction
        {
            /// <summary></summary>
            //HRESULT GetDisplayName([out] BSTR *pbstrName);
            void GetDisplayName([MarshalAs(UnmanagedType.BStr)] out string funcName );
        }

        /// <summary></summary>
        ///<SecurityNote>
        ///     Critical - performs an elevation.
        ///</SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("581f317e-fd9d-443f-b972-ed00467c5d40")]
        [SuppressUnmanagedCodeSecurity]
        public interface ITfCandidateString
        {
            // HRESULT GetString([out] BSTR *pbstr);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            void GetString([MarshalAs(UnmanagedType.BStr)] out string funcName );

            /// <summary></summary>
            // HRESULT GetIndex([out] ULONG *pnIndex);
            void GetIndex(out int nIndex );
        }

        /// <summary></summary>
        ///<SecurityNote>
        ///     Critical - performs an elevation.
        ///</SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("a3ad50fb-9bdb-49e3-a843-6c76520fbf5d")]
        [SuppressUnmanagedCodeSecurity]
        public interface ITfCandidateList
        {
            /// <summary></summary>
            // HRESULT EnumCandidates([out] IEnumTfCandidates **ppEnum);
            void EnumCandidates(out object enumCand);

            // HRESULT GetCandidate([in] ULONG nIndex,
            //                      [out] ITfCandidateString **ppCand);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]            
            void GetCandidate(int nIndex, out ITfCandidateString candstring);

            // HRESULT GetCandidateNum([out] ULONG *pnCnt);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            void GetCandidateNum(out int nCount);


            // HRESULT SetResult([in] ULONG nIndex,
            //                   [in] TfCandidateResult imcr);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            void SetResult(int nIndex, TfCandidateResult result);
        }


        /// <summary></summary>
        ///<SecurityNote>
        ///     Critical - performs an elevation.
        ///</SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("4cea93c0-0a58-11d3-8df0-00105a2799b5")]
        [SuppressUnmanagedCodeSecurity]
        public interface ITfFnReconversion
        {
            /// <summary></summary>
            // HRESULT GetDisplayName([out] BSTR *pbstrName);
            void GetDisplayName([MarshalAs(UnmanagedType.BStr)] out string funcName );

            // HRESULT QueryRange([in] ITfRange *pRange,
            //                    [in, out, unique] ITfRange **ppNewRange,
            //                    [out] BOOL *pfConvertable);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            [PreserveSig]
            int QueryRange(ITfRange range, 
                           out ITfRange newRange, 
                           [MarshalAs(UnmanagedType.Bool)] out bool isConvertable);

            // HRESULT GetReconversion([in] ITfRange *pRange,
            //                         [out] ITfCandidateList **ppCandList);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            [PreserveSig]
            int GetReconversion(ITfRange range, out ITfCandidateList candList);

            /// <summary></summary>
            /// HRESULT Reconvert([in] ITfRange *pRange);
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            [PreserveSig]
            int Reconvert(ITfRange range);
        }

        /// <summary></summary>
        ///<SecurityNote>
        ///     Critical - performs an elevation.
        ///</SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("88f567c6-1757-49f8-a1b2-89234c1eeff9")]
        [SuppressUnmanagedCodeSecurity]
        public interface ITfFnConfigure
        {
            /// <summary></summary>
            // HRESULT GetDisplayName([out] BSTR *pbstrName);
            void GetDisplayName([MarshalAs(UnmanagedType.BStr)] out string funcName );

            /// <summary></summary>
            // HRESULT Show([in] HWND hwndParent,
            //              [in] LANGID langid,
            //              [in] REFGUID rguidProfile);
            [PreserveSig]
            int Show(IntPtr hwndParent, short langid, ref Guid guidProfile);
        }

        /// <summary></summary>
        ///<SecurityNote>
        ///     Critical - performs an elevation.
        ///</SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("bb95808a-6d8f-4bca-8400-5390b586aedf")]
        [SuppressUnmanagedCodeSecurity]
        public interface ITfFnConfigureRegisterWord
        {
            /// <summary></summary>
            // HRESULT GetDisplayName([out] BSTR *pbstrName);
            void GetDisplayName([MarshalAs(UnmanagedType.BStr)] out string funcName );

            /// <summary></summary>
            // HRESULT Show([in] HWND hwndParent,
            //              [in] LANGID langid,
            //              [in] REFGUID rguidProfile,
            //              [in, unique] BSTR bstrRegistered);
            [PreserveSig]
            int Show(IntPtr hwndParent, 
                     short langid, 
                     ref Guid guidProfile,
                     [MarshalAs(UnmanagedType.BStr)] string bstrRegistered );
        }


        #region SpeechCommands

#if UNUSED
        /// <summary></summary>
        //
        // Speech command provider and related interfaces
        //
        ///<SecurityNote>
        ///     Critical - performs an elevation.
        ///</SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("8c5dac4f-083c-4b85-a4c9-71746048adca")]
        [SuppressUnmanagedCodeSecurity]
        public interface IEnumSpeechCommands
        {
            /// <summary></summary>
            //HRESULT Clone([out] IEnumSpeechCommands **ppEnum);
            void Clone([MarshalAs(UnmanagedType.Interface)] out object obj);

            /// <summary></summary>
            //HRESULT Next([in] ULONG ulCount,
            //            [out, size_is(ulCount), length_is(*pcFetched)] WCHAR **pSpCmds,
            //            [out] ULONG *pcFetched);
            [PreserveSig]
            int Next(int count, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] string [] spCmds, out int fetched);

            /// <summary></summary>
            //HRESULT Reset();
            void Reset();

            /// <summary></summary>
            //HRESULT Skip(ULONG ulCount);
            [PreserveSig]
            int Skip(int count);
        }

        /// <summary></summary>
        ///<SecurityNote>
        ///     Critical - performs an elevation.
        ///</SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("38e09d4c-586d-435a-b592-c8a86691dec6")]
        [SuppressUnmanagedCodeSecurity]
        public interface ISpeechCommandProvider
        {
            /// <summary></summary>
            //HRESULT  EnumSpeechCommands([in]  LANGID  langid, [out] IEnumSpeechCommands **ppEnum);
            void EnumSpeechCommands(short langid, [MarshalAs(UnmanagedType.Interface)] out object obj);

            /// <summary></summary>
            //HRESULT  ProcessCommand([in, size_is(cch)] const WCHAR *pszCommand,
            //                        [in] ULONG cch, 
            //                        [in] LANGID langid);
            void ProcessCommand([In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] char []command, int cch, short langid);
        }

        /// <summary></summary>
        ///<SecurityNote>
        ///     Critical - performs an elevation.
        ///</SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("fca6c349-a12f-43a3-8dd6-5a5a4282577b")]
        [SuppressUnmanagedCodeSecurity]
        public interface ITfFnCustomSpeechCommand 
        {
            /// <summary></summary>
            // ITfFunction method
            void GetDisplayName([MarshalAs(UnmanagedType.BStr)] out string funcName );

            /// <summary></summary>
            //HRESULT SetSpeechCommandProvider([in] ISpeechCommandProvider *pspcmdProvider);
            void SetSpeechCommandProvider([MarshalAs(UnmanagedType.Interface)] object obj /*ISpeechCommandProvider spcmdProvider*/);
        }

#endif // UNUSED

        #endregion SpeechCommands

        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("e4b24db0-0990-11d3-8df0-00105a2799b5")]
        [SuppressUnmanagedCodeSecurity]
        public interface IEnumTfFunctionProviders
        {
        }

        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [SuppressUnmanagedCodeSecurity]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("bb08f7a9-607a-4384-8623-056892b64371")]
        public interface ITfCompartment
        {
            // <summary></summary>
            //HRESULT SetValue([in] TfClientId tid,
            //                 [in] const VARIANT *pvarValue);
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            [SecurityCritical]
            [PreserveSig]
            int SetValue(int tid, ref object varValue);
        
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            [SecurityCritical]
            void GetValue(out object varValue);
        }
        
        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("743abd5f-f26d-48df-8cc5-238492419b64")]
        [SuppressUnmanagedCodeSecurity]
        public interface ITfCompartmentEventSink
        {
            /// <summary></summary>
            //HRESULT OnChange([in] REFGUID rguid);
            void OnChange(ref Guid rguid);
        }
        
        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("7dcf57ac-18ad-438b-824d-979bffb74b7c")]
        [SuppressUnmanagedCodeSecurity]
        public interface ITfCompartmentMgr
        {
            // <summary></summary>
            //HRESULT GetCompartment([in] REFGUID rguid,
            //                       [out] ITfCompartment **ppcomp);
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            void GetCompartment(ref Guid guid, out ITfCompartment comp);

            /// <summary></summary>
            //HRESULT ClearCompartment([in] TfClientId tid,
            //                        [in] REFGUID rguid);
            void ClearCompartment(int tid, Guid guid);

            /// <summary></summary>
            //HRESULT EnumCompartments([out] IEnumGUID **ppEnum);
            void EnumCompartments(out object /*IEnumGUID*/ enumGuid);
        }
        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("aa80e801-2021-11d2-93e0-0060b067b86e")]
        [SuppressUnmanagedCodeSecurity]
        internal interface ITfThreadMgr
        {
            // <summary></summary>
            //HRESULT Activate([out] TfClientId *ptid);
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            void Activate(out int clientId);

            // <summary></summary>
            //HRESULT Deactivate();
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            void Deactivate();

            // <summary></summary>
            //HRESULT CreateDocumentMgr([out] ITfDocumentMgr **ppdim);
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            void CreateDocumentMgr(out ITfDocumentMgr docMgr);

            /// <summary></summary>
            //HRESULT EnumDocumentMgrs([out] IEnumTfDocumentMgrs **ppEnum);
            void EnumDocumentMgrs(out IEnumTfDocumentMgrs enumDocMgrs);

            /// <summary></summary>
            //HRESULT GetFocus([out] ITfDocumentMgr **ppdimFocus);
            void GetFocus(out ITfDocumentMgr docMgr);

            // <summary></summary>
            //HRESULT SetFocus([in] ITfDocumentMgr *pdimFocus);
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            void SetFocus(ITfDocumentMgr docMgr);

            /// <summary></summary>
            //HRESULT AssociateFocus([in] HWND hwnd,
            //                       [in, unique] ITfDocumentMgr *pdimNew,
            //                       [out] ITfDocumentMgr **ppdimPrev);
            void AssociateFocus(IntPtr hwnd, ITfDocumentMgr newDocMgr, out ITfDocumentMgr prevDocMgr);

            /// <summary></summary>
            //HRESULT IsThreadFocus([out] BOOL *pfThreadFocus);
            void IsThreadFocus([MarshalAs(UnmanagedType.Bool)] out bool isFocus);

            //HRESULT GetFunctionProvider([in] REFCLSID clsid,
            //                            [out] ITfFunctionProvider **ppFuncProv);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            [PreserveSig]
            int GetFunctionProvider(ref Guid classId, out ITfFunctionProvider funcProvider);

            /// <summary></summary>
            //HRESULT EnumFunctionProviders([out] IEnumTfFunctionProviders **ppEnum);
            void EnumFunctionProviders(out IEnumTfFunctionProviders enumProviders);

            //HRESULT GetGlobalCompartment([out] ITfCompartmentMgr **ppCompMgr);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            void GetGlobalCompartment(out ITfCompartmentMgr compartmentMgr);
        }


        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("28888fe3-c2a0-483a-a3ea-8cb1ce51ff3d")]
        [SuppressUnmanagedCodeSecurity]
        public interface ITextStoreACP
        {
            /// <summary></summary>
            //HRESULT AdviseSink([in] REFIID riid,
            //                   [in, iid_is(riid)] IUnknown *punk,
            //                   [in] DWORD dwMask);
            void AdviseSink(ref Guid riid, [MarshalAs(UnmanagedType.Interface)] object obj, AdviseFlags flags);

            /// <summary></summary>
            //HRESULT UnadviseSink([in] IUnknown *punk);
            void UnadviseSink([MarshalAs(UnmanagedType.Interface)] object obj);
            
            /// <summary></summary>
            //HRESULT RequestLock([in] DWORD dwLockFlags,
            //                    [out] HRESULT *phrSession);
            void RequestLock(LockFlags flags, out int hrSession);

            /// <summary></summary>
            //HRESULT GetStatus([out] TS_STATUS *pdcs);
            void GetStatus(out TS_STATUS status);

            /// <summary></summary>
            //HRESULT QueryInsert([in] LONG acpTestStart,
            //                    [in] LONG acpTestEnd,
            //                    [in] ULONG cch,
            //                    [out] LONG *pacpResultStart,
            //                    [out] LONG *pacpResultEnd);
            void QueryInsert(int start, int end, int cch, out int startResult, out int endResult);

            /// <summary></summary>
            //HRESULT GetSelection([in] ULONG ulIndex,
            //                     [in] ULONG ulCount,
            //                     [out, size_is(ulCount), length_is(*pcFetched)] TS_SELECTION_ACP *pSelection,
            //                     [out] ULONG *pcFetched);
            void GetSelection(int index, int count, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] TS_SELECTION_ACP []selection, out int fetched);

            /// <summary></summary>
            //HRESULT SetSelection([in] ULONG ulCount,
            //                     [in, size_is(ulCount)] const TS_SELECTION_ACP *pSelection);
            void SetSelection(int count, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] TS_SELECTION_ACP []selection);

            /// <summary></summary>
            //HRESULT GetText([in] LONG acpStart,
            //                [in] LONG acpEnd,
            //                [out, size_is(cchPlainReq), length_is(*pcchPlainRet)] WCHAR *pchPlain,
            //                [in] ULONG cchPlainReq,
            //                [out] ULONG *pcchPlainRet,
            //                [out, size_is(cRunInfoReq), length_is(*pcRunInfoRet)] TS_RUNINFO *prgRunInfo,
            //                [in] ULONG cRunInfoReq,
            //                [out] ULONG *pcRunInfoRet,
            //                [out] LONG *pacpNext);
            void GetText(int start, int end,
                [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] char []text,
                int cchReq, out int charsCopied, 
                [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=6)] TS_RUNINFO []runInfo,
                int cRunInfoReq, out int cRunInfoRcv,
                out int nextCp);

            /// <summary></summary>
            //HRESULT SetText([in] DWORD dwFlags,
            //                [in] LONG acpStart,
            //                [in] LONG acpEnd,
            //                [in, size_is(cch)] const WCHAR *pchText,
            //                [in] ULONG cch,
            //                [out] TS_TEXTCHANGE *pChange);
            void SetText(SetTextFlags flags, int start, int end,
                [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] char []text,
                int cch, out TS_TEXTCHANGE change);

            /// <summary></summary>
            //HRESULT GetFormattedText([in] LONG acpStart,
            //                         [in] LONG acpEnd,
            //                         [out] IDataObject **ppDataObject);
            void GetFormattedText(int start, int end, [MarshalAs(UnmanagedType.Interface)] out object obj);

            /// <summary></summary>
            //HRESULT GetEmbedded([in] LONG acpPos,
            //                    [in] REFGUID rguidService,
            //                    [in] REFIID riid,
            //                    [out, iid_is(riid)] IUnknown **ppunk);
            void GetEmbedded(int position, ref Guid guidService, ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object obj);

            /// <summary></summary>
            //HRESULT QueryInsertEmbedded([in] const GUID *pguidService,
            //                            [in] const FORMATETC *pFormatEtc,
            //                            [out] BOOL *pfInsertable);
            void QueryInsertEmbedded(ref Guid guidService, int /*ref Win32.FORMATETC*/ formatEtc, [MarshalAs(UnmanagedType.Bool)] out bool insertable);

            /// <summary></summary>
            //HRESULT InsertEmbedded([in] DWORD dwFlags,
            //                       [in] LONG acpStart,
            //                       [in] LONG acpEnd,
            //                       [in] IDataObject *pDataObject,
            //                       [out] TS_TEXTCHANGE *pChange);
            void InsertEmbedded(InsertEmbeddedFlags flags, int start, int end, [MarshalAs(UnmanagedType.Interface)] object obj, out TS_TEXTCHANGE change);

            /// <summary></summary>
            //HRESULT InsertTextAtSelection([in] DWORD dwFlags,
            //                              [in, size_is(cch)] const WCHAR *pchText,
            //                              [in] ULONG cch,
            //                              [out] LONG *pacpStart,
            //                              [out] LONG *pacpEnd,
            //                              [out] TS_TEXTCHANGE *pChange);
            void InsertTextAtSelection(InsertAtSelectionFlags flags,
                [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] char []text,
                int cch,
                out int start, out int end, out TS_TEXTCHANGE change);

            /// <summary></summary>
            //HRESULT InsertEmbeddedAtSelection([in] DWORD dwFlags,
            //                                  [in] IDataObject *pDataObject,
            //                                  [out] LONG *pacpStart,
            //                                  [out] LONG *pacpEnd,
            //                                  [out] TS_TEXTCHANGE *pChange);
            void InsertEmbeddedAtSelection(InsertAtSelectionFlags flags, [MarshalAs(UnmanagedType.Interface)] object obj,
                                        out int start, out int end, out TS_TEXTCHANGE change);

            /// <summary></summary>
            //HRESULT RequestSupportedAttrs([in] DWORD dwFlags,
            //                              [in] ULONG cFilterAttrs,
            //                              [in, size_is(cFilterAttrs), unique] const TS_ATTRID *paFilterAttrs);
            [PreserveSig]
            int RequestSupportedAttrs(AttributeFlags flags, int count,
                [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] Guid []filterAttributes);

            /// <summary></summary>
            //HRESULT RequestAttrsAtPosition([in] LONG acpPos,
            //                               [in] ULONG cFilterAttrs,
            //                               [in, size_is(cFilterAttrs), unique] const TS_ATTRID *paFilterAttrs,
            //                               [in] DWORD dwFlags);
            [PreserveSig]
            int RequestAttrsAtPosition(int position, int count,
                [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] Guid []filterAttributes,
                AttributeFlags flags);

            /// <summary></summary>
            //HRESULT RequestAttrsTransitioningAtPosition([in] LONG acpPos,
            //                                            [in] ULONG cFilterAttrs,
            //                                            [in, size_is(cFilterAttrs), unique] const TS_ATTRID *paFilterAttrs,
            //                                            [in] DWORD dwFlags);
            void RequestAttrsTransitioningAtPosition(int position, int count,
                [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] Guid []filterAttributes,
                AttributeFlags flags);

            /// <summary></summary>
            //HRESULT FindNextAttrTransition([in] LONG acpStart,
            //                               [in] LONG acpHalt,
            //                               [in] ULONG cFilterAttrs,
            //                               [in, size_is(cFilterAttrs), unique] const TS_ATTRID *paFilterAttrs,
            //                               [in] DWORD dwFlags,
            //                               [out] LONG *pacpNext,
            //                               [out] BOOL *pfFound,
            //                               [out] LONG *plFoundOffset);
            void FindNextAttrTransition(int start, int halt, int count,
                [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] Guid []filterAttributes,
                AttributeFlags flags, out int acpNext, [MarshalAs(UnmanagedType.Bool)] out bool found, out int foundOffset);

            /// <summary></summary>
            //HRESULT RetrieveRequestedAttrs([in] ULONG ulCount,
            //                               [out, size_is(ulCount), length_is(*pcFetched)] TS_ATTRVAL *paAttrVals,
            //                               [out] ULONG *pcFetched);
            void RetrieveRequestedAttrs(int count,
                [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] TS_ATTRVAL []attributeVals,
                out int countFetched);

            /// <summary></summary>
            //HRESULT GetEnd([out] LONG *pacp);
            void GetEnd(out int end);

            /// <summary></summary>
            //HRESULT GetActiveView([out] TsViewCookie *pvcView);
            void GetActiveView(out int viewCookie);

            /// <summary></summary>
            //HRESULT GetACPFromPoint([in] TsViewCookie vcView,
            //                        [in] const POINT *ptScreen,
            //                        [in] DWORD dwFlags, [out] LONG *pacp);
            void GetACPFromPoint(int viewCookie, ref POINT point, GetPositionFromPointFlags flags, out int position);

            /// <summary></summary>
            //HRESULT GetTextExt([in] TsViewCookie vcView,
            //                   [in] LONG acpStart,
            //                   [in] LONG acpEnd,
            //                   [out] RECT *prc,
            //                   [out] BOOL *pfClipped);
            void GetTextExt(int viewCookie, int start, int end, out RECT rect, [MarshalAs(UnmanagedType.Bool)] out bool clipped);

            /// <summary></summary>
            //HRESULT GetScreenExt([in] TsViewCookie vcView,
            //                     [out] RECT *prc);
            void GetScreenExt(int viewCookie, out RECT rect);

            /// <summary></summary>
            //HRESULT GetWnd([in] TsViewCookie vcView,
            //               [out] HWND *phwnd);
            void GetWnd(int viewCookie, out IntPtr hwnd);
        };

        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("22d44c94-a419-4542-a272-ae26093ececf")]
        [SuppressUnmanagedCodeSecurity]
        public interface ITextStoreACPSink
        {
            /// <summary></summary>
            /// <SecurityNote>
            /// Critical - as this has SUC on it.
            /// </SecurityNote>
            //HRESULT OnTextChange([in] DWORD dwFlags,
            //                     [in] const TS_TEXTCHANGE *pChange);
            [SuppressUnmanagedCodeSecurity, SecurityCritical]
            void OnTextChange(OnTextChangeFlags flags, ref TS_TEXTCHANGE change);

            /// <summary></summary>
            /// <SecurityNote>
            /// Critical - as this has SUC on it.
            /// </SecurityNote>
            //HRESULT OnSelectionChange();
            [SuppressUnmanagedCodeSecurity, SecurityCritical]
            void OnSelectionChange();

            /// <summary></summary>
            /// <SecurityNote>
            /// Critical - as this has SUC on it.
            /// </SecurityNote>
            //HRESULT OnLayoutChange([in] TsLayoutCode lcode, [in] TsViewCookie vcView);
            [SuppressUnmanagedCodeSecurity, SecurityCritical]
            void OnLayoutChange(TsLayoutCode lcode, int viewCookie);

            /// <summary></summary>
            /// <SecurityNote>
            /// Critical - as this has SUC on it.
            /// </SecurityNote>
            //HRESULT OnStatusChange([in] DWORD dwFlags);
            [SuppressUnmanagedCodeSecurity, SecurityCritical]
            void OnStatusChange(DynamicStatusFlags flags);

            /// <summary></summary>
            /// <SecurityNote>
            /// Critical - as this has SUC on it.
            /// </SecurityNote>
            //HRESULT OnAttrsChange([in] LONG acpStart,
            //                      [in] LONG acpEnd,
            //                      [in] ULONG cAttrs,
            //                      [in, size_is(cAttrs)] const TS_ATTRID *paAttrs);
            [SuppressUnmanagedCodeSecurity, SecurityCritical]
            void OnAttrsChange(int start, int end, int count, Guid[] attributes);

            /// <summary></summary>
            /// <SecurityNote>
            /// Critical - as this has SUC on it.
            /// </SecurityNote>
            //HRESULT OnLockGranted([in] DWORD dwLockFlags);
            [PreserveSig]
            [SuppressUnmanagedCodeSecurity, SecurityCritical]
            int OnLockGranted(LockFlags flags);

            /// <summary></summary>
            /// <SecurityNote>
            /// Critical - as this has SUC on it.
            /// </SecurityNote>
            //HRESULT OnStartEditTransaction();
            [SuppressUnmanagedCodeSecurity, SecurityCritical]
            void OnStartEditTransaction();

            /// <summary></summary>
            /// <SecurityNote>
            /// Critical - as this has SUC on it.
            /// </SecurityNote>
            //HRESULT OnEndEditTransaction();
            [SuppressUnmanagedCodeSecurity, SecurityCritical]
            void OnEndEditTransaction();
        }

        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("c0f1db0c-3a20-405c-a303-96b6010a885f")]
        [SuppressUnmanagedCodeSecurity]
        public interface ITfThreadFocusSink
        {
            /// <summary></summary>
            //HRESULT OnSetThreadFocus();
            void OnSetThreadFocus();

            /// <summary></summary>
            //HRESULT OnKillThreadFocus();
            void OnKillThreadFocus();
        }

        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("4ea48a35-60ae-446f-8fd6-e6a8d82459f7")]
        [SuppressUnmanagedCodeSecurity]
        public interface ITfSource
        {
            // <summary></summary>
            //HRESULT AdviseSink([in] REFIID riid,
            //                   [in, iid_is(riid)] IUnknown *punk,
            //                   [out] DWORD *pdwCookie);
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            void AdviseSink(ref Guid riid, [MarshalAs(UnmanagedType.Interface)] object obj, out int cookie);

            //HRESULT UnadviseSink([in] DWORD dwCookie);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            void UnadviseSink(int cookie);
        }

        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("aa80e7f0-2021-11d2-93e0-0060b067b86e")]
        [SuppressUnmanagedCodeSecurity]
        public interface ITfKeystrokeMgr
        {
            /// <summary></summary>
            //HRESULT AdviseKeyEventSink([in] TfClientId tid,
            //                           [in] ITfKeyEventSink *pSink,
            //                           [in] BOOL fForeground);
            void AdviseKeyEventSink(int clientId, [MarshalAs(UnmanagedType.Interface)] object obj/*ITfKeyEventSink sink*/, [MarshalAs(UnmanagedType.Bool)] bool fForeground);

            /// <summary></summary>
            //HRESULT UnadviseKeyEventSink([in] TfClientId tid);
            void UnadviseKeyEventSink(int clientId);

            /// <summary></summary>
            //HRESULT GetForeground([out] CLSID *pclsid);
            void GetForeground(out Guid clsid);

            // <summary></summary>
            //HRESULT TestKeyDown([in] WPARAM wParam,
            //                    [in] LPARAM lParam,
            //                    [out] BOOL *pfEaten);
            // int should be ok here, bit fields are well defined for this call as 32 bit, no pointers
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            void TestKeyDown(int wParam, int lParam, [MarshalAs(UnmanagedType.Bool)] out bool eaten);

            // <summary></summary>
            //HRESULT TestKeyUp([in] WPARAM wParam,
            //                  [in] LPARAM lParam,
            //                  [out] BOOL *pfEaten);
            // int should be ok here, bit fields are well defined for this call as 32 bit, no pointers
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            void TestKeyUp(int wParam, int lParam, [MarshalAs(UnmanagedType.Bool)] out bool eaten);

            // <summary></summary>
            //HRESULT KeyDown([in] WPARAM wParam,
            //                [in] LPARAM lParam,
            //                [out] BOOL *pfEaten);
            // int should be ok here, bit fields are well defined for this call as 32 bit, no pointers
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            void KeyDown(int wParam, int lParam, [MarshalAs(UnmanagedType.Bool)] out bool eaten);

            // <summary></summary>
            //HRESULT KeyUp([in] WPARAM wParam,
            //              [in] LPARAM lParam,
            //              [out] BOOL *pfEaten);
            // int should be ok here, bit fields are well defined for this call as 32 bit, no pointers
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            void KeyUp(int wParam, int lParam, [MarshalAs(UnmanagedType.Bool)] out bool eaten);

            /// <summary></summary>
            //HRESULT GetPreservedKey([in] ITfContext *pic,
            //                        [in] const TF_PRESERVEDKEY *pprekey,
            //                        [out] GUID *pguid);
            void GetPreservedKey(ITfContext context, ref TF_PRESERVEDKEY key, out Guid guid);

            /// <summary></summary>
            //HRESULT IsPreservedKey([in] REFGUID rguid,
            //                       [in] const TF_PRESERVEDKEY *pprekey,
            //                       [out] BOOL *pfRegistered);
            void IsPreservedKey(ref Guid guid, ref TF_PRESERVEDKEY key, [MarshalAs(UnmanagedType.Bool)] out bool registered);

            /// <summary></summary>
            //HRESULT PreserveKey([in] TfClientId tid,
            //                    [in] REFGUID rguid,
            //                    [in] const TF_PRESERVEDKEY *prekey,
            //                    [in, size_is(cchDesc)] const WCHAR *pchDesc,
            //                    [in] ULONG cchDesc);
            void PreserveKey(int clientId, ref Guid guid, ref TF_PRESERVEDKEY key,
                            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] char []desc, int descCount);

            /// <summary></summary>
            //HRESULT UnpreserveKey([in] REFGUID rguid, 
            //                      [in] const TF_PRESERVEDKEY *pprekey);
            void UnpreserveKey(ref Guid guid, ref TF_PRESERVEDKEY key);

            /// <summary></summary>
            //HRESULT SetPreservedKeyDescription([in] REFGUID rguid,
            //                                   [in, size_is(cchDesc)] const WCHAR *pchDesc,
            //                                   [in] ULONG cchDesc);
            void SetPreservedKeyDescription(ref Guid guid,
                                            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] char []desc, int descCount);

            /// <summary></summary>
            //HRESULT GetPreservedKeyDescription([in] REFGUID rguid,
            //                                   [out] BSTR *pbstrDesc);
            void GetPreservedKeyDescription(ref Guid guid, [MarshalAs(UnmanagedType.BStr)] out string desc);

            /// <summary></summary>
            //HRESULT SimulatePreservedKey([in] ITfContext *pic,
            //                             [in] REFGUID rguid,
            //                             [out] BOOL *pfEaten);
            void SimulatePreservedKey(ITfContext context, ref Guid guid, [MarshalAs(UnmanagedType.Bool)] out bool eaten);
        };

        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [SuppressUnmanagedCodeSecurity]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("aa80e7ff-2021-11d2-93e0-0060b067b86e")]
        public interface ITfRange
        {
            //const DWORD TF_HF_OBJECT         = 1; // halt shift for TF_CHAR_EMBEDDED
            //const DWORD TF_TF_MOVESTART      = 1; // update start anchor
            //const DWORD TF_TF_IGNOREEND      = 2; // ignore the end anchor
            //const DWORD TF_ST_CORRECTION     = 1; // the replacement is a transform of existing content (correction), not new content
            //const DWORD TF_IE_CORRECTION     = 1;

            //typedef [uuid(49930d51-7d93-448c-a48c-fea5dac192b1)] struct  TF_HALTCOND
            //{
            //  ITfRange *pHaltRange; // halt shift if anchor encountered
            //  TfAnchor aHaltPos;    // ignored if pHaltRange == NULL
            //  DWORD dwFlags;        // TF_HF_*
            //} TF_HALTCOND;

            //HRESULT GetText([in] TfEditCookie ec,
            //                [in] DWORD dwFlags,
            //                [out, size_is(cchMax), length_is(*pcch)] WCHAR *pchText,
            //                [in] ULONG cchMax,
            //                [out] ULONG *pcch);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void GetText(int ec, /*GetTextFlags*/int flags,
                        [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] char []text,
                        int countMax, out int count);

            //HRESULT SetText([in] TfEditCookie ec,
            //                [in] DWORD dwFlags,
            //                [in, size_is(cch), unique] const WCHAR *pchText,
            //                [in] LONG cch);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void SetText(int ec, /*SetTextFlags*/ int flags,
                        [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] char []text,
                        int count);

            //HRESULT GetFormattedText([in] TfEditCookie ec,
            //                         [out] IDataObject **ppDataObject);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void GetFormattedText(int ec, [MarshalAs(UnmanagedType.Interface)] out object data);

            //HRESULT GetEmbedded([in] TfEditCookie ec,
            //                    [in] REFGUID rguidService,
            //                    [in] REFIID riid,
            //                    [out, iid_is(riid)] IUnknown **ppunk);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void GetEmbedded(int ec, ref Guid guidService, ref Guid iid, [MarshalAs(UnmanagedType.Interface)] out object obj);

            //HRESULT InsertEmbedded([in] TfEditCookie ec,
            //                       [in] DWORD dwFlags,
            //                       [in] IDataObject *pDataObject);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void InsertEmbedded(int ec, int flags, [MarshalAs(UnmanagedType.Interface)] object data);

            //HRESULT ShiftStart([in] TfEditCookie ec,
            //                   [in] LONG cchReq,
            //                   [out] LONG *pcch,
            //                   [in, unique] const TF_HALTCOND *pHalt);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void ShiftStart(int ec, int count, out int result, int ZeroForNow); // todo: "ZeroForNow" should be a struct ptr if we ever use this

            //HRESULT ShiftEnd([in] TfEditCookie ec,
            //                 [in] LONG cchReq,
            //                 [out] LONG *pcch,
            //                 [in, unique] const TF_HALTCOND *pHalt);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void ShiftEnd(int ec, int count, out int result, int ZeroForNow); // todo: "ZeroForNow" should be a struct ptr if we ever use this

            //HRESULT ShiftStartToRange([in] TfEditCookie ec,
            //                          [in] ITfRange *pRange,
            //                          [in] TfAnchor aPos);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void ShiftStartToRange(int ec, ITfRange range, TfAnchor position);

            //HRESULT ShiftEndToRange([in] TfEditCookie ec,
            //                        [in] ITfRange *pRange,
            //                        [in] TfAnchor aPos);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void ShiftEndToRange(int ec, ITfRange range, TfAnchor position);

            //HRESULT ShiftStartRegion([in] TfEditCookie ec,
            //                         [in] TfShiftDir dir,
            //                         [out] BOOL *pfNoRegion);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void ShiftStartRegion(int ec, TfShiftDir dir, [MarshalAs(UnmanagedType.Bool)] out bool noRegion);

            //HRESULT ShiftEndRegion([in] TfEditCookie ec,
            //                       [in] TfShiftDir dir,
            //                       [out] BOOL *pfNoRegion);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void ShiftEndRegion(int ec, TfShiftDir dir, [MarshalAs(UnmanagedType.Bool)] out bool noRegion);

            //HRESULT IsEmpty([in] TfEditCookie ec,
            //                [out] BOOL *pfEmpty);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void IsEmpty(int ec, [MarshalAs(UnmanagedType.Bool)] out bool empty);

            //HRESULT Collapse([in] TfEditCookie ec,
            //                 [in] TfAnchor aPos);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void Collapse(int ec, TfAnchor position);

            //HRESULT IsEqualStart([in] TfEditCookie ec,
            //                     [in] ITfRange *pWith,
            //                     [in] TfAnchor aPos,
            //                     [out] BOOL *pfEqual);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void IsEqualStart(int ec, ITfRange with, TfAnchor position, [MarshalAs(UnmanagedType.Bool)] out bool equal);

            //HRESULT IsEqualEnd([in] TfEditCookie ec,
            //                   [in] ITfRange *pWith,
            //                   [in] TfAnchor aPos,
            //                   [out] BOOL *pfEqual);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void IsEqualEnd(int ec, ITfRange with, TfAnchor position, [MarshalAs(UnmanagedType.Bool)] out bool equal);

            //HRESULT CompareStart([in] TfEditCookie ec,
            //                     [in] ITfRange *pWith,
            //                     [in] TfAnchor aPos,
            //                     [out] LONG *plResult);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void CompareStart(int ec, ITfRange with, TfAnchor position, out int result);

            //HRESULT CompareEnd([in] TfEditCookie ec,
            //                   [in] ITfRange *pWith,
            //                   [in] TfAnchor aPos,
            //                   [out] LONG *plResult);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void CompareEnd(int ec, ITfRange with, TfAnchor position, out int result);

            //HRESULT AdjustForInsert([in] TfEditCookie ec,
            //                        [in] ULONG cchInsert,
            //                        [out] BOOL *pfInsertOk);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void AdjustForInsert(int ec, int count, [MarshalAs(UnmanagedType.Bool)] out bool insertOk);

            //HRESULT GetGravity([out] TfGravity *pgStart,
            //                   [out] TfGravity *pgEnd);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void GetGravity(out TfGravity start, out TfGravity end);

            //HRESULT SetGravity([in] TfEditCookie ec,
            //                   [in] TfGravity gStart,
            //                   [in] TfGravity gEnd);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void SetGravity(int ec, TfGravity start, TfGravity end);

            //HRESULT Clone([out] ITfRange **ppClone);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void Clone(out ITfRange clone);

            //HRESULT GetContext([out] ITfContext **ppContext);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void GetContext(out ITfContext context);
        };

        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [SuppressUnmanagedCodeSecurity]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("057a6296-029b-4154-b79a-0d461d4ea94c")]
        public interface ITfRangeACP /*: ITfRange*/ // derivation isn't working, calls to GetExtent go to ITfRange::GetText/vtbl[0]
        {
            //HRESULT GetText([in] TfEditCookie ec,
            //                [in] DWORD dwFlags,
            //                [out, size_is(cchMax), length_is(*pcch)] WCHAR *pchText,
            //                [in] ULONG cchMax,
            //                [out] ULONG *pcch);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void GetText(int ec, /*GetTextFlags*/int flags,
                [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] char []text,
                int countMax, out int count);

            //HRESULT SetText([in] TfEditCookie ec,
            //                [in] DWORD dwFlags,
            //                [in, size_is(cch), unique] const WCHAR *pchText,
            //                [in] LONG cch);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void SetText(int ec, /*SetTextFlags*/ int flags,
                [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] char []text,
                int count);

            //HRESULT GetFormattedText([in] TfEditCookie ec,
            //                         [out] IDataObject **ppDataObject);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void GetFormattedText(int ec, [MarshalAs(UnmanagedType.Interface)] out object data);

            //HRESULT GetEmbedded([in] TfEditCookie ec,
            //                    [in] REFGUID rguidService,
            //                    [in] REFIID riid,
            //                    [out, iid_is(riid)] IUnknown **ppunk);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void GetEmbedded(int ec, ref Guid guidService, ref Guid iid, [MarshalAs(UnmanagedType.Interface)] out object obj);

            //HRESULT InsertEmbedded([in] TfEditCookie ec,
            //                       [in] DWORD dwFlags,
            //                       [in] IDataObject *pDataObject);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void InsertEmbedded(int ec, int flags, [MarshalAs(UnmanagedType.Interface)] object data);

            //HRESULT ShiftStart([in] TfEditCookie ec,
            //                   [in] LONG cchReq,
            //                   [out] LONG *pcch,
            //                   [in, unique] const TF_HALTCOND *pHalt);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void ShiftStart(int ec, int count, out int result, int ZeroForNow); // todo: "ZeroForNow" should be a struct ptr if we ever use this

            //HRESULT ShiftEnd([in] TfEditCookie ec,
            //                 [in] LONG cchReq,
            //                 [out] LONG *pcch,
            //                 [in, unique] const TF_HALTCOND *pHalt);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void ShiftEnd(int ec, int count, out int result, int ZeroForNow); // todo: "ZeroForNow" should be a struct ptr if we ever use this

            //HRESULT ShiftStartToRange([in] TfEditCookie ec,
            //                          [in] ITfRange *pRange,
            //                          [in] TfAnchor aPos);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void ShiftStartToRange(int ec, ITfRange range, TfAnchor position);

            //HRESULT ShiftEndToRange([in] TfEditCookie ec,
            //                        [in] ITfRange *pRange,
            //                        [in] TfAnchor aPos);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void ShiftEndToRange(int ec, ITfRange range, TfAnchor position);

            //HRESULT ShiftStartRegion([in] TfEditCookie ec,
            //                         [in] TfShiftDir dir,
            //                         [out] BOOL *pfNoRegion);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void ShiftStartRegion(int ec, TfShiftDir dir, [MarshalAs(UnmanagedType.Bool)] out bool noRegion);

            //HRESULT ShiftEndRegion([in] TfEditCookie ec,
            //                       [in] TfShiftDir dir,
            //                       [out] BOOL *pfNoRegion);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void ShiftEndRegion(int ec, TfShiftDir dir, [MarshalAs(UnmanagedType.Bool)] out bool noRegion);

            //HRESULT IsEmpty([in] TfEditCookie ec,
            //                [out] BOOL *pfEmpty);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void IsEmpty(int ec, [MarshalAs(UnmanagedType.Bool)] out bool empty);

            //HRESULT Collapse([in] TfEditCookie ec,
            //                 [in] TfAnchor aPos);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void Collapse(int ec, TfAnchor position);

            //HRESULT IsEqualStart([in] TfEditCookie ec,
            //                     [in] ITfRange *pWith,
            //                     [in] TfAnchor aPos,
            //                     [out] BOOL *pfEqual);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void IsEqualStart(int ec, ITfRange with, TfAnchor position, [MarshalAs(UnmanagedType.Bool)] out bool equal);

            //HRESULT IsEqualEnd([in] TfEditCookie ec,
            //                   [in] ITfRange *pWith,
            //                   [in] TfAnchor aPos,
            //                   [out] BOOL *pfEqual);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void IsEqualEnd(int ec, ITfRange with, TfAnchor position, [MarshalAs(UnmanagedType.Bool)] out bool equal);

            //HRESULT CompareStart([in] TfEditCookie ec,
            //                     [in] ITfRange *pWith,
            //                     [in] TfAnchor aPos,
            //                     [out] LONG *plResult);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void CompareStart(int ec, ITfRange with, TfAnchor position, out int result);

            //HRESULT CompareEnd([in] TfEditCookie ec,
            //                   [in] ITfRange *pWith,
            //                   [in] TfAnchor aPos,
            //                   [out] LONG *plResult);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void CompareEnd(int ec, ITfRange with, TfAnchor position, out int result);

            //HRESULT AdjustForInsert([in] TfEditCookie ec,
            //                        [in] ULONG cchInsert,
            //                        [out] BOOL *pfInsertOk);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void AdjustForInsert(int ec, int count, [MarshalAs(UnmanagedType.Bool)] out bool insertOk);

            //HRESULT GetGravity([out] TfGravity *pgStart,
            //                   [out] TfGravity *pgEnd);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void GetGravity(out TfGravity start, out TfGravity end);

            //HRESULT SetGravity([in] TfEditCookie ec,
            //                   [in] TfGravity gStart,
            //                   [in] TfGravity gEnd);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void SetGravity(int ec, TfGravity start, TfGravity end);

            //HRESULT Clone([out] ITfRange **ppClone);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void Clone(out ITfRange clone);

            //HRESULT GetContext([out] ITfContext **ppContext);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void GetContext(out ITfContext context);

            //HRESULT GetExtent([out] LONG *pacpAnchor,
            //                  [out] LONG *pcch);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void GetExtent(out int start, out int count);

            //HRESULT SetExtent([in] LONG acpAnchor,
            //                  [in] LONG cch);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void SetExtent(int start, int count);
        }

        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [SuppressUnmanagedCodeSecurity]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("D7540241-F9A1-4364-BEFC-DBCD2C4395B7")]
        public interface ITfCompositionView
        {
            //HRESULT GetOwnerClsid([out] CLSID *pclsid);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void GetOwnerClsid(out Guid clsid);

            //HRESULT GetRange([out] ITfRange **ppRange);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void GetRange(out ITfRange range);
        };

        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("5F20AA40-B57A-4F34-96AB-3576F377CC79")]
        [SuppressUnmanagedCodeSecurity]
        public interface ITfContextOwnerCompositionSink
        {
            /// <summary></summary>
            //HRESULT OnStartComposition([in] ITfCompositionView *pComposition,
            //                           [out] BOOL *pfOk);
            void OnStartComposition(ITfCompositionView view, [MarshalAs(UnmanagedType.Bool)] out bool ok);

            /// <summary></summary>
            //HRESULT OnUpdateComposition([in] ITfCompositionView *pComposition,
            //                            [in] ITfRange *pRangeNew);
            void OnUpdateComposition(ITfCompositionView view, ITfRange rangeNew);

            /// <summary></summary>
            //HRESULT OnEndComposition([in] ITfCompositionView *pComposition);
            void OnEndComposition(ITfCompositionView view);
        };

        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("D40C8AAE-AC92-4FC7-9A11-0EE0E23AA39B")]
        [SuppressUnmanagedCodeSecurity]
        public interface ITfContextComposition
        {
            /// <summary></summary>
            //HRESULT StartComposition([in] TfEditCookie ecWrite,
            //                         [in] ITfRange *pCompositionRange,
            //                         [in] ITfCompositionSink *pSink,
            //                         [out] ITfComposition **ppComposition);
            void StartComposition(int ecWrite, ITfRange range, [MarshalAs(UnmanagedType.Interface)] object /*ITfCompositionSink */sink, [MarshalAs(UnmanagedType.Interface)] out object /*ITfComposition */composition);

            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            //HRESULT EnumCompositions([out] IEnumITfCompositionView **ppEnum);
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            void EnumCompositions([MarshalAs(UnmanagedType.Interface)] out IEnumITfCompositionView enumView);

            /// <summary></summary>
            //HRESULT FindComposition([in] TfEditCookie ecRead,
            //                        [in] ITfRange *pTestRange,
            //                        [out] IEnumITfCompositionView **ppEnum);
            void FindComposition(int ecRead, ITfRange testRange, [MarshalAs(UnmanagedType.Interface)] out object /*IEnumITfCompositionView*/ enumView);

            /// <summary></summary>
            //HRESULT TakeOwnership([in] TfEditCookie ecWrite,
            //                      [in] ITfCompositionView *pComposition,
            //                      [in] ITfCompositionSink *pSink,
            //                      [out] ITfComposition **ppComposition);
            void TakeOwnership(int ecWrite, ITfCompositionView view, [MarshalAs(UnmanagedType.Interface)] object /*ITfCompositionSink */ sink,
                            [MarshalAs(UnmanagedType.Interface)] out object /*ITfComposition*/ composition);
        };

        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("86462810-593B-4916-9764-19C08E9CE110")]
        [SuppressUnmanagedCodeSecurity]
        public interface ITfContextOwnerCompositionServices /*: ITfContextComposition*/
        {
            /// <summary></summary>
            //HRESULT StartComposition([in] TfEditCookie ecWrite,
            //                         [in] ITfRange *pCompositionRange,
            //                         [in] ITfCompositionSink *pSink,
            //                         [out] ITfComposition **ppComposition);
            void StartComposition(int ecWrite, ITfRange range, [MarshalAs(UnmanagedType.Interface)] object /*ITfCompositionSink */sink, [MarshalAs(UnmanagedType.Interface)] out object /*ITfComposition */composition);

            /// <summary></summary>
            //HRESULT EnumCompositions([out] IEnumITfCompositionView **ppEnum);
            void EnumCompositions([MarshalAs(UnmanagedType.Interface)] out object /*IEnumITfCompositionView*/ enumView);

            /// <summary></summary>
            //HRESULT FindComposition([in] TfEditCookie ecRead,
            //                        [in] ITfRange *pTestRange,
            //                        [out] IEnumITfCompositionView **ppEnum);
            void FindComposition(int ecRead, ITfRange testRange, [MarshalAs(UnmanagedType.Interface)] out object /*IEnumITfCompositionView*/ enumView);

            /// <summary></summary>
            //HRESULT TakeOwnership([in] TfEditCookie ecWrite,
            //                      [in] ITfCompositionView *pComposition,
            //                      [in] ITfCompositionSink *pSink,
            //                      [out] ITfComposition **ppComposition);
            void TakeOwnership(int ecWrite, ITfCompositionView view, [MarshalAs(UnmanagedType.Interface)] object /*ITfCompositionSink */ sink,
                            [MarshalAs(UnmanagedType.Interface)] out object /*ITfComposition*/ composition);

            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            //HRESULT TerminateComposition([in] ITfCompositionView *pComposition);
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            [PreserveSig]
            int TerminateComposition(ITfCompositionView view);
        };

        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("5EFD22BA-7838-46CB-88E2-CADB14124F8F")]
        [SuppressUnmanagedCodeSecurity]
        internal interface IEnumITfCompositionView
        {
            /// <summary></summary>
            //HRESULT Clone([out] IEnumTfRanges **ppEnum);
            void Clone(out IEnumTfRanges ranges);

            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            //HRESULT Next([in] ULONG ulCount,
            //            [out, size_is(ulCount), length_is(*pcFetched)] ITfRange **ppRange,
            //            [out] ULONG *pcFetched);
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            [PreserveSig]
            unsafe int Next(int count, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] ITfCompositionView []compositionview, out int fetched);

            /// <summary></summary>
            //HRESULT Reset();
            void Reset();

            /// <summary></summary>
            //HRESULT Skip(ULONG ulCount);
            [PreserveSig]
            int Skip(int count);
        }

        /// <summary></summary>
        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [SuppressUnmanagedCodeSecurity]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("f99d3f40-8e32-11d2-bf46-00105a2799b5")]
        public interface IEnumTfRanges
        {
            //HRESULT Clone([out] IEnumTfRanges **ppEnum);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void Clone(out IEnumTfRanges ranges);

            //HRESULT Next([in] ULONG ulCount,
            //            [out, size_is(ulCount), length_is(*pcFetched)] ITfRange **ppRange,
            //            [out] ULONG *pcFetched);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            [PreserveSig]
            unsafe int Next(int count, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] ITfRange []ranges, out int fetched);

            //HRESULT Reset();
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void Reset();

            //HRESULT Skip(ULONG ulCount);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            [PreserveSig]
            int Skip(int count);
        }

        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("42d4d099-7c1a-4a89-b836-6c6f22160df0")]
        [SuppressUnmanagedCodeSecurity]
        public interface ITfEditRecord
        {
            //const DWORD TF_GTP_INCL_TEXT = 0x1;

            /// <summary></summary>
            //HRESULT GetSelectionStatus([out] BOOL *pfChanged);
            void GetSelectionStatus([MarshalAs(UnmanagedType.Bool)] out bool selectionChanged);

            /// <summary></summary>
            //HRESULT GetTextAndPropertyUpdates([in] DWORD dwFlags,
            //                                  [in, size_is(cProperties)] const GUID **prgProperties,
            //                                  [in] ULONG cProperties,
            //                                  [out] IEnumTfRanges **ppEnum);
            //
            // TODO: yutakas
            //
            // Use "ref IntPtr" Temporarily.
            // See the comment in InputMethodProperty.GetPropertyUpdate().
            //
            unsafe void GetTextAndPropertyUpdates(int flags,
                                                /*[In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)]*/ /*Guid ** */ ref IntPtr properties,
                                                int count,
                                                out IEnumTfRanges ranges);
        }

        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("8127d409-ccd3-4683-967a-b43d5b482bf7")]
        [SuppressUnmanagedCodeSecurity]
        public interface ITfTextEditSink
        {
            /// <summary></summary>
            //HRESULT OnEndEdit([in] ITfContext *pic, [in] TfEditCookie ecReadOnly, [in] ITfEditRecord *pEditRecord);
            void OnEndEdit(ITfContext context, int ecReadOnly, ITfEditRecord editRecord);
        }

        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("8c03d21b-95a7-4ba0-ae1b-7fce12a72930")]
        [SuppressUnmanagedCodeSecurity]
        public interface IEnumTfRenderingMarkup
        {
            /// <summary></summary>
            //HRESULT Clone([out] IEnumTfRenderingMarkup **ppClone);
            void Clone(out IEnumTfRenderingMarkup clone);

            /// <summary></summary>
            //HRESULT Next([in] ULONG ulCount,
            //            [out, size_is(ulCount), length_is(*pcFetched)] TF_RENDERINGMARKUP *rgMarkup,
            //            [out] ULONG *pcFetched);
            [PreserveSig]
            int Next(int count, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] TF_RENDERINGMARKUP []markup, out int fetched);

            /// <summary></summary>
            //HRESULT Reset();
            void Reset();

            /// <summary></summary>
            //HRESULT Skip([in] ULONG ulCount);
            [PreserveSig]
            int Skip(int count);
        }

        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("a305b1c0-c776-4523-bda0-7c5a2e0fef10")]
        [SuppressUnmanagedCodeSecurity]
        public interface ITfContextRenderingMarkup
        {
            /// <summary></summary>
            //HRESULT GetRenderingMarkup([in] TfEditCookie ec,
            //                        [in] DWORD dwFlags,
            //                        [in] ITfRange *pRangeCover,
            //                        [out] IEnumTfRenderingMarkup **ppEnum);
            void GetRenderingMarkup(int editCookie, GetRenderingMarkupFlags flags, ITfRange range, out IEnumTfRenderingMarkup enumMarkup);

            /// <summary></summary>
            //HRESULT FindNextRenderingMarkup([in] TfEditCookie ec,
            //                                [in] DWORD dwFlags,
            //                                [in] ITfRange *pRangeQuery,
            //                                [in] TfAnchor tfAnchorQuery,
            //                                [out] ITfRange **ppRangeFound,
            //                                [out] TF_RENDERINGMARKUP *ptfRenderingMarkup);
            void FindNextRenderingMarkup(int editCookie, FindRenderingMarkupFlags flags,
                                        ITfRange queryRange, TfAnchor queryAnchor,
                                        out ITfRange foundRange, out TF_RENDERINGMARKUP foundMarkup);
        }

        /// <summary></summary>
        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [SuppressUnmanagedCodeSecurity]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("1F02B6C5-7842-4EE6-8A0B-9A24183A95CA")]
        public interface ITfInputProcessorProfiles
        {
            // HRESULT Register([in] REFCLSID rclsid);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_Register();

            // HRESULT Unregister([in] REFCLSID rclsid);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_Unregister();

            // HRESULT AddLanguageProfile([in] REFCLSID rclsid,
            //                            [in] LANGID langid,
            //                            [in] REFGUID guidProfile,
            //                            [in, size_is(cchDesc)] const WCHAR *pchDesc,
            //                            [in] ULONG cchDesc,
            //                            [in, size_is(cchFile)] const WCHAR *pchIconFile,
            //                            [in] ULONG cchFile,
            //                            [in] ULONG uIconIndex);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_AddLanguageProfile();

            // HRESULT RemoveLanguageProfile([in] REFCLSID rclsid,
            //                               [in] LANGID langid,
            //                               [in] REFGUID guidProfile);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_RemoveLanguageProfile();

            // HRESULT EnumInputProcessorInfo([out] IEnumGUID **ppEnum);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_EnumInputProcessorInfo();

            // HRESULT GetDefaultLanguageProfile([in] LANGID langid,
            //                                  [in] REFGUID catid,
            //                                  [out] CLSID *pclsid,
            //                                  [out] GUID *pguidProfile);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_GetDefaultLanguageProfile();

            // HRESULT SetDefaultLanguageProfile([in] LANGID langid,
            //                                   [in] REFCLSID rclsid,
            //                                   [in] REFGUID guidProfiles);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_SetDefaultLanguageProfile();

            // HRESULT ActivateLanguageProfile([in] REFCLSID rclsid,
            //                                 [in] LANGID langid,
            //                                 [in] REFGUID guidProfiles);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void ActivateLanguageProfile(ref Guid clsid, short langid, ref Guid guidProfile);

            // HRESULT GetActiveLanguageProfile([in] REFCLSID rclsid,
            //                                  [out] LANGID *plangid,
            //                                  [out] GUID *pguidProfile);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            [PreserveSig]
            int GetActiveLanguageProfile(ref Guid clsid, out short langid, out Guid profile);
    
            // HRESULT GetLanguageProfileDescription([in] REFCLSID rclsid,
            //                                       [in] LANGID langid,
            //                                       [in] REFGUID guidProfile,
            //                                       [out] BSTR *pbstrProfile);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_GetLanguageProfileDescription();

            // HRESULT GetCurrentLanguage([out] LANGID *plangid);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void GetCurrentLanguage(out short langid);

            // HRESULT ChangeCurrentLanguage([in] LANGID langid);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            [PreserveSig]
            int ChangeCurrentLanguage(short langid);
    
            // HRESULT GetLanguageList([out] LANGID **ppLangId,
            //                         [out] ULONG *pulCount);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            [PreserveSig]
            int GetLanguageList(out IntPtr langids, out int count);

    
            // HRESULT EnumLanguageProfiles([in] LANGID langid,
            //                              [out] IEnumTfLanguageProfiles **ppEnum);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void EnumLanguageProfiles(short langid, out IEnumTfLanguageProfiles enumIPP);


            // HRESULT EnableLanguageProfile([in] REFCLSID rclsid,
            //                               [in] LANGID langid,
            //                               [in] REFGUID guidProfile,
            //                               [in] BOOL fEnable);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_EnableLanguageProfile();

            // HRESULT IsEnabledLanguageProfile([in] REFCLSID rclsid,
            //                                  [in] LANGID langid,
            //                                  [in] REFGUID guidProfile,
            //                                  [out] BOOL *pfEnable);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_IsEnabledLanguageProfile();

            // HRESULT EnableLanguageProfileByDefault([in] REFCLSID rclsid,
            //                                        [in] LANGID langid,
            //                                        [in] REFGUID guidProfile,
            //                                        [in] BOOL fEnable);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_EnableLanguageProfileByDefault();

            // HRESULT SubstituteKeyboardLayout([in] REFCLSID rclsid,
            //                                  [in] LANGID langid,
            //                                  [in] REFGUID guidProfile,
            //                                  [in] HKL hKL);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_SubstituteKeyboardLayout();
        }

        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("3d61bf11-ac5f-42c8-a4cb-931bcc28c744")]
        [SuppressUnmanagedCodeSecurity]
        internal interface IEnumTfLanguageProfiles
        {
            /// <summary></summary>
            // HRESULT Clone([out] IEnumTfLanguageProfiles **ppEnum);
            void Clone(out IEnumTfLanguageProfiles enumIPP);

            /// <summary></summary>
            // HRESULT Next([in] ULONG ulCount,
            //              [out, size_is(ulCount), length_is(*pcFetch)] TF_LANGUAGEPROFILE *pProfile,
            //              [out] ULONG *pcFetch);
            [PreserveSig]
            int Next(int count, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] TF_LANGUAGEPROFILE []profiles, out int fetched);

            /// <summary></summary>
            // HRESULT Reset();
            void Reset();

            /// <summary></summary>
            // HRESULT Skip([in] ULONG ulCount);
            void Skip(int count);
        }

        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("43c9fe15-f494-4c17-9de2-b8a4ac350aa8")]
        [SuppressUnmanagedCodeSecurity]
        public interface ITfLanguageProfileNotifySink
        {
            /// <summary></summary>
            // HRESULT OnLanguageChange([in] LANGID langid,
            //                          [out] BOOL *pfAccept);
            void OnLanguageChange(short langid, [MarshalAs(UnmanagedType.Bool)] out bool bAccept);

            /// <summary></summary>
            // HRESULT OnLanguageChanged();
            void OnLanguageChanged();
        }

        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [SuppressUnmanagedCodeSecurity]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("8ded7393-5db1-475c-9e71-a39111b0ff67")]
        public interface ITfDisplayAttributeMgr
        {
            // HRESULT OnUpdateInfo();
            //
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void OnUpdateInfo();

            // HRESULT EnumDisplayAttributeInfo([out] IEnumTfDisplayAttributeInfo **ppEnum);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_EnumDisplayAttributeInfo();

            // HRESULT GetDisplayAttributeInfo([in] REFGUID guid,
            //                         [out] ITfDisplayAttributeInfo **ppInfo,
            //                         [out] CLSID *pclsidOwner);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void GetDisplayAttributeInfo(ref Guid guid, out ITfDisplayAttributeInfo info, out Guid clsid);

        }

        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [SuppressUnmanagedCodeSecurity]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("70528852-2f26-4aea-8c96-215150578932")]
        public interface ITfDisplayAttributeInfo
        {
            // HRESULT GetGUID([out] GUID *pguid);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_GetGUID();

            // HRESULT GetDescription([out] BSTR *pbstrDesc);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_GetDescription();

            // HRESULT GetAttributeInfo([out] TF_DISPLAYATTRIBUTE *pda);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void GetAttributeInfo(out TF_DISPLAYATTRIBUTE attr);

            // HRESULT SetAttributeInfo([in] const TF_DISPLAYATTRIBUTE *pda);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_SetAttributeInfo();

            // HRESULT Reset();
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_Reset();
        }

        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [SuppressUnmanagedCodeSecurity]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("c3acefb5-f69d-4905-938f-fcadcf4be830")]
        public interface ITfCategoryMgr
        {
            // HRESULT RegisterCategory([in] REFCLSID rclsid,
            //                          [in] REFGUID rcatid,
            //                          [in] REFGUID rguid);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_RegisterCategory();

            // HRESULT UnregisterCategory([in] REFCLSID rclsid,
            //                            [in] REFGUID rcatid,
            //                            [in] REFGUID rguid);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_UnregisterCategory();

            // HRESULT EnumCategoriesInItem([in] REFGUID rguid,
            //                              [out] IEnumGUID **ppEnum);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_EnumCategoriesInItem();

            // HRESULT EnumItemsInCategory([in] REFGUID rcatid,
            //                             [out] IEnumGUID **ppEnum);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_EnumItemsInCategory();

            // HRESULT FindClosestCategory([in] REFGUID rguid,
            //                             [out] GUID *pcatid,
            //                             [in, size_is(ulCount)] const GUID **ppcatidList,
            //                             [in] ULONG ulCount);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_FindClosestCategory();

            // HRESULT RegisterGUIDDescription([in] REFCLSID rclsid,
            //                                 [in] REFGUID rguid,
            //                                 [in, size_is(cch)] const WCHAR *pchDesc,
            //                                 [in] ULONG cch);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_RegisterGUIDDescription();

            // HRESULT UnregisterGUIDDescription([in] REFCLSID rclsid,
            //                                   [in] REFGUID rguid);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_UnregisterGUIDDescription();

            // HRESULT GetGUIDDescription([in] REFGUID rguid,
            //                            [out] BSTR *pbstrDesc);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_GetGUIDDescription();

            // HRESULT RegisterGUIDDWORD([in] REFCLSID rclsid,
            //                           [in] REFGUID rguid,
            //                           [in] DWORD dw);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_RegisterGUIDDWORD();

            // HRESULT UnregisterGUIDDWORD([in] REFCLSID rclsid,
            //                             [in] REFGUID rguid);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_UnregisterGUIDDWORD();

            // HRESULT GetGUIDDWORD([in] REFGUID rguid,
            //                      [out] DWORD *pdw);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_GetGUIDDWORD();

            // HRESULT RegisterGUID([in] REFGUID rguid,
            //                      [out] TfGuidAtom *pguidatom);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_RegisterGUID();

            // HRESULT GetGUID([in] TfGuidAtom guidatom,
            //                 [out] GUID *pguid);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [PreserveSig]
            [SecurityCritical]
            int GetGUID(Int32 guidatom, out Guid guid);

            // HRESULT IsEqualTfGuidAtom([in] TfGuidAtom guidatom,
            //                           [in] REFGUID rguid,
            //                           [out] BOOL *pfEqual);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical - calls unmanaged code
            /// </SecurityNote>
            [SecurityCritical]
            void stub_IsEqualTfGuidAtom();
        }

        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("aa80e80c-2021-11d2-93e0-0060b067b86e")]
        [SuppressUnmanagedCodeSecurity]
        public interface ITfContextOwner
        {
            /// <summary></summary>
            // HRESULT GetACPFromPoint([in] const POINT *ptScreen,
            //                         [in] DWORD dwFlags,
            //                         [out] LONG *pacp);
            void GetACPFromPoint(ref POINT point, GetPositionFromPointFlags flags, out int position);

            /// <summary></summary>
            // HRESULT GetTextExt([in] LONG acpStart,
            //                    [in] LONG acpEnd,
            //                    [out] RECT *prc,
            //                    [out] BOOL *pfClipped);
            void GetTextExt(int start, int end, out RECT rect, [MarshalAs(UnmanagedType.Bool)] out bool clipped);

            /// <summary></summary>
            // HRESULT GetScreenExt([out] RECT *prc);
            void GetScreenExt(out RECT rect);

            /// <summary></summary>
            // HRESULT GetStatus([out] TF_STATUS *pdcs);
            void GetStatus(out TS_STATUS status);

            /// <summary></summary>
            // HRESULT GetWnd([out] HWND *phwnd);
            void GetWnd(out IntPtr hwnd);

            /// <summary></summary>
            // HRESULT GetAttribute([in] REFGUID rguidAttribute, [out] VARIANT *pvarValue);
            void GetValue(ref Guid guidAttribute, out object varValue);
        }


        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("b23eb630-3e1c-11d3-a745-0050040ab407")]
        [SuppressUnmanagedCodeSecurity]
        public interface ITfContextOwnerServices
        {
            /// <summary></summary>
            // HRESULT OnLayoutChange();
            void stub_OnLayoutChange();

            /// <summary></summary>
            // HRESULT OnStatusChange([in] DWORD dwFlags);
            void stub_OnStatusChange();

            /// <summary></summary>
            // HRESULT OnAttributeChange([in] REFGUID rguidAttribute);
            void stub_OnAttributeChange();

            /// <summary></summary>
            // HRESULT Serialize([in] ITfProperty *pProp,
            //                   [in] ITfRange *pRange,
            //                   [out] TF_PERSISTENT_PROPERTY_HEADER_ACP *pHdr,
            //                   [in] IStream *pStream);
            void stub_Serialize();

            /// <summary></summary>
            // HRESULT Unserialize([in] ITfProperty *pProp,
            //                     [in] const TF_PERSISTENT_PROPERTY_HEADER_ACP *pHdr,
            //                     [in] IStream *pStream,
            //                     [in] ITfPersistentPropertyLoaderACP *pLoader);
            void stub_Unserialize();
        
            /// <summary></summary>
            // HRESULT ForceLoadProperty([in] ITfProperty *pProp);
            void stub_ForceLoadProperty();

            /// <summary></summary>
            // HRESULT CreateRange([in] LONG acpStart,
            //                     [in] LONG acpEnd,
            //                     [out] ITfRangeACP **ppRange);
            void CreateRange(Int32 acpStart, Int32 acpEnd, out ITfRangeACP range);
        }

        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("a615096f-1c57-4813-8a15-55ee6e5a839c")]
        [SuppressUnmanagedCodeSecurity]
        public interface ITfTransitoryExtensionSink
        {
            /// <summary></summary>
            // HRESULT OnTransitoryExtensionUpdated([in] ITfContext *pic,
            //                                      [in] TfEditCookie ecReadOnly,
            //                                      [in] ITfRange *pResultRange,
            //                                      [in] ITfRange *pCompositionRange,
            //                                      [out] BOOL *pfDeleteResultRange);
            void OnTransitoryExtensionUpdated(ITfContext context, int ecReadOnly, ITfRange rangeResult, ITfRange rangeComposition, [MarshalAs(UnmanagedType.Bool)] out bool fDeleteResultRange);
        }

        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("fde1eaee-6924-4cdf-91e7-da38cff5559d")]
        [SuppressUnmanagedCodeSecurity]
        public interface ITfInputScope
        {
            /// <summary></summary>
            // HRESULT GetInputScopes([out] InputScope **pprgInputScopes,
            //                        [out] UINT *pcCount);
            void GetInputScopes(out IntPtr ppinputscopes, out int count);

            /// <summary></summary>
            // HRESULT GetPhrase([out] BSTR **ppbstrPhrases,
            //                   [out] UINT *pcCount);
            [PreserveSig]
            int GetPhrase(out IntPtr ppbstrPhrases, out int count);
        
            /// <summary></summary>
            // HRESULT GetRegularExpression([out] BSTR *pbstrRegExp);
            [PreserveSig]
            int GetRegularExpression([Out, MarshalAs(UnmanagedType.BStr)] out string desc);

            /// <summary></summary>
            // HRESULT GetSRGS([out] BSTR *pbstrSRGS);
            [PreserveSig]
            int GetSRGC([Out, MarshalAs(UnmanagedType.BStr)] out string desc);

            /// <summary></summary>
            // HRESULT GetXML([out] BSTR *pbstrXML);
            [PreserveSig]
            int GetXML([Out, MarshalAs(UnmanagedType.BStr)] out string desc);
        }


        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("3bdd78e2-c16e-47fd-b883-ce6facc1a208")]
        [SuppressUnmanagedCodeSecurity]
        public interface ITfMouseTrackerACP
        {
            /// <summary></summary>
            // HRESULT AdviseMouseSink([in] ITfRangeACP *range,
            //                         [in] ITfMouseSink *pSink,
            //                         [out] DWORD *pdwCookie);
            [PreserveSig]
            int AdviceMouseSink(ITfRangeACP range, ITfMouseSink sink, out int dwCookie);

            /// <summary></summary>
            // HRESULT UnadviseMouseSink([in] DWORD dwCookie);
            [PreserveSig]
            int UnadviceMouseSink(int dwCookie);
        }

        /// <summary></summary>
        /// <SecurityNote>
        ///     Critical - calls unmanaged code
        /// </SecurityNote>
        [SecurityCritical(SecurityCriticalScope.Everything)]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("a1adaaa2-3a24-449d-ac96-5183e7f5c217")]
        [SuppressUnmanagedCodeSecurity]
        public interface ITfMouseSink
        {
            // HRESULT OnMouseEvent([in] ULONG uEdge,
            //                      [in] ULONG uQuadrant,
            //                      [in] DWORD dwBtnStatus,
            //                      [out] BOOL *pfEaten);
            /// <summary></summary>
            /// <SecurityNote>
            ///     Critical: This code calls into an unmanaged COM function which is not
            ///     safe since it elevates
            /// </SecurityNote>
            [PreserveSig]
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            int OnMouseEvent(int edge, int quadrant, int btnStatus, [MarshalAs(UnmanagedType.Bool)] out bool eaten);
        }

        #endregion Interfaces
    }
}
