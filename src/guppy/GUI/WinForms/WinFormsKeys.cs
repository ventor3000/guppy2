using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Guppy2.GUI.WinForms
{
    public static class WinFormsKeys
    {
        private static byte[] keystates = new byte[256];


        /// <summary>
        /// Decodes a key as gotten in KeyDown etc, into a guppy KeyData structure.
        /// </summary>
        /// <param name="rawkeys"></param>
        /// <returns></returns>
        public static KeyData DecodeKey(Keys rawkeys)
        {
            uint rk = (uint)rawkeys;
            uint virtual_key = rk & 0xffff;
            KeyCode kc = KeyCode.Unknown;

            bool shift = (rk & 0x10000) != 0;
            bool ctrl = (rk & 0x20000) != 0;
            bool alt = (rk & 0x40000) != 0;

            string unshifted, shifted;
            char keychar = '\0';

            KeyOutput(virtual_key, ctrl, shift, alt, out unshifted, out shifted);

            if (unshifted != null && unshifted != "")
                kc = (KeyCode)unshifted[0];
            if (shifted != null && shifted != "")
                keychar = shifted[0];

            if (kc == KeyCode.Unknown)
            { //still not known, try special (non typable) keys
                switch ((Keys)virtual_key)
                {
                    case Keys.Home: kc = KeyCode.Home; break;
                    case Keys.Up: kc = KeyCode.Up; break;
                    case Keys.PageUp: kc = KeyCode.PageUp; break;
                    case Keys.PageDown: kc = KeyCode.PageDown; break;
                    case Keys.Left: kc = KeyCode.Left; break;
                    case Keys.Right: kc = KeyCode.Right; break;
                    case Keys.End: kc = KeyCode.End; break;
                    case Keys.Down: kc = KeyCode.Down; break;
                    case Keys.Insert: kc = KeyCode.Insert; break;
                    case Keys.Delete: kc = KeyCode.Delete; break;
                    case Keys.Pause: kc = KeyCode.Pause; break;
                    case Keys.CapsLock: kc = KeyCode.CapsLock; break;
                    case Keys.Escape: kc = KeyCode.Escape; break;
                    case Keys.F1: kc = KeyCode.F1; break;
                    case Keys.F2: kc = KeyCode.F2; break;
                    case Keys.F3: kc = KeyCode.F3; break;
                    case Keys.F4: kc = KeyCode.F4; break;
                    case Keys.F5: kc = KeyCode.F5; break;
                    case Keys.F6: kc = KeyCode.F6; break;
                    case Keys.F7: kc = KeyCode.F7; break;
                    case Keys.F8: kc = KeyCode.F8; break;
                    case Keys.F9: kc = KeyCode.F9; break;
                    case Keys.F10: kc = KeyCode.F10; break;
                    case Keys.F11: kc = KeyCode.F11; break;
                    case Keys.F12: kc = KeyCode.F12; break;
                    case Keys.Print: kc = KeyCode.Print; break;
                    case Keys.Apps: kc = KeyCode.Menu; break;
                    case Keys.NumLock: kc = KeyCode.NumLock; break;
                    case Keys.Scroll: kc = KeyCode.ScrollLock; break;

                }
            }

            if (kc == KeyCode.Unknown)
                return new KeyData(KeyCode.Unknown, (uint)rawkeys, '\0', ctrl, shift, alt);


            return new KeyData(kc, (uint)rawkeys, keychar, ctrl, shift, alt);
        }



        private static void KeyOutput(uint virtualkey, bool ctrl, bool shift, bool alt, out string unshifted, out string shifted)
        {
            shifted = unshifted = null;

            IntPtr hkl = GetKeyboardLayout(0 /*thread*/);

            if (hkl == IntPtr.Zero) //cant grab keyboard layout
                return;


            uint scancode = MapVirtualKey(virtualkey, 0);

            if (scancode != 0x2a)
            {
                int debug = 0;
            }

            //no ctrl+shift+alt to get unshifted key
            StringBuilder sb = new StringBuilder(10);
            keystates[0x10] = 0;
            keystates[0x11] = 0;
            keystates[0x12] = 0;

            int rc = ToUnicodeEx(virtualkey, scancode, keystates, sb, sb.Capacity, 0, hkl);
            unshifted = sb.ToString();

            //possible shifting
            if (shift) keystates[0x10] = 0x80;
            if (ctrl) keystates[0x11] = 0x80;
            if (alt) keystates[0x12] = 0x80;
            rc = ToUnicodeEx(virtualkey, scancode, keystates, sb, sb.Capacity, 0, hkl);
            shifted = sb.ToString();
        }


        #region PINVOKES
        [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern int ToUnicodeEx(
            uint wVirtKey,
            uint wScanCode,
            byte[] lpKeyState,
            StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags,
            IntPtr dwhkl);

        [DllImport("user32.dll", ExactSpelling = true)]
        internal static extern IntPtr GetKeyboardLayout(uint threadId);

        [DllImport("user32.dll")]
        internal static extern uint MapVirtualKey(uint uCode, uint uMapType);

        #endregion
    }
}
