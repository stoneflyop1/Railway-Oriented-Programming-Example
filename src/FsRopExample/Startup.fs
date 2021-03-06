﻿namespace FsRopExample

open System
open System.Web.Http
open FsRopExample.Controllers
open FsRopExample.SqlDatabase
open FsRopExample.DataAccessLayer
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.HttpsPolicy;
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection

 type Startup private () =
    new (configuration: IConfiguration) as this =
        Startup() then
        this.Configuration <- configuration

    // This method gets called by the runtime. Use this method to add services to the container.
    member this.ConfigureServices(services: IServiceCollection) =
        // Add framework services.
        services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2) |> ignore
        services.AddScoped<ICustomerDao, CustomerDao>() |> ignore
        services.AddScoped<CsRopExample.DataAccessLayer.ICustomerDao, CsRopExample.DataAccessLayer.CustomerDao>() |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure(app: IApplicationBuilder, env: IHostingEnvironment) =
        if (env.IsDevelopment()) then
            app.UseDeveloperExceptionPage() |> ignore
        else
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts() |> ignore

        app.UseMiddleware<LoggingMiddleware>()
        app.UseMvc() |> ignore

    member val Configuration : IConfiguration = null with get, set
