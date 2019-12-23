using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MultiPaste
{
    class GlobalEventHook : NativeWindow
    {
        private const int DISP_ID = 1; // stores ID of the keyboard hotkey to toggle displaying mainWindow (Ctrl + Alt + V)
        private readonly MainWindow mainWindow; // store instance of main window
        private IntPtr clipboardViewerNext; // store next clipboard viewer

        public GlobalEventHook(MainWindow mainWindow) : base()
        {
            // store and hook onto MainWindow instance
            this.mainWindow = mainWindow;
            this.mainWindow.HandleCreated += (sender, e) =>
            {
                AssignHandle(mainWindow.Handle);
            };
            this.mainWindow.HandleDestroyed += (sender, e) =>
            {
                ReleaseHandle();
            };

            // store next clipboard viewer after establishing this
            clipboardViewerNext = SetClipboardViewer(this.mainWindow.Handle);

            // fsModifiers: ALT = 1, CTRL = 2; 1 + 2 = 3
            RegisterHotKey(this.mainWindow.Handle, DISP_ID, 3, (int)Keys.V);
        }

        ~GlobalEventHook()
        {
            ChangeClipboardChain(mainWindow.Handle, clipboardViewerNext);
            UnregisterHotKey(mainWindow.Handle, DISP_ID);
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_DRAWCLIPBOARD = 0x0308; // clipboard changed event
            const int WM_CHANGECBCHAIN = 0x030D; // change clipboard chain event
            const int WM_HOTKEY = 0x0312; // hotkey pressed event

            switch (m.Msg)
            {
                case WM_DRAWCLIPBOARD:
                    if (LocalClipboard.GetHandleClipboard())
                        LocalClipboard.OnClipboardChange();
                    SendMessage(clipboardViewerNext, m.Msg, m.WParam, m.LParam);
                    break;

                case WM_CHANGECBCHAIN:
                    if (m.WParam == clipboardViewerNext)
                        clipboardViewerNext = m.LParam;
                    else
                        SendMessage(clipboardViewerNext, m.Msg, m.WParam, m.LParam);
                    break;

                case WM_HOTKEY:
                    if (m.WParam.ToInt32() == DISP_ID)
                    {
                        switch (mainWindow.WindowState)
                        {
                            case FormWindowState.Minimized:
                                mainWindow.WindowState = FormWindowState.Normal;
                                break;
                            case FormWindowState.Normal:
                                // if MultiPaste is the foreground window, minimize to the system tray
                                if (GetForegroundWindow() == mainWindow.Handle)
                                    mainWindow.Visible = false;
                                // else bring MultiPaste to the foreground
                                else
                                {
                                    SetForegroundWindow(mainWindow.Handle);
                                    if (!mainWindow.Visible)
                                    {
                                        mainWindow.Visible = true;
                                        LocalClipboard.Focus();
                                    }
                                }
                                break;
                            case FormWindowState.Maximized:
                                mainWindow.WindowState = FormWindowState.Minimized;
                                break;
                        }
                    }
                    break;
            }

            // continue the WndProc chain
            base.WndProc(ref m);
        }

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
