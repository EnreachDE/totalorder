namespace to.frontend
{
    using System;
    using System.Linq;

    using AutoMapper;

    using Constants;

    using contracts;

    using Factories;

    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Models.Backlog;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => { options.LoginPath = "/Login"; });

            services.AddAuthorization(options =>
            {
                var permissions = Enum.GetValues(typeof(Permission));
                foreach (var permission in permissions)
                    options.AddPolicy(permission.ToString(),
                        policy => policy.RequireClaim(CustomClaims.Permission, permission.ToString()));
            });

            services.AddSingleton<IRequestHandlerFactory, RequestHandlerFactory>();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");
            });

            Mapper.Initialize(config =>
            {
                config.CreateMap<BacklogCreationRequest, CreateBacklogViewModel>().ReverseMap();
                config.CreateMap<BacklogOrderQueryResult, BacklogEvalViewModel>().ReverseMap();
                config.CreateMap<BacklogEvalQueryResult, BacklogEvalViewModel>().ReverseMap();
                config.CreateMap<BacklogOrderRequest, BacklogOrderViewModel>().ReverseMap();
                config.CreateMap<BacklogOrderRequestViewModel, BacklogOrderRequest>()
                    .ForMember(d => d.UserStoryIndexes,
                        s => s.MapFrom(str =>
                            str.UserStoryIndexes.Split(',', StringSplitOptions.None).Select(int.Parse).ToList()));
            });
        }
    }
}