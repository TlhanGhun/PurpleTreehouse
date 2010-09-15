using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using PurpleTreeHouse;
using System.Linq;
using Snarl;
using Snarl.V41;

namespace NativeWindowApplication
{

    // Summary description for SnarlMsgWnd.
    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]


    public class SnarlMsgWnd : NativeWindow
    {
        CreateParams cp = new CreateParams();

        public string pathToIcon = "";
        private uint _globalMsgV41 = 0;
        private const Int32 ReplyMsgV41 = 17;

        public SnarlMsgWnd()
        {
            // Create the actual window
            this.CreateHandle(cp);
            _globalMsgV41 = Snarl.V41.SnarlConnector.Broadcast();
        }


        protected override void WndProc(ref Message m)
        {

            if (m.Msg == _globalMsgV41)
            {
                if (m.WParam == (IntPtr)Snarl.V41.SnarlConnector.GlobalEvent.SnarlQuit)
                {
                    AppController.Stop();
                }
                else if (m.WParam == (IntPtr)Snarl.V41.SnarlConnector.GlobalEvent.SnarlLaunched)
                {
                    AppController.Current.RegisterWithSnarl();
                }
            }
            else if (m.Msg == ReplyMsgV41)
            {
                if (m.WParam == (IntPtr)Snarl.V41.SnarlConnector.MessageEvent.NotificationAck)
                {
                     AppController.Current.MousebuttonHasBeenClicked("left", (Int32)m.LParam);
                }
                else if (m.WParam == (IntPtr)Snarl.V41.SnarlConnector.MessageEvent.NotificationCancelled)
                {
                    AppController.Current.MousebuttonHasBeenClicked("right", (Int32)m.LParam);
                }
                else if (m.WParam == (IntPtr)Snarl.V41.SnarlConnector.MessageEvent.NotificationMiddleButton)
                {
                    AppController.Current.MousebuttonHasBeenClicked("middle", (Int32)m.LParam);
                }
                else if (m.WParam == (IntPtr)Snarl.V41.SnarlConnector.MessageEvent.NotificationTimedOut)
                {

                }
            }
            base.WndProc(ref m);

        }


    }
}


