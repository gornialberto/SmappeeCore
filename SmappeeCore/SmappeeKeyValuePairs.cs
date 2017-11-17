using System;
using System.Collections.Generic;
using System.Text;

namespace SmappeeCore
{
    public class SmappeeKeyValuePairs
    {
        public string value { get; set; }
        public SmappeeValueEnum key { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1}", key, value);
        }
    }
}
