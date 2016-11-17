using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace FM89.Controllers
{
    public class UploadController : Controller
    {
        public const string FILE_NEW_NAME = "uniqueNewName";
        //
        // POST: /Upload?uniqueNewName={}
        public ActionResult Index()
        {
            return View();
        }

        public void Go() 
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            if (this.HasFile(context))
            {
                string path = this.Save(context);
                FM89.Initialize.Init(path);
            }
            context.Response.Redirect(context.Request.UrlReferrer.ToString());
        }

        private string Save(HttpContext context)
        {
            try
            {
                string newFileName = this.GetNewName(context);
                byte[] fileContent = this.GetFileContent(context);
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["LocalMusicsStore"]);
                string filePath = Path.Combine(folder, newFileName);
                if (System.IO.File.Exists(filePath)) 
                {
                    return filePath;
                }
                FileStream fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.ReadWrite);
                fs.Write(fileContent, 0, fileContent.Length);
                fs.Close();
               
                return filePath;
            }
            catch (Exception e)
            {
                //TODO: throw, log
                return string.Empty;
            }
        }

        private bool HasFile(HttpContext context)
        {
            if (context.Request.Files != null && context.Request.Files.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private long GetFileSize(HttpContext context)
        {
            return context.Request.Files[0].ContentLength;
        }

        /// <summary>
        /// ?n={0}
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private string GetNewName(HttpContext context)
        {
            if (context.Request.QueryString[UploadController.FILE_NEW_NAME] != null)
            {
                return context.Request.QueryString[UploadController.FILE_NEW_NAME];

            }
            else
            {
                //TODO: throw, log
                throw new ArgumentNullException("Need new file name");
            }
        }


        private byte[] GetFileContent(HttpContext context)
        {
            if (this.HasFile(context))
            {
                byte[] raw = new byte[this.GetFileSize(context)];
                Stream stream = context.Request.Files[0].InputStream;
                stream.Read(raw, 0, raw.Length);
                stream.Close();
                return raw;
            }
            else
            {
                //TODO:throw, log
                throw new FileNotFoundException("No File was found in request.");
            }
        }

    }
}
