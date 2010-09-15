using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace PurpleTreeHouse
{
    class IncomingRequest
    {
        public long Id { get; set; }
        public bool AlreadyShown { get; set; }
        public int SnarlNotificationId { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string Icon { get; set; }
        public int DisplayTime { get; set; }
        public string AppName { get; set; }
        public string ClassName { get; set; }
        public List<IPAddress> TargetIps { get; set; }
       // public List<DnsEndPoint> TargetHostnames { get; set; }
        public string LeftClickUrl { get; set; }
        public string MiddleClickUrl { get; set; }
        public string RightClickUrl { get; set; }
    }

}
