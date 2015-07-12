using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebLEDController.Api
{
    class Route
    {
        public string Path { get; set; }
        public string Decription { get; set; }
        public Func<string,string> Func { get; set; }
    }
}
