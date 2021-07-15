using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Navigator
{
    class Request
    {
        public string carName { get; set; }

        public Request(string carName)
        {
            this.carName = carName;
        }
        public static string Serial(Request req)
        {
            return JsonSerializer.Serialize(req);//序列化到json
        }
        public static Request DeSerial(String raw)
        {
            return JsonSerializer.Deserialize<Request>(raw);//反序列化
        }
    }
}
