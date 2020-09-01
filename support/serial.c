/* -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */

/* serial port functions
 *
 * Author: Chris Toshok <toshok@ximian.com>
 */

#include "map.h"
#include "mph.h"

#include <termios.h>
#include <unistd.h>
#include <fcntl.h>
#include <string.h>
#include <errno.h>
#if defined(HAVE_POLL_H)
#include <poll.h>
#elif defined(HAVE_SYS_POLL_H)
#include <sys/poll.h>
#endif
#include <sys/ioctl.h>

/* This is for ASYNC_*, serial_struct on linux */
#if defined(HAVE_LINUX_SERIAL_H)
#include <linux/serial.h>
#endif

#include <glib.h>

/* This is for FIONREAD on solaris */
#if defined(sun)
#include <sys/filio.h>
#endif

/* sys/time.h (for timeval) is required when using osx 10.3 (but not 10.4) */
/* IOKit is a private framework in iOS, so exclude there */
#if defined(__APPLE__) && !defined(HOST_IOS) && !defined(HOST_WATCHOS) && !defined(HOST_TVOS)
#define HAVE_IOKIT 1
#endif

#if defined(HAVE_IOKIT)
#include <sys/time.h>
#include <IOKit/IOKitLib.h>
#include <IOKit/serial/IOSerialKeys.h>
#include <IOKit/serial/ioss.h>
#include <IOKit/IOBSD.h>
#endif

/* This is a copy of System.IO.Ports.Handshake */
typedef enum {
	NoneHandshake = 0,
	XOnXOff = 1,
	RequestToSend = 2,
	RequestToSendXOnXOff = 3
} MonoHandshake;

/* This is a copy of System.IO.Ports.Parity */
typedef enum {
	NoneParity = 0,
	Odd = 1,
	Even = 2,
	Mark = 3,
	Space = 4
} MonoParity;

/* This is a copy of System.IO.Ports.StopBits */
typedef enum {
	NoneStopBits = 0,
	One = 1,
	Two = 2,
	OnePointFive = 3
} MonoStopBits;

/* This is a copy of System.IO.Ports.SerialSignal */
typedef enum {
	NoneSignal,
	Cd = 1, /* Carrier detect */
	Cts = 2, /* Clear to send */
	Dsr = 4, /* Data set ready */
	Dtr = 8, /* Data terminal ready */
	Rts = 16  /* Request to send */
} MonoSerialSignal;

/*
 * Silence the compiler, we do not need these prototypes to be public, since these are only
 * used by P/Invoke
 */

int              open_serial (char *devfile);
int              close_serial (int unix_fd);
guint32          read_serial (int fd, guchar *buffer, int offset, int count);
int              write_serial (int fd, guchar *buffer, int offset, int count, int timeout);
int              discard_buffer (int fd, gboolean input);
gint32           get_bytes_in_buffer (int fd, gboolean input);
gboolean         is_baud_rate_legal (int baud_rate);
int              setup_baud_rate (int baud_rate, gboolean *custom_baud_rate);
gboolean         set_attributes (int fd, int baud_rate, MonoParity parity, int dataBits, MonoStopBits stopBits, MonoHandshake handshake);
MonoSerialSignal get_signals (int fd, gint32 *error);
gint32           set_signal (int fd, MonoSerialSignal signal, gboolean value);
int              breakprop (int fd);
gboolean         poll_serial (int fd, gint32 *error, int timeout);
void            *list_serial_devices (void);

int
open_serial (char *devfile)
{
	int fd;
	fd = open (devfile, O_RDWR | O_NOCTTY | O_NONBLOCK);

	return fd;
}

int
close_serial (int unix_fd)
{
	// Linus writes: do not retry close after EINTR
	return close (unix_fd);
}

guint32
read_serial (int fd, guchar *buffer, int offset, int count)
{
	guint32 n;
 
	n = read (fd, buffer + offset, count);

	return (guint32) n;
}

