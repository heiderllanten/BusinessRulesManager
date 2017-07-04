using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReglasAspCore.Models;

using System.Reflection;
using System.Linq.Expressions;

namespace ReglasAspCore.Controllers
{
    public class ReglasController : Controller
    {
        private readonly reglasnegocioContext _context;

        public ReglasController(reglasnegocioContext context)
        {
            _context = context;    
        }

        // GET: Reglas
        public async Task<IActionResult> Index()
        {
            return View(await _context.Reglas.ToListAsync());
        }

        // GET: Reglas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reglas = await _context.Reglas
                .SingleOrDefaultAsync(m => m.Id == id);
            if (reglas == null)
            {
                return NotFound();
            }

            return View(reglas);
        }

        // GET: Reglas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Reglas/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Propiedad,Operador,ValorComparacion")] Reglas reglas)
        {
            if (ModelState.IsValid)
            {
                _context.Add(reglas);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(reglas);
        }

        // GET: Reglas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reglas = await _context.Reglas.SingleOrDefaultAsync(m => m.Id == id);
            if (reglas == null)
            {
                return NotFound();
            }
            return View(reglas);
        }

        // POST: Reglas/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Propiedad,Operador,ValorComparacion")] Reglas reglas)
        {
            if (id != reglas.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reglas);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReglasExists(reglas.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return View(reglas);
        }

        // GET: Reglas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reglas = await _context.Reglas
                .SingleOrDefaultAsync(m => m.Id == id);
            if (reglas == null)
            {
                return NotFound();
            }

            return View(reglas);
        }

        // POST: Reglas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reglas = await _context.Reglas.SingleOrDefaultAsync(m => m.Id == id);
            _context.Reglas.Remove(reglas);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Prueba()
        {
            var reglas = _context.Reglas;

            var user1 = new User
            {
                Age = 13,
                Name = "royi"
            }

            ;
            var user2 = new User
            {
                Age = 33,
                Name = "john"
            }

            ;
            var user3 = new User
            {
                Age = 53,
                Name = "paul"
            }

            ;
            
            foreach (var r in reglas)
            {
                Func<User, bool> compiledRule = CompileRule<User>(r);
            }
            return View();
        }

        private bool ReglasExists(int id)
        {
            return _context.Reglas.Any(e => e.Id == id);
        }

        public class User
        {
            public int Age
            {
                get;
                set;
            }

            public string Name
            {
                get;
                set;
            }
        }

        static Expression BuildExpr<T>(Reglas r, ParameterExpression param)
        {
            //acceso a un campo o una propiedad
            var left = MemberExpression.Property(param, r.Propiedad);
            //obtenemos el tipo del atributo de clase que necesitamos
            var tProp = typeof(T).GetProperty(r.Propiedad).PropertyType;
            ExpressionType tBinary;
            // is the operator a known .NET operator?
            // obtenemos la representacion de el nombre o valor numerico del operador
            // si este esta entre los operadores de .NET
            if (ExpressionType.TryParse(r.Operador, out tBinary))
            {
                // creamos un objeto del tipo del valor que especificamos
                var right = Expression.Constant(Convert.ChangeType(r.ValorComparacion, tProp));
                // use a binary operation, e.g. 'Equal' -> 'u.Age == 15'
                return Expression.MakeBinary(tBinary, left, right);
            }
            else
            {
                // buscamos el metodo publico del tipo de dato, con el nombre especificado
                var method = tProp.GetMethod(r.Operador);
                // obtenemos los parametros del metodo encontrado
                var tParam = method.GetParameters()[0].ParameterType;
                // creamos un tipo de dato del parametro, y le asignamos el valor especificado
                var right = Expression.Constant(Convert.ChangeType(r.ValorComparacion, tParam));
                // use a method call, e.g. 'Contains' -> 'u.Tags.Contains(some_tag)'
                return Expression.Call(left, method, right);
            }
        }

        public static Func<T, bool> CompileRule<T>(Reglas r)
        {
            //expresion de codigo de nivel de lenguaje en un nodo de un arbol
            var paramUser = Expression.Parameter(typeof(User));
            Expression expr = BuildExpr<T>(r, paramUser);
            // build a lambda function User->bool and compile it
            return Expression.Lambda<Func<T, bool>>(expr, paramUser).Compile();
        }
    }
}
