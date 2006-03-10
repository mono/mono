
#ifndef __SERIAL_H
#define __SERIAL_H

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

#endif

