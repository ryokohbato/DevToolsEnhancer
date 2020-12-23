using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;

namespace DevToolsEnhancer
{
  class Program
  {
    static void Main(string[] args)
    {
      Program pr = new Program();
      pr.Run();
    }

    // 定期実行
    public void Run()
    {
      Timer timer = new Timer(3000);
      timer.Elapsed += new ElapsedEventHandler(CheckWindow);
      timer.Start();

      Console.WriteLine("Enterで終了");
      Console.ReadLine();

      timer.Stop();
      timer.Dispose();
      Console.WriteLine("終了");
      Environment.Exit(0);
    }

    public void CheckWindow(object sender, ElapsedEventArgs e)
    {
      // ウィンドウを列挙する
      EnumWindows(new EnumWindowsDelegate(EnumWindowCallBack), IntPtr.Zero);
    }

    public delegate bool EnumWindowsDelegate(IntPtr hWnd, IntPtr lparam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-enumwindows
    public extern static bool EnumWindows(EnumWindowsDelegate lpEnumFunc, IntPtr lparam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowtexta
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowtextlengtha
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowlongptra
    private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    private static bool EnumWindowCallBack(IntPtr hWnd, IntPtr lparam)
    {
      int textLength = GetWindowTextLength(hWnd);
      if (0 < textLength)
      {
        StringBuilder windowTitle = new StringBuilder(textLength + 1);
        _ = GetWindowText(hWnd, windowTitle, windowTitle.Capacity);

        // タイトルの前方一致で雑に検知
        if (
          windowTitle.ToString().StartsWith("Developer Tools")   // Chromium系
          || windowTitle.ToString().StartsWith("開発ツール")     // Firefox
        )
        {
          // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos#parameters
          IntPtr HWND_TOPMOST = new IntPtr(-1);

          const uint SWP_NOSIZE = 0x0001;
          const uint SWP_NOMOVE = 0x0002;
          // x, y, cx, cy (ウィンドウ位置/サイズ) を無視
          const uint TOPMOST__NOSIZE_NOMOVE = (SWP_NOSIZE | SWP_NOMOVE);

          long WS_EX_TOPMOST = 0x00000008L;

          // すでに最前面かどうかチェック
          if (((uint)GetWindowLongPtr(hWnd, -20) & WS_EX_TOPMOST) == 0)
          {
            SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST__NOSIZE_NOMOVE);
            Console.WriteLine($"{DateTime.Now}: {windowTitle}");
          }
        }
      }

      return true;
    }
  }
}
