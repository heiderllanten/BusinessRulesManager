using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using ReglasServices.Models;

using System.Reflection;
using System.Linq.Expressions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ReglasServices.Controllers
{
    [Route("api/[controller]")]
    public class ReglaController : Controller
    {
        private readonly reglasnegocioContext _context;

        public ReglaController(reglasnegocioContext context)
        {
            _context = context;
        }

        // GET api/values/5
        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            return Ok(new { response = _context.Reglas.ToList() });
        }

        [HttpPost("createrule")]
        public IActionResult Create([FromBody] Reglas regla)
        {
            if(regla == null)
            {
                return NotFound();
            }

            _context.Reglas.Add(regla);
            _context.SaveChanges();

            return Ok(new { response = "EXITO" });
        }

        [HttpPost("prueba")]
        public IActionResult Prueba([FromBody] User usuario)
        {
            var reglas = _context.Reglas.ToList();
            List<Object> resp = new List<Object>();

            foreach (var r in reglas)
            {
                Func<User, bool> compiledRule = CompileRule<User>(r);
                if (!compiledRule(usuario))
                {
                    resp.Add(new {
                        atributoEvaluado = r.Propiedad,
                        regla = r.Operador,
                        valorRegla = r.ValorComparacion,
                        resultado = false });
                }
                else
                {
                    resp.Add(new
                    {
                        atributoEvaluado = r.Propiedad,
                        regla = r.Operador,
                        valorRegla = r.ValorComparacion,
                        resultado = true });
                }
            }
            return Ok(new { response = resp });
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

        private Expression BuildExpr<T>(Reglas r, ParameterExpression param)
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

        private Func<T, bool> CompileRule<T>(Reglas r)
        {
            //expresion de codigo de nivel de lenguaje en un nodo de un arbol
            var paramUser = Expression.Parameter(typeof(User));
            Expression expr = BuildExpr<T>(r, paramUser);
            // build a lambda function User->bool and compile it
            return Expression.Lambda<Func<T, bool>>(expr, paramUser).Compile();
        }
    }
}
