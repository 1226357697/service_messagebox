#define  _CRT_SECURE_NO_WARNINGS
#include <Windows.h>
#include <iostream>
#include <stdarg.h>
#include <wtsapi32.h>
using namespace std;

const int BUF_SIZE = 4096;
const char* g_map_name = "Global\\123456";

void debug_print(const char* format, ...)
{
  va_list ap;
  va_start(ap, format);
  char buffer[0x512] = {'\0'};
  vsprintf(buffer, format, ap);
  OutputDebugStringA(buffer);
  va_end(ap);
}

int main(int argc, char*argv[])
{
  if(argc != 2)
    return -1;

  HANDLE memory_mapping = NULL; //OpenFileMappingA(FILE_MAP_ALL_ACCESS, PAGE_READWRITE, g_map_name);
  sscanf(argv[1], "%d", (void*)&memory_mapping);
  debug_print("[+]subprocess memory_mapping %d", memory_mapping);

  if (memory_mapping)
  {
    PVOID mapping_buffer = MapViewOfFile(memory_mapping, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, BUF_SIZE);
    if (mapping_buffer)
    {
      debug_print("[+]subprocess communication success : %s", mapping_buffer);

      MessageBoxA(NULL, (char*)mapping_buffer, NULL, MB_OK);

      debug_print("[+]subprocess MessageBoxA over");

    }
    else
    {
      debug_print("[-]subprocess OpenFileMappingA error:0x%08x", GetLastError());
    }
  }
  else
  {
    debug_print("[-]subprocess OpenFileMappingA error:0x%08x", GetLastError());
  }


}