int
write_serial (int fd, guchar *buffer, int offset, int count, int timeout)
{
	struct pollfd pinfo;
	guint32 n;

	pinfo.fd = fd;
	pinfo.events = POLLOUT;
	pinfo.revents = POLLOUT;

	n = count;

	while (n > 0)
	{
		ssize_t t;
			
		if (timeout != 0) {
			int c;
			
			while ((c = poll (&pinfo, 1, timeout)) == -1 && errno == EINTR)
				;
			if (c == -1)
				return -1;
		}		

		do {
			t = write (fd, buffer + offset, n);
		} while (t == -1 && errno == EINTR);

		if (t < 0)
			return -1;
		
		offset += t;
		n -= t; 
	}
 
	return 0;
}

int
discard_buffer (int fd, gboolean input)
{
	return tcflush(fd, input ? TCIFLUSH : TCOFLUSH);
}

gint32
get_bytes_in_buffer (int fd, gboolean input)
{
#if defined(__HAIKU__)
	/* FIXME: Haiku doesn't support TIOCOUTQ nor FIONREAD on fds */
	return -1;
#define TIOCOUTQ 0
#endif
	gint32 retval;

	if (ioctl (fd, input ? FIONREAD : TIOCOUTQ, &retval) == -1) {
		return -1;
	}

	return retval;
}

gboolean
is_baud_rate_legal (int baud_rate)
{
	gboolean ignore = FALSE;
	return setup_baud_rate (baud_rate, &ignore) != -1;
}

int
setup_baud_rate (int baud_rate, gboolean *custom_baud_rate)
{
	switch (baud_rate)
	{
/*Some values are not defined on OSX, *BSD, or AIX */
#if defined(B921600)
	case 921600:
	    baud_rate = B921600;
	    break;
#endif
#if defined(B460800)
	case 460800:
	    baud_rate = B460800;
	    break;
#endif
#if defined(B230400)
	case 230400: 
	    baud_rate = B230400;
	    break;
#endif
#if defined(B115200)
	case 115200: 
	    baud_rate = B115200;
	    break;
#endif
#if defined(B57600)
	case 57600:
	    baud_rate = B57600;
	    break;
#endif
	case 38400: 
	    baud_rate = B38400;
	    break;
	case 19200: 
	    baud_rate = B19200;
	    break;
	case 9600: 
		baud_rate = B9600;
		break;
	case 4800: 
	    baud_rate = B4800;
		break;
	case 2400: 
	    baud_rate = B2400;
		break;
	case 1800: 
	    baud_rate = B1800;
		break;
	case 1200: 
	    baud_rate = B1200;
		break;
	case 600: 
	    baud_rate = B600;
	    break;
	case 300: 
	    baud_rate = B300;
	    break;
	case 200: 
	    baud_rate = B200;
	    break;
	case 150: 
	    baud_rate = B150;
	    break;
	case 134: 
	    baud_rate = B134;
	    break;
	case 110: 
	    baud_rate = B110;
	    break;
	case 75: 
	    baud_rate = B75;
	    break;
	case 50:
#ifdef B50
	    baud_rate = B50;
#else
	    baud_rate = -1;
	    break;
#endif
	case 0:
#ifdef B0
	    baud_rate = B0;
#else
	    baud_rate = -1;
#endif
	    break;
	default:
		*custom_baud_rate = TRUE;
		break;
	}
	return baud_rate;
}

