using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class Extension
    {
        public static string Format(this string word)
        {
            return $"{word}\r\n";
        }
    }
}
