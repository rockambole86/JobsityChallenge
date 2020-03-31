using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.AspNet.SignalR;


namespace JC.Site
{
    public class ChatHub : Hub
    {
        private static readonly List<Users>    _connectedUsers = new List<Users>();
        private static readonly List<Messages> _currentMessage = new List<Messages>();
        private readonly        Db             _db             = new Db();

        public ChatHub()
        {
            if (_connectedUsers.Count == 0 || !_connectedUsers.Exists(x => x.ConnectionId == "-1"))
            {
                _connectedUsers.Add(new Users
                {
                    ConnectionId = "-1",
                    UserName     = "JC Bot",
                    UserImage    = "images/bot.png",
                    LoginTime    = DateTime.Now.ToString(CultureInfo.InvariantCulture)
                });
            }
        }

        public void Connect(string userName)
        {
            var id = Context.ConnectionId;

            if (_connectedUsers.Count(x => x.ConnectionId == id) != 0) return;

            var userImg   = GetUserImage(userName);
            var loginTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);

            _connectedUsers.Add(new Users
            {
                ConnectionId = id,
                UserName     = userName,
                UserImage    = userImg,
                LoginTime    = loginTime
            });

            // send to caller
            Clients.Caller.onConnected(id, userName, _connectedUsers, _currentMessage);

            // send to all except caller client
            Clients.AllExcept(id).onNewUserConnected(id, userName, userImg, loginTime);
        }

        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            var item = _connectedUsers.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            if (item == null) return base.OnDisconnected(stopCalled);

            _connectedUsers.Remove(item);

            var id = Context.ConnectionId;
            Clients.All.onUserDisconnected(id, item.UserName);

            return base.OnDisconnected(stopCalled);
        }

        public void SendPrivateMessage(string toUserId, string message)
        {
            var fromUserId = Context.ConnectionId;

            var toUser   = _connectedUsers.FirstOrDefault(x => x.ConnectionId == toUserId);
            var fromUser = _connectedUsers.FirstOrDefault(x => x.ConnectionId == fromUserId);

            if (toUser == null || fromUser == null) return;

            var CurrentDateTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            var UserImg         = GetUserImage(fromUser.UserName);

            // send to
            Clients.Client(toUserId).sendPrivateMessage(fromUserId, fromUser.UserName, message, UserImg, CurrentDateTime);

            // send to caller user
            Clients.Caller.sendPrivateMessage(toUserId, fromUser.UserName, message, UserImg, CurrentDateTime);
        }

        public void SendMessageToAll(string userName, string message, string time)
        {
            //Check if message is command
            if (message.StartsWith("/"))
            {
                message = ParseCommandAsync(message);
                userName = "JC Bot";
            }

            if (string.IsNullOrEmpty(message)) return;

            var UserImg = GetUserImage(userName);

            // store last 50 messages in cache
            if (userName != "JC Bot") AddMessageinCache(userName, message, time, UserImg);

            // Broadcast message
            Clients.All.messageReceived(userName, message, time, UserImg);
        }

        private static void AddMessageinCache(string userName, string message, string time, string UserImg)
        {
            var msgBufferSize = Convert.ToInt32(ConfigurationManager.AppSettings["msgBufferSize"]);

            _currentMessage.Add(new Messages {UserName = userName, Message = message, Time = time, UserImage = UserImg});

            if (_currentMessage.Count > msgBufferSize)
                _currentMessage.RemoveAt(0);
        }

        public void ClearTimeout()
        {
            _currentMessage.Clear();
        }

        public string GetUserImage(string username)
        {
            var RetimgName = "images/dummy.png";
            try
            {
                var query     = $"select Photo from tbl_Users where UserName='{username}'";
                var ImageName = _db.GetColumnVal(query, "Photo");

                if (ImageName != "")
                    RetimgName = $"images/DP/{ImageName}";
            }
            catch //(Exception ex)
            {
                //TODO:
            }

            return RetimgName;
        }

        private string ParseCommandAsync(string cmd)
        {
            string msg;

            try
            {
                cmd = cmd.Replace("/", "");

                var arr = cmd.Split('=');
                var kv = new KeyValuePair<string, string>(arr[0], arr[1]);

                if (kv.Key.ToLowerInvariant() != "stock")
                    return "Unknown command";

                var restUri = new Uri($"https://stooq.com/q/l/?s={kv.Value}&f=sd2t2ohlcv&h&e=csv");

                using (var wc = new WebClient())
                {
                    using (var ms = new MemoryStream(wc.DownloadData(restUri)))
                    {
                        using (var sr = new StreamReader(ms))
                        {
                            var data = sr.ReadLine();
                            data = sr.ReadLine();

                            var values = data.Split(',');

                            msg = $"{values[0]} quote is {Convert.ToDecimal(values[6]):C} per share";

                            sr.Close();
                        }

                        ms.Close();
                    }
                }
            }
            catch
            {
                msg = "Error processing command";
            }


            return msg;
        }
    }
}