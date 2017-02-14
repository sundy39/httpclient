using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace XData.Net.Http
{
    public partial class HttpClient
    {
        public async Task<string[]> UploadAsync(string requestUriString, string[] fileNames)
        {
            HttpWebRequest request = CreateUploadRequest(requestUriString, fileNames);
            string result = await GetResponseStringAsync(request);
            string lowerResult = result.ToLower().Trim(new char[] { '\r', '\n' }).Trim(new char[] { '\r', '\n' });
            if (lowerResult.StartsWith("<!DOCTYPE html>".ToLower())) throw new WebException("Upload failed.");
            if (lowerResult.StartsWith("<html>") && lowerResult.EndsWith("</html>")) throw new WebException("Upload failed.");
            return result.Split(',');
        }

        public async Task<DownloadInfo> SaveAsAsync(string requestUriString, string identity, string saveAsFileName)
        {
            HttpWebRequest request = CreateDownloadRequest(requestUriString, identity);
            WebResponse response = null;
            try
            {
                response = await CreateTask(request);
                string contentType = response.ContentType;
                if (response.Headers["Content-Disposition"] == null) throw new WebException("Download failed.");
                string contentDisposition = response.Headers["Content-Disposition"];
                string fileDownloadName = GetFileDownloadName(contentDisposition);
                SaveAs(response, saveAsFileName);
                return new DownloadInfo(fileDownloadName, contentType);
            }
            catch (WebException ex)
            {
                response = ex.Response;
                string text = GetString(response);
                throw new WebException(ex.Message, new Exception(text));
            }
            finally
            {
                if (response != null) response.Close();
            }
        }

        public async Task<DownloadInfo> DownloadAsync(string requestUriString, string identity)
        {
            string fileDownloadName;
            string saveAsFileName;
            HttpWebRequest request = CreateDownloadRequest(requestUriString, identity);
            WebResponse response = null;
            try
            {
                response = await CreateTask(request);
                string contentType = response.ContentType;
                if (response.Headers["Content-Disposition"] == null) throw new WebException("Download failed.");
                string contentDisposition = response.Headers["Content-Disposition"];
                fileDownloadName = GetFileDownloadName(contentDisposition);
                string tempPath = Path.GetTempPath();
                saveAsFileName = Path.Combine(tempPath, fileDownloadName);
                int i = 0;
                string extension = Path.GetExtension(saveAsFileName);
                string forebody = saveAsFileName.Substring(0, saveAsFileName.Length - extension.Length);
                while (File.Exists(saveAsFileName))
                {
                    saveAsFileName = string.Format("{0} ({1}){2}", forebody, ++i, extension);
                }
                SaveAs(response, saveAsFileName);
                return new DownloadInfo(saveAsFileName, contentType);
            }
            catch (WebException ex)
            {
                response = ex.Response;
                string text = GetString(response);
                throw new WebException(ex.Message, new Exception(text));
            }
            finally
            {
                if (response != null) response.Close();
            }
        }

        public async Task<DownloadInfo> GetBytesAsync(string requestUriString, string identity)
        {
            HttpWebRequest request = CreateDownloadRequest(requestUriString, identity);

            WebResponse response = null;
            Stream responseStream = null;
            try
            {
                response = await CreateTask(request);
                responseStream = response.GetResponseStream();
                BinaryReader reader = new BinaryReader(responseStream);
                byte[] bytes = reader.ReadBytes((int)response.ContentLength);
                reader.Close();
                string contentType = response.ContentType;
                if (response.Headers["Content-Disposition"] == null) throw new WebException("Download failed.");
                string contentDisposition = response.Headers["Content-Disposition"];
                string fileDownloadName = GetFileDownloadName(contentDisposition);
                return new DownloadInfo(fileDownloadName, contentType, bytes);
            }
            catch (WebException ex)
            {
                response = ex.Response;
                string text = GetString(response);
                throw new WebException(ex.Message, new Exception(text));
            }
            finally
            {
                if (responseStream != null) responseStream.Close();
                if (response != null) response.Close();
            }
        }


    }

    public class DownloadInfo
    {
        public string FileDownloadName { get; private set; }
        public string ContentType { get; private set; }
        public byte[] Bytes { get; private set; }

        public DownloadInfo(string fileDownloadName, string contentType, byte[] bytes = null)
        {
            FileDownloadName = fileDownloadName;
            ContentType = contentType;
            Bytes = bytes;
        }
    }


}
