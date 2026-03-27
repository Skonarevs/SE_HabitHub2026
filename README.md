# SE_HabitHub2026

Software Engineering Project

HabitHub is a full-stack habit-tracking application designed to help users build and maintain healthy routines. This repository contains the source code for both the frontend and backend services, fully containerized using Docker.

---

## Sprint 1: Foundation & Infrastructure

The primary goal of the first sprint was to establish a solid "Full Stack" foundation. We focused on containerization, database connectivity, and the core authentication flow.

### ✅ Completed Tasks

#### Infrastructure & DevOps 
* **Multi-Container Orchestration:** Configured `docker-compose.yml` to synchronize three microservices: **Frontend** (Vite), **Backend** (.NET 9), and **Database** (MS SQL Server 2022).
* **Networking & Port Forwarding:** * **Frontend** accessible at `http://localhost:5173`.
    * **Backend API** (Swagger) accessible at `http://localhost:5100`.
    * **Database** internal communication via port `1433`.
* **Resilience:** Implemented `restart: always` policies to ensure the Backend automatically reconnects once the SQL Server initializes system files.

####  Backend API (.NET 9)
* **Framework Initialization:** Built the core Web API using .NET 9 and C#.
* **Database Integration:** Configured **Entity Framework Core** (EF Core) with automated database creation using `EnsureCreated()`.
* **Auth Logic:** Developed the `AuthService` and `AuthController` to handle user login and session management.
* **API Documentation:** Integrated **Swagger/OpenAPI** for real-time endpoint testing and documentation.

####  Frontend (React + TypeScript)
* **User Interface:** Created high-fidelity **Sign Up** and **Login** forms using `react-hook-form` and Tailwind CSS.
* **Security:** Developed **Protected Routes** logic to prevent unauthorized access to internal pages (Dashboard/Profile).
* **State Management (Zustand):** Implemented **Zustand** for lightweight and reactive global state management. It handles user authentication states and persists session data across the application.
* **Axios Integration:** Created a centralized `axiosInstance` to manage API calls and handle cross-origin requests.
* **Session Persistence:** Implemented `localStorage` logic to store `sessionId` and `userName` upon successful authentication.

---

##  Technology Stack

| Layer | Technology |
| :--- | :--- |
| **Frontend** | React 18, TypeScript, Zustand, Tailwind CSS, Vite |
| **Backend** | .NET 9 (C#), ASP.NET Core Web API |
| **Database** | Microsoft SQL Server 2022 |
| **Persistence** | Entity Framework Core (EF Core) |
| **DevOps** | Docker, Docker Compose |

