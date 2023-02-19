#define  _CRT_SECURE_NO_WARNINGS
#include <Windows.h>
#include <iostream>
using namespace std;

const int BUF_SIZE = 4096;
const char* g_map_name = "Local\\1234567894";
const char* g_event_name = "Global\\111111";

int main()
{
  HANDLE hMap = NULL;

  sscanf("828", "%p", &hMap);

  return 0;
  HANDLE memory_mapping = OpenFileMappingA(FILE_MAP_ALL_ACCESS, PAGE_READWRITE, g_map_name);

  if (memory_mapping)
  {
    HANDLE event = OpenEventA(EVENT_ALL_ACCESS, FALSE, g_event_name);
  
    if(event)
    {
      puts("waiting event ...");
      WaitForSingleObject(event, INFINITE);
      PVOID mapping_buffer = MapViewOfFile(memory_mapping, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, BUF_SIZE);
      if (mapping_buffer)
      {
        puts((char*)mapping_buffer);

      }
      else
      {
        printf("mapping_buffer error 0x%08x\n", GetLastError());
      }
    }
    else
    {
      printf("OpenEventA error 0x%08x\n", GetLastError());
    }
  }
  else
  {
    printf("OpenFileMappingA error 0x%08x\n", GetLastError());
  }
}

