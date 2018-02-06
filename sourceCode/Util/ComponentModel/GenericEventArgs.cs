using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adsophic.Common.Util.ComponentModel
{
    public class GenericEventArgs<TClass> : EventArgs
    {
        private TClass _arg;
        public GenericEventArgs(TClass arg)
        {
            _arg = arg;
        }

        public TClass Arg { get { return _arg; } }
    }
}
