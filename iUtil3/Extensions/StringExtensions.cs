using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iUtil3.Extensions
{
    public static class StringExtensions
    {

        public static bool EqualsIgnoreCase(this string s, string compare)
        {
            return s.ToLower().Equals(compare.ToLower());
        }

    }
}
