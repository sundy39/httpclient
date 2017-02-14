using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace XData.Net.Http
{
    public partial class HttpClient
    {
        public string[] Upload(string requestUriString, string[] fileNames)
        {
            HttpWebRequest request = CreateUploadRequest(requestUriString, fileNames);
            string result = GetResponseString(request);
            string lowerResult = result.ToLower().Trim(new char[] { '\r', '\n' }).Trim(new char[] { '\r', '\n' });
            if (lowerResult.StartsWith("<!DOCTYPE html>".ToLower())) throw new WebException("Upload failed.");
            if (lowerResult.StartsWith("<html>") && lowerResult.EndsWith("</html>")) throw new WebException("Upload failed.");
            return result.Split(',');
        }

        protected HttpWebRequest CreateUploadRequest(string requestUriString, string[] fileNames)
        {
            string boundary = new string('-', 32) + DateTime.Now.Ticks.ToString("x");

            HttpWebRequest request = CreateRequest(requestUriString);
            request.ContentType = string.Format("multipart/form-data; boundary={0}", boundary);
            request.ServicePoint.Expect100Continue = false;
            request.AllowAutoRedirect = false;
            request.KeepAlive = true;
            request.Timeout = int.MaxValue;
            request.Method = "POST";
            request.Accept = "text/plain,application/plain";
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Referer = requestUriString;
            request.UserAgent = UserAgent;
            request.Headers.Add("Cache-Control", "no-cache");
            request.Headers.Add("DNT", "1");

            //
            int contentLength = 0;
            List<byte[]> buffers = new List<byte[]>();
            for (int i = 0; i < fileNames.Length; i++)
            {
                byte[] buffer = GetBytes(i, fileNames[i], boundary);
                contentLength += buffer.Length;
                buffers.Add(buffer);
            }
            byte[] bytes = Encoding.UTF8.GetBytes("--" + boundary + "--" + Environment.NewLine);
            contentLength += bytes.Length;
            buffers.Add(bytes);

            request.ContentLength = contentLength;

            Stream requestStream = request.GetRequestStream();
            foreach (byte[] buffer in buffers)
            {
                requestStream.Write(buffer, 0, buffer.Length);
            }
            requestStream.Close();
            return request;
        }

        protected byte[] GetBytes(int index, string fileName, string boundary)
        {
            string __boundary = "--" + boundary;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(__boundary);
            sb.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"",
                index.ToString(), Path.GetFileName(fileName)));
            string contentType = MappingToContentType(Path.GetExtension(fileName));
            sb.AppendLine(contentType);
            sb.AppendLine();
            byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
            byte[] newLine = Encoding.UTF8.GetBytes(Environment.NewLine);

            FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (stream.Length > Int32.MaxValue) throw new Exception("The file is too big.");
            try
            {
                byte[] buffer = new byte[bytes.Length + stream.Length + newLine.Length];
                bytes.CopyTo(buffer, 0);
                stream.Read(buffer, bytes.Length, (int)stream.Length);
                newLine.CopyTo(buffer, bytes.Length + stream.Length);
                return buffer;
            }
            finally
            {
                stream.Close();
            }
        }

        protected virtual string MappingToContentType(string fileExtension)
        {
            switch (fileExtension.ToLower().TrimStart('.'))
            {
                case "htm":
                case "html":
                    return "Content-Type: text/plain";
                case "css":
                    return "Content-Type: text/css";
                case "txt":
                    return "Content-Type: text/plain";
                case "xml":
                    return "Content-Type: text/xml";
                case "docx":
                    return "Content-Type: application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case "xlsx":
                    return "Content-Type: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case "pptx":
                    return "Content-Type: application/vnd.openxmlformats-officedocument.presentationml.presentation";
                case "xsl":
                    return "Content-Type: application/vnd.ms-excel";
                case "pdf":
                    return "Content-Type: application/pdf";
                case "gif":
                    return "Content-Type: image/gif";
                case "jpg":
                    return "Content-Type: image/jpeg";
                case "png":
                    return "Content-Type: image/png";
                case "zip":
                    return "Content-Type: application/x-zip-compressed";
                case "rar":
                default:
                    return "Content-Type: application/octet-stream";
            }
        }

        public void SaveAs(string requestUriString, string identity, out string fileDownloadName, out string contentType, string saveAsFileName)
        {
            HttpWebRequest request = CreateDownloadRequest(requestUriString, identity);
            WebResponse response = null;
            try
            {
                response = request.GetResponse();
                contentType = response.ContentType;
                if (response.Headers["Content-Disposition"] == null) throw new WebException("Download failed.");
                string contentDisposition = response.Headers["Content-Disposition"];
                fileDownloadName = GetFileDownloadName(contentDisposition);
                SaveAs(response, saveAsFileName);
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

        public string Download(string requestUriString, string identity, out string contentType)
        {
            string fileDownloadName;
            string saveAsFileName;
            HttpWebRequest request = CreateDownloadRequest(requestUriString, identity);
            WebResponse response = null;
            try
            {
                response = request.GetResponse();
                contentType = response.ContentType;
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
                return saveAsFileName;
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

        protected void SaveAs(WebResponse response, string saveAsFileName)
        {
            Stream responseStream = null;
            FileStream fileStream = null;
            try
            {
                responseStream = response.GetResponseStream();
                fileStream = new FileStream(saveAsFileName, FileMode.CreateNew, FileAccess.Write, FileShare.None);

                byte[] buffer = new byte[1024];
                int length = responseStream.Read(buffer, 0, buffer.Length);
                while (length > 0)
                {
                    fileStream.Write(buffer, 0, length);
                    fileStream.Flush();
                    length = responseStream.Read(buffer, 0, buffer.Length);
                }
            }
            catch (WebException ex)
            {
                response = ex.Response;
                string text = GetString(response);
                throw new WebException(ex.Message, new Exception(text));
            }
            finally
            {
                if (fileStream != null) fileStream.Close();
                if (responseStream != null) responseStream.Close();
            }
        }

        public byte[] GetBytes(string requestUriString, string identity, out string fileDownloadName, out string contentType)
        {
            HttpWebRequest request = CreateDownloadRequest(requestUriString, identity);

            WebResponse response = null;
            Stream responseStream = null;
            try
            {
                response = request.GetResponse();
                responseStream = response.GetResponseStream();
                BinaryReader reader = new BinaryReader(responseStream);
                byte[] data = reader.ReadBytes((int)response.ContentLength);
                reader.Close();
                contentType = response.ContentType;
                if (response.Headers["Content-Disposition"] == null) throw new WebException("Download failed.");
                string contentDisposition = response.Headers["Content-Disposition"];
                fileDownloadName = GetFileDownloadName(contentDisposition);
                return data;
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

        protected string GetFileDownloadName(string contentDisposition)
        {
            // attachment; filename=google.html

            string[] array = contentDisposition.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string str in array)
            {
                int index = str.IndexOf("filename", StringComparison.OrdinalIgnoreCase);
                if (index == -1) continue;
                string s = str.Substring(index + "filename".Length);
                s = s.Trim();
                int index1 = s.IndexOf("=");
                s = s.Substring(index1 + "=".Length);
                s = s.Trim();
                s = s.TrimEnd(';');
                string[] splitArray = s.Split(new char[] { '\'' }, StringSplitOptions.RemoveEmptyEntries);
                if (splitArray[0] == "UTF-8")
                {
                    string s1 = splitArray[1];
                    List<byte> bytes = new List<byte>();
                    while (s1.StartsWith("%"))
                    {
                        bytes.Add(Convert.ToByte(s1.Substring(1, 2), 16));
                        s1 = s1.Substring(3);
                    }
                    return Encoding.UTF8.GetString(bytes.ToArray()) + s1;
                }
                return s;
            }
            return null;
        }

        protected HttpWebRequest CreateDownloadRequest(string requestUriString, string identity)
        {
            HttpWebRequest request = CreateRequest(requestUriString + "/" + identity);
            request.AllowAutoRedirect = false;
            request.KeepAlive = true;
            request.Timeout = int.MaxValue;
            request.Method = "POST";
            request.Accept = "application/octet-stream";
            request.UserAgent = UserAgent;
            request.ContentLength = 0;
            return request;
        }


    }
}
