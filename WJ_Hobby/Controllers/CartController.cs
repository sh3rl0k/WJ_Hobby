using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Net;
using System.Net.Mail;
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

        // GET: Cart/DecrementProduct
        public ActionResult DecrementProduct(int productId)
        {

            //init cart
            List<CartVm> cart = Session["cart"] as List<CartVm>;

            using (Db db = new Db())
            {

                //get cartvm from list
                CartVm model = cart.FirstOrDefault(x => x.ProductId == productId);

                //decrement qty
                if (model.Quantity > 1)
                {
                    model.Quantity--;
                }
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }
           

                //store needed data
                var result = new { qty = model.Quantity, price = model.Price };

                //return json with data
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }


        // GET: Cart/RemoveProduct
        public void RemoveProduct(int productId)
        {
            //init cart
            List<CartVm> cart = Session["cart"] as List<CartVm>;

            using (Db db = new Db())
            {
                //get model from list
                CartVm model = cart.FirstOrDefault(x => x.ProductId == productId);

                //remove model from list
                cart.Remove(model);
            }
        }

        public ActionResult PaypalPartial()
        {
            List<CartVm> cart = Session["cart"] as List<CartVm>;

            return PartialView(cart);
        }

        // POST: Cart/PlaceOrder
        [HttpPost]
        public void PlaceOrder()
        {

            //get cart list
            List<CartVm> cart = Session["cart"] as List<CartVm>;

            //get username
            string username = User.Identity.Name;

            //get orderid
            int orderId = 0;
        
            using (Db db = new Db())
            {
                //init orderdto
                OrderDTO orderDTO = new OrderDTO();

                //get user id
                var q = db.Users.FirstOrDefault(x => x.Username == username);
                int userId = q.Id;

                //add to orderdto and save
                orderDTO.UserId = userId;
                orderDTO.CreatedAt = DateTime.Now;

                db.Orders.Add(orderDTO);

                db.SaveChanges();

                //get inserted id
                orderId = orderDTO.OrderId;

                //init orderdetailsdto
                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();

                //add to orderdetailsdto
                foreach (var item in cart)
                {
                    orderDetailsDTO.OrderId = orderId;
                    orderDetailsDTO.UserId = userId;
                    orderDetailsDTO.ProductId = item.ProductId;
                    orderDetailsDTO.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDTO);

                    db.SaveChanges();
                }
            }

            //email admin
            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("f084686b9f4e93", "cf93504e64d05b"),
                EnableSsl = true
            };
            client.Send("admin@example.com", "admin@example.com", "New Order", "You have a new order. Order number " + orderId);

            //reset session
            Session["cart"] = null;
        }
    }
}