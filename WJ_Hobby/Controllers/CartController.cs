using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WJ_Hobby.Models.Data;
using WJ_Hobby.Models.ViewModels.Cart;

namespace WJ_Hobby.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {

            //init the cart list
            var cart = Session["cart"] as List<CartVm> ?? new List<CartVm>();

            //check if cart is empty
            if (cart.Count==0 || Session["cart"] == null)
            {
                ViewBag.Message = "Your cart is empty";
                return View();
            }

            //calculate total and save to viewbag
            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;

            // view with list
            return View(cart);
        }

        public ActionResult CartPartial()
        {
            //init cart vm
            CartVm model = new CartVm();

            //init qty
            int qty = 0;

            //init price
            decimal price = 0m;

            //check for cart session
            if (Session["cart"] != null)
            {
                //get total qty and price
                var list = (List < CartVm >) Session["cart"];

                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price;
                }

                model.Quantity = qty;
                model.Price = price;
            }
            else
            {
                //or set qty and price to 0
                model.Quantity = 0;
                model.Price = 0m;
            }


            //Return partial view with model
            return PartialView(model);
        }

        public ActionResult AddToCartPartial(int id)
        {

            //init cartvm list
            List<CartVm> cart = Session["cart"] as List<CartVm> ?? new List<CartVm>();

            //init cartvm
            CartVm model = new CartVm();

            using (Db db = new Db())
            {
                //get the product
                ProductDTO product = db.Products.Find(id);

                //chack if product is already in cart
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

                //if not add new 
                if (productInCart == null)
                {
                    cart.Add(new CartVm()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName
                    });
                }
                else
                {
                    //if it is, increment
                    productInCart.Quantity++;
                }
            }

            //get total qty and price and add to model
            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;

            //save cart back to session
            Session["cart"] = cart;

            //return partialview
            return PartialView(model);
        }

        // GET: Cart/IncrementProduct
        public JsonResult IncrementProduct(int productId)
        {

            //init cart list
            List<CartVm> cart = Session["cart"] as List<CartVm>;

            using (Db db = new Db())
            {

                //get cartvm from list
                CartVm model = cart.FirstOrDefault(x => x.ProductId == productId);

                //increment qty
                model.Quantity++;

                //store needed data
                var result = new { qty = model.Quantity, price = model.Price};

                //return json with data
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
    }
}