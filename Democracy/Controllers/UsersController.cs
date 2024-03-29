﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Democracy.Models;
using System.IO;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Democracy.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private DemocracyContext db = new DemocracyContext();

        // GET: Users
        public ActionResult Index()
        {
            return View(db.Users.ToList());
        }

        // GET: Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserView userView)
        {
            if (!ModelState.IsValid)
            {
                return View(userView);
            }
            //Subir la imagen al servidor
            string path = string.Empty;
            string pic = string.Empty;

            if (userView.Photo != null)
            {
                pic = Path.GetFileName(userView.Photo.FileName);
                path = Path.Combine(Server.MapPath("~/Content/Photos"),pic);
                userView.Photo.SaveAs(path);
                using (MemoryStream ms = new MemoryStream())
                {
                    userView.Photo.InputStream.CopyTo(ms);
                    byte[] array = ms.GetBuffer();
                }
            }
            //gruardar datos
            var user = new User 
            {
                Address=userView.Address,
                FirstName=userView.FirstName,
                Grade=userView.Grade,
                Group=userView.Group,
                LastName=userView.LastName,
                Phone=userView.Phone,
                UserName=userView.UserName,
                Photo = pic == string.Empty ? string.Empty : string.Format("~/Content/Photos/{0}",pic),
             };
            
            db.Users.Add(user);
            try
            {
                db.SaveChanges();
                this.CreateASPUser(userView);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && 
                    ex.InnerException.InnerException != null &&
                    ex.InnerException.InnerException.Message.Contains("UserNameIndex"))
                {
                    ViewBag.Error = "El Email o Nombre de usuario Ya se encuantra regisatrado";
                }
                else
                {
                    ViewBag.Error=ex.Message;
                }
                return View(userView);
            }
          
            return RedirectToAction("Index");
          
        }

        private void CreateASPUser(UserView userView)
        {
            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(userContext));

            //se crea un user role
            string roleName = "User";

            //se valida si el rol existe si no existe se crea
            if (!roleManager.RoleExists(roleName))
            {
                roleManager.Create(new IdentityRole(roleName));
            }

            //se crea el ASP NET User
            var userASP = new ApplicationUser
            {
                UserName=userView.UserName,
                Email=userView.UserName,
                PhoneNumber=userView.Phone,
            };
            //se crea el nuevo usuario el primer parametro es el objeto UserASP y el segundo parametro es la contraseña por defecto estamos mandando el correo
            userManager.Create(userASP,userASP.UserName);

            //buscamos el ID del usuario
            userASP = userManager.FindByName(userView.UserName);
            //mandamos la orden de que ese usuario pertenece al rol
            userManager.AddToRole(userASP.Id, "User");

        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = db.Users.Find(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            var userView = new UserView 
            {
                UserId=user.UserId,
                Address=user.Address,
                FirstName=user.FirstName,
                Grade=user.Grade,
                Group=user.Group,
                LastName=user.LastName,
                Phone=user.Phone,
                UserName=user.UserName,
                
            };
            return View(userView);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserView userView)
        {
            if (!ModelState.IsValid)
            {
                return View(userView);
            }

            //Subir la imagen al servidor
            string path = string.Empty;
            string pic = string.Empty;

            if (userView.Photo != null)
            {
                pic = Path.GetFileName(userView.Photo.FileName);
                path = Path.Combine(Server.MapPath("~/Content/Photos"), pic);
                userView.Photo.SaveAs(path);
                using (MemoryStream ms = new MemoryStream())
                {
                    userView.Photo.InputStream.CopyTo(ms);
                    byte[] array = ms.GetBuffer();
                }
            }

            var user = db.Users.Find(userView.UserId);
            user.Address = userView.Address;
            user.FirstName = userView.FirstName;     
            user.Grade = userView.Grade;
            user.Group = userView.Group;
            user.LastName = userView.LastName;
            user.Phone = userView.Phone;
            if(!string.IsNullOrEmpty(pic))
            {
                user.Photo =string.Format("~/Content/Photos/{0}", pic);
            }    

            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
           
        }

        // GET: Users/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
