using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using FM89.Models;
using System.Collections;
using System.Text;
using System.Configuration;
using FM89.Logical;

namespace FM89.Controllers
{
    public class MusicController : ApiController
    {
        private FM89Context db = new FM89Context();

        protected int GetRam(int min, int max)
        {
            Hashtable hashtable = new Hashtable();
            int seed = 0;
            {
                byte[] idArray = Guid.NewGuid().ToByteArray();

                int id1, id2, id3, id4;
                id1 = id2 = id3 = id4 = 0;
                id1 |= (int)idArray[0];
                id1 |= (int)idArray[1] << 8;
                id1 |= (int)idArray[2] << 16;
                id1 |= (int)idArray[3] << 24;
                id2 |= (int)idArray[4];
                id2 |= (int)idArray[5] << 8;
                id2 |= (int)idArray[6] << 16;
                id2 |= (int)idArray[7] << 24;
                id3 |= (int)idArray[8];
                id3 |= (int)idArray[9] << 8;
                id3 |= (int)idArray[10] << 16;
                id3 |= (int)idArray[11] << 24;
                id4 |= (int)idArray[12];
                id4 |= (int)idArray[13] << 8;
                id4 |= (int)idArray[14] << 16;
                id4 |= (int)idArray[15] << 24;
                seed = id1 ^ id2 ^ id3 ^ id4;
            }
            Random rm = new Random(seed);
            return rm.Next(min, max);
        }
        // GET api/Music
        public IEnumerable<Music> GetMusics()
        {
            return db.Musics.AsEnumerable();
        }

        public Music GetRandomMusic() 
        {
            int count =db.Musics.Count();
            Music selected= db.Musics.OrderBy(a => a.CreatedTime).Skip(GetRam(0,count)).First();
            if (this.IsMusicHatedByCurrentSalt(selected.MusicID))
            {
                return new Music()
                {
                    MusicFileName = "",
                    MusicFilePath = "",
                    MusicID = selected.MusicID,

                };
            }
            else
            {
                return MusicManager.GenericOutputMusic(selected);
            }
        }

        // GET api/Music/5
        public Music GetMusic(string id)
        {
            Music music = db.Musics.Single(a=>a.MusicID==id);
            if (music == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }
            
            return MusicManager.GenericOutputMusic(music);
        }



        // PUT api/Music/5
        public HttpResponseMessage PutMusic(string id, Music music)
        {
            if (ModelState.IsValid && id == music.MusicID)
            {
                db.Entry(music).State = EntityState.Modified;

                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        // POST api/Music
        public HttpResponseMessage PostMusic(Music music)
        {
            if (ModelState.IsValid)
            {
                db.Musics.Add(music);
                db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, music);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = music.MusicID }));
                return response;
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        // DELETE api/Music/5
        public HttpResponseMessage DeleteMusic(string id)
        {
            Music music = db.Musics.Find(id);
            if (music == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.Musics.Remove(music);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            return Request.CreateResponse(HttpStatusCode.OK, music);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        private bool IsMusicHatedByCurrentSalt(string musicId) 
        {
            var result = db.Salts.Where(a => a.SaltValue == SaltManager.CurrentSalt);
            if (result.Count() > 0)
            {
                Salt salt = result.First();
                var record = db.Records.Where(a => a.RecordType == RecordType.HatingId && a.RecordID == salt.Id && a.DataID == musicId);
                if (record.Count() > 0) 
                {
                    return true;
                }
            }
            return false;
        }

        [HttpGet]
        public int SetNeverPlay(string musicId) 
        {
            string saltValue = SaltManager.CurrentSalt;
             var result = db.Salts.Where(a => a.SaltValue == saltValue);
             if (result.Count() > 0) 
             {
                 Salt salt = result.First();
                 db.Records.Add(new Record() 
                 {
                     ID = Guid.NewGuid().ToString(),
                     RecordID = salt.Id,
                     DataID = musicId,
                     RecordType = RecordType.HatingId,
                    
                 });
                 db.SaveChanges();
                 
             }

             return db.Records.Count(a=>a.RecordType == RecordType.HatingId && a.DataID == musicId);
        }

        [HttpGet]
        public string ShareMusic(string musicId) 
        {
             string saltValue = SaltManager.CurrentSalt;
             var result = db.Salts.Where(a => a.SaltValue == saltValue);
             var musicResult = db.Musics.Where(a => a.MusicID == musicId);
             if (musicResult.Count()>0)
             {
                 Music music = musicResult.First();
                 if (result.Count() > 0)
                 {
                     Salt salt = result.First();

                     var sharedRecords = db.Records.Where(a => a.RecordID == music.MusicID && a.DataID == salt.Id && a.RecordType == RecordType.SharedListened);
                     if (sharedRecords.Count() == 0)
                     {
                         db.Records.Add(new Record()
                         {
                             ID = Guid.NewGuid().ToString(),
                             RecordID = music.MusicID,
                             DataID = salt.Id,
                             RecordType = RecordType.SharedListened,

                         });
                         db.SaveChanges();
                     }
                     return GetSharedMusicUrl(music.MusicID, salt.Id);
                 }
                 else 
                 {
                     return GetSharedMusicUrl(music.MusicID);
                 }
                 
             }
             return null;
        }

        private void UpdateSharingCount(Music music) 
        {
            var records = db.Records.Where(a=>a.RecordID== music.MusicID && a.RecordType == RecordType.SharedListendCount);
            if(records!=null && records.Count()>0)
            {
                Record currentRecord = records.First();
               
            }
        }

        private int PageSize = 10;
        [HttpGet]
        public List<Music> ListMusics(string sortMethod, int page) 
        {
            
            switch (sortMethod) 
            {
                case "listenedCount":
                    return (from music in db.Musics
                            join record in db.Records on music.MusicID equals record.RecordID into MusicRecords
                            from musicRecord in MusicRecords
                            orderby musicRecord.RecordData descending
                            where musicRecord.RecordType == RecordType.ListenedCount
                            select music).Skip(page * PageSize).Take(PageSize).ToOutputMusicList();
                case "createdTime":
                    return (from music in db.Musics
                            orderby music.CreatedTime descending
                            select music).Skip(page * PageSize).Take(PageSize).ToOutputMusicList();
                case "sharedListenedCount":
                    return (from music in db.Musics
                            join record in db.Records on music.MusicID equals record.RecordID into MusicRecords
                            from musicRecord in MusicRecords
                            orderby musicRecord.RecordData descending
                            where musicRecord.RecordType == RecordType.SharedListened
                            select music).Skip(page * PageSize).Take(PageSize).ToOutputMusicList();
                case "all":
                    return (from music in db.Musics
                            orderby music.CreatedTime descending
                            select music).ToOutputMusicList();
                default: return null;
            }



                            
                            
        }

        private string GetSharedMusicUrl(string musicId, string saltId) 
        {
            return string.Format("{0}/SharedMusic/ListenFrom?musicId={1}&saltId={2}", ConfigurationManager.AppSettings["ServiceAddress"], musicId, saltId);
        }

        private string GetSharedMusicUrl(string musicId)
        {
            return string.Format("{0}/SharedMusic/Listen?musicId={1}", ConfigurationManager.AppSettings["ServiceAddress"], musicId);
        }
    }
}