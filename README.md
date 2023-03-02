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
1. UI: Both web UI and mobile app are not included. I used Swagger to test the API.
2. Authentication: I didn't implement authentication related with Admin users. I only implemented the authentication for the normal users.
3. Database: I didn't implement the distributed database. All data are stored in a SQL Server database.
4. API Gateway: I didn't implement API Gateway. One possible use case is to route the request to the right API based on the request URL.
5. Tests: I didn't implement any unit tests. I only tested the API manually using Swagger. 
6. HTTPS: I didn't implement HTTPS. I used HTTP for simplicity.
7. Image: Currently the images size is not limited, which could affect the performance.
8. MQTT: The current eventbus is using AMQP, while for mobile devices, MQTT maybe better



### Issues & Solutions:
1. Spatial data mapping
2. Docker compose networking
3. Opening hours


