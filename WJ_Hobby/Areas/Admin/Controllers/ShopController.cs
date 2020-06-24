﻿using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using WJ_Hobby.Models.Data;
using WJ_Hobby.Models.ViewModels.Shop;

namespace WJ_Hobby.Areas.Admin.Controllers
{
    public class ShopController : Controller
    {
        // GET: Admin/Shop/Categories
        public ActionResult Categories()
        {
            //declare list of models
            List<CategoryVM> categoryVMList;
            using (Db db = new Db())
            {
                //inti list
                categoryVMList = db.Categories
                                    .ToArray()
                                    .OrderBy(x => x.Sorting)
                                    .Select(x => new CategoryVM(x))
                                    .ToList();
            }

            //return view with list
            return View(categoryVMList);
        }

        // POST: Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            //declare id
            string id;

            using (Db db = new Db())
            {
                //check that the category name is unique
                if (db.Categories.Any(x => x.Name == catName))
                    return "titletaken";

                //init dto
                CategoryDTO dto = new CategoryDTO();

                //add to dto
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;

                //save dto
                db.Categories.Add(dto);
                db.SaveChanges();

                //get the id
                id = dto.Id.ToString();
            }

            //return id
            return id;
        }

        // Post: Admin/shop/ReorderCategories
        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {

                //set initial count
                int count = 1;
                //declare CategoryDTO
                CategoryDTO dto;
                //set sorting for each category
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }


        }

        // GET: Admin/shop/DeleteCategory/id
        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {

                //get the category
                CategoryDTO dto = db.Categories.Find(id);
                //remove the category
                db.Categories.Remove(dto);
                //save
                db.SaveChanges();
            }

            //redirect
            return RedirectToAction("Categories");
        }

        // Post: Admin/shop/RenameCategory
        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db())
            {
                //check cat name unique
                if (db.Categories.Any(x => x.Name == newCatName))
                    return "titletaken";


                //get dto          
                CategoryDTO dto = db.Categories.Find(id);

                //edit dto
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                //save
                db.SaveChanges();
            }

            return "ok";

        }

        // GET: Admin/shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            //init model
            ProductVM model = new ProductVM();

            //add select list of categories to model
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            return View(model);
        }

        // POST: Admin/shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {

            //CHECK if model state 
            if (!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }

            }

            //make sure product name is unique
            using (Db db = new Db())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "That product name is taken");
                    return View(model);
                }
            }

            //declare pruduct id
            int id;

            //init and save product dto
            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();

                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDTO.Name;

                db.Products.Add(product);
                db.SaveChanges();

                //get the id
                id = product.Id;
            }

            //set temp data
            TempData["SM"] = "You have added a new product!";

            #region Upload Image

            //create the directories
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);

            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);

            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);

            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);

            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);

            //check if file was uploaded
            if (file != null && file.ContentLength > 0)
            {

                //get file extension
                string ext = file.ContentType.ToLower();
                //verify extension
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "The image was not uploaded - Wrong image extension.");
                        return View(model);

                    }
                }


                //init image name
                string imageName = file.FileName;
                //save image name to dto
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }

                //set original and thumb image paths
                var path = string.Format("{0}\\{1}", pathString2, imageName);
                var path2 = string.Format("{0}\\{1}", pathString3, imageName);

                //save original
                file.SaveAs(path);
                //create and save thumb
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }

            #endregion


            //redirect
            return RedirectToAction("AddProduct");
        }

        // GET: Admin/shop/Products

        public ActionResult Products(int? page, int? catId)
        {
            //declare a list of products
            List<ProductVM> listOfProductVM;

            //set page number
            var pageNumber = page ?? 1;

            using (Db db = new Db())
            {

                //init the list 
                listOfProductVM = db.Products.ToArray()
                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                    .Select(x => new ProductVM(x))
                    .ToList();

                //populate categories select list
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //set selected catefory
                ViewBag.SelectedCat = catId.ToString();
            }

            //set pagination
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);
            ViewBag.OnePageOfProducts = onePageOfProducts;

            //return view with list
            return View(listOfProductVM);
        }

        // GET: Admin/shop/EditProduct/id
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            //declare productvm
            ProductVM model;

            using (Db db = new Db())
            {
                //get the product
                ProductDTO dto = db.Products.Find(id);

                //make sure product exists
                if (dto == null)
                {
                    return Content("That product does not exist.");
                }

                //init model
                model = new ProductVM(dto);

                //make a select list
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //get all gallery images
                model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                               .Select(fn => Path.GetFileName(fn));
            }
            return View(model);
        }

        // POST: Admin/shop/EditProduct/id
        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {

            //get product id
            int id = model.Id;

            //populate categories select list and gallery images
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                               .Select(fn => Path.GetFileName(fn));

            //check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //make sure product name is unique
            using (Db db = new Db())
            {
                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "That product name is taken");
                    return View(model);
                }
            }

            //update product
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);

                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;

                db.SaveChanges();
            }

            //set temp data message
            TempData["SM"] = "You have edited the product";

            #region Image Upload

            //check for file upload
            if (file != null && file.ContentLength > 0)
            {

                //get extension
                string ext = file.ContentType.ToLower();

                //verify extension
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        ModelState.AddModelError("", "The image was not uploaded - Wrong image extension.");
                        return View(model);

                    }
                }
                //set upload directory paths
                var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                //delete files from directories
                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (FileInfo file2 in di1.GetFiles())
                    file2.Delete();

                foreach (FileInfo file3 in di2.GetFiles())
                    file3.Delete();

                //save image name
                string imageName = file.FileName;

                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }

                //save original and thumb images

                var path = string.Format("{0}\\{1}", pathString1, imageName);
                var path2 = string.Format("{0}\\{1}", pathString2, imageName);

                file.SaveAs(path);

                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }

            #endregion

            return RedirectToAction("EditProduct");
        }

        // GET: Admin/shop/DeleteProduct/id
        public ActionResult DeleteProduct(int id)
        {

            //delete product from db
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);

                db.SaveChanges();
            }
            //delete product folder
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
            string pathString = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString)) 
                Directory.Delete(pathString, true);

            //redirect
            return RedirectToAction("Products");
        }

        // POST: Admin/shop/SaveGalleryImages
        [HttpPost]
        public void SaveGalleryImages(int id)
        {
            //loop throught the files
            foreach (string fileName in Request.Files)
            {
                //init the file
                HttpPostedFileBase file = Request.Files[fileName];

                //check it's not null
                if (file != null && file.ContentLength > 0)
                {
                    //set directory paths
                    var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                    string pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");


                    //set image paths
                    var path = string.Format("{0}\\{1}", pathString1, file.FileName);
                    var path2 = string.Format("{0}\\{1}", pathString2, file.FileName);


                    //save original and thumb
                    file.SaveAs(path);
                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200);
                    img.Save(path2);
                }
            }


        }

        // POST: Admin/shop/DeleteImage
        public void DeleteImage(int id, string imageName)
        {
            string fullPath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/" + imageName);
            string fullPath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);

            if (System.IO.File.Exists(fullPath1))
                System.IO.File.Delete(fullPath1);

            if (System.IO.File.Exists(fullPath2))
                System.IO.File.Delete(fullPath2);
        }
    }
}