using System;
using System.Net.Http;
using System.Web.Http;
using System.Xml.Linq;

namespace XData.Web.Http.ApiControllers
{
    [Authorize]
    public class ValuesController : ApiController
    {
        // GET api/data
        public XElement Get()
        {
            XElement element1 = new XElement("Element");
            element1.Add(new XElement("HttpMethod", HttpMethod.Get.ToString()));
            element1.Add(new XElement("name1", "value1"));
            XElement element2 = new XElement("Element");
            element2.Add(new XElement("HttpMethod", HttpMethod.Get.ToString()));
            element2.Add(new XElement("name2", "value2"));
            return new XElement("ArrayOfElement", element1, element2);
        }

        // GET api/data/5
        public XElement Get(string id)
        {
            XElement element = new XElement("Element");
            element.Add(new XElement("HttpMethod", HttpMethod.Get.ToString() + "/id"));
            element.Add(new XElement("name1", "value1"));
            return element;
        }

        // POST api/data
        public void Post([FromBody]XElement value)
        {
            XElement element = value;
        }

        // PUT api/data/5
        public void Put(string id, [FromBody]XElement value)
        {
            XElement element = value;
        }

        // DELETE api/data/5
        public void Delete(string id)
        {
            throw new ApplicationException("Applica", new ArrayTypeMismatchException("Arraty"));
        }

        public Element Post(string id, [FromBody]XElement value)
        {
            Element element = new Element() { HttpMethod=  HttpMethod.Post.ToString() + "/id", Name1 = "name1", Value1= 123};                 
            return element;
        }


    }

    public class Element
    {
        public string HttpMethod { get; set; }
        public string Name1 { get; set; }
        public int Value1 { get; set; }
    }


}
