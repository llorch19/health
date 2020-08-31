using IdentityModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.OpenApi.Models;
using System.IO;
using health.common;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.FileProviders;
using util;
using health.Middleware;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Google.Protobuf.WellKnownTypes;
using System;
using IdGen;
using health.web.common;
using health.web.Domain;
using util.mysql;

namespace health
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            HostEnvironment = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment HostEnvironment { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
           

            services.AddSingleton<FastFailException>();
            services.AddControllers( options =>
            {
                options.Filters.Clear();
                options.Filters.Add<ZFExceptionFilter>();
                options.Filters.Add<ModelInvalid201Filter>();
                options.ModelValidatorProviders.Clear();
                options.ModelValidatorProviders.Add(new ZFModelValidatorProvider());
            }
            ).AddNewtonsoftJson().AddControllersAsServices();
            

            #region Auth 
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).
            AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "jonny",
                    ValidAudience = "jonny",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Const.SecurityKey))
                };
            });
            #endregion

            #region swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Web端API接口文档", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please insert JWT with Bearer into field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme
                        {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                        },
                        new string[] { }
                    }
                });
                // 为 Swagger JSON and UI设置xml文档注释路径
                var basePath = HostEnvironment.ContentRootPath;//获取应用程序所在目录（绝对，不受工作目录影响，建议采用此方法获取路径）
                var xmlPath = Path.Combine(basePath, "health.web.xml");
                c.IncludeXmlComments(xmlPath);
            });
            #endregion

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            #region IdGenerator
            var epoch = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Local);
            // Create an ID with 45 bits for timestamp, 2 for generator-id 
            // and 16 for sequence
            var structure = new IdStructure(45, 2, 16);
            // Prepare options
            var options = new IdGeneratorOptions(structure, new DefaultTimeSource(epoch));
            // Create an IdGenerator with it's generator-id set to 0, our custom epoch 
            // and id-structure
            var generator = new IdGenerator(0, options);
            services.AddSingleton<IdGenerator>(generator);
            #endregion

            services.AddMemoryCache();

            services.AddLazyResolution();


            services.AddTransient(typeof(dbfactory));
            services.AddTransient(typeof(AddressCategoryRepository));
            services.AddTransient(typeof(AreaRepository));
            services.AddTransient(typeof(DetectionResultTypeRepository));
            services.AddTransient(typeof(AttandentRepository));
            services.AddTransient(typeof(TransferRepository));
            services.AddTransient(typeof(PersonRepository));
            services.AddTransient(typeof(DomiTypeRepository));
            services.AddTransient(typeof(GenderRepository));
            services.AddTransient(typeof(IdCategoryRepository));
            services.AddTransient(typeof(MedicationDosageFormRepository));
            services.AddTransient(typeof(MedicationFreqCategoryRepository));
            services.AddTransient(typeof(MedicationPathwayRepository));
            services.AddTransient(typeof(NationRepository));
            services.AddTransient(typeof(OccupationRepository));
            services.AddTransient(typeof(TreatmentOptionRepository));
            services.AddTransient(typeof(CheckProductRepository));
            services.AddTransient(typeof(MedicationRepository));
            services.AddTransient(typeof(MenuRepository));
            services.AddTransient(typeof(OptionRepository));
            services.AddTransient(typeof(OrgnizationRepository));
            services.AddTransient(typeof(AppointRepository));
            services.AddTransient(typeof(VaccRepository));
            services.AddTransient(typeof(FollowupRepository));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            config conf = new config();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web端");
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();


            // 使用中间件拦截未登录的请求，响应时间在10ms以内。
            app.UseMiddleware<NotLogin401MiddleWare>();


            // 在生产环境当中，upload不可以static资源发放，应该以UploadController的形式发放
            app.UseStaticFiles(new StaticFileOptions
            {
                    FileProvider = new PhysicalFileProvider(
                            Path.Combine(env.ContentRootPath, conf.GetValue("user:static"))),
                    RequestPath = "/"+ conf.GetValue("user:static")
            }
            );


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
