using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Text;
using System.IO;
using System.Xml;
using Snarl.V41;
using System.Timers;

namespace PurpleTreeHouse
{
    class AppController
    {
        public static AppController Current;
        private List<IncomingRequest> Requests;
        private SnarlConnector Notifier = new SnarlConnector();
        private NativeWindowApplication.SnarlMsgWnd comWindow;
        Timer UpdateTimer;
        bool ShowDebug;

        private AppController()
        {
            Requests = new List<IncomingRequest>();
            comWindow = new NativeWindowApplication.SnarlMsgWnd();
            if (Properties.Settings.Default.ResetAlreadySeenIds)
            {
                Properties.Settings.Default.LastIdShown = 0;
            }
            ShowDebug = Properties.Settings.Default.ShowErrorNotifications;

            RegisterWithSnarl();

            UpdateTimer = new Timer(Properties.Settings.Default.UpdateInterval *1000);
            UpdateTimer.Elapsed += new ElapsedEventHandler(UpdateTimer_Elapsed);
            
            FetchNotifications();
        }

        public void RegisterWithSnarl()
        {
            Notifier.RegisterApp(Properties.Settings.Default.AppName, Properties.Settings.Default.AppName, Properties.Settings.Default.AppIcon, comWindow.Handle, 17, SnarlConnector.AppFlags.AppIsWindowless);
            char[] separator = { ';' };
            string[] classes = Properties.Settings.Default.ListOfNotificationClasses.Split(separator);
            foreach (string className in classes)
            {
                Notifier.AddClass(className, className);
            }
            if (ShowDebug)
            {
                Notifier.AddClass("Debug", "Debug");
            }
        }

        void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            FetchNotifications();
        }

        ~AppController() {
            Notifier.UnregisterApp();
        }

        static public void Start()
        {
            if (Current == null)
            {
                Current = new AppController();
            }
        }

        static public void Stop()
        {
            if (Current != null)
            {
                Current.Notifier.UnregisterApp();
                App.Current.Shutdown();
            }
        }

        public void FetchNotifications()
        {
            string rawData = "";
            if (Properties.Settings.Default.UrlOrUnc.ToLower().StartsWith("http"))
            {
                rawData = GetHttpContent(Properties.Settings.Default.UrlOrUnc);
            }
            else
            {
                if (File.Exists(Properties.Settings.Default.UrlOrUnc))
                {
                    rawData = File.ReadAllText(Properties.Settings.Default.UrlOrUnc);
                }
                else
                {
                    if (ShowDebug)
                    {
                        Notifier.EZNotify("Debug", "File not found", Properties.Settings.Default.UrlOrUnc, 10, Properties.Settings.Default.AppIcon);
                    }
                    UpdateTimer.Start();
                    return;
                }
            }
            if (rawData == "")
            {
                if (ShowDebug)
                {
                    Notifier.EZNotify("Debug", "Empty Response", Properties.Settings.Default.UrlOrUnc, 10, Properties.Settings.Default.AppIcon);
                }
                UpdateTimer.Start();
                return;
            }
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.LoadXml(rawData);
            }
            catch (Exception exp)
            {
                if (ShowDebug)
                {
                    Notifier.EZNotify("Debug", "XML general parsing error", exp.Message, 10, Properties.Settings.Default.AppIcon);
                }
                UpdateTimer.Start();
                return;
            }

            if (xmlDoc.DocumentElement.Name != "alerts")
            {
                if (ShowDebug)
                {
                    Notifier.EZNotify("Debug", "Invalid XML", "XML open tag is not \"alerts\"", 10, Properties.Settings.Default.AppIcon);
                }
                UpdateTimer.Start();
                return;
            }

