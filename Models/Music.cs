using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace FM89.Models
{
    public class Music
    {
        public string MusicID { get; set; }
        public string MusicFileName { get; set; }
        public string MusicFilePath { get; set; }
        public string Desciption { get;set;}
        public string UploaderID { get; set; }
        public string CreatedTime { get; set; }
        public string TagInfo { get; set; }
        public string TagPicFileName { get; set; }
    }

    public enum RecordType 
    {
        ListenedCount = 0,
        SharedListened =1,
        HatingId= 3,
        //WhoShared = 4,
        SharedListendCount= 5,
        SharingUrl = 6,
    }

    public class Salt
    {
        public string Id { get; set; }
        public string SaltValue { get; set; }
        public string ComputerIP { get; set; }
        public string NickName { get; set; }
    }

    public class Record 
    {
        public string ID { get; set; }
        public string RecordID { get; set; }
        public RecordType RecordType { get; set; }
        public string DataID { get; set; }
        public string RecordData { get; set; }
    }

    public class FM89Context : DbContext 
    {
        public DbSet<Record> Records { get; set; }
        public DbSet<Music> Musics { get; set; }
        public DbSet<Salt> Salts { get; set; }
    }
}