// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Class:  SerialPort
**
** Purpose: SerialPort wraps an internal SerialStream class,
**        : providing a high but complete level of Serial Port I/O functionality
**        : over the handle/Win32 object level of the SerialStream.
**
**
** Date:  August 2002
**
===========================================================*/


using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Security;
using System.Security.Permissions;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;


namespace System.IO.Ports
{

    [MonitoringDescription(SR.SerialPortDesc)]
    public class SerialPort : System.ComponentModel.Component
    {
        public const int InfiniteTimeout = -1;

        // ---------- default values -------------*

        private const int defaultDataBits = 8;
        private const Parity defaultParity = Parity.None;
        private const StopBits defaultStopBits = StopBits.One;
        private const Handshake defaultHandshake = Handshake.None;
        private const int defaultBufferSize = 1024;
        private const string defaultPortName = "COM1";
        private const int defaultBaudRate = 9600;
        private const bool defaultDtrEnable = false;
        private const bool defaultRtsEnable = false;
        private const bool defaultDiscardNull = false;
        private const byte defaultParityReplace = (byte) '?';
        private const int defaultReceivedBytesThreshold = 1;
        private const int defaultReadTimeout = SerialPort.InfiniteTimeout;
        private const int defaultWriteTimeout = SerialPort.InfiniteTimeout;
        private const int defaultReadBufferSize = 4096;
        private const int defaultWriteBufferSize = 2048;
        private const int maxDataBits = 8;
        private const int minDataBits = 5;
        private const string defaultNewLine = "\n";

        private const string SERIAL_NAME = @"\Device\Serial";

        // --------- members supporting exposed properties ------------*
        private int baudRate = defaultBaudRate;
        private int dataBits = defaultDataBits;
        private Parity parity = defaultParity;
        private StopBits stopBits = defaultStopBits;
        private string portName = defaultPortName;
        private Encoding encoding = System.Text.Encoding.ASCII; // ASCII is default encoding for modem communication, etc.
        private Decoder decoder = System.Text.Encoding.ASCII.GetDecoder();
        private int maxByteCountForSingleChar = System.Text.Encoding.ASCII.GetMaxByteCount(1);
        private Handshake handshake = defaultHandshake;
        private int readTimeout = defaultReadTimeout;
        private int writeTimeout = defaultWriteTimeout;
        private int receivedBytesThreshold = defaultReceivedBytesThreshold;
        private bool discardNull = defaultDiscardNull;
        private bool dtrEnable = defaultDtrEnable;
        private bool rtsEnable = defaultRtsEnable;
        private byte parityReplace = defaultParityReplace;
        private string newLine = defaultNewLine;
        private int readBufferSize = defaultReadBufferSize;
        private int writeBufferSize = defaultWriteBufferSize;

        // ---------- members for internal support ---------*
        private SerialStream internalSerialStream = null;
        private byte[] inBuffer = new byte[defaultBufferSize];
        private int readPos = 0;    // position of next byte to read in the read buffer.  readPos <= readLen
        private int readLen = 0;    // position of first unreadable byte => CachedBytesToRead is the number of readable bytes left.
        private char[] oneChar = new char[1];
        private char[] singleCharBuffer = null;

        // ------ event members ------------------*
        //public event EventHandler Disposed;
        [MonitoringDescription(SR.SerialErrorReceived)]
        public event SerialErrorReceivedEventHandler ErrorReceived;
        
        [MonitoringDescription(SR.SerialPinChanged)]
        public event SerialPinChangedEventHandler PinChanged;
        
        [MonitoringDescription(SR.SerialDataReceived)]
        public event SerialDataReceivedEventHandler DataReceived;

        //--- component properties---------------*

        // ---- SECTION: public properties --------------*
        // Note: information about port properties passes in ONE direction: from SerialPort to
        // its underlying Stream.  No changes are able to be made in the important properties of
        // the stream and its behavior, so no reflection back to SerialPort is necessary.

        // Gets the internal SerialStream object.  Used to pass essence of SerialPort to another Stream wrapper.
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Stream BaseStream
        {
            get { 
                if (!IsOpen)
                    throw new InvalidOperationException(SR.GetString(SR.BaseStream_Invalid_Not_Open));
                    
                return internalSerialStream; 
            }
        }

