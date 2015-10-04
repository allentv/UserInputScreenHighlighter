/**
 * Author: Allen Thomas Varghese
 * Date : 04-October-2015
 * Adapted from http://blogs.msdn.com/b/toub/archive/2006/05/03/589423.aspx
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace UserInputScreenHighlighter
{
    public partial class Form1 : Form
    {
        private MenuItem showMenuItem;
        private MenuItem hideMenuItem;
        private MenuItem exitMenuItem;

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYUP = 0x0105;    // Modifier Keys
        private const int WM_SYSKEYDOWN = 0x0104;  // ALT Key
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private static Label keyPressMessage;
        private static String prevMessage = "", currMessage = "";

        public Form1()
        {
            _hookID = SetHook(_proc);
            InitializeComponent();
            keyPressMessage = label1;
            SetNotifyIconProperties();
        }

        /**
         * Set properties for the Notify Icon in the System Tray
         */
        private void SetNotifyIconProperties()
        {
            this.notifyIcon1.Icon = Form1.ActiveForm.Icon;
            this.notifyIcon1.ContextMenu = new ContextMenu();
            showMenuItem = new MenuItem("Show", new EventHandler(HandleNotifyIconClick));
            hideMenuItem = new MenuItem("Hide", new EventHandler(HandleNotifyIconClick));
            exitMenuItem = new MenuItem("Exit", new EventHandler(HandleNotifyIconClick));
            this.notifyIcon1.ContextMenu.MenuItems.Add(showMenuItem);
            this.notifyIcon1.ContextMenu.MenuItems.Add(hideMenuItem);
            this.notifyIcon1.ContextMenu.MenuItems.Add(exitMenuItem);
        }

        /**
         * Event Handler for the Notify Icon
         */
        private void HandleNotifyIconClick(object sender, EventArgs e)
        {
            if (sender == showMenuItem)
            {
                Console.WriteLine("Show Menu Item Clicked!");
                this.Show();
                showMenuItem.Enabled = false;
                hideMenuItem.Enabled = true;
            }
            else if (sender == hideMenuItem)
            {
                Console.WriteLine("Hide Menu Item Clicked!");
                this.Hide();
                showMenuItem.Enabled = true;
                hideMenuItem.Enabled = false;
            }
            else if (sender == exitMenuItem)
            {
                Console.WriteLine("Exit Menu Item Clicked!");
                this.Close();
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            
        }

        // Handle mouse click throughs
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= 0x00000020; // WS_EX_TRANSPARENT

                return createParams;
            }
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYUP || wParam == (IntPtr)WM_SYSKEYDOWN)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    //Console.WriteLine((Keys)vkCode);
                    ProcessUserKeyPress((Keys)vkCode);
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        // Process user pressed keys
        private static void ProcessUserKeyPress(Keys keys)
        {
            //Console.WriteLine("prevMessage: " + prevMessage + "  currMessage: " + currMessage);
            if (keys.ToString().Equals("LControlKey") || keys.ToString().Equals("RControlKey"))
            {
                currMessage = "Ctrl";
            }
            else if (keys.ToString().Equals("LShiftKey") || keys.ToString().Equals("RShiftKey"))
            {
                currMessage = "Shift";
            }
            else if (keys.ToString().Equals("LMenu") || keys.ToString().Equals("RMenu"))
            {
                currMessage = "Alt";
            }
            else
            {
                currMessage = keys.ToString();
            }
            // Check for modifier combinations
            if("+Shift+".Contains(prevMessage) || "+Ctrl+".Contains(prevMessage) || "+Alt+".Contains(prevMessage))
            {
                if (!prevMessage.Equals(currMessage) && !prevMessage.Equals(""))
                {
                    currMessage = prevMessage + "+" + currMessage;
                }
            }
            keyPressMessage.Text = currMessage;
            prevMessage = currMessage;
        }
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private void Form1_Activated(object sender, EventArgs e)
        {
            this.Location = new Point()
            {
                X = (Screen.PrimaryScreen.WorkingArea.Width / 2 - this.Width / 2),
                Y = 0
            };
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
        }

    }
}
