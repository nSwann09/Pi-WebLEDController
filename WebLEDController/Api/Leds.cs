using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;

namespace WebLEDController.Api
{
    class Leds
    {
        public List<Route> Routes { get; set; } = new List<Route>();

        public List<Led> LedList { get; set; } = new List<Led>();

        GpioController gpio = null;

        int DefaultState = 1;

        #region "Init / Disposal"
        public Leds()
        {try {
                InitGPIO();
            }
            catch (Exception ex)
            {
                var exc = ex;
            }
            var RoutePrefix = @"LED/";
            var StatusRoutePrefix = RoutePrefix + "Status/";
            Routes.Add(new Route { Path = StatusRoutePrefix + "TurnOn/", Func = TurnOnLeds, Decription = string.Format("Turns LED(s) on, Use as {0}/PinId e.g {0}/5 to turn an individual LED on OR {0}/all to turn all registered LEDs on", StatusRoutePrefix + "TurnOn/") });
            Routes.Add(new Route { Path = StatusRoutePrefix + "TurnOff/", Func = TurnOffLeds, Decription = string.Format("Turns LED(s) off, Use as {0}/PinId e.g {0}/5 to turn an individual LED off OR {0}/all to turn all registered LEDs off", StatusRoutePrefix + "TurnOff/") });
            Routes.Add(new Route { Path = StatusRoutePrefix + "Toggle/", Func = ToggleLeds, Decription = string.Format("toggle LED(s) on/off, Use as {0}/PinId e.g {0}/5 to toggle an individual LED OR {0}/all to toggle all registered LEDs", StatusRoutePrefix + "Toggle/" )});

            var ManagementRoutePrefix = RoutePrefix + "Manage/";
            Routes.Add(new Route { Path = ManagementRoutePrefix + "List", Func = ListLeds, Decription = "Lists all registered LEDs return is Json format" });
            Routes.Add(new Route { Path = ManagementRoutePrefix + "Add/", Func = AddLed, Decription = string.Format("Adds an LED, Use as {0}/PinId e.g {0}/5", ManagementRoutePrefix + "Add/") });
            Routes.Add(new Route { Path = ManagementRoutePrefix + "Remove/", Func = RemoveLed, Decription = string.Format("Removes an LED, Use as {0}/PinId e.g {0}/5", ManagementRoutePrefix + "Remove/") });
            Routes.Add(new Route { Path = ManagementRoutePrefix + "DefaultState", Func = GetSetDefaultState, Decription = string.Format("Gets the default state when an LED is added (on/off), You can also set the default state by adding /on or /off e.g. {0}/off", ManagementRoutePrefix + "DefaultState") });
        }

        public void DisposeLeds()
        {
            foreach (var led in LedList)
            {
                led.Pin.Dispose();
            }
        }

        void InitGPIO()
        {
            gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                throw new Exception("There is no GPIO controller on this device.");
            }
        }

        #endregion //"Init / Disposal"

        #region "Led Status"

        public string TurnOnLeds(string request)
        {
            var rtn = "Success";
            if (request.ToLowerInvariant().EndsWith("all", StringComparison.CurrentCulture))
            {
                foreach (var led in LedList)
                {
                    led.TurnOn();
                    if (led.IsOff)
                    {
                        rtn = string.Format("Error Tunrning LED at pin {0} On, Request = {1}", led.PinId, request);
                    }
                }
            }
            else
            {
                var ledIdString = request.Substring(request.LastIndexOf("/", StringComparison.CurrentCulture) + 1);
                var ledId = -1;
                if (int.TryParse(ledIdString, out ledId))
                {
                    var led = LedList.SingleOrDefault(l => l.PinId == ledId);
                    if (led == null)
                    {
                        return string.Format("Error No LED Registered at pin {0}, Request = {1}", ledId, request);
                    }
                    led.TurnOn();
                    if (led.IsOff)
                    {
                        rtn = string.Format("Error Tunrning LED at pin {0} On, Request = {1}", led.PinId, request);
                    }
                }
                else
                {
                    rtn = string.Format("Error Tunrning LED at pin {0} On, Request = {1}", ledIdString, request);
                }

            }
            return rtn;
        }
        public string TurnOffLeds(string request)
        {
            var rtn = "Success";
            if (request.ToLowerInvariant().EndsWith("all", StringComparison.CurrentCulture))
            {
                foreach (var led in LedList)
                {
                    led.TurnOff();
                    if (led.IsOn)
                    {
                        rtn = string.Format("Error Tunrning LED at pin {0} Off, Request = {1}", led.PinId, request);
                    }
                }
            }
            else
            {
                var ledIdString = request.Substring(request.LastIndexOf("/", StringComparison.CurrentCulture) + 1);
                var ledId = -1;
                if (int.TryParse(ledIdString, out ledId))
                {
                    var led = LedList.SingleOrDefault(l => l.PinId == ledId);
                    if (led == null)
                    {
                        return string.Format("Error No LED Registered at pin {0}, Request = {1}", ledId, request);
                    }
                    led.TurnOff();
                    if (led.IsOn)
                    {
                        rtn = string.Format("Error Tunrning LED at pin {0} Off, Request = {1}", led.PinId, request);
                    }
                }
                else
                {
                    rtn = string.Format("Error Tunrning LED at pin {0} Off, Request = {1}", ledIdString, request);
                }

            }
            return rtn;
        }
        public string ToggleLeds(string request)
        {
            var rtn = "Success";
            if (request.ToLowerInvariant().EndsWith("all", StringComparison.CurrentCulture))
            {
                foreach (var led in LedList)
                {
                    var status = led.Status;
                    led.Toggle();
                    if (led.Status == status)
                    {
                        rtn = string.Format("Error Toggling LED at pin {0}, Request = {1}", led.PinId, request);
                    }
                }
            }
            else
            {
                var ledIdString = request.Substring(request.LastIndexOf("/", StringComparison.CurrentCulture) + 1);
                var ledId = -1;
                if (int.TryParse(ledIdString, out ledId))
                {
                    var led = LedList.SingleOrDefault(l => l.PinId == ledId);
                    if (led == null)
                    {
                        return string.Format("Error No LED Registered at pin {0}, Request = {1}", ledId, request);
                    }
                    var status = led.Status;
                    led.Toggle();
                    if (led.Status == status)
                    {
                        rtn = string.Format("Error Toggling LED at pin {0}, Request = {1}", led.PinId, request);
                    }
                }
                else
                {
                    rtn = string.Format("Error Toggling LED at pin {0}, Request = {1}", ledIdString, request);
                }
            }
            return rtn;
        }

