using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WJ_Hobby.Models.Data;
using WJ_Hobby.Models.ViewModels.Pages;

namespace WJ_Hobby.Areas.Admin.Controllers
{
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            //declare list of page vm
            List<PageVM> pagesList;
            using (Db db = new Db())
            {
                //init list
                pagesList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
            }

            //return view with list
            return View(pagesList);
            
        }

        // GET: Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }

        // Post: Admin/Pages/addpage
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            //check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            using (Db db = new Db())
            {


                //declare slug
                string slug;
                //init pageDTO
                PageDTO dto = new PageDTO();
                //DTO title
                dto.Title = model.Title;
                //check for and set slug
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();

                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }
                //make sure title and slug are unique
                if (db.Pages.Any(x => x.Title == model.Title) || db.Pages.Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "This title or slug already exists");
                    return View(model);
                }

                //dto the rest
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;

                //save dto
                db.Pages.Add(dto);
                db.SaveChanges();

            }

            //set tempdata message
            TempData["SM"] = "You added a new page.";


            //redirect
            return RedirectToAction("AddPage");
        }

        // GET: Admin/Pages/EditPage/id      
        [HttpGet]
        public ActionResult EditPage(int id)
        {

            //declare pagevm
            PageVM model;

            using (Db db = new Db())
            {


                //get page
                PageDTO dto = db.Pages.Find(id);

                //confirm page exists
                if (dto == null)
                {
                    return Content("The page does not exist.");
                }
                //init pagevm
                model = new PageVM(dto);
            }

            //return view with model
            return View(model);
        }

        // Post: Admin/Pages/EditPage/id
        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {

            //check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            using (Db db = new Db())
            {
                //get page id
                int id = model.Id;
                //init the slug
                string slug = "home";
                //get the page
                PageDTO dto = db.Pages.Find(id);
                //dto the title
                dto.Title = model.Title;
                //check for slug and set it if need be
                if (model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }
                //make sure slug and title are unique
                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title) ||
                    db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "That title or slug already exists.");
                    return View(model);
                }

                //dto the rest
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                //save the dto
                db.SaveChanges();
            }

            //set tempdata message
            TempData["SM"] = "You have edited the page";

            //redirect
            return RedirectToAction("EditPage");
        }

        // GET: Admin/Pages/PageDetails/id
        public ActionResult PageDetails(int id)
        {

            //declare pagevm
            PageVM model;

            using (Db db = new Db())
            {

                //get page
                PageDTO dto = db.Pages.Find(id);
                //confirm page exists
                if (dto == null)
                {
                    return Content("The page does not exist.");
                }
                //init pagevm
                model = new PageVM(dto);
            }
            //return view with model
            return View(model);
        }

        // GET: Admin/Pages/DeletePage/id
        public ActionResult DeletePage(int id)
        {
            using (Db db = new Db())
            {

                //get the page
                PageDTO dto = db.Pages.Find(id);
                //remove the page
                db.Pages.Remove(dto);
                //save
                db.SaveChanges();
            }

            //redirect
            return RedirectToAction("Index");
        }

        // Post: Admin/Pages/ReorderPages
        [HttpPost]
        public void ReorderPages(int[] id)
        {
            using (Db db = new Db())
            {

                //set initial count
                int count = 1;
                //declare page dto
                PageDTO dto;
                //set sorting for each page
                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }


        }

        // GET: Admin/Pages/EditSidebar
        [HttpGet]
        public ActionResult EditSidebar()
        {
            //declare model
            SidebarVM model;

            using (Db db = new Db())
            {

                //get the dto
                SidebarDTO dto = db.Sidebar.Find(1);

                //init model
                model = new SidebarVM(dto);


            }

            //return view with model
            return View(model);
        }

        // POST: Admin/Pages/EditSidebar
        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {

            using (Db db = new Db())
            {
                //get the dto
                SidebarDTO dto = db.Sidebar.Find(1);

                //dto the body
                dto.Body = model.Body;

                //save
                db.SaveChanges();
            }

            //set tempdata message
            TempData["SM"] = "You have edited the sidebar!";

            //redirect
            return RedirectToAction("EditSidebar");
        }

    }
}