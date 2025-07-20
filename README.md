# RealTimeChatApp

This project implements a **Real-Time Chat Backend** using **ASP.NET Core**, **PostgreSQL**, **Redis**, **SignalR**, and **Cloudinary** for file uploads. It includes JWT-based authentication, message sending, editing, deleting, and full-text search functionality. The solution is containerized using **Docker** and includes CI/CD pipeline setup via **GitHub Actions**.

## Table of Contents
1. [Setup Guide](#setup-guide)
2. [API Usage](#api-usage)
3. [CI/CD Overview](#cicd-overview)
4. [Deployment Steps](#deployment-steps)
5. [Assumptions & Tradeoffs](#assumptions--tradeoffs)

## Setup Guide

### Prerequisites:
- **Docker** (for containerization)
- **PostgreSQL** and **Redis** (for database and cache)
- **Cloudinary** (for file uploads)
- **ASP.NET Core SDK 7.x** (for local development)
- **TablePlus** (for PostgreSQL database management)

### Docker Setup:

1. Clone the repository:
   ```bash
   git clone https://github.com/RealTimeChatApp.git
   cd RealTimeChatApp
   ````

2. Build the Docker containers:

   ```bash
   docker-compose up --build
   ```

3. This will spin up the following services:

   * **Backend API** (ASP.NET Core)
   * **PostgreSQL** database
   * **Redis** for SignalR pub/sub scaling

4. After the services are up, you can access the **Swagger UI** for API testing:

   ```
   http://localhost:5072/swagger
   ```

### Database Configuration:

* **PostgreSQL** is used as the relational database.
* **TablePlus** is used for easy database management.
* To configure your database, use the connection string in `appsettings.json` or `.env` file.

Sample connection string:

```json
"ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=chatdb;Username=postgres;Password=password"
}
```

## API Usage

### Authentication (JWT-based):

#### Secure API Calls:

* Use the JWT token returned from the login endpoint.
* Add this token in the **Authorization header** for protected routes:

  ```bash
  Authorization: Bearer <token>
  ```

### Group Operations:

### Message Operations:

### WebSocket (SignalR) Endpoints:

#### Connect to Chat Hub:

* `POST /chathub`
* This endpoint establishes a WebSocket connection for real-time message broadcasting.
* Use SignalR to send/receive messages via the `ChatHub`.

---

## CI/CD Overview

### GitHub Actions Workflow:

A basic **CI/CD** pipeline is set up using **GitHub Actions** to automate the build, test, and deploy processes.

#### Workflow Steps:

1. **Checkout code**:

   * `actions/checkout` checks out the code repository.

2. **Set up .NET environment**:

   * `actions/setup-dotnet` installs the required .NET SDK version.

3. **Restore dependencies**:

   * `dotnet restore` restores project dependencies.

4. **Build the project**:

   * `dotnet build` builds the solution.

5. **Run tests**:

   * `dotnet test` runs unit and integration tests using `xUnit`.

6. **Deploy Docker Image (optional)**:

   * Push Docker image to Docker Hub or a container registry (e.g., GitHub Packages, AWS ECR).

GitHub Actions YAML configuration is located in `.github/workflows/ci.yml`.

> You can customize the actions to suit your deployment platform.

---

## Deployment Steps

### Docker Deployment:

1. Ensure your `docker-compose.yml` is set up as per the setup guide.

2. Run the following command to bring up the containers:

   ```bash
   docker-compose up --build
   ```

### Cloud Deployment (Optional):

1. Deploy on **Railway** or **Render** using Docker for the backend.
2. Ensure **PostgreSQL** and **Redis** are configured on the cloud platform.
3. Set the environment variables and connection strings via your cloud platform’s dashboard.

### Cloudinary Setup:

* Set up a **Cloudinary** account and configure your environment variables for file uploads.

Example configuration:

```json
"Cloudinary": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
}
```

---

## Assumptions & Tradeoffs

### Assumptions:

* The application will be used for small to medium-scale chat groups.
* JWT-based authentication is sufficient for the application’s security needs.
* File uploads are handled by Cloudinary (alternative: AWS S3).

### Tradeoffs:

* **Database choice**: PostgreSQL was selected for relational data, but NoSQL (e.g., MongoDB) could have been more flexible for unstructured data.
* **File upload**: Cloudinary was chosen for simplicity, but AWS S3 might be more suitable for larger-scale production environments.
* **SignalR + Redis**: Redis was used for scaling SignalR, but this may not be necessary for smaller applications with limited traffic.

---

## Conclusion

This **Real-Time Chat Backend** is a scalable solution built with **ASP.NET Core**, **PostgreSQL**, **Redis**, **SignalR**, and **Cloudinary**. It supports features like group creation, message sending/editing, file uploads, and real-time communication. Docker and CI/CD integration ensure smooth deployment and scalability. The architecture follows the **Onion Architecture** to ensure maintainability and testability.

```

---

Now you can **copy** this markdown, paste it into your `README.md` file, and you're all set! Let me know if you'd like further tweaks or help with any remaining tasks.
```

