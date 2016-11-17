using FM89.Logical;
using FM89.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace FM89.Controllers
{
    public class SaltApiController : ApiController
    {
        private FM89Context context = new FM89Context();

        [HttpGet]
        public string CreateSalt(string code, string nickName) 
        {
            string saltValue= SaltManager.CreateCurrentSalt(code);
            var result = context.Salts.Where(a=>a.SaltValue == saltValue);
            if (result==null || result.Count()== 0)
            {
                string id = Guid.NewGuid().ToString();

                context.Salts.Add(new Salt()
                {
                    ComputerIP = SaltManager.CurrentUserIp,
                    Id = id,
                    NickName = nickName,
                    SaltValue = saltValue
                });
                context.SaveChanges();
                this.SetSaltCookie(saltValue);
                return id;
            }
            else 
            {
                throw new Exception("Already Existed.");
            }
            
        }

        private void SetSaltCookie(string salt) 
        {
            HttpContext.Current.Response.AppendCookie(new HttpCookie(SaltManager.SALTCOOKIENAME, salt));
        }

        [HttpGet]
        public Salt GetCurrentSalt() 
        {
            string saltValue = SaltManager.CurrentSalt;
            if (string.IsNullOrEmpty(saltValue))
            {
                return null;
            }
            else 
            {
                var result = context.Salts.Where(a => a.SaltValue == saltValue);
                if (result.Count() > 0)
                {
                    this.SetSaltCookie(result.First().SaltValue);
                    return result.First();
                }
                else 
                {
                    return null;
                }
            }
        }

        [HttpGet]
        public Salt ResetSalt(string code) 
        {
            string saltValue = SaltManager.CreateSalt(SaltManager.CurrentUserIp, code);
            var result = context.Salts.Where(a => a.SaltValue == saltValue);
            if (result.Count() > 0)
            {
                this.SetSaltCookie(result.First().SaltValue);
                return result.First();
            }
            else
            {
                return null;
            }
        }

        [HttpGet]
        public Salt GetSalt(string id)
        {
            var result = context.Salts.Where(a => a.Id == id);
            if (result.Count() > 0)
            {
                return result.First();
            }
            else
            {
                return null;
            }
        }


        [HttpGet]
        public Salt GetbackSalt(string ip, string code) 
        {
            string saltValue = SaltManager.CreateSalt(ip, code);
            var result = context.Salts.Where(a => a.SaltValue == saltValue);
            if (result.Count() > 0)
            {
                this.SetSaltCookie(result.First().SaltValue);
                return result.First();
            }
            else
            {
                return null;
            }
        }

        [HttpGet]
        public void ResetNickName(string code, string nickName) 
        {
            string saltValue = SaltManager.CreateSalt(SaltManager.CurrentUserIp, code);
            var result = context.Salts.Where(a => a.SaltValue == saltValue);
            if (result.Count() > 0)
            {
                Salt salt = result.First();
                salt.NickName = nickName;
                context.Entry<Salt>(salt).State = System.Data.EntityState.Modified;
                context.SaveChanges();

            }
        }

        
    }
}