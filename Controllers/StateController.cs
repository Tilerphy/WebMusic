using FM89.Logical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace FM89.Controllers
{
    public class StateController : ApiController
    {
        [HttpGet]
        public string CurrentUsers() 
        {
            return MusicManager.OnlineUsers().ToString();
        }
        [HttpGet]
        public List<string> CurrentUsersIp()
        {
            return MusicManager.OnlineIps();
        }
    }
}