        [Browsable(true),
        DefaultValue(defaultBaudRate),
        MonitoringDescription(SR.BaudRate)]
        public int BaudRate
        {
            get { return baudRate;  }
            set {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("BaudRate", SR.GetString(SR.ArgumentOutOfRange_NeedPosNum));
                
                if (IsOpen)
                    internalSerialStream.BaudRate = value;
                baudRate = value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool BreakState
        {
            get
            {
                if (!IsOpen)
                    throw new InvalidOperationException(SR.GetString(SR.Port_not_open));

                return internalSerialStream.BreakState;
            }

            set {
                if (!IsOpen)
                    throw new InvalidOperationException(SR.GetString(SR.Port_not_open));

                internalSerialStream.BreakState = value;
            }
        }

        // includes all bytes available on serial driver's output buffer.  Note that we do not internally buffer output bytes in SerialPort.
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BytesToWrite
        {
            get
            {
                if (!IsOpen)
                    throw new InvalidOperationException(SR.GetString(SR.Port_not_open));
                return internalSerialStream.BytesToWrite;
            }
        }

        // includes all bytes available on serial driver's input buffer as well as bytes internally buffered int the SerialPort class.
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BytesToRead
        {
            get
            {
                if (!IsOpen)
                    throw new InvalidOperationException(SR.GetString(SR.Port_not_open));
                return internalSerialStream.BytesToRead + CachedBytesToRead; // count the number of bytes we have in the internal buffer too.
            }
        }

        private int CachedBytesToRead {
            get {
                return readLen - readPos;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CDHolding
        {
            get
            {
                if (!IsOpen)
                    throw new InvalidOperationException(SR.GetString(SR.Port_not_open));
                return internalSerialStream.CDHolding;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CtsHolding
        {
            get
            {
                if (!IsOpen)
                    throw new InvalidOperationException(SR.GetString(SR.Port_not_open));
                return internalSerialStream.CtsHolding;
            }
        }


        [Browsable(true),
        DefaultValue(defaultDataBits),
        MonitoringDescription(SR.DataBits)]
        public int DataBits
        {
            get
            { return dataBits;  }
            set
            {
                if (value < minDataBits || value > maxDataBits)
                    throw new ArgumentOutOfRangeException("DataBits", SR.GetString(SR.ArgumentOutOfRange_Bounds_Lower_Upper, minDataBits, maxDataBits));
                
                if (IsOpen)
                    internalSerialStream.DataBits = value;
                dataBits = value;
            }
        }

        [Browsable(true),
        DefaultValue(defaultDiscardNull),
        MonitoringDescription(SR.DiscardNull)]
        public bool DiscardNull
        {
            get
            {
                return discardNull;
            }
            set
            {
                if (IsOpen)
                    internalSerialStream.DiscardNull = value;
                discardNull = value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DsrHolding
        {
            get
            {
                if (!IsOpen)
                    throw new InvalidOperationException(SR.GetString(SR.Port_not_open));
                return internalSerialStream.DsrHolding;
            }
        }

        [Browsable(true),
        DefaultValue(defaultDtrEnable),
        MonitoringDescription(SR.DtrEnable)]
        public bool DtrEnable
        {
            get { 
                if (IsOpen)
                    dtrEnable = internalSerialStream.DtrEnable;

                return dtrEnable; 
            }
            set
            {
                if (IsOpen)
                    internalSerialStream.DtrEnable = value;
                dtrEnable = value;
            }
        }

        // Allows specification of an arbitrary encoding for the reading and writing functions of the port
        // which deal with chars and strings.  Set by default in the code to System.Text.ASCIIEncoding(), which
        // is the standard text encoding for modem commands and most of serial communication.
        // Clearly not designable.
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        MonitoringDescription(SR.Encoding)]
        public Encoding Encoding
        {
            get { 
                return encoding; 
            }
            set { 
                if (value == null)
                    throw new ArgumentNullException("Encoding");

                // Limit the encodings we support to some known ones.  The code pages < 50000 represent all of the single-byte
                // and double-byte code pages.  Code page 54936 is GB18030.  Finally we check that the encoding's assembly
                // is mscorlib, so we don't get any weird user encodings that happen to set a code page less than 50000. 
                if (!(value is ASCIIEncoding || value is UTF8Encoding || value is UnicodeEncoding || value is UTF32Encoding || 
                      ((value.CodePage < 50000 || value.CodePage == 54936)&& value.GetType().Assembly == typeof(String).Assembly))) {
                      
                    throw new ArgumentException(SR.GetString(SR.NotSupportedEncoding, value.WebName), "value");
                }
                
                encoding = value; 
                decoder = encoding.GetDecoder();
                
                // This is somewhat of an approximate guesstimate to get the max char[] size needed to encode a single character
                maxByteCountForSingleChar = encoding.GetMaxByteCount(1); 
                singleCharBuffer = null;
            }
        }

        [Browsable(true),
        DefaultValue(defaultHandshake),
        MonitoringDescription(SR.Handshake)]
        public Handshake Handshake
        {
            get
            {
                return handshake;
            }
            set
            {
                if (value < Handshake.None || value > Handshake.RequestToSendXOnXOff)
                    throw new ArgumentOutOfRangeException("Handshake", SR.GetString(SR.ArgumentOutOfRange_Enum));
                
                if (IsOpen)
                    internalSerialStream.Handshake = value;
                handshake = value;
            }
        }

        // true only if the Open() method successfully called on this SerialPort object, without Close() being called more recently.
        [Browsable(false)]
        public bool IsOpen
        {
            get { return (internalSerialStream != null && internalSerialStream.IsOpen); }
        }

        [
            Browsable(false),
            DefaultValue(defaultNewLine),
            MonitoringDescription(SR.NewLine)
        ]
        public string NewLine {
            get { return newLine; }
            set { 
                if (value == null)
                    throw new ArgumentNullException();
                if (value.Length == 0)
                    throw new ArgumentException(SR.GetString(SR.InvalidNullEmptyArgument, "NewLine"));

                newLine = value;
            }
        }

        [Browsable(true),
        DefaultValue(defaultParity),
        MonitoringDescription(SR.Parity)]
        public Parity Parity
        {
            get
            {

                return parity;
            }
            set
            {
                if (value < Parity.None || value > Parity.Space)
                    throw new ArgumentOutOfRangeException("Parity", SR.GetString(SR.ArgumentOutOfRange_Enum));
                
                if (IsOpen)
                    internalSerialStream.Parity = value;
                parity = value;
            }
        }

        [Browsable(true),
        DefaultValue(defaultParityReplace),
        MonitoringDescription(SR.ParityReplace)]
        public byte ParityReplace
        {
            get {   return parityReplace;   }
            set
            {
                if (IsOpen)
                    internalSerialStream.ParityReplace = value;
                parityReplace = value;
            }
        }



        // Note that the communications port cannot be meaningfully re-set when the port is open,
        // and so once set by the constructor becomes read-only.
        [Browsable(true),
        DefaultValue(defaultPortName),
        MonitoringDescription(SR.PortName)]
        public string PortName
        {
            get { 
                return portName; 
            }
            [ResourceExposure(ResourceScope.Machine)]
            set
            {
                if (value == null)
                    throw new ArgumentNullException("PortName");
                if (value.Length ==0)
                    throw new ArgumentException(SR.GetString(SR.PortNameEmpty_String), "PortName");
                
                // disallow access to device resources beginning with @"\\", instead requiring "COM2:", etc.
                // Note that this still allows freedom in mapping names to ports, etc., but blocks security leaks.
                if (value.StartsWith("\\\\", StringComparison.Ordinal))
                    throw new ArgumentException(SR.GetString(SR.Arg_SecurityException), "PortName");
                
                if (IsOpen)
                    throw new InvalidOperationException(SR.GetString(SR.Cant_be_set_when_open, "PortName"));
                portName = value;
            }
        }

        [Browsable(true),
        DefaultValue(defaultReadBufferSize),
        MonitoringDescription(SR.ReadBufferSize)]
        public int ReadBufferSize {
            get {
                return readBufferSize;
            }
            set {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value");

                if (IsOpen)
                    throw new InvalidOperationException(SR.GetString(SR.Cant_be_set_when_open, "value"));

                readBufferSize = value;
            }
        }

        // timeout for all read operations.  May be set to SerialPort.InfiniteTimeout, 0, or any positive value
        [Browsable(true),
        DefaultValue(SerialPort.InfiniteTimeout),
        MonitoringDescription(SR.ReadTimeout)]
        public int ReadTimeout
        {
            get
            {
                return readTimeout;
            }

            set
            {
                if (value < 0 && value != SerialPort.InfiniteTimeout)
                    throw new ArgumentOutOfRangeException("ReadTimeout", SR.GetString(SR.ArgumentOutOfRange_Timeout));
                
                if (IsOpen)
                    internalSerialStream.ReadTimeout = value;
                readTimeout = value;
            }
        }

        [Browsable(true),
        DefaultValue(defaultReceivedBytesThreshold),
        MonitoringDescription(SR.ReceivedBytesThreshold)]
        // If we have the SerialData.Chars event set, this property indicates the number of bytes necessary
        // to exist in our buffers before the event is thrown.  This is useful if we expect to receive n-byte
        // packets and can only act when we have this many, etc.
        public int ReceivedBytesThreshold
        {
            get
            {
                return receivedBytesThreshold;
            }

            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("ReceivedBytesThreshold",
                        SR.GetString(SR.ArgumentOutOfRange_NeedPosNum));
                receivedBytesThreshold = value;

                if (IsOpen) {
                    // fake the call to our event handler in case the threshold has been set lower
                    // than how many bytes we currently have.
                    SerialDataReceivedEventArgs args = new SerialDataReceivedEventArgs(SerialData.Chars);
                    CatchReceivedEvents(this, args);
                }
            }
        }

        [Browsable(true),
        DefaultValue(defaultRtsEnable),
        MonitoringDescription(SR.RtsEnable)]
        public bool RtsEnable
        {
            get
            {
                if (IsOpen)
                    rtsEnable = internalSerialStream.RtsEnable;
                
                return rtsEnable;
            }
            set
            {
                if (IsOpen)
                    internalSerialStream.RtsEnable = value;
                rtsEnable = value;
            }
        }

        // StopBits represented in C# as StopBits enum type and in Win32 as an integer 1, 2, or 3.
        [Browsable(true),
        DefaultValue(defaultStopBits),
        MonitoringDescription(SR.StopBits)
        ]
        public StopBits StopBits
        {
            get
            {
                return stopBits;
            }
            set
            {
                // this range check looks wrong, but it really is correct.  One = 1, Two = 2, and OnePointFive = 3
                if (value < StopBits.One || value > StopBits.OnePointFive)
                    throw new ArgumentOutOfRangeException("StopBits", SR.GetString(SR.ArgumentOutOfRange_Enum));
                
                if (IsOpen)
                    internalSerialStream.StopBits = value;
                stopBits = value;
            }
        }

        [Browsable(true),
        DefaultValue(defaultWriteBufferSize),
        MonitoringDescription(SR.WriteBufferSize)]
        public int WriteBufferSize {
            get {
                return writeBufferSize;
            }
            set {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value");

                if (IsOpen)
                    throw new InvalidOperationException(SR.GetString(SR.Cant_be_set_when_open, "value"));

                writeBufferSize = value;
            }
        }

        // timeout for all write operations.  May be set to SerialPort.InfiniteTimeout or any positive value
        [Browsable(true),
        DefaultValue(defaultWriteTimeout),
        MonitoringDescription(SR.WriteTimeout)]
        public int WriteTimeout
        {
            get
            {
                return writeTimeout;
            }
            set
            {
                if (value <= 0 && value != SerialPort.InfiniteTimeout)
                    throw new ArgumentOutOfRangeException("WriteTimeout", SR.GetString(SR.ArgumentOutOfRange_WriteTimeout));
                
                if (IsOpen)
                    internalSerialStream.WriteTimeout = value;
                writeTimeout = value;
            }
        }



        // -------- SECTION: constructors -----------------*
        public SerialPort(System.ComponentModel.IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            container.Add(this);
        }

        public SerialPort()
        {
        }

        // Non-design SerialPort constructors here chain, using default values for members left unspecified by parameters
        // Note: Calling SerialPort() does not open a port connection but merely instantiates an object.
        //     : A connection must be made using SerialPort's Open() method.
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public SerialPort(string portName) : this (portName, defaultBaudRate, defaultParity, defaultDataBits, defaultStopBits)
        {
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public SerialPort(string portName, int baudRate) : this (portName, baudRate, defaultParity, defaultDataBits, defaultStopBits)
        {
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public SerialPort(string portName, int baudRate, Parity parity) : this (portName, baudRate, parity, defaultDataBits, defaultStopBits)
        {
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public SerialPort(string portName, int baudRate, Parity parity, int dataBits) : this (portName, baudRate, parity, dataBits, defaultStopBits)
        {
        }

        // all the magic happens in the call to the instance's .Open() method.
        // Internally, the SerialStream constructor opens the file handle, sets the device
        // control block and associated Win32 structures, and begins the event-watching cycle.
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public SerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            this.PortName = portName;
            this.BaudRate = baudRate;
            this.Parity = parity;
            this.DataBits = dataBits;
            this.StopBits = stopBits;
        }

        // Calls internal Serial Stream's Close() method on the internal Serial Stream.
        public void Close()
        {
            Dispose();
        }

        protected override void Dispose( bool disposing )
        {
            if( disposing ) {
                if (IsOpen) {
                    internalSerialStream.Flush();
                    internalSerialStream.Close();
                    internalSerialStream = null;
                }
            }
            base.Dispose( disposing );
        }

        public void DiscardInBuffer()
        {
            if (!IsOpen)
                throw new InvalidOperationException(SR.GetString(SR.Port_not_open));
            internalSerialStream.DiscardInBuffer();
            readPos = readLen = 0;
        }

        public void DiscardOutBuffer()
        {
            if (!IsOpen)
                throw new InvalidOperationException(SR.GetString(SR.Port_not_open));
            internalSerialStream.DiscardOutBuffer();
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static string[] GetPortNames() {
            RegistryKey baseKey = null;
            RegistryKey serialKey = null;
            
            String[] portNames = null;

            RegistryPermission registryPermission = new RegistryPermission(RegistryPermissionAccess.Read, 
                                    @"HKEY_LOCAL_MACHINE\HARDWARE\DEVICEMAP\SERIALCOMM");                                    
            registryPermission.Assert();

            try {
                baseKey = Registry.LocalMachine;
                serialKey = baseKey.OpenSubKey(@"HARDWARE\DEVICEMAP\SERIALCOMM", false);

                if (serialKey != null) {

                    string[] deviceNames = serialKey.GetValueNames();
                    portNames = new String[deviceNames.Length];

                    for (int i=0; i<deviceNames.Length; i++)
                        portNames[i] = (string)serialKey.GetValue(deviceNames[i]);    
                }
            }
            finally {
                if (baseKey != null) 
                    baseKey.Close();
                
                if (serialKey != null) 
                    serialKey.Close();
                
                RegistryPermission.RevertAssert();
            }

            // If serialKey didn't exist for some reason
            if (portNames == null) 
                portNames = new String[0];

            return portNames;
        }

#if NYT
        public static string[] GetPortNames() {
            if (Environment.OSVersion.Platform == PlatformID.Win32Windows)
                throw new PlatformNotSupportedException(SR.GetString(SR.NotSupportedOS));
            
            // Get all the registered serial device names
            RegistryPermission registryPermission = new RegistryPermission(PermissionState.Unrestricted);
            registryPermission.Assert();

            RegistryKey baseKey = null;
            RegistryKey serialKey = null;
            
            Hashtable portNames = new Hashtable(10);

            try {
                baseKey = Registry.LocalMachine;
                serialKey = baseKey.OpenSubKey(@"HARDWARE\DEVICEMAP\SERIALCOMM", true);

                if (serialKey != null) {

                    string[] devices = serialKey.GetValueNames();
                    for (int j=0; j<devices.Length; j++) {
                        portNames.Add(devices[j], null);   
                    }
                }
            }
            finally {
                if (baseKey != null) 
                    baseKey.Close();
                
                if (serialKey != null) 
                    serialKey.Close();
                
                RegistryPermission.RevertAssert();
            }
            
            // Get all the MS-DOS names on the local machine 
            //(sending null for lpctstrName gets all the names)
            int dataSize;
            char[] buffer = CallQueryDosDevice(null, out dataSize); 

            // From QueryDosDevice, we get back a long string where the names are delimited by \0 and the end
            // of the string is indicated by two \0s
            ArrayList names = new ArrayList();
            ArrayList deviceNames = new ArrayList();

            int i=0;
            while (i < dataSize) {
                // Walk through the buffer building a name until we hit the delimiter \0
                int start = i;
                while (buffer[i] != '\0') {
                    i++;
                }

                if (i != start) {
                    // We now have an MS-DOS name (the common name). We call QueryDosDevice again with
                    // this name to get the underlying system name mapped to the MS-DOS name. 
                    string currentName = (new String(buffer, start, i-start)).Trim();
                    int nameSize;
                    char[] nameBuffer = CallQueryDosDevice(currentName, out nameSize);

                    // If we got a system name, see if it's a serial port name. If it is, add the common name
                    // to our list
                    if (nameSize > 0) {
                        // internalName will include the trailing null chars as well as any additional
                        // names that may get returned.  This is ok, since we are only interested in the
                        // first name and we can use StartsWith. 
                        string internalName = new string(nameBuffer, 0, nameSize-2).Trim();
                        
                        if (internalName.StartsWith(SERIAL_NAME) || portNames.ContainsKey(internalName)) {
                            names.Add(currentName);
                            deviceNames.Add(internalName);
                        }
                    }
                }
                i++;
            }
            
            string[] namesArray = new String[names.Count];
            names.CopyTo(namesArray);

            string[] deviceNamesArray = new String[deviceNames.Count];
            deviceNames.CopyTo(deviceNamesArray);

            // sort the common names according to their actual device ordering
            Array.Sort(deviceNamesArray, namesArray, Comparer.DefaultInvariant);
            
            return namesArray;
        }
        
        private static unsafe char[] CallQueryDosDevice(string name, out int dataSize) {
            char[] buffer = new char[1024];

            fixed (char *bufferPtr = buffer) {
                dataSize =  UnsafeNativeMethods.QueryDosDevice(name, buffer, buffer.Length);
                while (dataSize <= 0) {
                    int lastError = Marshal.GetLastWin32Error();
                    if (lastError == NativeMethods.ERROR_INSUFFICIENT_BUFFER || lastError == NativeMethods.ERROR_MORE_DATA) {
                        buffer = new char[buffer.Length * 2];
                        dataSize = UnsafeNativeMethods.QueryDosDevice(null, buffer, buffer.Length);
                    }
                    else {
                        throw new Win32Exception();
                    }
                }
            }
            return buffer;
        }
#endif
        
        // SerialPort is open <=> SerialPort has an associated SerialStream.
        // The two statements are functionally equivalent here, so this method basically calls underlying Stream's
        // constructor from the main properties specified in SerialPort: baud, stopBits, parity, dataBits,
        // comm portName, handshaking, and timeouts.
        [ResourceExposure(ResourceScope.None)]  // Look at Name property
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        public void Open()
        {
            if (IsOpen)
                throw new InvalidOperationException(SR.GetString(SR.Port_already_open));

            // Demand unmanaged code permission
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();

            internalSerialStream = new SerialStream(portName, baudRate, parity, dataBits, stopBits, readTimeout,
                writeTimeout, handshake, dtrEnable, rtsEnable, discardNull, parityReplace);

            internalSerialStream.SetBufferSizes(readBufferSize, writeBufferSize);

            internalSerialStream.ErrorReceived += new SerialErrorReceivedEventHandler(CatchErrorEvents);
            internalSerialStream.PinChanged += new SerialPinChangedEventHandler(CatchPinChangedEvents);
            internalSerialStream.DataReceived += new SerialDataReceivedEventHandler(CatchReceivedEvents);
        }

        // Read Design pattern:
        //  : ReadChar() returns the first available full char if found before, throws TimeoutExc if timeout.
        //  : Read(byte[] buffer..., int count) returns all data available before read timeout expires up to *count* bytes
        //  : Read(char[] buffer..., int count) returns all data available before read timeout expires up to *count* chars.
        //  :                                   Note, this does not return "half-characters".
        //  : ReadByte() is the binary analogue of the first one.
        //  : ReadLine(): returns null string on timeout, saves received data in buffer
        //  : ReadAvailable(): returns all full characters which are IMMEDIATELY available.

        public int Read(byte[] buffer, int offset, int count)
        {
            if (!IsOpen)
                throw new InvalidOperationException(SR.GetString(SR.Port_not_open));
            if (buffer==null)
                throw new ArgumentNullException("buffer", SR.GetString(SR.ArgumentNull_Buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNumRequired));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNumRequired));
            if (buffer.Length - offset < count)
                throw new ArgumentException(SR.GetString(SR.Argument_InvalidOffLen));
            int bytesReadToBuffer=0;

            // if any bytes available in internal buffer, return those without calling any read ops.
            if (CachedBytesToRead >= 1)
            {
                bytesReadToBuffer = Math.Min(CachedBytesToRead, count);
                Buffer.BlockCopy(inBuffer, readPos, buffer, offset, bytesReadToBuffer);
                readPos += bytesReadToBuffer;
                if (bytesReadToBuffer == count) {
                    if (readPos == readLen) readPos = readLen = 0;  // just a check to see if we can reset buffer
                    return count;
                }

                // if we have read some bytes but there's none immediately available, return.
                if (BytesToRead == 0) 
                    return bytesReadToBuffer;
            }

            Debug.Assert(CachedBytesToRead == 0, "there should be nothing left in our internal buffer");
            readLen = readPos = 0;

            int bytesLeftToRead = count - bytesReadToBuffer;

            // request to read the requested number of bytes to fulfill the contract,
            // doesn't matter if we time out.  We still return all the data we have available.
            bytesReadToBuffer += internalSerialStream.Read(buffer, offset + bytesReadToBuffer, bytesLeftToRead);

            decoder.Reset();
            return bytesReadToBuffer;
        }

        // publicly exposed "ReadOneChar"-type: Read()
        // reads one full character from the stream
        public int ReadChar()
        {
            if (!IsOpen)
                throw new InvalidOperationException(SR.GetString(SR.Port_not_open));

            return ReadOneChar(readTimeout);
        }

        // gets next available full character, which may be from the buffer, the stream, or both.
        // this takes size^2 time at most, where *size* is the maximum size of any one character in an encoding.
        // The user can call Read(1) to mimic this functionality.

        // We can replace ReadOneChar with Read at some point
        private int ReadOneChar(int timeout)
        {
            int nextByte;
            int timeUsed = 0;
            Debug.Assert(IsOpen, "ReadOneChar - port not open");

            // case 1: we have >= 1 character in the internal buffer.
            if (decoder.GetCharCount(inBuffer, readPos, CachedBytesToRead) != 0)
            {
                int beginReadPos = readPos;
                // get characters from buffer.
                do
                {
                    readPos++;
                } while (decoder.GetCharCount(inBuffer, beginReadPos, readPos - beginReadPos) < 1);

                try {
                    decoder.GetChars(inBuffer, beginReadPos, readPos - beginReadPos, oneChar, 0);
                }
                catch {

                    // Handle surrogate chars correctly, restore readPos
                    readPos = beginReadPos;
                    throw;
                }
                return oneChar[0];
            }
            else
            {

                // need to return immediately.
                if (timeout == 0) {
                    // read all bytes in the serial driver in here.  Make sure we ask for at least 1 byte
                    // so that we get the proper timeout behavior
                    int bytesInStream = internalSerialStream.BytesToRead; 
                    if (bytesInStream == 0)
                        bytesInStream = 1;
                    MaybeResizeBuffer(bytesInStream);
                    readLen += internalSerialStream.Read(inBuffer, readLen, bytesInStream); // read all immediately avail.

                    // If what we have in the buffer is not enough, throw TimeoutExc
                    // if we are reading surrogate char then ReadBufferIntoChars 
                    // will throw argexc and that is okay as readPos is not altered
                    if (ReadBufferIntoChars(oneChar, 0, 1, false) == 0) 
                        throw new TimeoutException();
                    else 
                        return oneChar[0];
                }

                // case 2: we need to read from outside to find this.
                // timeout is either infinite or positive.
                int startTicks = Environment.TickCount;
                do
                {
                    if (timeout == SerialPort.InfiniteTimeout)
                        nextByte = internalSerialStream.ReadByte(InfiniteTimeout);
                    else if (timeout - timeUsed >= 0) {
                        nextByte = internalSerialStream.ReadByte(timeout - timeUsed);
                        timeUsed = Environment.TickCount - startTicks;
                    }
                    else
                        throw new TimeoutException();

                    MaybeResizeBuffer(1);
                    inBuffer[readLen++] = (byte) nextByte;  // we must add to the end of the buffer
                } while (decoder.GetCharCount(inBuffer, readPos, readLen - readPos) < 1);
            }

            // If we are reading surrogate char then this will throw argexc 
            // we need not deal with that exc because we have not altered readPos yet.
            decoder.GetChars(inBuffer, readPos, readLen - readPos, oneChar, 0);

            // Everything should be out of inBuffer now.  We'll just reset the pointers. 
            readLen = readPos = 0;
            return oneChar[0];
        }

        // Will return 'n' (1 < n < count) characters (or) TimeoutExc
        public int Read(char[] buffer, int offset, int count)
        {
            if (!IsOpen)
                throw new InvalidOperationException(SR.GetString(SR.Port_not_open));
            if (buffer==null)
                throw new ArgumentNullException("buffer", SR.GetString(SR.ArgumentNull_Buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNumRequired));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNumRequired));
            if (buffer.Length - offset < count)
                throw new ArgumentException(SR.GetString(SR.Argument_InvalidOffLen));

            return InternalRead(buffer, offset, count, readTimeout, false);
        }

        private int InternalRead(char[] buffer, int offset, int count, int timeout, bool countMultiByteCharsAsOne)
        {
            Debug.Assert(IsOpen, "port not open!");
            Debug.Assert(buffer!=null, "invalid buffer!");
            Debug.Assert(offset >= 0, "invalid offset!");
            Debug.Assert(count >= 0, "invalid count!");
            Debug.Assert(buffer.Length - offset >= count, "invalid offset/count!");

            if (count == 0) return 0;   // immediately return on zero chars desired.  This simplifies things later.

            // Get the startticks before we read the underlying stream
            int startTicks = Environment.TickCount;
            
            // read everything else into internal buffer, which we know we can do instantly, and see if we NOW have enough.
            int bytesInStream = internalSerialStream.BytesToRead;
            MaybeResizeBuffer(bytesInStream);
            readLen += internalSerialStream.Read(inBuffer, readLen, bytesInStream);    // should execute instantaneously.

            int charsWeAlreadyHave = decoder.GetCharCount(inBuffer, readPos, CachedBytesToRead); // full chars already in our buffer
            if (charsWeAlreadyHave > 0)
            {
                // we found some chars after reading everything the SerialStream had to offer.  We'll return what we have
                // rather than wait for more. 
                return ReadBufferIntoChars(buffer, offset, count, countMultiByteCharsAsOne);
            }

            if (timeout == 0) 
                throw new TimeoutException();

            // else: we need to do incremental reads from the stream.
            // -----
            // our internal algorithm for finding exactly n characters is a bit complicated, but must overcome the
            // hurdle of NEVER READING TOO MANY BYTES from the Stream, since we can time out.  A variable-length encoding
            // allows anywhere between minimum and maximum bytes per char times number of chars to be the exactly correct
            // target, and we have to take care not to overuse GetCharCount().  The problem is that GetCharCount() will never tell
            // us if we've read "half" a character in our current set of collected bytes; it underestimates.
            // size = maximum bytes per character in the encoding.  n = number of characters requested.
            // Solution I: Use ReadOneChar() to read successive characters until we get to n.
            // Read calls: size * n; GetCharCount calls: size * n; each byte "counted": size times.
            // Solution II: Use a binary reduction and backtracking to reduce the number of calls.
            // Read calls: size * log n; GetCharCount calls: size * log n; each byte "counted": size * (log n) / n times.
            // We use the second, more complicated solution here.  Note log is actually log_(size/size - 1)...


            // we need to read some from the stream
            // read *up to* the maximum number of bytes from the stream
            // we can read more since we receive everything instantaneously, and we don't have enough,
            // so when we do receive any data, it will be necessary and sufficient.

            int justRead;
            int maxReadSize = Encoding.GetMaxByteCount(count); 
            do {
                MaybeResizeBuffer(maxReadSize);
                
                readLen += internalSerialStream.Read(inBuffer, readLen, maxReadSize);
                justRead = ReadBufferIntoChars(buffer, offset, count, countMultiByteCharsAsOne);
                if (justRead > 0) {
                    return justRead;
                }
            } while (timeout == SerialPort.InfiniteTimeout || (timeout - GetElapsedTime(Environment.TickCount, startTicks) > 0));

            // must've timed out w/o getting a character.
            throw new TimeoutException();
        }

        // ReadBufferIntoChars reads from Serial Port's inBuffer up to *count* chars and
        // places them in *buffer* starting at *offset*.
        // This does not call any stream Reads, and so takes "no time".
        // If the buffer specified is insufficient to accommodate surrogate characters
        // the call to underlying Decoder.GetChars will throw argexc. 
        private int ReadBufferIntoChars(char[] buffer, int offset, int count, bool countMultiByteCharsAsOne)
        {
            Debug.Assert(count != 0, "Count should never be zero.  We will probably see bugs further down if count is 0.");

            int bytesToRead = Math.Min(count, CachedBytesToRead);

            // There are lots of checks to determine if this really is a single byte encoding with no
            // funky fallbacks that would make it not single byte
            DecoderReplacementFallback fallback = encoding.DecoderFallback as DecoderReplacementFallback;
            if (encoding.IsSingleByte && encoding.GetMaxCharCount(bytesToRead) == bytesToRead && 
                fallback != null && fallback.MaxCharCount == 1)
            {   
                // kill ASCII/ANSI encoding easily.
                // read at least one and at most *count* characters
                decoder.GetChars(inBuffer, readPos, bytesToRead, buffer, offset);

                readPos += bytesToRead;
                if (readPos == readLen) readPos = readLen = 0;
                return bytesToRead;
            }
            else
            {
                //
                // We want to turn inBuffer into at most count chars.  This algorithm basically works like this:
                // 1) Take the largest step possible that won't give us too many chars
                // 2) If we find some chars, walk backwards until we find exactly how many bytes
                //    they occupy.  lastFullCharPos points to the end of the full chars.
                // 3) if we don't have enough chars for the buffer, goto #1

                int totalBytesExamined = 0; // total number of Bytes in inBuffer we've looked at
                int totalCharsFound = 0;     // total number of chars we've found in inBuffer, totalCharsFound <= totalBytesExamined
                int currentBytesToExamine; // the number of additional bytes to examine for characters
                int currentCharsFound; // the number of additional chars found after examining currentBytesToExamine extra bytes
                int lastFullCharPos = readPos; // first index AFTER last full char read, capped at ReadLen.
                do
                {
                    currentBytesToExamine = Math.Min(count - totalCharsFound, readLen - readPos - totalBytesExamined);
                    if (currentBytesToExamine <= 0)
                        break;

                    totalBytesExamined += currentBytesToExamine;
                    // recalculate currentBytesToExamine so that it includes leftover bytes from the last iteration. 
                    currentBytesToExamine = readPos + totalBytesExamined - lastFullCharPos; 

                    // make sure we don't go beyond the end of the valid data that we have. 
                    Debug.Assert((lastFullCharPos + currentBytesToExamine) <= readLen, "We should never be attempting to read more bytes than we have");
                    
                    currentCharsFound = decoder.GetCharCount(inBuffer, lastFullCharPos, currentBytesToExamine);

                    if (currentCharsFound > 0)
                    {
                        if ((totalCharsFound + currentCharsFound) > count) {

                            // Multibyte unicode sequence (possibly surrogate chars) 
                            // at the end of the buffer. We should not split the sequence, 
                            // instead return with less chars now and defer reading them 
                            // until next time
                            if (!countMultiByteCharsAsOne) 
                                break;

                            // If we are here it is from ReadTo which attempts to read one logical character 
                            // at a time. The supplied singleCharBuffer should be large enough to accommodate 
                            // this multi-byte char
                            Debug.Assert((buffer.Length - offset - totalCharsFound) >= currentCharsFound, "internal buffer to read one full unicode char sequence is not sufficient!");
                        }

                        // go backwards until we know we have a full set of currentCharsFound bytes with no extra lead-bytes.
                        int foundCharsByteLength = currentBytesToExamine;
                        do
                        {
                            foundCharsByteLength--;
                        } while (decoder.GetCharCount(inBuffer, lastFullCharPos, foundCharsByteLength) == currentCharsFound);

                        // Fill into destination buffer all the COMPLETE characters we've read.
                        // If the buffer specified is insufficient to accommodate surrogate character
                        // the call to underlying Decoder.GetChars will throw argexc. We need not 
                        // deal with this exc because we have not altered readPos yet.
                        decoder.GetChars(inBuffer, lastFullCharPos, foundCharsByteLength + 1, buffer, offset + totalCharsFound);
                        lastFullCharPos = lastFullCharPos + foundCharsByteLength + 1; // update the end position of last known char.
                    }

                    totalCharsFound += currentCharsFound;
                } while ((totalCharsFound < count) && (totalBytesExamined < CachedBytesToRead));

                readPos = lastFullCharPos;

                if (readPos == readLen) readPos = readLen = 0;
                return totalCharsFound;
            }
        }

        public int ReadByte()
        {
            if (!IsOpen)
                throw new InvalidOperationException(SR.GetString(SR.Port_not_open));
            if (readLen != readPos)         // stuff left in buffer, so we can read from it
                return inBuffer[readPos++];

            decoder.Reset();
            return internalSerialStream.ReadByte(); // otherwise, ask the stream.
        }

        public string ReadExisting()
        {
            if (!IsOpen)
                throw new InvalidOperationException(SR.GetString(SR.Port_not_open));

            byte [] bytesReceived = new byte[BytesToRead];

            if (readPos < readLen)
            {           // stuff in internal buffer
                Buffer.BlockCopy(inBuffer, readPos, bytesReceived, 0, CachedBytesToRead);
            }
            internalSerialStream.Read(bytesReceived, CachedBytesToRead, bytesReceived.Length - (CachedBytesToRead));    // get everything
            // Read full characters and leave partial input in the buffer. Encoding.GetCharCount doesn't work because
            // it returns fallback characters on partial input, meaning that it overcounts. Instead, we use 
            // GetCharCount from the decoder and tell it to preserve state, so that it returns the count of full 
            // characters. Note that we don't actually want it to preserve state, so we call the decoder as if it's 
            // preserving state and then call Reset in between calls. This uses a local decoder instead of the class 
            // member decoder because that one may preserve state across SerialPort method calls.
            Decoder localDecoder = Encoding.GetDecoder();
            int numCharsReceived = localDecoder.GetCharCount(bytesReceived, 0, bytesReceived.Length);
            int lastFullCharIndex = bytesReceived.Length;
            
            if (numCharsReceived == 0)
            {
                Buffer.BlockCopy(bytesReceived, 0, inBuffer, 0, bytesReceived.Length); // put it all back!
                // don't change readPos. --> readPos == 0?
                readPos = 0;
                readLen = bytesReceived.Length;
                return "";
            }

            do 
            {
                localDecoder.Reset();
                lastFullCharIndex--;
            } while (localDecoder.GetCharCount(bytesReceived, 0, lastFullCharIndex) == numCharsReceived);

            readPos = 0;
            readLen = bytesReceived.Length - (lastFullCharIndex + 1);

            Buffer.BlockCopy(bytesReceived, lastFullCharIndex + 1, inBuffer, 0, bytesReceived.Length - (lastFullCharIndex + 1));
            return Encoding.GetString(bytesReceived, 0, lastFullCharIndex + 1);
        }

        public string ReadLine() {
            return ReadTo(NewLine);
        }

        public string ReadTo(string value)
        {
            if (!IsOpen)
                throw new InvalidOperationException(SR.GetString(SR.Port_not_open));
            if (value == null)
                throw new ArgumentNullException("value");
            if (value.Length == 0)
                throw new ArgumentException(SR.GetString(SR.InvalidNullEmptyArgument, "value"));
                
            int startTicks = Environment.TickCount;
            int numCharsRead;
            int timeUsed = 0;
            int timeNow;
            StringBuilder currentLine = new StringBuilder();
            char lastValueChar = value[value.Length-1];

            // for timeout issues, best to read everything already on the stream into our buffers.
            // first make sure inBuffer is big enough
            int bytesInStream = internalSerialStream.BytesToRead;
            MaybeResizeBuffer(bytesInStream);

            readLen += internalSerialStream.Read(inBuffer, readLen, bytesInStream);
            int beginReadPos = readPos;

            if (singleCharBuffer == null) {
                // This is somewhat of an approximate guesstimate to get the max char[] size needed to encode a single character
                singleCharBuffer = new char[maxByteCountForSingleChar];
            }

            try {
                while (true)
                {
                    if(readTimeout == InfiniteTimeout) {
                        numCharsRead = InternalRead(singleCharBuffer, 0, 1, readTimeout, true);
                    }
                    else if (readTimeout - timeUsed >= 0) {
                        timeNow = Environment.TickCount;
                        numCharsRead = InternalRead(singleCharBuffer, 0, 1, readTimeout - timeUsed, true);
                        timeUsed += Environment.TickCount - timeNow;
                    }
                    else 
                        throw new TimeoutException();
                    
#if _DEBUG
                    if (numCharsRead > 1) {
                        for (int i=0; i<numCharsRead; i++)
                            Debug.Assert((Char.IsSurrogate(singleCharBuffer[i])), "number of chars read should be more than one only for surrogate characters!");
                    }
#endif
                    Debug.Assert((numCharsRead > 0), "possible bug in ReadBufferIntoChars, reading surrogate char?");
                    currentLine.Append(singleCharBuffer, 0, numCharsRead);
                    
                    if (lastValueChar == (char) singleCharBuffer[numCharsRead-1] && (currentLine.Length >= value.Length)) {
                        // we found the last char in the value string.  See if the rest is there.  No need to
                        // recompare the last char of the value string.
                        bool found = true;
                        for (int i=2; i<=value.Length; i++) {
                            if (value[value.Length-i] != currentLine[currentLine.Length-i]) {
                                found = false;
                                break;
                            }
                        }

                        if (found) {
                            // we found the search string.  Exclude it from the return string.
                            string ret = currentLine.ToString(0, currentLine.Length - value.Length);
                            if (readPos == readLen) readPos = readLen = 0;
                            return ret;
                        }
                    }
                }
            }
            catch {
                // We probably got here due to timeout. 
                // We will try our best to restore the internal states, it's tricky!
                
                // 0) Save any existing data
                // 1) Restore readPos to the original position upon entering ReadTo 
                // 2) Set readLen to the number of bytes read since entering ReadTo
                // 3) Restore inBuffer so that it contains the bytes from currentLine, resizing if necessary.
                // 4) Append the buffer with any saved data from 0) 
                
                byte[] readBuffer = encoding.GetBytes(currentLine.ToString());
                
                // We will compact the data by default
                if (readBuffer.Length > 0) {
                    int bytesToSave = CachedBytesToRead;
                    byte[] savBuffer = new byte[bytesToSave];
                        
                    if (bytesToSave > 0) 
                        Buffer.BlockCopy(inBuffer, readPos, savBuffer, 0, bytesToSave);

                    readPos = 0;
                    readLen = 0;

                    MaybeResizeBuffer(readBuffer.Length + bytesToSave);

                    Buffer.BlockCopy(readBuffer, 0, inBuffer, readLen, readBuffer.Length);
                    readLen += readBuffer.Length;

                    if (bytesToSave > 0) {
                        Buffer.BlockCopy(savBuffer, 0, inBuffer, readLen, bytesToSave);
                        readLen += bytesToSave; 
                    }
                }
                
                throw;
            }
        }
        
        // Writes string to output, no matter string's length.
        public void Write(string text)
        {
            if (!IsOpen)
                throw new InvalidOperationException(SR.GetString(SR.Port_not_open));
            if (text == null)
                throw new ArgumentNullException("text");
            if (text.Length == 0) return;
            byte [] bytesToWrite;

            bytesToWrite = encoding.GetBytes(text);

            internalSerialStream.Write(bytesToWrite, 0, bytesToWrite.Length, writeTimeout);
        }

        // encoding-dependent Write-chars method.
        // Probably as performant as direct conversion from ASCII to bytes, since we have to cast anyway (we can just call GetBytes)
        public void Write(char[] buffer, int offset, int count) {
            if (!IsOpen)
                throw new InvalidOperationException(SR.GetString(SR.Port_not_open));
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNumRequired));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNumRequired));
            if (buffer.Length - offset < count)
                throw new ArgumentException(SR.GetString(SR.Argument_InvalidOffLen));

            if (buffer.Length == 0) return;

            byte [] byteArray = Encoding.GetBytes(buffer,offset, count);
            Write(byteArray, 0, byteArray.Length);

        }

        // Writes a specified section of a byte buffer to output.
        public void Write(byte[] buffer, int offset, int count)
        {
            if (!IsOpen)
                throw new InvalidOperationException(SR.GetString(SR.Port_not_open));
            if (buffer==null)
                throw new ArgumentNullException("buffer", SR.GetString(SR.ArgumentNull_Buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNumRequired));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNumRequired));
            if (buffer.Length - offset < count)
                throw new ArgumentException(SR.GetString(SR.Argument_InvalidOffLen));
            if (buffer.Length == 0) return;

