/* -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */

/* serial port functions
 *
 * Author: Chris Toshok <toshok@ximian.com>
 */

#include <termios.h>
#include <unistd.h>
#include <fcntl.h>
#include <string.h>
#include <sys/poll.h>

#include <glib.h>

int
open_serial (char* devfile)
{
	int fd;
	struct termios newtio;

	fd = open (devfile, O_RDWR);

	if (fd == -1)
		return -1;

	newtio.c_cflag = CLOCAL | CREAD;
	newtio.c_iflag = 0;
	newtio.c_oflag = 0;
	newtio.c_lflag = 0;

	tcflush(fd, TCIOFLUSH);
	tcsetattr(fd,TCSANOW,&newtio);

	fcntl (fd, F_SETFL, O_NONBLOCK);

	return fd;
}

void
close_serial (int unix_fd)
{
	close (unix_fd);
}

guint32
read_serial (int fd, guchar *buffer, int offset, int count, int timeout)
{
	guint32 n;
	struct pollfd ufd;

	ufd.fd = fd;
	ufd.events = POLLHUP | POLLIN | POLLERR;

	poll (&ufd, 1, timeout);

	if ((ufd.revents & POLLIN) != POLLIN) {
		return -1;
	}
 
	n = read (fd, buffer + offset, count);

	return (guint32) n;
}

void
write_serial (int fd, guchar *buffer, int offset, int count, int timeout)
{
	guint32 n;

	struct pollfd ufd;

	ufd.fd = fd;
	ufd.events = POLLHUP | POLLOUT | POLLERR;

	poll (&ufd, 1, timeout);

	if ((ufd.revents & POLLOUT) != POLLOUT) {
		return;
	}
 
	n = write (fd, buffer + offset, count);
}

void
discard_buffer (int fd, gboolean input)
{
	tcflush(fd, input ? TCIFLUSH : TCOFLUSH);
}

gboolean
set_attributes (int fd, int baud_rate, int parity, int dataBits, int stopBits, int handshake)
{
	struct termios newtio;

	tcgetattr (fd, &newtio);

	switch (baud_rate) {
	case 230400: baud_rate = B230400; break;
	case 115200: baud_rate = B115200; break;
	case 57600: baud_rate = B57600; break;
	case 38400: baud_rate = B38400; break;
	case 19200: baud_rate = B19200; break;
	case 9600: baud_rate = B9600; break;
	case 4800: baud_rate = B4800; break;
	case 2400: baud_rate = B2400; break;
	case 1800: baud_rate = B1800; break;
	case 1200: baud_rate = B1200; break;
	case 600: baud_rate = B600; break;
	case 300: baud_rate = B300; break;
	case 200: baud_rate = B200; break;
	case 150: baud_rate = B150; break;
	case 134: baud_rate = B134; break;
	case 110: baud_rate = B110; break;
	case 75: baud_rate = B75; break;
	case 50:
	case 0:
	default:
		baud_rate = B9600;
		break;
	}

	switch (parity) {
	case 0: /* Even */
		newtio.c_iflag &= ~IGNPAR;
		newtio.c_cflag |= PARENB;
		break;
	case 1: /* Mark */
		/* XXX unhandled */
		break;
	case 2: /* None */
		newtio.c_iflag |= IGNPAR;
		newtio.c_cflag &= ~(PARENB | PARODD);
		break;
	case 3: /* Odd */
		newtio.c_iflag &= ~IGNPAR;
		newtio.c_cflag |= PARENB | PARODD;
		break;
	case 4: /* Space */
		/* XXX unhandled */
		break;
	}

	newtio.c_cflag &= ~CSIZE;
	switch (dataBits) {
	case 5: newtio.c_cflag |= CS5; break;
	case 6: newtio.c_cflag |= CS6; break;
	case 7: newtio.c_cflag |= CS7; break;
	case 8:
	default:
		newtio.c_cflag |= CS8;
		break;
	}

	newtio.c_cflag &= ~CSTOPB;
	switch (stopBits) {
	case 0: /* One */
		/* do nothing, the default is one stop bit */
		break;
	case 1: /* OnePointFive */
		/* XXX unhandled */
		break;
	case 2: /* Two */
		newtio.c_cflag |= CSTOPB;
		break;
	}

	newtio.c_iflag &= ~IXOFF;
	newtio.c_oflag &= ~IXON;
	newtio.c_cflag &= ~CRTSCTS;
	switch (handshake) {
	case 0: /* None */
		/* do nothing */
		break;
	case 1: /* RequestToSend (RTS) */
		newtio.c_cflag |= CRTSCTS;
		break;
	case 2: /* RequestToSendXOnXOff (RTS + XON/XOFF) */
		newtio.c_cflag |= CRTSCTS;
		/* fall through */
	case 3: /* XOnXOff */
		newtio.c_iflag |= IXOFF;
		//		newtio.c_oflag |= IXON;
		break;
	}
	
	cfsetospeed (&newtio, baud_rate);
	cfsetispeed (&newtio, baud_rate);

	tcsetattr(fd,TCSADRAIN,&newtio);

	return TRUE;
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

