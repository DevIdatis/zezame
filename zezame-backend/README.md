
## MIGRATIONS

dotnet ef migrations add InitialCreate --project Zezame.Infrastructure --startup-project Zezame.Api
dotnet ef database update --project Sesame.Infrastructure --startup-project Sesame.Api