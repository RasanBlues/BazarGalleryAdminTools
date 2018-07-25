using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace BazarGallery.Models
{
    public partial class ShopWorkingHours
    {
        public string ToJson()
        {
            List<JsonObj> list = new List<JsonObj>()
            {
                new JsonObj(){day="Sa",fromTime = SatFrom, toTime=SatTo},
                new JsonObj(){day="Su",fromTime = SunFrom, toTime=SunTo},
                new JsonObj(){day="M",fromTime = MonFrom, toTime=MonTo},
                new JsonObj(){day="T",fromTime = TueFrom, toTime=TueTo},
                new JsonObj(){day="W",fromTime = WedFrom, toTime=WedTo},
                new JsonObj(){day="Th",fromTime = ThuFrom, toTime=ThuTo},
                new JsonObj(){day="F",fromTime = FriFrom, toTime=FriTo}
            };

            return JsonConvert.SerializeObject(list);
        }
        public class JsonObj
        {
            public string day { get; set; }
            public string fromTime { get; set; }
            public string toTime { get; set; }
        }

    }
}
