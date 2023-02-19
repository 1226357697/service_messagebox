#define  _CRT_SECURE_NO_WARNINGS
#include <Windows.h>
#include <iostream>
using namespace std;

const int BUF_SIZE = 4096;
const char* g_map_name = "Local\\1234567894";
const char* g_event_name = "Global\\111111";

int main()
{
  HANDLE memory_mapping = CreateFileMappingA(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, BUF_SIZE, g_map_name);
  
  if (memory_mapping)
  {
    HANDLE event = CreateEventA(NULL, FALSE, FALSE, g_event_name);
    PVOID mapping_buffer = MapViewOfFile(memory_mapping, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    if(mapping_buffer)
    {
      strcpy((char*)mapping_buffer, "Hello World");
      SetEvent(event);


      printf("create sub process\n");

      STARTUPINFOA si = {sizeof(si)};
      PROCESS_INFORMATION pi = {};
      char param[0x200] = {'\0'};
      strcpy(param, "client.exe");
      if(CreateProcessA(NULL, param, NULL, NULL, FALSE, 0, NULL, NULL, &si, &pi))
      {
        CloseHandle(pi.hProcess);
        CloseHandle(pi.hThread);
      }
      else
      {
        printf("CreateProcessA error:0x%08x\n ", GetLastError());
      }




      printf("ok\n");

    }
    else
    {
      printf("MapViewOfFile error 0x%08x\n", GetLastError());
    }
  }
  else
  {
    printf("CreateFileMappingA error 0x%08x\n", GetLastError());
  }

  getchar();
}

