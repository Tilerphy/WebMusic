using FM89.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Web;

namespace FM89
{
    public class Initialize
    {
        //private FM89Context db = new FM89Context();

        public static void Init()
        {
                string localMusicPath = ConfigurationManager.AppSettings["LocalMusicsStore"];
                List<Music> musics = ReadLocalMusics(localMusicPath);
                FM89Context db = new FM89Context();
                foreach (Music music in musics)
                {
                    db.Musics.Add(music);
                    CreateMusicDefaultRecord(music);
                }
                db.SaveChanges();
        }

        public static void Init(string musicFullaPath) 
        {
            FM89Context db = new FM89Context();
            Music music = ReadLocalMusic(musicFullaPath);
            if (music != null)
            {
                db.Musics.Add(music);
                CreateMusicDefaultRecord(music);
            }
            db.SaveChanges();
        }

        /// <summary>
        /// ListenedCount = 0,
        ///SharedListened =1,
        /// HatingId= 3,
        /// WhoShared = 4,
        /// SharedListendCount= 5,
        /// SharingUrl = 6,
        /// </summary>
        /// <param name="storePath"></param>
        /// <returns></returns>
        /// 
        public static void CreateMusicDefaultRecord(Music music)
        {
            CreateMusicListenedCount(music);
            //CreateMusicSharedListendCount(music);
        }

        private static void CreateMusicListenedCount(Music music)
        {
            FM89Context db = new FM89Context();
            db.Records.Add(new Record
            {
                ID = Guid.NewGuid().ToString(),
                RecordID = music.MusicID,
                RecordType = RecordType.ListenedCount,
                RecordData = "0"
            });

            db.SaveChanges();
        }

        private static void CreateMusicSharedListendCount(Music music)
        {
            FM89Context db = new FM89Context();
            db.Records.Add(new Record
            {
                ID = Guid.NewGuid().ToString(),
                RecordID = music.MusicID,
                RecordType = RecordType.SharedListendCount,
                RecordData = "0"
            });

            db.SaveChanges();
        }

        private static List<Music> ReadLocalMusics(string storePath)
        {
            DirectoryInfo folder = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, storePath));
            FileInfo[] hasSync = folder.GetFiles("*.sync");
            if (hasSync != null && hasSync.Length > 0)
            {
                return new List<Music>();
            }
            else
            {
                FileInfo[] musicFiles = folder.GetFiles("*.mp3");
                List<Music> musics = new List<Music>();
          
                foreach (FileInfo musicFile in musicFiles)
                {
                    Music m = ReadLocalMusic(musicFile.FullName);
                    if (m != null)
                    {
                        musics.Add(m);
                    }
                }
                File.Create(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, storePath, "sync.sync")).Close();
                return musics;
            }
        }

        private static Music ReadLocalMusic(string singleMusicPath) 
        {
            if (singleMusicPath.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                FileInfo rawfile = new FileInfo(singleMusicPath);
                TagLib.File file = null;
                try
                {
                    file = TagLib.File.Create(rawfile.FullName);
                }
                catch
                {
                    return null;
                }
                if (file.MimeType.Contains("mp3"))
                {
                    Music music = new Music()
                    {
                        CreatedTime = DateTime.UtcNow.Ticks.ToString(),
                        Desciption = "2013 Musics",
                        MusicFileName = Uri.EscapeUriString(rawfile.Name),
                        MusicFilePath = rawfile.FullName,
                        MusicID = Guid.NewGuid().ToString(),
                        UploaderID = "SELF",
                        TagInfo = file==null?null: string.Format("{0}-{1}", file.Tag.FirstPerformer, file.Tag.Title),
                        TagPicFileName = file==null?null: CreateTagPic(rawfile, file)

                    };
                    return music;
                }
                return null;
            }
            else 
            {
                return null;
            }

        }

        private static string CreateTagPic(FileInfo musicFile, TagLib.File file) 
        {
            try
            {
                if (file.Tag.Pictures != null && file.Tag.Pictures.Length > 0)
                {
                    string createFileName = musicFile.Name + ".png";
                    string folder = musicFile.DirectoryName;
                    System.Drawing.Image img = System.Drawing.Bitmap.FromStream(new MemoryStream(file.Tag.Pictures[0].Data.ToArray()));
                    img.Save(Path.Combine(folder, createFileName));
                    img.Dispose();
                    return createFileName;
                }
                else
                {
                    return null;
                }
            }
            catch 
            {
                musicFile.Delete();
                return null;
            }
        }
    }
}