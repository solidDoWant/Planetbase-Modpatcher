using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompilerTest
{
    public class Class1
    {
        public virtual int someMethod(string someValue)
        {
            Console.WriteLine(someValue);
            return 3;
        }

        public void callTheMethod()
        {
            someMethod("WOOHOO");
        }
    }
}
