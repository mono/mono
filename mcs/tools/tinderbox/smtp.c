// simple tool for sending smtp messages
//
//
#include <stdio.h>
#include <sys/socket.h>
#include <arpa/inet.h>
#include <string.h>
#include <netdb.h>

#include <unistd.h>
#include <stdlib.h>
#define _GNU_SOURCE
#include <getopt.h>
#include <ctype.h>
#include <time.h>

typedef int bool;
enum { false = 0, true = 1};

struct option rgOptions [] =
{
	{"cc",      required_argument, 	NULL, 'c'},
	{"help",    required_argument, 	NULL, '?'},
	{"to",      required_argument, 	NULL, 't'},
	{"from",    required_argument, 	NULL, 'f'},
	{"message", required_argument, 	NULL, 'm'},
	{"subject", required_argument, 	NULL, 's'},
	{"host",    required_argument, 	NULL, 'h'},
	{"attach",  required_argument, 	NULL, 'a'},
};

typedef struct MailFields
{
	const char *szTo;
	const char *szFrom;
	const char *szHost;
	const char *szSubject;
	bool fAttachments;
	bool fCC;
	int cArgs;
	char **rgszArgs;
	FILE *pfMsg;
} MailFields;

const char *szOptions = "t:f:s:h:c:a:m:?";

void help ()
{
	printf (
		"Usage: smtp [OPTIONS]\n\n"
		"Mandatory arguments:\n"
		"\t-t, --to ADDRESS\tspecify destination email address\n"
		"\t-f, --from ADDRESS\tspecify sender's email address\n"
		"Optional arguments:\n"
		"\t-s, --subject SUBJECT\tspecify subject of message\n"
		"\t-m, --message FILENAME\tread text of message from FILENAME\n"
		"\t-a, --attach FILENAME\tadd FILENAME to message as attachment\n"
		"\t-c, --cc ADDRESS\tadd ADDRESS to CC list\n"
		"\t-h, --host HOSTNAME\tconnect to smpt server HOSTNAME (default: localhost)\n"
	);
}

int GetResponse (FILE *ps)
{
	char szLine [1024];
	char *psz;
	int hr;

	fflush (ps);

	do
	{
		fgets (szLine, sizeof (szLine), ps);

		for (psz = szLine; isdigit (*psz); psz++)
			;
	}
	while (*psz != '\0' && *psz != ' ');

	hr = atol (szLine);
	return hr;
}

FILE *TcpOpen (const char *szHost, int nPort)
{
	int s;
	struct sockaddr_in sa;
	struct hostent *he;
	struct protoent *pe;
	FILE *ps;

	pe = getprotobyname ("TCP");
	s = socket (PF_INET, SOCK_STREAM, pe->p_proto);
	endprotoent ();

	bzero ((char *)&sa,sizeof (sa));
	sa.sin_family = AF_INET;
	sa.sin_addr.s_addr = inet_addr (szHost);
	sa.sin_port = htons (25);

	if ((he = gethostbyname (szHost)) != NULL)
		bcopy (he->h_addr, (char *)&sa.sin_addr, he->h_length);
	else if ((sa.sin_addr.s_addr = inet_addr (szHost)) < 0)
		perror ("gethostbyname ()");

	if (connect (s, (struct sockaddr *) &sa, 16) == -1)
		perror ("connect ()");
	else if ((ps = fdopen (s, "r+")) == NULL)
		perror ("fdopen ()");
	else
		return ps;

	close (s);
	return NULL;
}


