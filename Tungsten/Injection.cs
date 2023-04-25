using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

public class Injection
{

    #region Windows API

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool WaitNamedPipe(string name, int timeout);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern int WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, int size, int lpNumberOfBytesWritten);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern int CloseHandle(IntPtr hObject);

    #endregion

    #region Functions

    public static void WriteToPipe(string input, string pipeName)
    {
        if (NamedPipeExists(pipeName))
        {
            using (NamedPipeClientStream stream = new NamedPipeClientStream(".", pipeName, PipeDirection.Out))
            {
                stream.Connect();
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(input);
                    writer.Dispose();
                }
                stream.Dispose();
            }
        }
    }

    public static bool NamedPipeExists(string pipeName)
    {
        try
        {
            if (!WaitNamedPipe(Path.GetFullPath("\\\\.\\pipe\\" + pipeName), 0))
            {
                int lastWin32Error = Marshal.GetLastWin32Error();
                if (lastWin32Error == 0 || lastWin32Error == 2)
                {
                    return false;
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsGhostProc(ProcessModuleCollection pmc)
    {
        foreach (object obj in pmc)
        {
            ProcessModule processModule = (ProcessModule)obj;
            string text = processModule.FileName.ToString();
            if (text.Contains("cryptnet") || text.Contains("mswsock") || text.Contains("urlmon") || text.Contains("XInput1_4") || text.Contains("CoreUIComponents"))
            {
                return false;
            }
        }
        return true;
    }

    public static DllInjectionResult Inject(string dllPath, string processName)
    {
        DllInjectionResult result;
        if (!File.Exists(dllPath))
        {
            result = DllInjectionResult.DllNotFound;
        }
        else
        {
            int procId = 0;
            Process[] processes = Process.GetProcesses();
            for (int i = 0; i < processes.Length; i++)
            {
                if (!(processes[i].ProcessName != processName))
                {
                    procId = processes[i].Id;
                    break;
                }
            }
            if (procId == 0U)
            {
                result = DllInjectionResult.GameProcessNotFound;
            }
            else
            {
                IntPtr proces = OpenProcess(1082U, true, procId);
                if (proces == (IntPtr)0)
                {
                    result = DllInjectionResult.InjectionFailed;
                }
                else
                {
                    IntPtr procAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
                    if (procAddress == (IntPtr)0)
                    {
                        result = DllInjectionResult.InjectionFailed;
                    }
                    else
                    {
                        IntPtr baseAddress = VirtualAllocEx(proces, (IntPtr)0, (IntPtr)dllPath.Length, 12288U, 64U);
                        if (baseAddress == (IntPtr)0)
                        {
                            result = DllInjectionResult.InjectionFailed;
                        }
                        else
                        {
                            byte[] bytes = Encoding.ASCII.GetBytes(dllPath);
                            if (WriteProcessMemory(proces, baseAddress, bytes, bytes.Length, 0) == 0)
                            {
                                result = DllInjectionResult.InjectionFailed;
                            }
                            else
                            {
                                if (CreateRemoteThread(proces, (IntPtr)0, (IntPtr)0, procAddress, baseAddress, 0U, (IntPtr)0) == (IntPtr)0)
                                {
                                    result = DllInjectionResult.InjectionFailed;
                                }
                                else
                                {
                                    CloseHandle(proces);
                                    result = DllInjectionResult.Success;
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    #endregion

}

public enum DllInjectionResult
{
    DllNotFound,
    GameProcessNotFound,
    InjectionFailed,
    Success
}