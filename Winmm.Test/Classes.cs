
namespace Winmm.Test
{
    internal class InnerStruct
    {
        public int id;
        public string name;
        public int value;
        public string strProp { get { return name; } }

        public string Echo(string arg1)
        {
            return arg1;
        }
    }

    internal class InternalClass
    {
        public static int Field;
        public static int Property { get; set; }

        public static InnerStruct[] Array =
        {
            new InnerStruct() { id = 0, name = "first", value = 11},
            new InnerStruct() { id = 1, name = "second", value = 12},
            new InnerStruct() { id = 2, name = "third", value = 13},
            new InnerStruct() { id = 3, name = "fourth", value = 14},
            new InnerStruct() { id = 4, name = "fifth", value = 15},
        };

        public static InnerStruct Nested = new InnerStruct() { id = 0, name = "nexted", value = 11 };

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
        public static string StringProperty { get; set; }

        public static int Method()
        {
            return 42;
        }

        static PublicClass()
        {
            Field = 42;
            Property = 42;
            StringProperty = "Hi";
        }
    }
}
