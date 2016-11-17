using FM89.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;

namespace FM89.Logical
{
    public class MusicManager
    {
        public const string Listen_Mask = "Listen";
        public const string SharedListen_Mask = "SharedListen";
        public static Dictionary<string, long> RefreshCache = new Dictionary<string, long>();
        public static Dictionary<string, long> Ips = new Dictionary<string,long>();
        public static object _l = new object();
        public static bool HasOutOfCache(params string[] checkerArgs) 
        {
            string mask = SaltManager.UniqueCombinedMask(checkerArgs);
            if (MusicManager.RefreshCache.ContainsKey(mask))
            {
                long time = MusicManager.RefreshCache[mask];
                long currentTime = DateTime.UtcNow.Ticks;
                long offset = currentTime - time;
                if (offset / (10000 * 1000 * 60) >= int.Parse(ConfigurationManager.AppSettings["RefreshCacheOffset"]))
                {
                    return true;
                }
                else 
                {
                    return false;
                }
            }
            else 
            {
                return true;
            }
        }

        public static void AppendIps(string ip) 
        {
            lock (_l)
            {
                if (Ips.ContainsKey(ip))
                {
                    Ips[ip] = DateTime.UtcNow.Ticks;
                }
                else
                {
                    Ips.Add(ip, DateTime.UtcNow.Ticks);
                }
            }
        }

        public static int OnlineUsers() 
        {
            lock (_l)
            {
                int count = 0;
                foreach (string key in Ips.Keys)
                {
                    if (DateTime.UtcNow.Ticks - Ips[key] > 10L * 1000 * 10000 * 60)
                    {

                    }
                    else
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public static List<string> OnlineIps() 
        {
            lock (_l)
            {
                List<string> ips = new List<string>();
                foreach (string key in Ips.Keys)
                {
                    if (DateTime.UtcNow.Ticks - Ips[key] > 10L * 1000 * 10000 * 60)
                    {

                    }
                    else
                    {
                        ips.Add(key);
                    }
                }

                return ips;
            }
        }

        public static void CreateRefreshMask(params string[] args) 
        {
            if (args != null && args.Length > 0)
            {
                string mask = SaltManager.UniqueCombinedMask(args);
                if (MusicManager.RefreshCache.ContainsKey(mask))
                {
                    MusicManager.RefreshCache[mask] = DateTime.UtcNow.Ticks;
                }
                else
                {
                    MusicManager.RefreshCache.Add(mask, DateTime.UtcNow.Ticks);
                }
            }
            else 
            {
                throw new ArgumentException("args can't be null or empty.");
            }
        }

        

        public static Music GenericOutputMusic(Music music, bool justOutputMetadata= false)
        {
            FM89Context db = new FM89Context();
            StringBuilder builder = new StringBuilder("/");
            string localMusicPath = ConfigurationManager.AppSettings["LocalMusicsStore"];
            string uploadedMusicPath = ConfigurationManager.AppSettings["UploadedMusicsStore"];
            if (music.UploaderID == "SELF")
            {
                builder.AppendFormat("{0}/{1}", localMusicPath, music.MusicFileName);
            }
            else
            {
                builder.AppendFormat("{0}/{1}", uploadedMusicPath, music.MusicFileName);
            }
            if (justOutputMetadata) 
            {
                music.MusicFileName = HttpUtility.HtmlEncode(music.MusicFileName);
            }
            //cast to url path, but do not save into database
            music.MusicFilePath = builder.ToString();

            if (!justOutputMetadata)
            {
                if (MusicManager.HasOutOfCache(HttpContext.Current.Request.UserHostAddress, music.MusicID, MusicManager.Listen_Mask))
                {
                    UpdateMusicListenedRecord(music.MusicID);
                    MusicManager.CreateRefreshMask(HttpContext.Current.Request.UserHostAddress, music.MusicID, MusicManager.Listen_Mask);
                }
            }
            MusicManager.AppendIps(HttpContext.Current.Request.UserHostAddress);
            return music;
        }

        public static string UpdateMusicListenedRecord(string musicId) 
        {
            FM89Context db = new FM89Context();
            var records = db.Records.Where(a=>a.RecordID == musicId && a.RecordType == RecordType.ListenedCount);
            if (records.Count() > 0)
            {
                Record record = records.First();
                record.RecordData = (int.Parse(record.RecordData) + 1).ToString();
                db.Entry<Record>(record).State = System.Data.EntityState.Modified;
                db.SaveChanges();
                return record.RecordData;
            }
            else 
            {
                Record record = new Record()
                {
                    ID = Guid.NewGuid().ToString(),
                    RecordID = musicId,
                    RecordData = "1",
                    RecordType = RecordType.ListenedCount
                };
                db.Records.Add(record);
                return record.RecordData;
            }
        
        }
    }
}