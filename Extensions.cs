using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Query.Validators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.UriParser;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;

namespace My.Functions
{
    public static class Extensions
    {
        private static IServiceProvider _provider = null;
        private static RouteBuilder _routeBuilder = null;

        public static IQueryable ApplyTo<TEntity>(this HttpRequest request, IQueryable<TEntity> query)
            where TEntity : class
        {
            // Part 1 - The components required by the implementation of
            // Microsoft ASP.NET Core OData and are stored in a static variable
            if (_provider == null)
            {
                var collection = new ServiceCollection();
                collection.AddMvc().AddNewtonsoftJson();
                collection.AddLogging();
                collection.AddOData();
                collection.AddTransient<ODataUriResolver>();
                collection.AddTransient<ODataQueryValidator>();
                collection.AddTransient<TopQueryValidator>();
                collection.AddTransient<FilterQueryValidator>();
                collection.AddTransient<SkipQueryValidator>();
                collection.AddTransient<OrderByQueryValidator>();
                _provider = collection.BuildServiceProvider();
            }

            // Part 2 - ASP.NET Core OData path is configured
            if (_routeBuilder == null)
            {
                _routeBuilder = new RouteBuilder(new ApplicationBuilder(_provider));
                _routeBuilder.EnableDependencyInjection();
            }

            // Part 3 - An HTTP request is simulated as if it came from ASP.NET Core
            var modelBuilder = new ODataConventionModelBuilder(_provider);
            modelBuilder.AddEntityType(typeof(TEntity));
            var edmModel = modelBuilder.GetEdmModel();

            var httpContext = request.HttpContext;
            httpContext.RequestServices = _provider;
            
            HttpRequest req = request;
            
                req.Method = "GET";
                req.Host = request.Host;
                req.Path = request.Path;
                req.QueryString = request.QueryString;
                req.HttpContext.RequestServices = _provider;
            

            var oDataQueryContext = new ODataQueryContext(edmModel, typeof(TEntity), new Microsoft.AspNet.OData.Routing.ODataPath());
            var odataQuery = new ODataQueryOptions<TEntity>(oDataQueryContext, req);

            // Part 4 - The OData query is applied to the queryable that is passed to us by parameter
            return odataQuery.ApplyTo(query.AsQueryable());
        }
    }
}