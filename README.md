# EduSync Assessment

A .NET Core web application for managing educational assessments and courses.

## Features

- User Authentication and Authorization
- Course Management
- Assessment Creation and Management
- Result Tracking
- File Upload Support (using Azure Blob Storage)

## Technical Stack

- .NET Core
- Entity Framework Core
- Azure Blob Storage
- NUnit for Testing
- Moq for Mocking

## Project Structure

- `EduSync_Assessment/` - Main application
  - `Controllers/` - API Controllers
  - `Models/` - Data Models
  - `Data/` - Database Context
  - `DTO/` - Data Transfer Objects
  - `BlobServices/` - Azure Blob Storage Services

- `EduSync_Assessment.Tests/` - Test Project
  - `Controllers/` - Controller Tests

## Getting Started

1. Clone the repository
2. Restore NuGet packages
3. Update the connection string in `appsettings.json`
4. Run database migrations
5. Start the application

## Testing

The project includes comprehensive unit tests for all controllers using NUnit and Moq. 