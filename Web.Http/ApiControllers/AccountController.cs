using System.Web.Http;
using System.Web.Security;
using System.Xml.Linq;
using XData.Web.Http.Models;

namespace XData.Web.Http.ApiControllers
{
    [Authorize]
    public class AccountController : ApiController
    {

        [AllowAnonymous]
        public XElement Post(string id, [FromBody]XElement value)
        {
            LoginModel model = new LoginModel();
            model.UserName = value.Element("UserName").Value;
            model.Password = value.Element("Password").Value;
            model.RememberMe = false;
            if (ModelState.IsValid)
            //&& WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            {
                FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                return new XElement("Status", "OK");
            }
            return new XElement("Status", "Unauthorized");
        }

        public void Delete(string id)
        {
            //WebSecurity.Logout();
            FormsAuthentication.SignOut();
        }

 
    }
}
