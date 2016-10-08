using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

internal class InnerStruct
{
    public int id;
    public string name;
    public int value;
}


internal class MMInternalClass
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

    public static int Method()
    {
        return 42;
    }



    static MMInternalClass()
    {
        Field = 42;
        Property = 42;
    }
}


public class MMPublicClass
{
    public static int Field;
    public static int Property { get; set; }

    public static int Method()
    {
        return 42;
    }

    static MMPublicClass()
    {
        Field = 42;
        Property = 42;
        //System.Type.GetType("MMPublicClass").FullName
        {
            System.Reflection.Assembly asm = null;
            var assems = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (var t in assems) if (t.GetName().Name == "Winmm-Test-pass") { asm = t; break; }
            if (asm != null) { var t = asm.GetType("MMPublicClass"); System.Console.WriteLine(t.FullName); }
        }
        {
            string asmname = "Assembly-CSharp-firstpass";
            System.Reflection.Assembly asm = null; foreach (var t in System.AppDomain.CurrentDomain.GetAssemblies()) if (string.Equals(t.GetName().Name, asmname, StringComparison.InvariantCultureIgnoreCase)) { asm = t; break; }
            if (asm != null) { var t = asm.GetType("MMPublicClass");
                System.Console.WriteLine(t.GetMethod("Method").Invoke(null, new object[0]).ToString());
            }

        }
        // System.Type.GetType("MMPublicClass", false, true).FullName;
    }



}
