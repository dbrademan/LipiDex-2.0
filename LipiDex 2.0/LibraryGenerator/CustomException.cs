using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LipiDex2.LibraryGenerator
{
    public class CustomException : Exception
    {
        Exception e;

        //Parameterless Constructor
        public CustomException(Exception e)
        {
            this.e = e;
        }

        //Constructor that accepts a message
        public CustomException(string message, Exception e)
        {
            this.e = e;
        }

        //Returns original exception
        public Exception GetException()
        {
            return e;
        }
    }
}
