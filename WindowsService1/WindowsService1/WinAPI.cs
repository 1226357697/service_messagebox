using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;



namespace WindowsService1
{

  internal static class WinAPI
  {


    [Flags]
    public enum PageProtection : uint
    {
      //Not included in standard API
      NoAccess = 0x01,
      ReadOnly = 0x02,
      ReadWrite = 0x04,
      WriteCopy = 0x08,

      Execute = 0x10,
      ExecuteRead = 0x20,
      ExecuteReadWrite = 0x40,
      ExecuteWriteCopy = 0x80,

      //included in standard API
      PageReadOnly = 0x02,
      PageReadWrite = 0x04,
      PageWriteCopy = 0x08,

      PageExecute = 0x10,
      PageExecuteRead = 0x20,
      PageExecuteReadWrite = 0x40,
      PageExecuteWriteCopy = 0x80,

      SectionCommit = 0x8000000,
      SectionImage = 0x1000000,
      SectionNoCache = 0x10000000,
      SectionReserve = 0x4000000
  
    }

    [Flags]
    public enum FileDesiredAccess : uint
    {
      FILE_MAP_ALL_ACCESS = 0xF001F
    }

    public enum WTS_CONNECTSTATE_CLASS
    {
      Active,
      Connected,
      ConnectQuery,
      Shadow,
      Disconnected,
      Idle,
      Listen,
      Reset,
      Down,
      Init
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct STARTUPINFO
    {
      internal int cb;
      internal string lpReserved;
      internal string lpDesktop;
      internal string lpTitle;
      internal uint dwX;
      internal uint dwY;
      internal uint dwXSize;
      internal uint dwYSize;
      internal uint dwXCountChars;
      internal uint dwYCountChars;
      internal uint dwFillAttribute;
      internal uint dwFlags;
      internal short wShowWindow;
      internal short cbReserved2;
      internal IntPtr lpReserved2;
      internal IntPtr hStdInput;
      internal IntPtr hStdOutput;
      internal IntPtr hStdError;
    }


    [StructLayout(LayoutKind.Sequential)]
    internal struct PROCESS_INFORMATION
    {
      internal IntPtr hProcess;
      internal IntPtr hThread;
      internal uint dwProcessId;
      internal uint dwThreadId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WTS_SESSION_INFO
    {
      public int SessionID;
      [MarshalAs(UnmanagedType.LPStr)]
      public string pWinStationName;
      public WTS_CONNECTSTATE_CLASS State;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SECURITY_ATTRIBUTES
    {
      public int nLength;
      public IntPtr lpSecurityDescriptor;
      public bool bInheritHandle;
    }

    public const int INVALID_HANDLE_VALUE = -1;
    public static readonly IntPtr INVALID_HANDLE_VALUE_PTR = new IntPtr(-1);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr CreateFileMapping(IntPtr hFile, IntPtr lpAttributes, uint flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CloseHandle(IntPtr hObject);
    
    [DllImport("Kernel32.dll")]
    public static extern uint GetLastError();

    [DllImport("wtsapi32.dll")]
    public static extern IntPtr WTSEnumerateSessions(
                      IntPtr hServer,
                      [MarshalAs(UnmanagedType.U4)] int Reserved,
                      [MarshalAs(UnmanagedType.U4)] int Version,
                      ref IntPtr ppSessionInfo,
                      [MarshalAs(UnmanagedType.U4)] ref int pCount);

    [DllImport("wtsapi32.dll")]
    public static extern bool WTSFreeMemory(IntPtr ppCurrentServer);

    [DllImport("Wtsapi32.dll")]
    public static extern int WTSQueryUserToken(int SessionId, out IntPtr Token);

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool CreateProcessAsUser(
        IntPtr hToken,
        string lpApplicationName,
        string lpCommandLine,
        IntPtr lpProcessAttributes,
        IntPtr lpThreadAttributes,
        bool bInheritHandles,
        uint dwCreationFlags,
        IntPtr lpEnvironment,
        string lpCurrentDirectory,
        ref STARTUPINFO lpStartupInfo,
        out PROCESS_INFORMATION lpProcessInformation);


    [DllImport("Kernel32.dll", EntryPoint = "CreatePipe")]
    public static extern bool CreatePipe(out IntPtr hReadPipe, out IntPtr hWritePipe, ref SECURITY_ATTRIBUTES lpPipeAttributes, uint nSize);

    [DllImport("Kernel32.dll", EntryPoint = "WriteFile")]
    public static extern bool WriteFile(IntPtr hHandle, byte[] lpBuffer, uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, IntPtr lpOverlapped);

  }
}
