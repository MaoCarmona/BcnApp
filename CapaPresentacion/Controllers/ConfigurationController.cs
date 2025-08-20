using DotNetBcnModule.Presentation.Models;
using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Configuration;
using System.Text;

namespace DotNetBcnModule.Presentation.Controllers
{
    public class ConfigurationController : Controller
    {
        // GET: Configuration
        public ActionResult Index()
        {
            var model = LoadConfiguration();
            return PartialView(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(ConfigurationViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    SaveConfiguration(model);
                    TempData["Message"] = "La configuración se ha guardado correctamente.";
                    TempData["IsError"] = false;
                }
                catch (Exception ex)
                {
                    TempData["Message"] = $"Error al guardar la configuración: {ex.Message}";
                    TempData["IsError"] = true;
                }
            }
            else
            {
                TempData["Message"] = "Los datos proporcionados no son válidos.";
                TempData["IsError"] = true;
            }

            var newModel = LoadConfiguration();
            return PartialView("Index", newModel);
        }

        private ConfigurationViewModel LoadConfiguration()
        {
            try
            {
                var config = WebConfigurationManager.OpenWebConfiguration("~");
                var connectionStrings = config.ConnectionStrings.ConnectionStrings;
                var appSettings = config.AppSettings.Settings;

                return new ConfigurationViewModel
                {
                    DBARES = DecodeBase64(connectionStrings["DBARES"] != null ? connectionStrings["DBARES"].ConnectionString : string.Empty),
                    DBAROMSS = DecodeBase64(connectionStrings["DBAROMSS"] != null ? connectionStrings["DBAROMSS"].ConnectionString : string.Empty),
                    DBBCN = DecodeBase64(connectionStrings["DBBCN"] != null ? connectionStrings["DBBCN"].ConnectionString : string.Empty),
                    URLARES = appSettings["URLARES"] != null ? appSettings["URLARES"].Value : string.Empty
                };
            }
            catch (Exception ex)
            {
                // Manejo de errores de carga
                return new ConfigurationViewModel();
            }
        }

        private void SaveConfiguration(ConfigurationViewModel model)
        {
            var config = WebConfigurationManager.OpenWebConfiguration("~");
            var connectionStrings = config.ConnectionStrings.ConnectionStrings;
            var appSettings = config.AppSettings.Settings;

            UpdateOrCreateConnectionString(connectionStrings, "DBARES", EncodeBase64(model.DBARES));
            UpdateOrCreateConnectionString(connectionStrings, "DBAROMSS", EncodeBase64(model.DBAROMSS));
            UpdateOrCreateConnectionString(connectionStrings, "DBBCN", EncodeBase64(model.DBBCN));
            UpdateOrCreateAppSetting(appSettings, "URLARES", model.URLARES);

            config.Save(ConfigurationSaveMode.Modified);
        }

        private void UpdateOrCreateConnectionString(ConnectionStringSettingsCollection collection, string key, string value)
        {
            if (collection[key] != null)
            {
                collection[key].ConnectionString = value;
            }
            else
            {
                collection.Add(new ConnectionStringSettings(key, value));
            }
        }

        private void UpdateOrCreateAppSetting(KeyValueConfigurationCollection settings, string key, string value)
        {
            if (settings[key] != null)
            {
                settings[key].Value = value;
            }
            else
            {
                settings.Add(key, value);
            }
        }

        private string DecodeBase64(string base64)
        {
            if (string.IsNullOrWhiteSpace(base64)) return string.Empty;
            try
            {
                var bytes = Convert.FromBase64String(base64);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                // Si no es base64 válido, retorna el valor original
                return base64;
            }
        }

        private string EncodeBase64(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText)) return string.Empty;
            var bytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(bytes);
        }
    }
} 