using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winmm.Test
{
    internal class InternalClass
    {
        public static int Field;
        public static int Property { get; set; }

        public static int Method()
        {
            return 42;
        }

        static InternalClass()
        {
            Field = 42;
            Property = 42;
        }
    }


    public class PublicClass
    {
        public static int Field;
        public static int Property { get; set; }

        public static int Method()
        {
            return 42;
        }

        static PublicClass()
        {
            Field = 42;
            Property = 42;
        }
    }
}
