# ★ POKÉDEX — C# ASP.NET Core API (INTE 202)

> A Game Boy-themed Pokédex web application with a full RESTful API backend and a retro-styled single-page frontend.

---

## 👥 Members

| Name | Role |
|------|------|
| Baldoza, Anthony F. | Developer (Solo) |
| Nidea, Aron L. | Member |

**Course:** INTE 202 — Integrative Programming and Technologies
**School:** Polytechnic University of the Philippines — Calauan Campus

---

## 📌 Project Overview

The **Pokédex API** is a RESTful web service built as a Final Project for INTE 202. The system serves as a digital Pokédex that allows users to manage a complete database of **Generation I Pokémon (1-151)**. It supports full CRUD operations — users can create, retrieve, update, and delete Pokémon records through a **Game Boy-themed web interface**, as well as search Pokémon by name and filter them by type.

The system follows a clean **N-Layer Architecture** that separates the application into distinct layers — Controller, Service, Repository, and Database — ensuring maintainability, scalability, and separation of concerns throughout the codebase.

---

## ✨ Features

### 🖥️ Frontend (Game Boy-Themed SPA)
- 🎮 Retro Game Boy-inspired UI design
- 📋 View all 151 Generation I Pokémon with sprite images
- 🔍 Real-time search by Pokémon name
- 🎯 Filter Pokémon by type
- 📊 Base Stats breakdown with colored progress bars (HP, Attack, Defense, Sp. Attack, Sp. Defense, Speed)
- 🔄 Evolution chain navigation — clickable links (e.g. #001 ▶ #002 ▶ #003)
- ✨ Shiny toggle — switch between normal and shiny sprite variants
- ⚔️ Compare Mode — side-by-side stat comparison with battle result banner
- ➕ Add new Pokémon via modal form
- ✏️ Edit existing Pokémon data
- ❌ Delete Pokémon with confirmation prompt

### ⚙️ Backend (REST API)
- Full CRUD operations via RESTful endpoints
- Search by name (`/api/pokemon/search?name=`)
- Filter by type
- Global exception handling middleware
- Swagger UI for API documentation and testing
- PostgreSQL database with Entity Framework Core
- Auto-seeded with all 151 Gen 1 Pokémon on first run

---

## 🛠️ Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | ASP.NET Core 10 Web API |
| ORM | Entity Framework Core 9 (Npgsql PostgreSQL Provider) |
| Database | PostgreSQL (Port: 5432) |
| Frontend | Vanilla HTML, CSS, JavaScript (SPA) |
| API Docs | Swagger / Swashbuckle |
| Architecture | N-Layer: Controller → Service → Repository → DbContext |
| Port | Frontend & Backend: 5010 |

---

## 📁 Project Structure

```
Pokedex/
├── Controllers/
│   ├── Models/
│   │   └── Pokemon.cs                        # Pokemon entity model
│   ├── PokemonController.cs                  # REST API endpoints (7 endpoints)
│   └── Services/
│       └── PokemonService.cs                 # Business logic & DTO mapping
├── DTOs/
│   └── PokemonDto.cs                         # PokemonRequestDto & PokemonResponseDto
├── Data/
│   ├── PokedexDbContext.cs                   # EF Core DbContext
│   ├── SeedData.cs                           # Seeds 151 Gen 1 Pokémon on startup
│   └── docker-compose.yml                    # PostgreSQL Docker setup
├── Middlewares/
│   └── GlobalExceptionHandlerMiddleware.cs   # Structured JSON error responses
├── Migrations/                               # EF Core auto-generated migrations
├── Repositories/
│   ├── IPokemonRepositories.cs               # Repository interface
│   └── PokemonRepository.cs                  # EF Core database operations
├── Program.cs                                # App entry point & DI configuration
├── appsettings.json                          # DB connection string & config
└── wwwroot/
    └── index.html                            # Game Boy-themed SPA frontend
```

---

## 🗄️ Database Structure

| Column | Data Type | Description |
|--------|-----------|-------------|
| Id | INTEGER (PK) | Auto-incremented primary key |
| Name | VARCHAR(100) | Pokémon name — required |
| Type | VARCHAR | Primary type (e.g. Fire, Water) — required |
| Type2 | VARCHAR (nullable) | Secondary type — optional |
| Description | VARCHAR(500) | Pokédex entry description |
| HP | INTEGER | Hit Points stat (1-255) |
| Attack | INTEGER | Attack stat (1-255) |
| Defense | INTEGER | Defense stat (1-255) |
| SpAttack | INTEGER | Special Attack stat (1-255) |
| SpDefense | INTEGER | Special Defense stat (1-255) |
| Speed | INTEGER | Speed stat (1-255) |
| Height | FLOAT | Height in meters |
| Weight | FLOAT | Weight in kilograms |
| IsLegendary | BOOLEAN | Marks if the Pokémon is legendary |
| EvolvesFromId | INTEGER (nullable) | References the pre-evolution |
| EvolvesToId | INTEGER (nullable) | References the next evolution |

---

## 📡 API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/pokemon` | Get all Pokémon |
| GET | `/api/pokemon/{id}` | Get Pokémon by ID |
| GET | `/api/pokemon/search?name=` | Search Pokémon by name |
| GET | `/api/pokemon/filter?type=` | Filter Pokémon by type |
| POST | `/api/pokemon` | Add new Pokémon |
| PUT | `/api/pokemon/{id}` | Update Pokémon |
| DELETE | `/api/pokemon/{id}` | Delete Pokémon |

---

## 🚀 How to Run

### Prerequisites
- .NET 10 SDK
- Docker Desktop
- PostgreSQL

### Steps

**1. Start PostgreSQL via Docker:**
```bash
cd Pokedex/Data
docker-compose up -d
```

**2. Apply database migrations:**
```bash
cd Pokedex
dotnet ef database update
```

**3. Run the application:**
```bash
dotnet run
```

**4. Open the App:**
- Frontend: `http://localhost:5010`
- Swagger UI: `http://localhost:5010/swagger`

---

## ⚠️ Challenges & Solutions

**Challenge 1: Float vs Double Type Mismatch**
Encountered a build error — `Cannot implicitly convert type 'double' to 'float'` — when mapping Height and Weight from DTO to Model.
**Solution:** Added explicit casts `(float)dto.Height` and `(float)dto.Weight` in the mapping method.

**Challenge 2: PostgreSQL Service Not Starting**
PostgreSQL 17 service would start and immediately stop.
**Solution:** Corrected the username and password credentials in `appsettings.json` to match the local PostgreSQL installation.

**Challenge 3: Height & Weight Displaying Too Many Decimal Places**
Values like `1.6000000023841858m` appeared due to floating point representation.
**Solution:** Applied `.toFixed(1)` in the JavaScript frontend to limit display to one decimal place.

---

## 📄 License

This project was developed as a Final Project for **INTE 202 — Integrative Programming and Technologies** at **PUP Calauan, Laguna Campus**.
