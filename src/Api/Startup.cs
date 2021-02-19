using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Hangfire;
using Hangfire.Mongo;
using MongoDB.Driver;
using Contracts;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using System.Threading;

namespace Api
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            string databaseUrl = System.Environment.GetEnvironmentVariable("URL_DATABASE");
            var mongoUrlBuilder = new MongoUrlBuilder(databaseUrl);
            var mongoClient = new MongoClient(mongoUrlBuilder.ToMongoUrl());

            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseMongoStorage(mongoClient, mongoUrlBuilder.DatabaseName, new MongoStorageOptions
                {
                    MigrationOptions = new MongoMigrationOptions
                    {
                        MigrationStrategy = new MigrateMongoMigrationStrategy(),
                        BackupStrategy = new CollectionMongoBackupStrategy()
                    },
                    Prefix = "hangfire.mongo",
                    CheckConnection = true
                })
            );

            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHangfireDashboard("/dashboard",
                    new DashboardOptions
                    {
                        DashboardTitle = "Dashboard Tasks",
                        DisplayStorageConnectionString = false
                    });



                endpoints.MapPost("/enqueue/{jobId}",
                    async context =>
                {
                    
                    var jobid = (string)context.Request.RouteValues["jobId"];
                    var enqueueID = BackgroundJob
                        .Enqueue<IHandlerJob>(
                            handler => handler.HandleSync(jobid, CancellationToken.None));

                    await context.Response.WriteAsync(enqueueID);
                });

                endpoints.MapDelete("/queue/{jobId}",
                    async context =>
                    {
                        string jobid = (string)context.Request.RouteValues["jobId"];
                        bool queueDeleted =  BackgroundJob.Delete(jobid);
                        await context.Response.WriteAsync(queueDeleted.ToString());
                    });

            });


        }
    }
}