        #endregion //"Led Status"

        #region "Led Management"

        public string ListLeds(string request)
        {
            var leds = new StringBuilder();
            leds.AppendLine("{");
            leds.AppendLine("   \"leds\":[");
            var index = 0;
            foreach (var led in LedList)
            {
                if (index > 0) leds.Append(",");
                leds.AppendFormat("     {{\"Number\":\"{0}\", \"Status\":\"{1}\" }}", led.PinId, led.Status).AppendLine();
                index++;
            }
            leds.Remove(leds.Length - 1, 1);

            leds.AppendLine("   ]");
            leds.AppendLine("}");
            return leds.ToString();
        }

        public string AddLed(string request)
        {
            var ledIdString = request.Substring(request.LastIndexOf("/", StringComparison.CurrentCulture) + 1);
            var ledId = -1;
            if (int.TryParse(ledIdString, out ledId))
            {
                var led = LedList.SingleOrDefault(l => l.PinId == ledId);
                if (led != null)
                {
                    return string.Format("Error an LED is already Registered  at pin {0}, Request = {1}", ledId, request);
                }
                LedList.Add(new Led(gpio, ledId, DefaultState != 0));
                return "Success";
            }
            return string.Format("Error Adding LED at pin {0} On, Request = {1}", ledIdString, request);
        }

        public string RemoveLed(string request)
        {
            var ledIdString = request.Substring(request.LastIndexOf("/", StringComparison.CurrentCulture) + 1);
            var ledId = -1;
            if (int.TryParse(ledIdString, out ledId))
            {
                var led = LedList.SingleOrDefault(l => l.PinId == ledId);
                if (led == null)
                {
                    return string.Format("Error No LED Registered at pin {0}, Request = {1}", ledId, request);
                }
                led.TurnOff(); 
                LedList.Remove(led);
                return "Success";
            }
            return string.Format("Error Adding LED at pin {0} On, Request = {1}", ledIdString, request);
        }

        public string GetSetDefaultState(string request)
        {
            if (request.ToLowerInvariant().EndsWith("/on", StringComparison.CurrentCultureIgnoreCase))
            {
                DefaultState = 1;
                return "Success";
            }
            else if (request.ToLowerInvariant().EndsWith("/off", StringComparison.CurrentCultureIgnoreCase))
            {
                DefaultState = 0;
                return "Success";
            }

            return DefaultState == 0 ? "Off" : "On";
        }

        #endregion //"Led Management"

        public class Led
        {
            public bool IsOn { get { return Status == 1; } }
            public bool IsOff { get { return Status == 0; } }
            public int Status { get; set; }
            public int PinId { get; set; }
            public GpioPin Pin { get; set; }

            public Led(GpioController gpio, int PinNumber, bool StartOn)
            {
                PinId = PinNumber;
                Pin = gpio.OpenPin(PinId);
                Pin.Write(StartOn ? GpioPinValue.Low : GpioPinValue.High);
                Pin.SetDriveMode(GpioPinDriveMode.Output);
            }

            public void TurnOn()
            {
                Status = 1;
                Pin.Write(GpioPinValue.Low);
            }
            public void TurnOff()
            {
                Status = 0;
                Pin.Write(GpioPinValue.High);
            }
            public void Toggle()
            {
                if (IsOn)
                {
                    TurnOff();
                }
                else
                {
                    TurnOn();
                }
            }
        }


    }
}