            internalSerialStream.Write(buffer, offset, count, writeTimeout);
        }

        public void WriteLine(string text) {
            Write(text + NewLine);
        }


        // ----- SECTION: internal utility methods ----------------*

        // included here just to use the event filter to block unwanted invocations of the Serial Port's events.
        // Plus, this enforces the requirement on the received event that the number of buffered bytes >= receivedBytesThreshold
        private void CatchErrorEvents(object src, SerialErrorReceivedEventArgs e)
        {
            SerialErrorReceivedEventHandler eventHandler = ErrorReceived;
            SerialStream stream = internalSerialStream;

            if ((eventHandler != null) && (stream != null)){
                lock (stream) {
                    if (stream.IsOpen)
                        eventHandler(this, e);
                }
            }
        }

        private void CatchPinChangedEvents(object src, SerialPinChangedEventArgs e)
        {
            SerialPinChangedEventHandler eventHandler = PinChanged;
            SerialStream stream = internalSerialStream;

            if ((eventHandler != null) && (stream != null)){
                lock (stream) {
                    if (stream.IsOpen)
                        eventHandler(this, e);
                }
            }
        }

        private void CatchReceivedEvents(object src, SerialDataReceivedEventArgs e)
        {
            SerialDataReceivedEventHandler eventHandler = DataReceived;
            SerialStream stream = internalSerialStream;

            if ((eventHandler != null) && (stream != null)){
                lock (stream) {
                    // SerialStream might be closed between the time the event runner
                    // pumped this event and the time the threadpool thread end up 
                    // invoking this event handler. The above lock and IsOpen check 
                    // ensures that we raise the event only when the port is open

                    bool raiseEvent = false;
                    try {
                        raiseEvent = stream.IsOpen && (SerialData.Eof == e.EventType || BytesToRead >= receivedBytesThreshold);    
                    }
                    catch {
                        // Ignore and continue. SerialPort might have been closed already! 
                    }
                    finally {
                        if (raiseEvent)
                            eventHandler(this, e);  // here, do your reading, etc. 
                    }
                }
            }
        }

