using System.Collections.Generic;
using System.Windows.Forms;

namespace HSceneEditor
{
    public class Keyboard : IMessageFilter
    {
        const int WM_KEYDOWN = 0x0100;
        const int WM_KEYUP = 0x0101;
        static bool m_keyPressed = false;
        static Dictionary<Keys, bool> m_keyTable = new Dictionary<Keys, bool>();
        public Dictionary<Keys, bool> KeyTable
        {
            get { return m_keyTable; }
            private set { m_keyTable = value; }
        }

        public bool IsKeyPressed()
        {
            return m_keyPressed;
        }

        public static bool IsKeyPressed(Keys k)
        {
            bool pressed = false;
            if (m_keyTable.TryGetValue(k, out pressed))
            {
                return pressed;
            }
            return false;
        }

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_KEYDOWN)
            {
                KeyTable[(Keys)m.WParam] = true;
                m_keyPressed = true;
            }
            if (m.Msg == WM_KEYUP)
            {
                KeyTable[(Keys)m.WParam] = false;
                m_keyPressed = false;
            }
            return false;
        }
    }
}

