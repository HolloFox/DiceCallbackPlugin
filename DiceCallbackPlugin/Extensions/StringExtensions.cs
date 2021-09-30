using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceCallbackPlugin.Extensions
{
    public static class StringExtensions
    {
        public static List<string> SplitTags(this string flavor)
        {
            if (string.IsNullOrWhiteSpace(flavor)) return new List<string>();
            flavor = flavor.Replace(" ", ""); // strip white space
            flavor = flavor.Replace("/", ""); // backslash
            var i = flavor.IndexOf("<");
            if (i <= 0) return new List<string>();
            var xml = flavor.Substring(i);
            var split = xml.Split('>').ToList();
            while (split.Remove("")) { }
            return split;
        }

        public static IEnumerable<string> SplitInParts(this string s, int partLength)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (partLength <= 0)
                throw new ArgumentException("Part length has to be positive.", nameof(partLength));

            for (var i = 0; i < s.Length; i += partLength)
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
        }

    }
}
