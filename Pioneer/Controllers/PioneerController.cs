using Microsoft.AspNetCore.Mvc;
using Pioneer.Models;

namespace Pioneer.Controllers
{
    public class PioneerController : Controller
    {
        PioneerDBContext dbContext;
        public PioneerController(PioneerDBContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public IActionResult PioneerPortal()
        {
            return View();
        }

        [HttpPost]
        public IActionResult PioneerPortal(PioneerLogin login)
        {
            if (ModelState.IsValid)
            {
                var r = dbContext.Freshmen.Where
                        (l => l.RegistrationNumber == login.RegistrationNumber).FirstOrDefault();

                if (r != null)
                    return RedirectToAction("Dashboard", login);

                return View();
            }
            return View();
        }

        public IActionResult InsertFreshman()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult InsertFreshman(Freshman freshman)
        {
            if (ModelState.IsValid)
            {
                if (freshman.RegistrationNumber == freshman.Password)
                {
                    ViewBag.Message = "RegistrationNumber and Password should not be the same";
                }
                else
                {
                    dbContext.Freshmen.Add(freshman);
                    dbContext.SaveChanges();
                    return RedirectToAction("PioneerPortal");
                }

                return View();
            }

            return View();
        }

        public IActionResult Dashboard(PioneerLogin model)
        {
            return View(model);
        }
    }
}
