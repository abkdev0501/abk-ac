using Arity.Data.Dto;
using Arity.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ArityApp.Controllers
{
    [Authorize]
    public class ParticularController : Controller
    {
        ParticularServices _particularServices = new ParticularServices();


        // GET: Particular
        //Listing page for Particulars 
        public ActionResult Index()
        {
            ViewBag.FromDate = Convert.ToDateTime(new DateTime(DateTime.Now.Year - 1, DateTime.Now.Month, 01));
            ViewBag.ToDate = Convert.ToDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)));
            return View();
        }

        public async Task<JsonResult> LoadParticulars(DateTime from, DateTime to)
        {
            to = to + new TimeSpan(23, 59, 59);
            from = from + new TimeSpan(00, 00, 1);
            var List = await _particularServices.FetchParticular(to, from);
            return Json(new { data = List }, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// To add new Particular
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddParticular(int? id)
        {
            ParticularDto particularDetail = new ParticularDto();
            if (id != null && id > 0)
                particularDetail = _particularServices.FetchParticularById(id ?? 0);
            return PartialView("_AddParticular", particularDetail);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parti"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddParticularToDB(ParticularDto parti)
        {
            try
            {
                _particularServices.AddParticular(parti);
            }
            catch (Exception ex)
            {

            }
            return RedirectToAction("Index");
        }
    }
}