﻿using backend.Data;
using backend.Models;
using backend.Models.inputs;
using backend.Models.outputs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Xml.Linq;

namespace backend.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OffreSpecialeController : ControllerBase
    {
        private readonly ApiContext _db;

        public OffreSpecialeController(ApiContext db)
        {
            _db = db;
        }
        //get offres
        [HttpGet]
        public JsonResult GetOffres()
        {
            var offres = _db.OffreSpeciales
                .Include(v => v.Voiture)
                .Include(v => v.User)
                .Include(v => v.Voiture.Marque)
                .ToList();
            if (offres == null)
            {
                return new JsonResult(NotFound());
            }
            return new JsonResult(Ok(offres.Select(v => new OffreDto
            {
                Id = v.Id,
                Name = v.Name,
                TauxRemise = v.TauxRemise,
                DateAdded = v.DateAdded,
                DateExpiration = v.DateExpiration,
                IsAprouved = v.IsAprouved,
                Voiture = new Voiture
                {
                    Id = v.Voiture.Id,
                    Name = v.Voiture.Name,
                    Couleur = v.Voiture.Couleur,
                    Photo = v.Voiture.Photo,
                    Annee = v.Voiture.Annee,
                    Km = v.Voiture.Km,
                    DateAdded = v.Voiture.DateAdded,
                    UserId = v.Voiture.UserId,
                    MarqueId = v.Voiture.MarqueId,
                    Prix = v.Voiture.Prix,
                    Marque = new Marque
                    {
                        Id = v.Voiture.Marque.Id,
                        Libelle = v.Voiture.Marque.Libelle
                    }
                },
                User = new User
                {
                    Id = v.User.Id,
                    Username = v.User.Username,
                    Photo = v.User.Photo
                }

            }).ToList()));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OffreSpeciale>> GetOffreById(int id)
        {
            // Retrieve the requested offre object from the database
            var offre = await _db.OffreSpeciales.FindAsync(id);

            if (offre == null)
            {
                return NotFound();
            }

            return  offre;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = " proprietaire")]

        public async Task<ActionResult<OffreSpeciale>> Create(OffreInput offre)
        {
            // Validate the incoming request data
            if (offre == null)
            {
                return BadRequest("No data provided.");
            }

            var user = _db.Users.Find(offre.UserId);
            if (user == null)
            {
                return NotFound("User Not found");
            }

            var offreToAdd = new OffreSpeciale
            {
                Name=offre.Name,
                UserId=offre.UserId,
                TauxRemise=offre.TauxRemise,    
                DateExpiration=offre.DateExpiration,
                DateAdded=DateTime.Now,
                VoitureId=offre.VoitureId,
                IsAprouved=false
               
            };

            // Add the new marque object to the database
            _db.OffreSpeciales.Add(offreToAdd);
            await _db.SaveChangesAsync();

            return new JsonResult(Ok(offre));
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = " proprietaire")]

        public async Task<IActionResult> Update(int id, OffreInput offre)
        {
            // Validate the incoming request data
            if (offre == null || offre.Id != id)
            {
                return BadRequest("Invalid data provided.");
            }



            // Check if the offre exists in the database 
            var offre_from_db = await _db.OffreSpeciales.FindAsync(id);
            if (offre_from_db == null)
            {
                return NotFound();
            }

            var user = _db.Users.Find(offre_from_db.UserId);
            if (user == null)
            {
                return NotFound("User Not found");
            }

            offre_from_db.Name = offre.Name;
            offre_from_db.UserId = offre.UserId;
            offre_from_db.TauxRemise = offre.TauxRemise;
               offre_from_db.DateExpiration = offre.DateExpiration;
            offre_from_db.VoitureId=offre.VoitureId;
            
       
            _db.OffreSpeciales.Update(offre_from_db);
            await _db.SaveChangesAsync();

            return new JsonResult("updated successfully");
        }

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, proprietaire")]

        public async Task<IActionResult> Delete(int id)
        {
            // Check if the offre object with the specified ID exists in the database
            var offre = await _db.OffreSpeciales.FindAsync(id);
            if (offre == null)
            {
                return NotFound();
            }

            // Delete the marque object from the database
            _db.OffreSpeciales.Remove(offre);
            await _db.SaveChangesAsync();

            return new JsonResult("Deleted successfully");
        }

        [HttpPut]
        public async Task<IActionResult> AprovedOffre(int id)
        {
            var offre = await _db.OffreSpeciales.FindAsync(id);
            if (offre == null)
            {
                return NotFound();
            }
            offre.IsAprouved = true;
            _db.OffreSpeciales.Update(offre);
            await _db.SaveChangesAsync();
            return Ok(offre);
        }


        //get offre by user
        [HttpGet("offreByUser")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public JsonResult getOffresByUser(int userId)
        {
            var offres = _db.OffreSpeciales
                .Where(v => v.UserId == userId)
                .Include(v => v.User)
                .ToList();

            return new JsonResult(Ok(offres.Select(v => new OffreDto
            {
                Id = v.Id,
                Name = v.Name,
                TauxRemise = v.TauxRemise,
                DateAdded = v.DateAdded,
                DateExpiration = v.DateExpiration,
                User = new User
                {
                    Id = v.User.Id,
                    Username = v.User.Username,
                    Email = v.User.Email
                }

            }).ToList()));
        }

        [HttpGet("offreByCar")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public JsonResult getOffresByCar(int voitureId)
        {
            var offres = _db.OffreSpeciales
               .Where(v => v.VoitureId == voitureId)
               .Include(v => v.User)
               .ToList();

            return new JsonResult(Ok(offres.Select(v => new OffreDto
            {
                Id = v.Id,
                Name = v.Name,
                TauxRemise = v.TauxRemise,
                DateAdded = v.DateAdded,
                DateExpiration = v.DateExpiration,
                Voiture = new Voiture
                {
                    Id = v.Voiture.Id,
                    Name = v.Voiture.Name,
                    Couleur = v.Voiture.Couleur,
                    Photo = v.Voiture.Photo,
                    Annee = v.Voiture.Annee,
                    Km = v.Voiture.Km,
                    DateAdded = v.Voiture.DateAdded,
                    UserId = v.Voiture.UserId,
                    MarqueId = v.Voiture.MarqueId,
                },
                User = new User
                {
                    Id = v.User.Id,
                    Username = v.User.Username,
                    Photo = v.User.Photo
                }

            }).ToList()));
            return null;
        }
    }
}
