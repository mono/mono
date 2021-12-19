/* -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */

/* serial port - custom speed setting
 *
 * code can't be included in serial.c because of header conflicts
 *
 * Author: Thomas Kuehne <thomaS@kuehne.cn>
 */

#include <config.h>
#include <string.h>

#if defined(HAVE_ASM_TERMBITS_H)
#include <asm/termbits.h>
#endif

#if defined(HAVE_SYS_IOCTL_H)
#include <sys/ioctl.h>
#endif


#if defined(BOTHER) && defined(TCSETS2)
int
set_attributes_custom_speed (int fd, speed_t baud_rate, struct termios *tio);

int
set_attributes_custom_speed (int fd, speed_t baud_rate, struct termios *tio)
{
	struct termios2 newtio;

	/* clone settings */
	newtio.c_iflag = tio->c_iflag;
	newtio.c_oflag = tio->c_oflag;
	newtio.c_cflag = tio->c_cflag;
	newtio.c_lflag = tio->c_lflag;
	newtio.c_line = tio->c_line;
	memcpy (newtio.c_cc, tio->c_cc, sizeof(newtio.c_cc));

	/* set speed */
	newtio.c_cflag |= BOTHER;
	newtio.c_ispeed = baud_rate;
	newtio.c_ospeed = baud_rate;

	if (ioctl(fd, TCSETS2, &newtio) == -1)
		return 0;

	return 1;
}

#else
int
set_attributes_custom_speed (int fd, speed_t baud_rate, struct termios *tio);

int
set_attributes_custom_speed (int fd, speed_t baud_rate, struct termios *tio)
{
	/* Don't know how to set custom baud rate on this platform. */
	return -1;
}

#endif
