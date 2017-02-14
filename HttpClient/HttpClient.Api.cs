using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Linq;

namespace XData.Net.Http
{
    public partial class HttpClient
    {
        public XElement ApiGet(string requestUriString)
        {
            HttpWebRequest request = ApiCreateRequest(requestUriString, "GET", null);
            return ApiGetResponseElement(request);
        }

        // overload
        public XElement ApiGet(string requestUriString, string id)
        {
            string requestUri = requestUriString + "/" + id;
            return ApiGet(requestUri);
        }

        public void ApiPost(string requestUriString, XElement value)
        {
            HttpWebRequest request = ApiCreateRequest(requestUriString, "POST", ApiGetBytes(value));
            ApiResponseEmpty(request);
        }

        public void ApiPut(string requestUriString, string id, XElement value)
        {
            string requestUri = requestUriString + "/" + id;
            HttpWebRequest request = ApiCreateRequest(requestUri, "PUT", ApiGetBytes(value));
            ApiResponseEmpty(request);
        }

        public void ApiDelete(string requestUriString, string id)
        {
            string requestUri = requestUriString + "/" + id;
            HttpWebRequest request = ApiCreateRequest(requestUri, "DELETE", null);
            ApiResponseEmpty(request);
        }

        //
        public XElement ApiPost(string requestUriString, string id, XElement value)
        {
            string requestUri = requestUriString + "/" + id;
            HttpWebRequest request = ApiCreateRequest(requestUri, "POST", ApiGetBytes(value));
            return ApiGetResponseElement(request);
        }

        protected HttpWebRequest ApiCreateRequest(string requestUriString, string method, byte[] content)
        {
            return CreateRequest(requestUriString, method, "text/xml,application/xml", content, "application/xml");
        }

        protected byte[] ApiGetBytes(XElement element)
        {
            MemoryStream stream = new MemoryStream();
            element.Save(stream);
            byte[] buffer = stream.ToArray();
            stream.Close();
            return buffer;
        }

        protected XElement ApiGetResponseElement(WebRequest request)
        {
            WebResponse response = null;
            try
            {
                response = request.GetResponse();
                XElement element = GetElement(response);
                return element;
            }
            catch (WebException ex)
            {
                response = ex.Response;
                XElement element = GetElement(response);

                Debug.Assert(element.Name.LocalName == "Error");

                throw new WebException(ex.Message,
                    new Exception(element.Element("ExceptionMessage").Value, new Exception(element.ToString())));
            }
            finally
            {
                if (response != null) response.Close();
            }
        }

        protected XElement GetElement(WebResponse response)
        {
            Stream responseStream = null;
            try
            {
                responseStream = response.GetResponseStream();
                XmlReader reader = null;
                try
                {
                    reader = XmlReader.Create(responseStream);
                    XElement element = XElement.Load(reader);
                    return element;
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
            finally
            {
                if (responseStream != null) responseStream.Close();
            }
        }

        protected void ApiResponseEmpty(WebRequest request)
        {
            WebResponse response = null;
            try
            {
                response = request.GetResponse();
                string text = GetString(response);
                if (text == string.Empty) return;
                XElement element = XElement.Parse(text);
            }
            catch (WebException ex)
            {
                response = ex.Response;
                XElement element = GetElement(response);

                Debug.Assert(element.Name.LocalName == "Error");

                throw new WebException(ex.Message,
                    new Exception(element.Element("ExceptionMessage").Value, new Exception(element.ToString())));
            }
            finally
            {
                if (response != null) response.Close();
            }
        }

        //
        public XElement ApiLogin(string requestUriString, string userName, string password, bool rememberMe)
        {
            XElement element = new XElement("Login");
            element.Add(new XElement("UserName", userName));
            element.Add(new XElement("Password", password));
            element.Add(new XElement("RememberMe", rememberMe.ToString()));
            return ApiPost(requestUriString, "0", element);
        }

        public void ApiLogOff(string requestUriString)
        {
            ApiDelete(requestUriString, "0");
        }


    }
}
