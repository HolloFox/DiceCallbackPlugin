using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceCallbackPlugin
{
    public class DiceColor
    {
        public float Red;
        public float Green;
        public float Blue;

        public DiceColor(UnityEngine.Color color)
        {
            Red = color.r;
            Green = color.g;
            Blue = color.b;
        }
    }
}
