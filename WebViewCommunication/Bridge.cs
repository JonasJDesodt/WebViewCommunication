using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WebViewCommunication
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class Bridge
    {

        private string _prop;
        public string Prop
        {
            get => _prop;
            set
            {
                _prop = value;
                PropChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler PropChanged;

    }
}
