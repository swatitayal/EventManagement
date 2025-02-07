﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebMvc.Models;
using WebMvc.Services;
using WebMvc.ViewModels;

namespace WebMvc.Controllers
{
    public class EventController : Controller
    {
        private readonly IEventService _eventService;
        private readonly ICartService _cartSvc;
        private readonly IIdentityService<ApplicationUser> _identitySvc;
        public EventController(IEventService eventservice, ICartService cartSvc,
            IIdentityService<ApplicationUser> identitySvc)
        {
            _eventService = eventservice;
            _cartSvc = cartSvc;
            _identitySvc = identitySvc;
        }

        public async Task<IActionResult> Index(
            int? page,
            int? locationFilterApplied,
            int? formatFilterApplied,
            int? categoryFilterApplied,
            string dateFilterApplied,
            bool onlineFilterApplied,
            string priceFilterApplied)
        {
            var user = _identitySvc.Get(HttpContext.User);
            await _cartSvc.ClearCart(user); 

            var eventsOnPage = 6;
            DateTime? startDate = null, endDate = null;
            if (!string.IsNullOrWhiteSpace(dateFilterApplied))
            {
                DateFilterEnum selectedDateFilter = (DateFilterEnum)Enum.Parse(typeof(DateFilterEnum), dateFilterApplied);
                switch (selectedDateFilter)
                {

                    case DateFilterEnum.Today:
                        startDate = DateTime.Now.Date;
                        endDate = DateTime.Now.Date;
                        break;
                    case DateFilterEnum.Tomorrow:
                        startDate = DateTime.Today.AddDays(1).Date;
                        endDate = DateTime.Today.AddDays(1).Date;
                        break;
                    case DateFilterEnum.ThisWeek:
                        startDate = DateTime.Now.Date;
                        endDate = DateTime.Now.AddDays(7).Date;
                        break;
                    case DateFilterEnum.NextMonth:
                        var nextMonthDate = DateTime.Now.AddMonths(1);
                        startDate = new DateTime(nextMonthDate.Year, nextMonthDate.Month, 1);
                        endDate = new DateTime(nextMonthDate.Year, nextMonthDate.Month, DateTime.DaysInMonth(nextMonthDate.Year, nextMonthDate.Month));
                        break;
                    case DateFilterEnum.All:
                    default:
                        startDate = null;
                        endDate = null;
                        break;
                }
            }

            bool? isPaid = null;

            if (!string.IsNullOrWhiteSpace(priceFilterApplied))
            {
                PriceFilterEnum priceFilterEnum = (PriceFilterEnum)Enum.Parse(typeof(PriceFilterEnum), priceFilterApplied);
                switch (priceFilterEnum)
                {
                    case PriceFilterEnum.Free:
                       isPaid = false;
                        break;
                    case PriceFilterEnum.Paid:
                        isPaid = true;
                        break;
                    case PriceFilterEnum.All:
                    default:
                        isPaid = null;
                        break;
                }
            }

            PaginatedEvents paginatedEvents = await _eventService.GetEventsAsync(
                page ?? 0,
                eventsOnPage,
                startDate,
                endDate,
                categoryFilterApplied,
                formatFilterApplied,
                locationFilterApplied,
                onlineFilterApplied,
                isPaid);
            var locationsData = await _eventService.GetLocationsAsync();

            var eventIndexViewModel = new EventIndexViewModel
            {
                Location = locationsData,
                Format = await _eventService.GetFormatAsync(),
                Category = await _eventService.GetCategoriesAsync(),
                Date = _eventService.GetDates(),
                Price = _eventService.GetPrice(),
                Online = onlineFilterApplied,
                Events = paginatedEvents.Data,
                PaginationInfo = new PaginationInfo
                {
                    TotalItems = paginatedEvents.Count,
                    ItemsPerPage = paginatedEvents.PageSize,
                    ActualPage = paginatedEvents.PageIndex,
                    TotalPages = (int)Math.Ceiling((decimal)paginatedEvents.Count / eventsOnPage)
                },
                LocationFilterApplied = locationFilterApplied,
                FormatFilterApplied = formatFilterApplied,
                CategoryFilterApplied = categoryFilterApplied,
                DateFilterApplied = dateFilterApplied,
                PriceFilterApplied = priceFilterApplied,
                OnlineFilterApplied = onlineFilterApplied,
            };

            return View(eventIndexViewModel);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your Application Description Page";

            return View();
        }
      
    }
}
