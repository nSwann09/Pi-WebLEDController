using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WebLEDController.Api
{
    class Processor
    {
        public List<Route> Routes { get; set; }
        Func<string, string> ErrorRoute { get; set; }
        public Processor()
        {
            Routes = new List<Route>();
            Routes.Add(new Route { Path = "list", Func = ListRoutes, Decription = "Lists available routes" });
            ErrorRoute = DefaultErrorProcess;
        }
        
        public Processor(List<Route> routes)
        {
            Routes = routes;
            Routes.Add(new Route { Path = "list", Func = ListRoutes, Decription = "Lists available routes" });
            ErrorRoute = DefaultErrorProcess;
        }
        public Processor(List<Route> routes, Func<string, string> errorRoute)
        {
            Routes = routes;
            Routes.Add(new Route { Path = "list", Func = ListRoutes, Decription = "Lists available routes" });
            ErrorRoute = errorRoute;
        }

        public string process(string request)
        {
            //Get a route that matches exactly
           var route = Routes.FirstOrDefault(k => k.Path == request);

            if (route == null)
            {

                //No exact matches so try starts with
                route = Routes.FirstOrDefault(k => request.StartsWith(k.Path, StringComparison.Ordinal));
                if (route == null)
                {
                    //Okay last ditch attempt does the path even contain a route
                    route = Routes.FirstOrDefault(k => request.Contains(k.Path));
                }
            }

            if (route != null)
            {
                return route.Func(request);
            }
            return ErrorRoute(request);
            }

        public string DefaultErrorProcess(string request)
        {
            var rtn = new StringBuilder();
            rtn.AppendFormat("Error Processing - '{0}'", request).AppendLine();
            rtn.AppendFormat("Available routes are - {0}", ListRoutes("")).AppendLine();

            return rtn.ToString();
        }

        public string ListRoutes(string request)
        {
            var leds = new StringBuilder();
            leds.AppendLine("{");
            leds.AppendLine("   \"routes\":[");
            var index = 0;
            foreach (var led in Routes)
            {
                if (index > 0) leds.Append(",");
                leds.AppendFormat("     {{\"Path\":\"{0}\", \"Description\":\"{1}\" }}", led.Path, led.Decription).AppendLine();
                index++;
            }

            leds.AppendLine("   ]");
            leds.AppendLine("}");
            return leds.ToString();
        }

    }
}

