﻿using BookStore.FrontEnd.Site.Models.EFModels;
using BookStore.FrontEnd.Site.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Security.Principal;

namespace BookStore.FrontEnd.Site.Controllers
{
    public class CartController : Controller
    {
        [Authorize]
        public ActionResult AddItem(int productId)
        {
            //取得目前登入者
            var account =User.Identity.Name;

            //加入購物車
            int qty = 1;
            Add2Cart(account, productId, qty);

            return new EmptyResult();
        }

        /// <summary>
        /// 加入購物車,若明細不存在就新增一筆,若存在就更新數量
        /// </summary>
        /// <param name="account"></param>
        /// <param name="productId"></param>
        /// <param name="qty"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Add2Cart(string account, int productId, int qty)
        {
            //取得購物車
            int cartId =GetCartInfo(account).Id;

            //加入購物車
            AddCartItem(cartId, productId, qty);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cartId"></param>
        /// <param name="productId"></param>
        /// <param name="qty"></param>
        private void AddCartItem(int cartId, int productId, int qty)
        {
            var db = new AppDbContext();
            var cartItem = db.CartItems.FirstOrDefault(c => c.CartId == cartId && c.ProductId == productId);

            if(cartItem == null)
            {
                cartItem = new CartItem
                {
                    CartId = cartId,
                    ProductId = productId,
                    Qty = qty
                };
                db.CartItems.Add(cartItem);
            }
            else
            {
                cartItem.Qty += qty;
            }
            db.SaveChanges();
        }

        /// <summary>
        /// 取得目前購物車主檔,若沒有,就立刻新增一筆並傳回
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private CartVm GetCartInfo(string account)
        {
            var db = new AppDbContext();
            var cart = db.Carts.FirstOrDefault(c => c.MemberAccount == account);

            if (cart == null)
            {
                cart = new Cart { MemberAccount = account };
                db.Carts.Add(cart);
                db.SaveChanges();
            }

            var cartInfo = db.Carts.AsNoTracking().Include(c => c.CartItems.Select(ci=>ci.Product))
                .Where(c => c.MemberAccount == account)
                .Select(c => new CartVm
                {
                    Id = c.Id,
                    MemberAccount = c.MemberAccount,
                    CartItems = c.CartItems.Select(ci => new CartItemVm
                    {
                        Id = ci.Id,
                        CartId = ci.CartId,
                        ProductId = ci.ProductId,
                        Product = new ProductIndexVm
                        {
                            Id = ci.Product.Id,
                            CategoryName = ci.Product.Category.Name,
                            Name = ci.Product.Name,
                            Price = ci.Product.Price,
                        },
                        Qty = ci.Qty
                    })
                })
                .First();
            return cartInfo;

        }

    }
}