        private void CompactBuffer()
        {
            Buffer.BlockCopy(inBuffer, readPos, inBuffer, 0, CachedBytesToRead);
            readLen = CachedBytesToRead;
            readPos = 0;
        }

        // This method guarantees that our inBuffer is big enough.  The parameter passed in is
        // the number of bytes that our code is going to add to inBuffer.  MaybeResizeBuffer will 
        // do one of three things depending on how much data is already in the buffer and how 
        // much will be added:
        // 1) Nothing.  The current buffer is big enough to hold it all
        // 2) Compact the existing data and keep the current buffer. 
        // 3) Create a new, larger buffer and compact the existing data into it.
        private void MaybeResizeBuffer(int additionalByteLength)
        {
            // Case 1.  No action needed
            if (additionalByteLength + readLen <= inBuffer.Length)
                return;

            // Case 2.  Compact                
            if (CachedBytesToRead + additionalByteLength <= inBuffer.Length / 2)
                CompactBuffer();
            else {
                // Case 3.  Create a new buffer
                int newLength = Math.Max(CachedBytesToRead + additionalByteLength, inBuffer.Length * 2);

                Debug.Assert(inBuffer.Length >= readLen, "ResizeBuffer - readLen > inBuffer.Length");
                byte[] newBuffer = new byte[newLength];
                // only copy the valid data from inBuffer, and put it at the beginning of newBuffer.
                Buffer.BlockCopy(inBuffer, readPos, newBuffer, 0, CachedBytesToRead);
                readLen = CachedBytesToRead;
                readPos = 0;
                inBuffer = newBuffer;
            }
        }

        private static int GetElapsedTime(int currentTickCount, int startTickCount)
        {
            int elapsedTime = unchecked(currentTickCount - startTickCount);
            return (elapsedTime >= 0) ? (int)elapsedTime : Int32.MaxValue;
        }
    }
}
