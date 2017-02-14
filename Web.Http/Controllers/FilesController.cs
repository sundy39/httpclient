using XData.Web.Http.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace XData.Web.Http.Controllers
{
    [Authorize]
    public class FilesController : Controller
    {
        [HttpPost]
        public ActionResult Upload(FormCollection collection)
        {
            FilesService service = new FilesService();
            List<UploadFileModel> models = new List<UploadFileModel>();
            foreach (string name in Request.Files)
            {
                if (Request.Files[name] == null) continue;
                if (Request.Files[name].ContentLength == 0) continue;
                string saveAsFileName = service.GetSaveAsFileName();
                Request.Files[name].SaveAs(saveAsFileName);
                UploadFileModel model = new UploadFileModel(int.Parse(name), Request.Files[name].FileName, Request.Files[name].ContentType, Request.Files[name].ContentLength, saveAsFileName);
                models.Add(model);
            }
            string[] result = service.Append(models.OrderBy(p => p.Index).ToArray());
            return Content(result.Aggregate((p, v) => string.Format("{0},{1}", p, v)));
        }

        public ActionResult Download(string id)
        {
            FilesService service = new FilesService();
            UploadFileModel model = service.Get(id);
            return File(model.SaveAsFileName, model.ContentType, model.FileName);
        }


    }
}
