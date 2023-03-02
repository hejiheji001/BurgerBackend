# BurgerBackend

### Architecture:
1. Event Driven
2. Microservices
3. CQRS
4. DDD
5. RESTful API
6. Docker
7. Redis
8. ASP.Net Core
9. Entity Framework Core 

## Part of this project is based on eShopContainer
https://github.com/dotnet-architecture/eShopOnContainers

### What I have implemented:
1. Listing.API: 
	a. This API is used to get the burger places info from the database by their location or address or name
	b. Search result is cached in Redis

2. Identity.API:
	a. This API allows users to login
	b. Allows Logged in user to submit review to Review.API
	c. Nonlogged in user can only view the burger places info and reviews.

3. Review.API:
	a. This API is used to submit reviews for burger places


### What I didn't implemented:
1. UI: Both web UI and mobile app are not included. I only implemented the RESTful APIs.
2. Authentication: I didn't implement authentication related with Admin users. I only implemented the authentication for the normal users.
3. Database: I didn't implement the distributed database. All data are stored in a SQL Server database.
4. API Gateway: I didn't implement API Gateway. One possible use case is to route the request to the right API based on the request URL.
5. Tests: I didn't implement any unit tests. I only tested the API manually using Swagger. 
6. HTTPS: I didn't implement HTTPS. I used HTTP for simplicity.
7. Image: Currently the images upload is not supported.
8. MQTT: The current eventbus is using AMQP, while for mobile devices, MQTT maybe better


### Issues & Solutions:
1. Spatial Data:
	- EF Core Support: https://learn.microsoft.com/en-us/ef/core/modeling/spatial
	- DB Mapping: https://learn.microsoft.com/en-us/ef/core/providers/sql-server/spatial
	- JSON Serialization: https://github.com/NetTopologySuite/NetTopologySuite.IO.GeoJSON
    - Distance Calculation: 
      - Projection: https://learn.microsoft.com/en-us/ef/core/modeling/spatial#srid-ignored-during-client-operations
      - Coordinate System Lookup: https://epsg.io/
      - Degree to Distance: https://sciencing.com/convert-latitude-longtitude-feet-2724.html
      
2. Docker compose networking:
   - Config: https://docs.docker.com/compose/networking/
   - VPN Issues: Container(ASP.Net) can't access another container(SQL Server) in the same docker compose network. Try close VPN!!
   
3. EF Core:
   - Iterating with `IAsyncEnumerable`:
     https://learn.microsoft.com/en-us/archive/msdn-magazine/2019/november/csharp-iterating-with-async-enumerables-in-csharp-8
   - Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency
   - `TimeOnly`: https://docs.microsoft.com/en-us/dotnet/api/system.timeonly?view=netcore-3.1
   - `GC.SuppressFinalize`: https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1816

