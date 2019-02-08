﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NARFO_BE.Models;

namespace NARFO_BE.Controllers
{
    [Route("api/member")]
   
    public class MembersController : ControllerBase
    {
        private readonly narfoContext _context;
        List<Member> members = new List<Member>();
        private IConfiguration Config;
        public MembersController(narfoContext context, IConfiguration config)
        {
            Config = config;
            _context = context;
        }
        
      
        private ActionResult<Member> Json(object p) {
            throw new NotImplementedException();
        }

        private string BuildToken(Member user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[] {
        new Claim(JwtRegisteredClaimNames.Sub, user.Username),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };
            var token = new JwtSecurityToken(Config["Jwt:Issuer"],
              Config["Jwt:Issuer"],claims,
              expires: DateTime.Now.AddDays(360),
              signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //

        // POST: /Account/Login
        [HttpPost("post/login")]
        public async Task<ActionResult <Member>> Login([FromBody] Member model)
        {
            Member user = null;
            if(model.Email != null)
            {
                user = await _context.Member.FirstOrDefaultAsync(member => member.Email == model.Email && Encryption.VerifyPassword(model.Password, member.Password));
            }else
            {
                user = await _context.Member.FirstOrDefaultAsync(member => member.Username == model.Username && Encryption.VerifyPassword(model.Password,member.Password));
            }
           
           
                if(user == null)
                {
                    return BadRequest(new { status = "Failed", message = "Invalid Login"});
                } else
                {
                    var tokenString = BuildToken(user);
                    return Ok(new { status = "Success", token = tokenString });
                }
     }

        
        [HttpGet("all/user")]
        public  async Task<ActionResult<MemberPrototype>> GetMembersEmail()
        {
            List<MemberPrototype> endpoint = new List<MemberPrototype>();
            foreach (Member member in  await _context.Member.ToArrayAsync())
            {
               if(member.Username !=  null && member.Email != null )
               endpoint.Add(new MemberPrototype(member.Username,member.Email));       
            }
            if(endpoint == null)
            {
                return BadRequest(new { status = "failed", error = "Failed to connect" });
            }
          return      Ok(new { status = "success", members=endpoint });
        }
       
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Member>>> GetMembers() {
            return await _context.Member.ToListAsync();
        }
     
        [HttpGet("{id}")]
        public async Task<ActionResult<Member>> GetMembers(string id){
            var Members = await _context.Member.FindAsync(id);
            if (Members == null)
            {
                return BadRequest(new { status = "failed", error = "Failed to connect" });
            }
            return Ok(new { status = "success", members = Members });
        }
      

       [HttpPost("post/set")]

       public async Task<ActionResult<Member>> SetMember([FromBody]Member member)
        {
            member.Password = Encryption.CreatePasswordHash(member.Password);
            await _context.Member.AddAsync(member);
            await _context.SaveChangesAsync();         
            if (members == null)
            {
             return BadRequest(new { status = "failed", error = "Failed to connect" });
            }
            var tokenString = BuildToken(member);
            return Ok(new { status = "success", token = tokenString });

        }

       
    }
}
