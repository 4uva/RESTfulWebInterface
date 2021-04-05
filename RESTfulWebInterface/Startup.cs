using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using RESTfulWebInterface.Persistence;
using RESTfulWebInterface.Persistence.EF;
using RESTfulWebInterface.Persistence.InMemory;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace RESTfulWebInterface
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            this.env = env;
        }

        public IConfiguration Configuration { get; }
        private IWebHostEnvironment env;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<CsvLoader>();
            services.AddSingleton(serviceProvider => BuildCsvRepository(serviceProvider));
            services.AddControllers()
                    .AddJsonOptions(options =>
                        {
                            options.JsonSerializerOptions.PropertyNamingPolicy = new JsonLowercaseNamingPolicy();
                            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumMemberConverter());
                        })
                    .AddControllersAsServices();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "RESTfulWebInterface", Version = "v1" });
            });

            services.AddDbContext<PersonsContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("PersonsContext")));
            services.AddScoped(serviceProvider => BuildRepository(serviceProvider));
        }

        public class JsonLowercaseNamingPolicy : JsonNamingPolicy
        {
            public override string ConvertName(string name) => name.ToLowerInvariant();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "RESTfulWebInterface v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        InMemoryRepository BuildCsvRepository(IServiceProvider serviceProvider)
        {
            var csvPath = Configuration["DataSources:CSV"];
            if (csvPath != null)
            {
                var fileInfo = env.ContentRootFileProvider.GetFileInfo(csvPath);
                if (!fileInfo.Exists)
                    throw new InvalidOperationException("Configuration points to nonexisting file");
                var csvLoader = serviceProvider.GetRequiredService<CsvLoader>();
                using var stream = fileInfo.CreateReadStream();
                var personsFromCsv = csvLoader.Load(stream, strict: false);
                return new InMemoryRepository(personsFromCsv);
            }
            throw new InvalidOperationException("Configuration didn't specify CSV data source");
        }

        IUowRepository BuildRepository(IServiceProvider serviceProvider)
        {
            var useCsv = (Configuration["DataSources:CSV"] != null);
            if (useCsv)
            {
                var csvRepo = serviceProvider.GetRequiredService<InMemoryRepository>();
                return new InMemoryContext(csvRepo);
            }
            var useEf = (Configuration["DataSources:EF"] == "true");
            if (useEf)
            {
                var context = serviceProvider.GetRequiredService<PersonsContext>();
                context.Database.EnsureCreated();
                return context;
            }
            throw new InvalidOperationException("Configuration didn't specify any data sources");
        }
    }
}
