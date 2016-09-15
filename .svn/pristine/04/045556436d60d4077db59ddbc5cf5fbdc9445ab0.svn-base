using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Objects.DataClasses;
using System.Text;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Text.RegularExpressions;

using RestSharp;
using BLTServices;
using BLTServices.Authentication;
using BLTServices.Resources;
using BLTWeb.Utilities;
using BLTWeb.Models;
using BLTWeb.Helpers;

namespace BLTWeb.Controllers
{
    [RequireSSL]
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            int EventId = Convert.ToInt32(Session["EventId"]);
            if (EventId >= 1)
            {
                BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "/Events/{eventId}/PULAs";
                request.RootElement = "ArrayOfPULA";
                request.AddParameter("eventId", EventId, ParameterType.UrlSegment);
                PULAList PULAlist = serviceCaller.Execute<PULAList>(request);
                if (PULAlist != null)
                {
                    //loop thru and see if any PULA
                    Boolean anyPublished = PULAlist.PULA.Any(a => a.isPublished == 1 && !a.Expired.HasValue) ? true : false;
                    if (anyPublished) ViewData["Published"] = anyPublished;
                }
            }
            return View();
        }

        public ActionResult About()
        {
            return View();
        }
        public ActionResult ShowMapper()
        {
            return PartialView("index.html");
        }
    }
}
