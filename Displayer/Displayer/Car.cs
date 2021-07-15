using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Displayer
{
    class Car
    {
        public string posiX { get; set; }
        public string posiY { get; set; }

        public int posiXInt()
        {
            int a2;
            int.TryParse(posiX, out a2);
            return a2;
        }
        public int posiYInt()
        {
            int a2;
            int.TryParse(posiY, out a2);
            return a2;
        }

        public Car(string posiX, string posiY)
        {
            this.posiX = posiX;
            this.posiY = posiY;
        }
    }
}