            try
            {
                bool alreadyShown = false;
                foreach (XmlNode alert in xmlDoc.GetElementsByTagName("alert"))
                {
                    alreadyShown = false;
                    IncomingRequest currentAlert = new IncomingRequest();
                    foreach (XmlNode childNode in alert.ChildNodes)
                    {
                        if (alreadyShown)
                        {
                            break;
                        }
                        switch (childNode.Name.ToLower())
                        {
                            case "id":
                                currentAlert.Id = Convert.ToInt64(childNode.InnerText);
                                if (currentAlert.Id > Properties.Settings.Default.LastIdShown)
                                {

                                    if (Requests.Where(r => r.Id == currentAlert.Id).Count() > 0)
                                    {
                                        alreadyShown = true;
                                        continue;
                                    }
                                }
                                else
                                {
                                    alreadyShown = true;
                                    continue;
                                }

                                break;

                            case "title":
                                currentAlert.Title = childNode.InnerText;
                                break;

                            case "text":
                                currentAlert.Text = childNode.InnerText;
                                break;

                            case "icon":
                                currentAlert.Icon = childNode.InnerText;
                                break;

                            case "displaytime":
                                currentAlert.DisplayTime = Convert.ToInt32(childNode.InnerXml);
                                break;

                            case "app":
                                currentAlert.AppName = childNode.InnerText;
                                break;

                            case "class":
                                currentAlert.ClassName = childNode.InnerText;
                                break;

                            case "leftclickurl":
                                currentAlert.LeftClickUrl = childNode.InnerText;
                                break;

                            case "middleclickurl":
                                currentAlert.MiddleClickUrl = childNode.InnerText;
                                break;

                            case "rightclickurl":
                                currentAlert.RightClickUrl = childNode.InnerText;
                                break;

                            default:
                                break;
                        }

                    }
                    if (!alreadyShown && SnarlConnector.GetSnarlWindow() != IntPtr.Zero)
                    {
                        if (currentAlert.Icon == null)
                        {
                            currentAlert.Icon = Properties.Settings.Default.DefaultNotificationIcon;
                        }
                        if (currentAlert.DisplayTime == null)
                        {
                            currentAlert.DisplayTime = Properties.Settings.Default.DefaultDisplayTime;
                        }
                        if (currentAlert.ClassName == null)
                        {
                            currentAlert.ClassName = Properties.Settings.Default.DefaultNotificationClass;
                        }

                        currentAlert.SnarlNotificationId = Notifier.EZNotify(currentAlert.ClassName, currentAlert.Title, currentAlert.Text, 10, currentAlert.Icon);

                        if (currentAlert.SnarlNotificationId != 0)
                        {
                            currentAlert.AlreadyShown = true;
                            if (Properties.Settings.Default.LastIdShown < currentAlert.Id)
                            {
                                Properties.Settings.Default.LastIdShown = currentAlert.Id;
                                Properties.Settings.Default.Save();
                            }
                            Requests.Add(currentAlert);
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                if (ShowDebug)
                {
                    Notifier.EZNotify("Debug", "XML Parsing error", exp.Message, 10, Properties.Settings.Default.AppIcon);
                }
            }
            UpdateTimer.Start();
        }

        string GetHttpContent(string url)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "GET";
            request.AllowAutoRedirect = true;
            request.Accept = "*/*";
            request.UserAgent = "Purple Treehouse (http://tlhan-ghun.de/)";
            string returnValue = "";
            try
            {
                HttpWebResponse responseTemp = (HttpWebResponse)request.GetResponse();
                HttpWebResponse response = responseTemp;
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    returnValue = reader.ReadToEnd();
                }
            }
            catch (Exception exp)
            {
                if (ShowDebug)
                {
                    Notifier.EZNotify("Debug", "HTTP Communication error", exp.Message, 10, Properties.Settings.Default.AppIcon);
                }
            }


            return returnValue;
        }

        public void MousebuttonHasBeenClicked(string button, Int32 NotificationId) {
            IncomingRequest request = Requests.Where(r => r.SnarlNotificationId == NotificationId).FirstOrDefault();
            if (request != null)
            {
                switch (button)
                {
                    case "left":
                        if(request.LeftClickUrl != null) {
                            try
                            {
                                System.Diagnostics.Process.Start(request.LeftClickUrl);
                            }
                            catch { }
                        }
                        break;

                    case "right":
                        if (request.RightClickUrl != null)
                        {
                            try {
                            System.Diagnostics.Process.Start(request.RightClickUrl);
                            }
                            catch { }
                        }
                        break;

                    case "middle":
                        if (request.RightClickUrl != null)
                        {
                            try {
                            System.Diagnostics.Process.Start(request.MiddleClickUrl);
                            }
                            catch { }
                        }
                        break;

                    default:
                        break;

                }
            }

        }
    }
}
