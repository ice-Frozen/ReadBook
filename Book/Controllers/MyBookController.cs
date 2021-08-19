﻿using AutoMapper;
using Book.Data;
using Book.Models;
using Book.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Book.Controllers
{
    public class MyBookController : Controller
    {
        private readonly IMapper _mapper;
        private readonly DataContext _dataContext;
        public MyBookController(IMapper mapper, DataContext dataContext)
        {
            _mapper = mapper;
            _dataContext = dataContext;
        }

        [Authorize]
        public async Task<IActionResult> Writer()
        {
            var queryBcat = await _dataContext.CategoryMaster.AsNoTracking().ToListAsync();
            var map = _mapper.Map<IEnumerable<GetCategoryMasterVm>>(queryBcat);
            ViewData["GetCategoryMaster"] = map;
            return View();
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Writer(WriteBookVm writeBookVm)
        {
            try
            {
                if (ModelState.IsValid == false) return BadRequest();
                if (_dataContext.Book.Any(x => x.BookName == writeBookVm.BookName))
                {
                    return BadRequest(new { Message = "นิยายเล่มนี้มีอยู่ในระบบแล้ว โปรดใช้ชื่ออื่นนะคะ" });
                } //end if
                if (writeBookVm.CategoryId.Length <= 0)
                {
                    return BadRequest(new { message = "โปรดเลือกหมวดหมู่ก่อนนะคะ" });
                }
                var map = _mapper.Map<BookModel>(writeBookVm);

                map.Auther = User.Identity.Name;
                map.DateCreated = DateTime.Now;
                foreach (var item in writeBookVm.CategoryId)
                {
                    var query = await _dataContext.CategoryMaster.SingleOrDefaultAsync(x => x.Id == item);
                    var catMap = new BookCategoryModel()
                    {
                        CategoryName = query.Name
                    };
                    map.BookCategorys.Add(catMap);
                }
                await _dataContext.Book.AddAsync(map);
                await _dataContext.SaveChangesAsync();
                return Ok(new { ResponseMessage = "ok" });
            }
            catch (Exception e)
            {
                return BadRequest(new { Message = e.Message });
            } //end catch
        } //end method.Writer
    } //end class
}
