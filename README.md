# Geology-Api

Here is a README draft for your Geology Department REST API:

---

# Geology Department REST API

This API provides a platform for users to upload, download, view department files, and manage user authentication using ASP.NET Core 8 LTS, Dropbox API, and BCrypt for password hashing.

## Features

- **File Management**:  
  Users can upload, download, and view files stored securely via Dropbox API integration.
  
- **Authentication**:  
  User authentication is handled with secure login and logout functionalities. Passwords are hashed using BCrypt.

## Technologies Used

- **ASP.NET Core 8 LTS**: Backend framework for building and running the API.
- **Dropbox API**: Used to handle file storage, retrieval, and download functionality.
- **BCrypt**: Ensures secure password hashing for user authentication.

## Endpoints

### 1. **User Authentication**

#### POST `/api/auth/register`
Register a new user by providing a username and password.

**Request Body:**
```json
{
  "username": "user123",
  "password": "YourPassword"
}
```

#### POST `/api/auth/login`
Log in with a valid username and password to receive a JWT for authentication.

**Request Body:**
```json
{
  "username": "user123",
  "password": "YourPassword"
}
```

**Response:**
```json
{
  "token": "jwt_token_here"
}
```

#### POST `/api/auth/logout`
Log out by invalidating the current JWT.

---

### 2. **File Management**

#### POST `/api/files/upload`
Upload a file to Dropbox.

**Request Header:**
- `Authorization: Bearer {jwt_token}`

**Request Body (multipart/form-data):**
```json
{
  "file": "your_file",
  "name": "file_name",
  "courseCode": "course_code"
}
```

#### GET `/api/files/{fileId}`
Download a file from Dropbox by specifying its file ID.

**Request Header:**
- `Authorization: Bearer {jwt_token}`

#### GET `/api/files/view/{fileId}`
View a file’s details (e.g., name, course code) without downloading it.

**Request Header:**
- `Authorization: Bearer {jwt_token}`

---

## Setup

### Prerequisites

- [.NET SDK 8 LTS](https://dotnet.microsoft.com/en-us/download)
- [Dropbox Developer Account](https://www.dropbox.com/developers)
- A database (e.g., PostgreSQL) for user data.

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/geology-department-api.git
   cd geology-department-api
   ```

2. Set up your environment variables for Dropbox API and database connection strings in `appsettings.json`.

3. Run the application:
   ```bash
   dotnet run
   ```

4. Access the API at `https://localhost:5001`.

---

## Security

- Passwords are securely hashed using BCrypt.
- JWT is used for user authentication and authorization.
- Dropbox API handles file storage with restricted access.

---

## License

This project is licensed under the MIT License.

---

