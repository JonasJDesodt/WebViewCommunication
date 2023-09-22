using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Demo
{
    // Bridge and BridgeAnotherClass are C# classes that implement IDispatch and works with AddHostObjectToScript.
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class BridgeAnotherClass
    {
        // Sample property.
        public string Prop { get; set; } = "Example";

        public event EventHandler Fire;

        public void InvokeFireEvent()
        {
            Fire?.Invoke(this, EventArgs.Empty);
        }

        public string GetAddDivFunction()
        {
            //return "function AddDiv(x, y){const div = document.createElement('div'); div.style.position = 'absolute'; div.style.left = x + 'px'; div.style.top = y + 'px'; div.style.width = '250px'; div.style.height = '250px'; div.style.background = 'red'; document.getElementsByTagName('body')[0].appendChild(div); console.log('flag')};";
            return "function AddDiv(x, y) {console.log('flag');}";

        }
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class Bridge
    {
        public string Func(string param)
        {
            return "Example: " + param;
        }

        public BridgeAnotherClass AnotherObject { get; set; } = new BridgeAnotherClass();

        // Sample indexed property.
        [System.Runtime.CompilerServices.IndexerName("Items")]
        public string this[int index]
        {
            get { return m_dictionary[index]; }
            set { m_dictionary[index] = value; }
        }
        private Dictionary<int, string> m_dictionary = new Dictionary<int, string>();
    }
}
