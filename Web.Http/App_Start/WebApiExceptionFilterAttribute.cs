using System;
using System.Net.Http;
using System.Web.Http.Filters;
using System.Xml.Linq;

namespace XData.Web.Http
{
    public class WebApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            //XElement element = new XElement("Error");
            //element.Add(new XElement("Message", "An error occurred."));
            //element.Add(new XElement("ExceptionMessage", actionExecutedContext.Exception.Message));
            //element.Add(new XElement("ExceptionType", actionExecutedContext.Exception.GetType().FullName));
            //element.Add(new XElement("StackTrace", actionExecutedContext.Exception.StackTrace));

            //Translate(actionExecutedContext.Exception, element);

            //actionExecutedContext.Response = new HttpResponseMessage() { Content = new StringContent(element.ToString()) };

            base.OnException(actionExecutedContext);
        }

        protected void Translate(Exception exception, XElement parent)
        {
            XElement element = Translate(exception);
            parent.Add(element);
            if (exception.InnerException == null) return;
            Translate(exception.InnerException, element);
        }

        protected XElement Translate(Exception exception)
        {
            XElement element = new XElement("Exception");
            Type type = exception.GetType();
            element.Add(new XElement("Type", type.FullName));
            foreach (var property in type.GetProperties())
            {
                if (property.Name == "InnerException") continue;
                if (property.GetValue(exception) != null)
                {
                    element.Add(new XElement(property.Name, property.GetValue(exception).ToString()));
                }
            }
            return element;
        }


    }
}