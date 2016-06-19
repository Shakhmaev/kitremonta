using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Store.WebUI.Infrastructure.Hubs
{
    [Authorize(Roles="Admin")]
    public class NotifyHub:Hub
    {
        private readonly static ConnectionMapping<string> _connections =
            new ConnectionMapping<string>();

        private static IHubContext hubContext;
        /// <summary>Gets the hub context.</summary>
        /// <value>The hub context.</value>
        public static IHubContext HubContext
        {
            get
            {
                if (hubContext == null)
                    hubContext = GlobalHost.ConnectionManager.GetHubContext<NotifyHub>();
                return hubContext;
            }
        }

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

        public void SendChatMessage(string who, string message)
        {
            string name = Context.User.Identity.Name;

            foreach (var connectionId in _connections.GetConnections(who))
            {
                Clients.Client(connectionId).addmsg(message);
            }
        }

        public static void SendServerMessageTo(string who, string message)
        {
            foreach (var connectionId in _connections.GetConnections(who))
            {
                HubContext.Clients.Client(connectionId).addmsg(message);
            }
        }

        public string GetName()
        {
            return Context.User.Identity.Name;
        }

        public override Task OnConnected()
        {
            string name = Context.User.Identity.Name;

            _connections.Add(name, Context.ConnectionId);

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            string name = Context.User.Identity.Name;

            _connections.Remove(name, Context.ConnectionId);

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            string name = Context.User.Identity.Name;

            if (!_connections.GetConnections(name).Contains(Context.ConnectionId))
            {
                _connections.Add(name, Context.ConnectionId);
            }
            
            return base.OnReconnected();
        }

        public static IHubContext GetNotify()
        {
            return GlobalHost.ConnectionManager.GetHubContext<NotifyHub>();
        }
    }

    public class ConnectionMapping<T>
    {
        private readonly Dictionary<T, HashSet<string>> _connections =
            new Dictionary<T, HashSet<string>>();

        public int Count
        {
            get
            {
                return _connections.Count;
            }
        }

        public void Add(T key, string connectionId)
        {
            lock (_connections)
            {
                HashSet<string> connections;
                if (!_connections.TryGetValue(key, out connections))
                {
                    connections = new HashSet<string>();
                    _connections.Add(key, connections);
                }

                lock (connections)
                {
                    connections.Add(connectionId);
                }
            }
        }

        public IEnumerable<string> GetConnections(T key)
        {
            HashSet<string> connections;
            if (_connections.TryGetValue(key, out connections))
            {
                return connections;
            }

            return Enumerable.Empty<string>();
        }

        public void Remove(T key, string connectionId)
        {
            lock (_connections)
            {
                HashSet<string> connections;
                if (!_connections.TryGetValue(key, out connections))
                {
                    return;
                }

                lock (connections)
                {
                    connections.Remove(connectionId);

                    if (connections.Count == 0)
                    {
                        _connections.Remove(key);
                    }
                }
            }
        }
    }
}