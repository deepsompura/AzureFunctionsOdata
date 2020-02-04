using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using My.Functions.Models;
using System.Collections.Generic;
using System.Linq;

namespace My.Functions
{
    public static class OdataAzureFunction
    {
        [FunctionName("OdataAzureFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products")] HttpRequest req,
            ILogger log)
        {
           // We initialize a list of products (It can be an SQL or CosmosDB table through Entity Framework Core)
            var data = new List<Product>() {
                new Product() { Title = "Mountain Bike SERIOUS ROCKVILLE", Category = "Mountain Bicycle" },
                new Product() { Title = "Mountain Bike eléctrica HAIBIKE SDURO HARD SEVEN", Category = "Mountain Bicycle" },
                new Product() { Title = "Sillín BROOKS CAMBIUM C15 CARVED ALL WEATHER", Category = "Sillin" },
                new Product() { Title = "Poncho VAUDE COVERO II Amarillo", Category = "Chaquetas" },
            };

            // We apply the OData query to the IQueryable <Product> to the previous data source
            var result = req.ApplyTo<Product>(data.AsQueryable());

            // The result is returned
            return new OkObjectResult(result);
        }
    }
}
