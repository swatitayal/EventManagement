﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebMvc.Infrastructure;
using WebMvc.Models;
using WebMvc.Models.CartModels;
using WebMvc.Models.OrderModels;

namespace WebMvc.Services
{
    public class CartService : ICartService
    {
        private readonly IConfiguration _config;
        private IHttpClient _apiClient;
        private readonly string _remoteServiceBaseUrl;
        private IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;
        public CartService(IConfiguration config,
            IHttpContextAccessor httpContextAccesor, IHttpClient httpClient, ILoggerFactory logger)
        {
            _config = config;
            _remoteServiceBaseUrl = $"{_config["CartUrl"]}/api/cart";
            _httpContextAccessor = httpContextAccesor;
            _apiClient = httpClient;
            _logger = logger.CreateLogger<CartService>();
        }

        private async Task<string> GetUserTokenAsync()
        {
            var context = _httpContextAccessor.HttpContext;

            return await context.GetTokenAsync("access_token");

        }
        public async Task AddItemToCart(ApplicationUser user, CartItem ticket)
        {
            var cart = await GetCart(user);
            _logger.LogDebug("User Name: " + user.Email);
           
            if (cart.Tickets != null)
            {
                var basketItem = cart.Tickets
                    .Where(p => p.TicketId == ticket.TicketId)
                    .FirstOrDefault();

                if (basketItem == null && ticket.Quantity > 0)
                {
                    cart.Tickets.Add(ticket);
                }
                else if (basketItem != null && basketItem.Quantity != ticket.Quantity && ticket.Quantity != 0)
                {
                    basketItem.Quantity = ticket.Quantity;
                }
                else if (basketItem != null && ticket.Quantity == 0)
                {
                    cart.Tickets.Remove(basketItem);
                }
            }
            await UpdateCart(cart);
        }


        public async Task<Cart> GetCart(ApplicationUser user)
        {
            var token = await GetUserTokenAsync();
            _logger.LogInformation(" Now in GetCart() and user is " + user.Email);
            _logger.LogInformation("remoteServiceBaseUrl : " + _remoteServiceBaseUrl);


            var getBasketUri = ApiPaths.Basket.GetBasket(_remoteServiceBaseUrl, user.Email);
            _logger.LogInformation(" getBasketUri : " + getBasketUri);
            var dataString = await _apiClient.GetStringAsync(getBasketUri, token);
            _logger.LogInformation("This basket contains : " + dataString);

            var response = JsonConvert.DeserializeObject<Cart>(dataString.ToString()) ??
               new Cart()
               {
                   UserId = user.Email,
               };
            return response;
        }

      public async Task ClearCart(ApplicationUser user)
        {
            var token = await GetUserTokenAsync();
            var cleanBasketUri = ApiPaths.Basket.CleanBasket(_remoteServiceBaseUrl, user.Email);
            _logger.LogDebug("Now cleaning this basket URI : " + cleanBasketUri);
            var response = await _apiClient.DeleteAsync(cleanBasketUri, token);
            _logger.LogDebug(" Basket/Cart is now clean");
        }


        public async Task<Cart> SetQuantities(ApplicationUser user, Dictionary<string, int> quantities)
        {
            var basket = await GetCart(user);

            basket.Tickets.ForEach(x =>

            {
                if (quantities.TryGetValue(x.CartItemId, out var quantity))
                {
                    x.Quantity = quantity;
                }
            });

            return basket;
        }

        public async Task<Cart> UpdateCart(Cart cart)
        {
            var token = await GetUserTokenAsync();
            _logger.LogDebug("Service url: " + _remoteServiceBaseUrl);
            var updateBasketUri = ApiPaths.Basket.UpdateBasket(_remoteServiceBaseUrl);
            _logger.LogDebug("Update Basket url: " + updateBasketUri);
            var response = await _apiClient.PostAsync(updateBasketUri, cart, token);
            response.EnsureSuccessStatusCode();

            return cart;
        }

        public Order MapCartToOrder(Cart cart)
        {
            var order = new Order();
            order.OrderTotal = 0;
            _logger.LogInformation(" Now in MapCartToOrder");
            cart.Tickets.ForEach(x =>
            {
                order.OrderItems.Add(new OrderItem()
                {
                    TicketId = int.Parse(x.TicketId),
                    TicketCategoryName = x.TicketCategoryName,
                    TicketQuantity = x.Quantity,
                    TicketPrice = x.TicketPrice,
                    EventTitle = x.EventTitle,
                    EventSelectedDate = x.UserSelectedDate
                });
                order.OrderTotal += (x.Quantity * x.TicketPrice);
            });
            _logger.LogInformation(" Completed MapCartToOrder");
            return order;
        }
    }
}
