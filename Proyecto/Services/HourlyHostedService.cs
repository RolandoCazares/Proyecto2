using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using proyecto.Contracts; // Asegúrate de incluir el espacio de nombres correcto
using proyecto.Models.Authentication; // Ajusta según tus modelos
using Microsoft.Extensions.DependencyInjection; // Asegúrate de incluir este espacio de nombres


using proyecto.Models.FPY;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using proyecto.Helpers;

namespace proyecto.Services
{
    public class HourlyHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<HourlyHostedService> _logger;
        private readonly IServiceScopeFactory _scopeFactory; // Agrega esta línea
        private Timer _timer;
        private readonly SemaphoreSlim _semaphore;

        public HourlyHostedService(ILogger<HourlyHostedService> logger, IServiceScopeFactory scopeFactory, SemaphoreSlim semaphore)
        {
            _logger = logger;
            _scopeFactory = scopeFactory; // Almacena la referencia del scope factory
            _semaphore = semaphore;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        private async void DoWork(object state)
        {
            await _semaphore.WaitAsync();
            try
            {
                //_logger.LogInformation("Hourly work running at: {time}", DateTimeOffset.Now);

                using (var scope = _scopeFactory.CreateScope()) // Crea un nuevo scope
                {
                    var filterFPY = scope.ServiceProvider.GetRequiredService<IFilterFPY>(); // Obtén el servicio scoped

                    string Product = "Todos"; // Ajusta según sea necesario
                    int TypeSearch = 3; // Ajusta según sea necesario
                    string Week = "null"; // Ajusta según sea necesario
                    DateTime now = DateTime.Now;

                    DateTime ahora = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0); // Hora actual, minutos y segundos en 0
                    DateTime Last24hours = ahora.AddDays(-1);


                    string ahoraString = ahora.ToString("yyyy-MM-dd HH:mm:ss"); // Formato deseado
                    string Last24hoursString = Last24hours.ToString("yyyy-MM-dd HH:mm:ss"); // Formato deseado
                    List<Response> ProducedAndFilter = await filterFPY.FilterFPYProducto(Product, TypeSearch, Week, Last24hoursString, ahoraString);

                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
