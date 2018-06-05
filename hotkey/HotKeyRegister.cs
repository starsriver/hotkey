using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;

namespace hotkey
{
    class HotKeyRegister : IMessageFilter, IDisposable
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, KeyModifiers fsModifiers, Keys vk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public static KeyModifiers GetModifiers(Keys keydata, out Keys key)
        {
            key = keydata;
            KeyModifiers modifiers = KeyModifiers.None;
            if((keydata & Keys.Control) == Keys.Control)
            {
                modifiers |= KeyModifiers.Control;
                key = key ^ Keys.Control;
            }

            if ((keydata & Keys.Shift) == Keys.Shift)
            {
                modifiers |= KeyModifiers.Shift;
                key = key ^ Keys.Shift;
            }

            if ((keydata & Keys.Alt) == Keys.Alt)
            {
                modifiers |= KeyModifiers.Alt;
                key = key ^ Keys.Alt;
            }

            if(key == Keys.ShiftKey || key == Keys.ControlKey || key == Keys.Menu)
            {
                key = Keys.None;
            }

            return modifiers;
        }

        bool disposed = false;

        const int WM_HOTKEY = 0x0312;

        public IntPtr Handle { get; private set; }

        public int ID { get; private set; }

        public KeyModifiers Modifiers { get; private set; }

        public Keys Key { get; private set; }

        public event EventHandler HotKeyPressed;

        public HotKeyRegister(IntPtr handle, int id, KeyModifiers modifiers, Keys key)
        {
            if(key == Keys.None || modifiers == KeyModifiers.None)
            {
                throw new ArgumentException("The key or modifiers could not be None.");
            }

            this.Handle = handle;
            this.ID = id;
            this.Modifiers = modifiers;
            this.Key = key;

            RegisterHotKey();
            Application.AddMessageFilter(this);
        }

        private void RegisterHotKey()
        {
            bool isKeyRegistered = RegisterHotKey(Handle, ID, Modifiers, Key);

            if (!isKeyRegistered)
            {
                UnregisterHotKey(IntPtr.Zero, ID);
                isKeyRegistered = RegisterHotKey(Handle, ID, Modifiers, Key);
                if (!isKeyRegistered)
                {
                    throw new ApplicationException("The hotkey is use.");
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public bool PreFilterMessage(ref Message m)
        {
            if(m.Msg == WM_HOTKEY && m.HWnd == this.Handle && m.WParam == (IntPtr)this.ID && HotKeyPressed != null)
            {
                HotKeyPressed(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }
            if(disposing)
            {
                Application.RemoveMessageFilter(this);
                UnregisterHotKey(Handle, ID);
            }
            disposed = true;
        }
    }
}
