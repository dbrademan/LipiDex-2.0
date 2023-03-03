using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LipiDex2.PeakFinder
{
    public class Library
    {
        string name;  // Library name
        string file;  // Library filepath TODO: change to Path() object
        bool active;  // true iff library is active

        public Library(string name, string file)
        {
            this.name = name;
            this.file = file;
        }
        // Activate library
        public void ChangeActive()
        {
            if (active) active = false;
            else active = true;
        }
    }
}
