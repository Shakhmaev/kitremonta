﻿using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Store.WebUI.Infrastructure.Hubs
{
    [Authorize(Roles="Admin")]
    public class NotifyHub:Hub
    {
        public void SendAdminNotify(string message)
        {
            Clients.Group("Admins").notify(message);
        }
        public void RegisterAdmin()
        {
            Groups.Add(Context.ConnectionId, "Admins");
        }

        public void NewOrder()
        {
            Clients.Group("Admins").neworder();
        }

        public void AddCallToAdminInfo(string number)
        {
            Clients.Group("Admins").addcall(number);
        }

        public void RemoveCallFromAdminInfo(string number)
        {
            Clients.Group("Admins").removecall(number);
        }
    }
}