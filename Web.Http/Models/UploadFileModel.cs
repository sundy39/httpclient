using System;
using System.Collections.Generic;
using System.Linq;

namespace XData.Web.Http.Models
{
    public class UploadFileModel
    {
        public int Id { get; set; }

        public int Index { get; private set; }

        public string FileName { get; private set; }

        public string ContentType { get; private set; }

        public int ContentLength { get; private set; }

        public string SaveAsFileName { get; private set; }

        public UploadFileModel(int index, string fileName, string contentType, int contentLength, string saveAsFileName)
        {
            Index = index;
            FileName = fileName;
            ContentType = contentType;
            ContentLength = contentLength;
            SaveAsFileName = saveAsFileName;
        }


    }
}