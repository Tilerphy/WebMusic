using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace FM89.Logical
{
    public class SaltManager
    {
        public const string SALTCOOKIENAME = "MUSIC_SALT";
        public static string CurrentSalt 
        {
            get 
            {
                HttpCookie cookie = HttpContext.Current.Request.Cookies[SaltManager.SALTCOOKIENAME];
                if (HttpContext.Current.Request.Cookies[SaltManager.SALTCOOKIENAME] == null)
                {
                    return string.Empty;
                }
                else 
                {
                    return cookie.Value;
                }
            }
        }

        public static string CurrentUserIp 
        {
            get 
            {
                HttpRequest request = HttpContext.Current.Request;
                return request.UserHostAddress;
            }
        }

        public static string CreateCurrentSalt(string primaryCode) 
        {
            string currentIp = CurrentUserIp;
            return CreateSalt(currentIp, primaryCode);
        }

        public static string CreateSalt(string ip, string primaryCode) 
        {
            string combined = string.Format("{0}_{1}", ip, primaryCode);
            SHA1 sha = SHA1.Create();
            byte[] salt = sha.ComputeHash(Encoding.UTF8.GetBytes(combined));
            return Convert.ToBase64String(salt);
        }

        public static string UniqueCombinedMask(params string[] args) 
        {
            if (args != null && args.Length > 0)
            {
                StringBuilder builder = new StringBuilder();
                foreach (string arg in args) 
                {
                    builder.Append(arg);
                }

                SHA1 sha = SHA1.Create();
                byte[] mask = sha.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));
                return Convert.ToBase64String(mask);
            }
            else 
            {
                throw new ArgumentException("args can't be empty or null.");
            }
        }
    }
}