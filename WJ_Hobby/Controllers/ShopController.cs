using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Management;
using System.Web.Mvc;
using WJ_Hobby.Models.Data;
using WJ_Hobby.Models.ViewModels.Shop;

namespace WJ_Hobby.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }

        public ActionResult CategoryMenuPartial()
        {

            //declare list of categories
            List<CategoryVM> categoryVMList;

            //init the list
            using (Db db = new Db())
            {
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            }

            //return partial view with list
            return PartialView(categoryVMList);
        }

        // GET: /shop/category/name
        public ActionResult Category(string name)
        {

            //declare a list of productvm
            List<ProductVM> ProductVMList;

            using (Db db = new Db())
            {
                //get category id
                CategoryDTO categoryDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();
                int catId = categoryDTO.Id;

                //init the list
                ProductVMList = db.Products.ToArray().Where(x => x.CategoryId == catId).Select(x => new ProductVM(x)).ToList();

                //get category name
                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();
                ViewBag.CategoryName = productCat.CategoryName;
            }

            //return view with list
            return View(ProductVMList);
        }

        // GET: /shop/product-details/name
        [ActionName("product-details")]
        public ActionResult ProductDetails(string name)
        {

            //declare the vm and dto
            ProductVM model;
            ProductDTO dto;

            //init product id
            int id = 0;

            using (Db db = new Db())
            {
                //check if product exists
                if (! db.Products.Any(x => x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");
                }

                //init productdto
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();

                //get id
                id = dto.Id;

                //init model
                model = new ProductVM(dto);
            }

            //get gallery images
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                               .Select(fn => Path.GetFileName(fn));

            //return view with model
            return View("ProductDetails", model);
        }
    }
}