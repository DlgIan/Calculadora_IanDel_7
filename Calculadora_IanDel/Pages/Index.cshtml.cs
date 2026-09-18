using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Collections.Generic;
using System;

namespace Calculadora_IanDEl.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public double Peso { get; set; }

        [BindProperty]
        public double Altura { get; set; }

        public double? Imc { get; private set; }
        public string Categoria { get; private set; }

        // Lista de cálculos realizados durante la sesión
        public List<Calculation> Calculations { get; private set; } = new List<Calculation>();

        public void OnGet()
        {
            // Cargar cálculos almacenados en sesión
            Calculations = GetCalculationsFromSession();
        }

        public IActionResult OnPost()
        {
            if (Peso <= 0 || Altura <= 0)
            {
                ModelState.AddModelError(string.Empty, "Peso y altura deben ser mayores que cero.");
                return Page();
            }

            Imc = Peso / (Altura * Altura);
            Categoria = ClasificarImc(Imc.Value);

            // Añadir el cálculo a la lista de sesión
            Calculations = GetCalculationsFromSession();
            Calculations.Add(new Calculation
            {
                Fecha = DateTime.Now,
                Peso = Peso,
                Altura = Altura,
                Imc = Imc.Value,
                Categoria = Categoria
            });

            SaveCalculationsToSession(Calculations);

            return Page();
        }

        public IActionResult OnPostClear()
        {
            // Limpiar cálculos de la sesión
            HttpContext.Session.Remove(SessionKey);
            Calculations = new List<Calculation>();
            return Page();
        }

        private string ClasificarImc(double imc)
        {
            if (imc < 18.5) return "Bajo peso";
            if (imc < 25.0) return "Normal";
            if (imc < 30.0) return "Sobrepeso";
            return "Obesidad";
        }

        private const string SessionKey = "Calculations";

        private List<Calculation> GetCalculationsFromSession()
        {
            var json = HttpContext.Session.GetString(SessionKey);
            if (string.IsNullOrEmpty(json)) return new List<Calculation>();
            try
            {
                return JsonSerializer.Deserialize<List<Calculation>>(json) ?? new List<Calculation>();
            }
            catch
            {
                return new List<Calculation>();
            }
        }

        private void SaveCalculationsToSession(List<Calculation> calculations)
        {
            var json = JsonSerializer.Serialize(calculations);
            HttpContext.Session.SetString(SessionKey, json);
        }

        public class Calculation
        {
            public DateTime Fecha { get; set; }
            public double Peso { get; set; }
            public double Altura { get; set; }
            public double Imc { get; set; }
            public string Categoria { get; set; }
        }
    }
}
