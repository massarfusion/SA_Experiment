using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Navigator
{
    class Step
    {
       public string destX { get; set; }
       public string destY { get; set; }

        public Step(string destX, string destY)
        {
            this.destX = destX;
            this.destY = destY;
        }

        public int destXInt()
        {
            int a2;
            int.TryParse(destX, out a2);
            return a2;
        }
        public int destYInt()
        {
            int a2;
            int.TryParse(destY, out a2);
            return a2;
        }

        public static string Serial(Step step)
        {
            return JsonSerializer.Serialize(step);//序列化到json
        }
        public static Step DeSerial(String raw)
        {
            return JsonSerializer.Deserialize<Step>(raw);//反序列化
        }



    }
}
