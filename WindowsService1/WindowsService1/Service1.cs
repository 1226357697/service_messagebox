using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace WindowsService1
{
  public partial class Service1 : ServiceBase
  {

    private IntPtr MappingHandle = IntPtr.Zero;
    private IntPtr MappingBuffer = IntPtr.Zero;
    public Service1()
    {
      InitializeComponent();
    }

    public static IntPtr StructToIntPtr<T>(T req) where T : struct
    {
      int size = Marshal.SizeOf(req);
      byte[] bytes = new byte[size];
      IntPtr structPtr = Marshal.AllocHGlobal(size);
      Marshal.StructureToPtr(req, structPtr, false);
      Marshal.Copy(structPtr, bytes, 0, size);
      return structPtr;
    }

    private static WinAPI.WTS_SESSION_INFO[] IntPtrToSessionInfoArray(IntPtr ptr, int count)
    {

      int offset =  Marshal.SizeOf(typeof(WinAPI.WTS_SESSION_INFO));
      WinAPI.WTS_SESSION_INFO[] infos = new WinAPI.WTS_SESSION_INFO[count];

      for (int i = 0; i < infos.Length; ++i)
      {
        infos[i] = (WinAPI.WTS_SESSION_INFO)Marshal.PtrToStructure(ptr, typeof(WinAPI.WTS_SESSION_INFO));
        ptr = IntPtr.Add(ptr, offset);
        offset += Marshal.SizeOf(typeof(WinAPI.WTS_SESSION_INFO));
      }

      return infos;

    }


    private int? GetCurrentSessionId()
    {
        
      int? current_id = null;
      const int WTS_CURRENT_SERVER = 0;
      IntPtr session_info_ptr = IntPtr.Zero;
      int count = 0;
      WinAPI.WTSEnumerateSessions((IntPtr)WTS_CURRENT_SERVER, 0, 1, ref session_info_ptr, ref count);
      WinAPI.WTS_SESSION_INFO[] session_info = IntPtrToSessionInfoArray(session_info_ptr, count);
      for (int i = 0; i < count; ++i)
      {
        if (session_info[i].State == WinAPI.WTS_CONNECTSTATE_CLASS.Active)
        {
          current_id = session_info[i].SessionID;
          break;
        }

        Debug.WriteLine($"[+] session id: {session_info[i].SessionID}");

      }

      WinAPI.WTSFreeMemory(session_info_ptr);

      return current_id;
    }

    protected override void OnStart(string[] args)
    {
      Debug.WriteLine("[+]Service OnStart");

      const uint BUF_SIZE = 0x1000;
      const string MAPPING_NAME = "Global\\123456";

      WinAPI.SECURITY_ATTRIBUTES sa = new WinAPI.SECURITY_ATTRIBUTES();
      sa.nLength = Marshal.SizeOf(sa);
      sa.bInheritHandle = true;

      MappingHandle = WinAPI.CreateFileMapping(WinAPI.INVALID_HANDLE_VALUE_PTR, StructToIntPtr(sa),
                              (uint)(WinAPI.PageProtection.ReadWrite | WinAPI.PageProtection.SectionCommit),
                              0, BUF_SIZE, MAPPING_NAME);

      if(MappingHandle != null)
      {
        MappingBuffer = WinAPI.MapViewOfFile(MappingHandle, (uint)(WinAPI.FileDesiredAccess.FILE_MAP_ALL_ACCESS), 0, 0, 0);
        if (MappingBuffer != IntPtr.Zero)
        {
          string hello = "HelloWorld";
          Marshal.Copy(System.Text.Encoding.ASCII.GetBytes(hello), 0, MappingBuffer, hello.Length);

          string param = $"service_client.exe {MappingHandle.ToInt64()}";

          Debug.WriteLine($"[+] Service: {Marshal.PtrToStringAnsi(MappingBuffer)}");
          Debug.WriteLine("[+]initialize suceesss");

          Debug.WriteLine("[+] create sub process");

          int? current_id = GetCurrentSessionId();
          if (current_id != null)
          {

            IntPtr hToken ;
            int ok = WinAPI.WTSQueryUserToken(current_id.Value, out hToken);
            if (ok != 0)
            {
              WinAPI.STARTUPINFO si= new WinAPI.STARTUPINFO();
              si.cb = Marshal.SizeOf(si);
              WinAPI.PROCESS_INFORMATION pi;
              bool ret = WinAPI.CreateProcessAsUser(hToken, null, param, IntPtr.Zero, IntPtr.Zero, true, 0, IntPtr.Zero, null, ref si, out pi);

              if (ret)
              {
                Debug.WriteLine($"[+] subprocess success param: {param}");
              }
              else
              {
                Debug.WriteLine($"[-] subprocess failed, {WinAPI.GetLastError().ToString("X8")}");
              }

            }
            else 
            {
              Debug.WriteLine($"[-]WTSQueryUserToken failed, {WinAPI.GetLastError().ToString("X8")}");
            }
              

          }
          else
          {
            Debug.WriteLine("[-]GetCurrentSessionId failed");
          }

#if false
          Process subprocess = new Process();
          subprocess.StartInfo = new ProcessStartInfo("D:\\Work\\TestCode\\cpp\\Test\\Debug\\service_client.exe");
          
          subprocess.Start();
#endif
          Debug.WriteLine("[+]subprocess overt");

        }
        else
        {
          Debug.WriteLine($"[-]MapViewOfFile failed, {WinAPI.GetLastError().ToString("X8")}");
        }
      }
      else
      {
        Debug.WriteLine($"[-]CreateFileMapping failed, {WinAPI.GetLastError().ToString("X8")}");
      }
    }

    protected override void OnStop()
    {
      Debug.WriteLine("[+]Service OnStop");
      WinAPI.UnmapViewOfFile(MappingBuffer);
      WinAPI.CloseHandle(MappingHandle);

    }
  }
}