gboolean
set_attributes (int fd, int baud_rate, MonoParity parity, int dataBits, MonoStopBits stopBits, MonoHandshake handshake)
{
	struct termios newtio;
	gboolean custom_baud_rate = FALSE;

	if (tcgetattr (fd, &newtio) == -1)
		return FALSE;

	newtio.c_cflag |=  (CLOCAL | CREAD);
	newtio.c_lflag &= ~(ICANON | ECHO | ECHOE | ECHOK | ECHONL | ISIG | IEXTEN );
	newtio.c_oflag &= ~(OPOST);
	newtio.c_iflag = IGNBRK;

	/* setup baudrate */
	baud_rate = setup_baud_rate (baud_rate, &custom_baud_rate);

	/* char lenght */
	newtio.c_cflag &= ~CSIZE;
	switch (dataBits)
	{
	case 5: 
	    newtio.c_cflag |= CS5;
	    break;
	case 6: 
	    newtio.c_cflag |= CS6;
	    break;
	case 7: 
	    newtio.c_cflag |= CS7;
	    break;
	case 8:
	default:
		newtio.c_cflag |= CS8;
		break;
	}

	/* stopbits */
	switch (stopBits)
	{
	case NoneStopBits:
		/* Unhandled */
		break;
	case One: /* One */
		/* do nothing, the default is one stop bit */
	    newtio.c_cflag &= ~CSTOPB;
		break;
	case Two: /* Two */
		newtio.c_cflag |= CSTOPB;
		break;
	case OnePointFive: /* OnePointFive */
		/* 8250 UART handles stop bit flag as 1.5 stop bits for 5 data bits */
		newtio.c_cflag |= CSTOPB;
		break;
	}

	/* parity */
	newtio.c_iflag &= ~(INPCK | ISTRIP );

	switch (parity)
	{
	case NoneParity: /* None */
	    newtio.c_cflag &= ~(PARENB | PARODD);
	    break;
	    
	case Odd: /* Odd */
	    newtio.c_cflag |= PARENB | PARODD;
	    break;
	    
	case Even: /* Even */
	    newtio.c_cflag &= ~(PARODD);
	    newtio.c_cflag |= (PARENB);
	    break;
	    
	case Mark: /* Mark */
	    /* XXX unhandled */
	    break;
	case Space: /* Space */
	    /* XXX unhandled */
	    break;
	}

   	newtio.c_iflag &= ~(IXOFF | IXON);
#ifdef CRTSCTS
	newtio.c_cflag &= ~CRTSCTS;
#endif /* def CRTSCTS */

	switch (handshake)
	{
	case NoneHandshake: /* None */
		/* do nothing */
		break;
	case RequestToSend: /* RequestToSend (RTS) */
#ifdef CRTSCTS
		newtio.c_cflag |= CRTSCTS;
#endif /* def CRTSCTS */
		break;
	case RequestToSendXOnXOff: /* RequestToSendXOnXOff (RTS + XON/XOFF) */
#ifdef CRTSCTS
		newtio.c_cflag |= CRTSCTS;
#endif /* def CRTSCTS */
		/* fall through */
	case XOnXOff: /* XOnXOff */
	    newtio.c_iflag |= IXOFF | IXON;
		break;
	}

	if (custom_baud_rate == FALSE) {
		if (cfsetospeed (&newtio, baud_rate) < 0 || cfsetispeed (&newtio, baud_rate) < 0)
			return FALSE;
	} else {
#if __linux__ || defined(HAVE_IOKIT)

		/* On Linux to set a custom baud rate, we must set the
		 * "standard" baud_rate to 38400.   On Apple we set it purely
		 * so that tcsetattr has something to use (and report back later), but
		 * the Apple specific API is still opaque to these APIs, see:
		 * https://developer.apple.com/library/mac/samplecode/SerialPortSample/Listings/SerialPortSample_SerialPortSample_c.html#//apple_ref/doc/uid/DTS10000454-SerialPortSample_SerialPortSample_c-DontLinkElementID_4
		 */
		if (cfsetospeed (&newtio, B38400) < 0 || cfsetispeed (&newtio, B38400) < 0)
			return FALSE;
#endif
	}

	if (tcsetattr (fd, TCSANOW, &newtio) < 0)
		return FALSE;

	if (custom_baud_rate == TRUE){
#if defined(HAVE_LINUX_SERIAL_H)
		struct serial_struct ser;

		if (ioctl (fd, TIOCGSERIAL, &ser) < 0)
		{
			return FALSE;
		}

		ser.custom_divisor = ser.baud_base / baud_rate;
		ser.flags &= ~ASYNC_SPD_MASK;
		ser.flags |= ASYNC_SPD_CUST;

		if (ioctl (fd, TIOCSSERIAL, &ser) < 0)
		{
			return FALSE;
		}
#elif defined(HAVE_IOKIT)
		speed_t speed = baud_rate;
		if (ioctl(fd, IOSSIOSPEED, &speed) == -1)
			return FALSE;
#else
		/* Don't know how to set custom baud rate on this platform. */
		return FALSE;
#endif
	}

	return TRUE;
}


