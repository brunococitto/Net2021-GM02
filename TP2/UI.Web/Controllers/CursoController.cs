﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Business.Logic;
using Business.Entities;
using UI.Web.Models;

namespace UI.Web.Controllers
{
    public class CursoController : Controller
    {
        private readonly ILogger<CursoController> _logger;
        private readonly CursoLogic _cursoLogic;
        private readonly MateriaLogic _materiaLogic;
        private readonly ComisionLogic _comisionLogic;
        public CursoController(ILogger<CursoController> logger, CursoLogic cursoLogic, MateriaLogic materiaLogic, ComisionLogic comisionLogic)
        {
            _logger = logger;
            _logger.LogDebug("Inicializado controlador CursoController");
            _cursoLogic = cursoLogic;
            _materiaLogic = materiaLogic;
            _comisionLogic = comisionLogic;
        }
        public IActionResult Index() => RedirectToAction("List");
        public IActionResult List() => View(_cursoLogic.GetAll());

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();
            Curso? curso = _cursoLogic.GetOne((int)id);
            if (curso == null) return NotFound();
            return View(new EditCursoViewModel(curso, _materiaLogic.GetAll(), _comisionLogic.GetAll()));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("ID, AnoCalendario, Comision, Cupo, Descripcion, IDComision, IDMateria, Materia")] Curso curso)
        {
            if (id != curso.ID) return NotFound();
            if (ModelState.IsValid)
            {
                curso.State = BusinessEntity.States.Modified;
                _cursoLogic.Save(curso);
                return RedirectToAction("List");
            }
            return View(new EditCursoViewModel(curso, _materiaLogic.GetAll(), _comisionLogic.GetAll()));
        }
        [HttpGet]
        public IActionResult Create() => View(new CreateCursoViewModel(null, _materiaLogic.GetAll(), _comisionLogic.GetAll()));
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("ID, AnoCalendario, Comision, Cupo, Descripcion, IDComision, IDMateria, Materia")] Curso curso)
        {
            if (ModelState.IsValid)
            {
                curso.State = BusinessEntity.States.New;
                _cursoLogic.Save(curso);
                return RedirectToAction("List");
            }
            return View(new CreateCursoViewModel(curso, _materiaLogic.GetAll(), _comisionLogic.GetAll()));
        }
        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();
            Curso? curso = _cursoLogic.GetOne((int)id);
            if (curso == null) return NotFound();
            return View(curso);
        }
        [HttpPost]
        public IActionResult Delete(int id, Curso curso)
        {
            if (id != curso.ID) return NotFound();
            curso.State = BusinessEntity.States.Deleted;
            _cursoLogic.Save(curso);
            return RedirectToAction("List");
        }
    }
}