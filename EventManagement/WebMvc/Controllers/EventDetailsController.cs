﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebMvc.Services;
using WebMvc.ViewModels;

namespace WebMvc.Controllers
{
    public class EventDetailsController : Controller
    {
        private readonly IEventService _eventService;
        public EventDetailsController(IEventService eventservice)
        {
            _eventService = eventservice;
        }
        public async Task<IActionResult> Index(int id , string dateSelected)
        {


           var EventDetail = await _eventService.GetEventDetails(id);
          
            var availableDates = GetAvailableDates(EventDetail.StartDate, EventDetail.EndDate);

            var eventDetail = new EventDetail
            {
                Id = id,
                StartDate = EventDetail.StartDate,
                EndDate = EventDetail.EndDate,
                AvailableDates = availableDates,
                EventImageUrl = EventDetail.EventImageUrl ,
                MaxOccupancy = EventDetail.MaxOccupancy ,
                MaxTicketsPerUser= EventDetail.MaxTicketsPerUser,
                IsOnlineEvent = EventDetail.IsOnlineEvent ,
                IsPaidEvent = EventDetail.IsPaidEvent ,
                Address= EventDetail.Address,
                Description=EventDetail.Description ,
                Title = EventDetail.Title,
                FormatName= EventDetail.FormatName,
                SubCategoryName=EventDetail.SubCategoryName ,
                CategoryName= EventDetail.CategoryName,
                City= EventDetail.City,
                State= EventDetail.State,
                MinPrice=EventDetail.MinPrice,
                MaxPrice=EventDetail.MaxPrice,
                DateSelected = dateSelected
            };

            return View(eventDetail);

        }

        public IEnumerable<SelectListItem> GetAvailableDates(DateTime startDate, DateTime endDate)
        {

            if ((startDate < DateTime.Now))
            {
                startDate = DateTime.Now;
            }

            int totalDays = (int)((endDate.Date - startDate.Date).TotalDays + 1);
            
            var dates = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = null,
                    Text="AVAILABLE DATES",
                    Selected = true
                }
            };

            for (int i = 1; i <= totalDays; i++)
            {
                dates.Add(
                    new SelectListItem
                    {
                        Value = $"{startDate.AddDays(i - 1).ToShortDateString()}",
                        Text = $"{startDate.AddDays(i - 1).ToShortDateString()}"
                    });
            }

            return dates;
        }
    }
}
