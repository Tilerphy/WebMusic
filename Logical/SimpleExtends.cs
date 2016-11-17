using FM89.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FM89.Logical
{
    public static class SimpleExtends
    {
        public static List<Music> ToOutputMusicList<TSource>(this IQueryable<TSource> source)
        {

            List<Music> result = new List<Music>();
            foreach (TSource t in source)
            {
                if (t is Music)
                {
                    result.Add(MusicManager.GenericOutputMusic(t as Music, true));
                }
                else
                {
                    throw new Exception("Can't not output source as Music.");
                }
            }
            return result;
        }
    }
}