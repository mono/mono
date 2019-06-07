#ifndef __MONO_PROFHELPER_H__
#define __MONO_PROFHELPER_H__

#include <sys/select.h>

void add_to_fd_set (fd_set *set, int fd, int *max_fd);
void close_socket_fd (int fd);
void setup_command_server (int *server_socket, int *command_port, const char* profiler_name);

#endif