static gint32
get_signal_code (MonoSerialSignal signal)
{
	switch (signal) {
		case Cd:
			return TIOCM_CAR;
		case Cts:
			return TIOCM_CTS;
		case Dsr:
			return TIOCM_DSR;
		case Dtr:
			return TIOCM_DTR;
		case Rts:
			return TIOCM_RTS;
		default:
			return 0;
	}

	/* Not reached */
	return 0;
}

static MonoSerialSignal
get_mono_signal_codes (int signals)
{
	MonoSerialSignal retval = NoneSignal;

	if ((signals & TIOCM_CAR) != 0)
		retval |= Cd;
	if ((signals & TIOCM_CTS) != 0)
		retval |= Cts;
	if ((signals & TIOCM_DSR) != 0)
		retval |= Dsr;
	if ((signals & TIOCM_DTR) != 0)
		retval |= Dtr;
	if ((signals & TIOCM_RTS) != 0)
		retval |= Rts;

	return retval;
}

MonoSerialSignal
get_signals (int fd, gint32 *error)
{
	int signals;

	*error = 0;
	
	if (ioctl (fd, TIOCMGET, &signals) == -1) {
		*error = -1;
		return NoneSignal;
	}
	
	return get_mono_signal_codes (signals);
}

gint32
set_signal (int fd, MonoSerialSignal signal, gboolean value)
{
	int signals, expected, activated;

	expected = get_signal_code (signal);
	if (ioctl (fd, TIOCMGET, &signals) == -1)
		{
			/* Return successfully for pseudo-ttys. */
			if (errno == EINVAL)
				return 1;

			return -1;
		}
	
	activated = (signals & expected) != 0;
	if (activated == value) /* Already set */
		return 1;
	
	if (value)
		signals |= expected;
	else
		signals &= ~expected;
	
	if (ioctl (fd, TIOCMSET, &signals) == -1)
		return -1;
	
	return 1;
}

int
breakprop (int fd)
{
	return tcsendbreak (fd, 0);
}

gboolean
poll_serial (int fd, gint32 *error, int timeout)
{
	struct pollfd pinfo;
	
	*error = 0;
	
	pinfo.fd = fd;
	pinfo.events = POLLIN;
	pinfo.revents = 0;

	while (poll (&pinfo, 1, timeout) == -1 && errno == EINTR) {
		/* EINTR is an OK condition, we should not throw in the upper layer an IOException */
		if (errno != EINTR){
			*error = -1;
			return FALSE;
		}
	}

	return (pinfo.revents & POLLIN) != 0 ? 1 : 0;
}

/*
 * mono internals should not be used here.
 * this serial stuff needs to be implemented with icalls.
 * make this at least compile until the code is moved elsewhere
 * defined(linux) is wrong, too
 */
void*
list_serial_devices (void)
{
	return NULL;
}

#if 0
MonoArray *
list_serial_devices (void)
{
	MonoArray *array;
#if defined(linux)
	/* Linux serial files are of the form ttyS[0-9]+ */
	GSList *l, *list = NULL;
	GDir* dir = g_dir_open ("/dev", 0, NULL);
	const char *filename;
	int i = 0;

	while ((filename = g_dir_read_name (dir))) {
		if (filename) {
			if (!strncmp (filename, "ttyS", 4))
				list = g_slist_append (list, g_strconcat ("/dev/", filename, NULL));
		}
	}

	g_dir_close (dir);
  
	array = mono_array_new (mono_domain_get (), mono_get_string_class (), g_slist_length (list));
	for (l = list; l; l = l->next) {
		mono_array_set (array, gpointer, i++, mono_string_new (mono_domain_get (), (char*)l->data));
		g_free (l->data);
	}

	g_slist_free (list);

#else
#warning "list_serial_devices isn't ported to this OS"
#endif

	return array;
}
#endif

