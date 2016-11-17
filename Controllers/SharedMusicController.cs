using FM89.Logical;
using FM89.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FM89.Controllers
{
    public class SharedMusicController : Controller
    {
        //
        // GET: /SharedMusic/
        private const string AnonymousSalt = "ANONYMOUS";
        private FM89.Models.FM89Context context = new Models.FM89Context();
        public ActionResult ListenFrom(string musicId, string saltId)
        {
            Music music = this.FindMusic(musicId);
            Salt salt = this.FindSalt(saltId);
            if (music != null && salt != null)
            {
                if (MusicManager.HasOutOfCache(HttpContext.Request.UserHostAddress, music.MusicID, MusicManager.SharedListen_Mask))
                {
                    UpdateSharedListened(musicId, saltId);
                    MusicManager.CreateRefreshMask(HttpContext.Request.UserHostAddress, music.MusicID, MusicManager.SharedListen_Mask);
                }

                ViewBag.MusicId = musicId;
                ViewBag.SaltId = saltId;

            }
            else
            {
                return this.HttpNotFound("Can't find music or sharing people information.");
            }
            
            return View("Index");
        }

        public ActionResult Listen(string musicId)
        {
            Music music = this.FindMusic(musicId);
            if (music != null)
            {
                if (MusicManager.HasOutOfCache(HttpContext.Request.UserHostAddress, music.MusicID, MusicManager.SharedListen_Mask))
                {
                    UpdateSharedListened(musicId, null);
                    MusicManager.CreateRefreshMask(HttpContext.Request.UserHostAddress, music.MusicID, MusicManager.SharedListen_Mask);
                }

                ViewBag.MusicId = musicId;

            }
            else
            {
                return this.HttpNotFound("Can't find music or sharing people information.");
            }

            return View("Index");
        }


        private Music FindMusic(string musicId) 
        {
            var musics = context.Musics.Where(a=>a.MusicID == musicId);
            if (musics.Count() > 0)
            {
                return musics.First();
            }
            else 
            {
                return null;
            }
        }

        private Salt FindSalt(string saltId)
        {
            var salts = context.Salts.Where(a => a.Id == saltId);
            if (salts.Count() > 0)
            {
                return salts.First();
            }
            else
            {
                return null;
            }
        }

        private void UpdateSharedListened(string musicId, string saltId) 
        {
            if (string.IsNullOrEmpty(saltId)) 
            {
                saltId = AnonymousSalt;
            }
            var records = context.Records.Where(a=> a.RecordType == RecordType.SharedListened && a.RecordID == musicId && a.DataID == saltId);
            if (records.Count() > 0)
            {
                Record record = records.First();
                record.RecordData = (int.Parse(record.RecordData) + 1).ToString();
                context.Entry<Record>(record).State = System.Data.EntityState.Modified;
                context.SaveChanges();
            }
            else 
            {
                context.Records.Add(new Record()
                {
                    ID = Guid.NewGuid().ToString(),
                    RecordID = musicId,
                    DataID = saltId,
                    RecordType = RecordType.SharedListened,
                    RecordData = "1"
                });
                context.SaveChanges();
            }
            
            
        }

        

    }
}
