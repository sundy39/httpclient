using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using XData.Web.Http.Models;

namespace XData.Web.Http
{
    public class FilesService
    {
        public const string Upload_Directory_Key = "UploadDirectory";

        protected static int Id = 0;
        protected static List<UploadFileModel> Mock = new List<UploadFileModel>();

        public string GetSaveAsFileName()
        {
            string baseDirectory = ConfigurationManager.AppSettings[Upload_Directory_Key];
            if (!Directory.Exists(baseDirectory))
            {
                Directory.CreateDirectory(baseDirectory);
            }
            return Path.Combine(baseDirectory, Guid.NewGuid().ToString().Replace("-", string.Empty));
        }

        public string[] Append(UploadFileModel[] models)
        {
            string[] ids = new string[models.Length];
            for (int i = 0; i < models.Length; i++)
            {
                models[i].Id = ++Id;
                Mock.Add(models[i]);
                ids[i] = models[i].Id.ToString();
            }
            return ids;
        }

        public UploadFileModel Get(string id)
        {
            return Mock.FirstOrDefault(p => p.Id == int.Parse(id));
        }


    }
}