void SendMail (const char *szTo, const MailFields *pmf)
{
	char rgchBoundary [20];
	FILE *ps;
	int hr;

	ps = TcpOpen (pmf->szHost, 25);

	hr = GetResponse (ps);

	fprintf (ps, "HELO\r\n");
	hr = GetResponse (ps);

	fprintf (ps, "MAIL FROM: %s\r\n", pmf->szFrom);
	hr = GetResponse (ps);

	fprintf (ps, "RCPT TO: %s\r\n", szTo);
	hr = GetResponse (ps);

	fprintf (ps, "DATA %s\r\n", pmf->szSubject);
	hr = GetResponse (ps);

	fprintf (ps, "From: %s\r\nTo: %s\r\nSubject: %s\r\n", pmf->szFrom, pmf->szTo, pmf->szSubject);

	if (pmf->fCC)
	{
		bool fFirst = true;
		int nOpt, iOpt;
		fprintf (ps, "CC:");
		optind = 0;
		while ((nOpt = getopt_long (pmf->cArgs, pmf->rgszArgs, szOptions, rgOptions, &iOpt)) != -1)
		{
			if (nOpt == 'c')
			{
				if (!fFirst)
					fprintf (ps, ";");
				fprintf (ps, " %s", optarg);
				fFirst = false;
			}
		}
		fprintf (ps, "\r\n");
	}

	if (pmf->fAttachments)
	{
		int ich;
		srand ((int) time (NULL));
		for (ich = 0; ich < sizeof (rgchBoundary) - 1; ich ++)
			rgchBoundary [ich] = rand () % ('Z'-'A') + 'A';
		rgchBoundary [ich] = '\0';

		fprintf (ps, 
			"MIME-Version: 1.0\r\n"
			"Content-Type: multipart/mixed; boundary=\"multipart-%s\"\r\n\r\n", rgchBoundary);

		fprintf (ps, "--multipart-%s\r\n\r\n", rgchBoundary);
	}

	if (pmf->pfMsg != NULL)
	{
		rewind (pmf->pfMsg);
		while (!feof (pmf->pfMsg))
		{
			char strLine [1024];
			int cch;

			fgets (strLine, sizeof (strLine), pmf->pfMsg);
			cch = strlen (strLine);
			while (strLine [cch - 1] == '\r' || strLine [cch - 1] == '\n')
				cch --;
			strLine [cch] = '\0';

			if (cch == 1 && strLine [0] == '.')
				fputc ('.', ps);

			fprintf (ps, "%s\r\n", strLine);
			fflush (ps);
		}
	}

	if (pmf->fAttachments)
	{
		int nOpt, iOpt;
		optind = 0;
		while ((nOpt = getopt_long (pmf->cArgs, pmf->rgszArgs, szOptions, rgOptions, &iOpt)) != -1)
		{
			if (nOpt == 'a')
			{
				const char rgchBase64 [] = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
				FILE *pf = fopen (optarg, "r");
				int cch = 0;

				const char *szBasename = (char *) strrchr ((const char *) optarg, '/');
				if (szBasename == NULL || szBasename [1] == '\0')
					szBasename = optarg;
				else
					szBasename ++;

				fprintf (ps, "--multipart-%s\r\nContent-Type: text/plain; name=\"%s\"\r\nContent-Disposition: attachment; name=\"%s\"\r\nContent-Transfer-Encoding: base64\r\n\r\n", rgchBoundary, szBasename, szBasename);

				while (true)
				{
					int ch1, ch2, ch3;
					if ((ch1 = fgetc (pf)) == -1)
						break;

					fputc (rgchBase64 [ch1 >> 2], ps);

					if ((ch2 = fgetc (pf)) == -1)
					{
						fputc (rgchBase64 [(ch1 << 4) & 0x30], ps);
						fputc ('=', ps);
						fputc ('=', ps);
						break;
					}

					fputc (rgchBase64 [(ch2 >> 4) | ((ch1 << 4) & 0x30)], ps);

					if ((ch3 = fgetc (pf)) == -1)
					{
						fputc (rgchBase64 [(ch2 << 2) & 0x3c], ps);
						fputc ('=', ps);
						break;
					}

					fputc (rgchBase64 [((ch2 << 2) & 0x3c) | (ch3 >> 6)], ps);
					fputc (rgchBase64 [ch3 & 0x3f], ps);
					
					cch += 4;
					if (cch >= 76)
						fprintf (ps, "\r\n");
				}
				fclose (pf);
				fprintf (ps, "\r\n\r\n");
			}
		}
	}

	fprintf (ps, "\r\n.\r\nQUIT\r\n");
	hr = GetResponse (ps);

	fclose (ps);
}

int main (int cArgs, char *rgszArgs [])
{
	int nOpt, iOpt = 0;
	MailFields mf;
	bzero ((char *) &mf, sizeof (mf));
	mf.cArgs = cArgs;
	mf.rgszArgs = rgszArgs;

	while ((nOpt = getopt_long (cArgs, rgszArgs, szOptions, rgOptions, &iOpt)) != -1)
	{
		switch (nOpt)
		{
			case 't':
				if (mf.szTo != NULL)
					goto _default;
				mf.szTo = optarg;
				break;
			case 'f':
				if (mf.szFrom != NULL)
					goto _default;
				mf.szFrom = optarg;
				break;
			case 's':
				if (mf.szSubject != NULL)
					goto _default;
				mf.szSubject = optarg;
				break;
			case 'h':
				if (mf.szHost != NULL)
					goto _default;
				mf.szHost = optarg;
				break;
			case 'c':
				mf.fCC = true;
				break;
			case 'a':
				{
					FILE *pfTmp = fopen (optarg, "r");
					mf.fAttachments = true;
					if (pfTmp == NULL)
					{
						fprintf (stderr, "File not found: %s\n", optarg);
						exit (1);
					}
					fclose (pfTmp);
				}
				break;
			case 'm':
				if (mf.pfMsg != NULL)
					goto _default;

				mf.pfMsg = fopen (optarg, "r");
				if (mf.pfMsg == NULL)
				{
					fprintf (stderr, "File not found: %s\n", optarg);
					exit (1);
				}
				break;
			case '?':
			default: _default:
				printf ("%c: %i\n", nOpt, iOpt);
				help ();
				return 1;
		}
	}
	
	if (mf.szHost == NULL)
		mf.szHost = "localhost";

	if (mf.szSubject == NULL)
		mf.szSubject = "";

	if (mf.szTo == NULL)
	{
		perror ("must specify 'to' field");
		help ();
		return 1;
	}

	if (mf.szFrom == NULL)
	{
		perror ("must specify 'from' field");
		help ();
		return 1;
	}

	SendMail (mf.szTo, &mf);

	if (mf.fCC)
	{
		optind = 0;
		while ((nOpt = getopt_long (cArgs, rgszArgs, szOptions, rgOptions, &iOpt)) != -1)
		{
			if (nOpt == 'c')
			{
				int optindTmp = optind;
				SendMail (optarg, &mf);
				optind = optindTmp;
			}
		}
	}

	if (mf.pfMsg != NULL)
		fclose (mf.pfMsg);

	return 0;
}

