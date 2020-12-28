// game-test (c) by mjt
using System.Collections.Generic;
using System.Drawing;

namespace GameTest
{
    public class GameLog
    {
        public List<string> log = new List<string>();
        public List<Brush> brush = new List<Brush>();
        public bool Updated = true;

        public void Add(string str, Brush color)
        {
            log.Add(str);
            brush.Add(color);
            Updated = true;
        }

        /// <summary>
        /// poistaa vanhemmat kuin 'older' viestiä
        /// </summary>
        /// <param name="older"></param>
        public void ClearOlder(int older)
        {
            if (log.Count > older)
            {
                log.RemoveAt(0);
                brush.RemoveAt(0);
                Updated = true;
            }

            if (log.Count > older) ClearOlder(older);
        }
    }

}
