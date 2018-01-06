using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services.Cexio
{
    public class CexioResponse<T>
    {
        public string e { get; set; }
        public string ok { get; set; }
        public T data { get; set; }
    }
}
