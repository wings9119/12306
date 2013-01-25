using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace _12306Helper
{
    public class JsonHelper
    {
        public static T DeserializeToObj<T>(string jsonString)
        {
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            return jsonSerializer.Deserialize<T>(jsonString);
        }
    }
}
