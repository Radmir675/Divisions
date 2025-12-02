using CSharpFunctionalExtensions;
using Devisions.Domain.Department;
using Devisions.Domain.Location;
using Devisions.Domain.Position;
using Devisions.Infrastructure.Postgres.Database;
using Microsoft.Extensions.Logging;
using Shared.Errors;

namespace Devisions.Infrastructure.Postgres.Seeder;

public class DevisionsSeeder : ISeeder
{
    private const int MAX_LOCATIONS = 10;
    private const int MAX_DEPARTMENTS = 10;
    private const int MAX_POSITIONS = 10;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<DevisionsSeeder> _logger;
    private readonly CancellationToken _cancellationToken;
    private IEnumerable<Position> _positions = null!;
    private IEnumerable<Department> _departments = null!;
    private IEnumerable<Location> _locations = null!;

    public DevisionsSeeder(
        AppDbContext dbContext,
        ILogger<DevisionsSeeder> logger,
        CancellationToken cancellationToken = default)
    {
        _dbContext = dbContext;
        _logger = logger;
        _cancellationToken = cancellationToken;
    }

    public async Task CreateAsync(CancellationToken cancellationToken)
    {
        if (_dbContext.Departments.Any() == false)
            await SeedAsync(_cancellationToken);
    }

    private async Task SeedAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Seeding started");
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(_cancellationToken);
        try
        {
            // Очистка базы данных
            var result = await ClearDatabase(cancellationToken);
            if (result.IsFailure)
            {
                await transaction.RollbackAsync(cancellationToken);
            }

            // Сидирование данных
            _locations = await SeedLocationsAsync(cancellationToken);

            _departments = await SeedDepartmentsAsync(
                _locations.Select(x => x.Id).ToList(),
                cancellationToken);

            _positions = await SeedPositionsAsync(cancellationToken);

            await transaction.CommitAsync(_cancellationToken);

            _logger.LogInformation("Seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while seeding the database");
        }
    }

    private async Task<UnitResult<Error>> ClearDatabase(CancellationToken cancellationToken)
    {
        try
        {
            _dbContext.DepartmentPositions.RemoveRange(_dbContext.DepartmentPositions);
            _dbContext.DepartmentLocations.RemoveRange(_dbContext.DepartmentLocations);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _dbContext.Positions.RemoveRange(_dbContext.Positions);
            _dbContext.Departments.RemoveRange(_dbContext.Departments);
            _dbContext.Locations.RemoveRange(_dbContext.Locations);

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Clearing database");
            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while clearing the database");
            return GeneralErrors.DatabaseError();
        }
    }

    private async Task<IEnumerable<Location>> SeedLocationsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting seeding locations data ...");

        var locations = new List<Location>();
        try
        {
            var locationBaseNames = new[]
            {
                "Haven", "Vale", "Ridge", "Hollow", "Meadow", "Pines", "Springs", "Thorn", "Bluff",
            };

            var random = new Random(); // ✅ один экземпляр

            for (int i = 0; i < MAX_LOCATIONS; i++)
            {
                // Генерация уникального названия локации
                string baseName = locationBaseNames[random.Next(locationBaseNames.Length)];
                int suffix = random.Next(1, 100); // от 1 до 99, чтобы избежать "Haven0"
                string locationName = $"{baseName}{suffix}";

                // Создание адреса
                var addressResult = Address.Create("Russia", "Moscow", $"Lenina {i + 1}",
                    random.Next(1, 200),
                    null);
                if (addressResult.IsFailure)
                {
                    _logger.LogWarning("Skipping location due to invalid address: {Error}", addressResult.Error);
                    continue;
                }

                // Создание часового пояса
                var timezoneResult = Timezone.Create("Europe/Moscow");
                if (timezoneResult.IsFailure)
                {
                    _logger.LogWarning("Skipping location due to invalid timezone: {Error}", timezoneResult.Error);
                    continue;
                }

                // Создание локации
                var locationResult = Location.Create(
                    locationName,
                    addressResult.Value,
                    isActive: true,
                    timezoneResult.Value);

                if (locationResult.IsFailure)
                {
                    _logger.LogError("Location could not be created: {Error}", locationResult.Error);
                    continue;
                }

                locations.Add(locationResult.Value);
            }

            if (locations.Count > 0)
            {
                _dbContext.Set<Location>().AddRange(locations);
                await _dbContext.SaveChangesAsync(cancellationToken); // ✅ используем переданный токен
            }

            _logger.LogInformation("{Count} locations seeded successfully.", locations.Count);
            return locations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed seeding locations");
            return locations;
        }
    }

    private async Task<IEnumerable<Department>> SeedDepartmentsAsync(
        IReadOnlyList<LocationId> existingLocationIds,
        CancellationToken cancellationToken)
    {
        if (!existingLocationIds.Any())
            throw new InvalidOperationException("Cannot seed departments: no locations available.");

        _logger.LogInformation("Starting seeding departments data ...");

        var departments = new List<Department>();
        var random = new Random();

        var departmentNameBases = new[]
        {
            "Engineering", "Marketing", "Sales", "Human Resources", "Finance", "Support", "R&D", "Legal",
            "Operations", "Product",
        };

        var identifierPrefixes = new[] { "ENG", "MKT", "SLS", "HR", "FIN", "SUP", "RND", "LGL", "OPS", "PRD" };

        // Буквенные суффиксы вместо чисел (можно расширить)
        var suffixes = new[]
        {
            "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta", "Theta", "Iota", "Kappa", "Lambda", "Mu",
            "Nu", "Xi", "Omicron", "Pi",
        };

        try
        {
            for (int i = 0; i < MAX_DEPARTMENTS; i++)
            {
                // === 1. Отображаемое имя (может содержать пробелы и цифры) ===
                string displayName = $"{departmentNameBases[random.Next(departmentNameBases.Length)]} {i + 1}";
                var nameResult = DepartmentName.Create(displayName);
                if (nameResult.IsFailure)
                {
                    _logger.LogWarning("Skipping department due to invalid name: {Error}", nameResult.Error);
                    continue;
                }

                // === 2. Идентификатор БЕЗ ЦИФР: ENG-Alpha, HR-Beta и т.д. ===
                string prefix = identifierPrefixes[random.Next(identifierPrefixes.Length)];
                string suffix = suffixes[i % suffixes.Length]; // или random, если нужно
                string identifierStr = $"{prefix}-{suffix}";

                var identifierResult = Identifier.Create(identifierStr);
                if (identifierResult.IsFailure)
                {
                    _logger.LogWarning(
                        "Generated invalid identifier '{Identifier}': {Error}",
                        identifierStr,
                        identifierResult.Error);
                    continue;
                }

                // === 3. Локации (1–3 шт) ===
                int locationCount = random.Next(1, Math.Min(4, existingLocationIds.Count + 1));
                var selectedLocationIds = existingLocationIds
                    .OrderBy(_ => random.Next())
                    .Take(locationCount)
                    .ToList();

                // === 4. Родитель? ===
                Department? parent = null;

                // 70% → дочерних
                if (departments.Count > 0 && random.Next(0, 10) >= 3)
                {
                    parent = departments[random.Next(departments.Count)];
                }

                // === 5. Создание ===
                Result<Department, Error> departmentResult;
                if (parent == null)
                {
                    departmentResult = Department.CreateParent(
                        nameResult.Value,
                        identifierResult.Value,
                        selectedLocationIds);
                }
                else
                {
                    departmentResult = Department.CreateChild(
                        nameResult.Value,
                        identifierResult.Value,
                        parent,
                        selectedLocationIds);
                }

                if (departmentResult.IsFailure)
                {
                    _logger.LogError("Department creation failed: {Error}", departmentResult.Error);
                    continue;
                }

                departments.Add(departmentResult.Value);
            }

            if (departments.Count > 0)
            {
                _dbContext.Set<Department>().AddRange(departments);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            _departments = departments;
            _logger.LogInformation("{Count} departments seeded successfully.", departments.Count);
            return departments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed seeding departments");
            return departments;
        }
    }

    private async Task<IEnumerable<Position>> SeedPositionsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting seeding positions data ...");

        var departmentIds = _departments.Select(x => x.Id).ToList();
        if (!departmentIds.Any())
        {
            throw new InvalidOperationException("Cannot seed positions: no departments available.");
        }

        var positions = new List<Position>();
        try
        {
            // --- Источники данных (вынесены из цикла) ---
            var positionTitles = new[]
            {
                "Manager", "Team Lead", "Developer", "Analyst", "Coordinator", "Specialist", "Consultant",
                "Engineer", "Administrator", "Officer", "Director", "Supervisor", "Associate", "Executive",
                "Architect", "Designer", "Technician", "Operator", "Planner", "Advisor",
            };

            var random = new Random(); // ✅ один экземпляр

            for (int i = 0; i < MAX_POSITIONS; i++)
            {
                // Генерация уникального имени позиции
                string randomPositionName = positionTitles[random.Next(positionTitles.Length)] +
                                            random.Next(1, 101);

                var positionId = new PositionId(Guid.NewGuid());
                var departmentsPositions = new List<DepartmentPosition>();

                // Сколько департаментов привязать к позиции (0–4)
                int departmentCount = random.Next(0, 5);

                for (int j = 0; j < departmentCount; j++)
                {
                    var randomDeptId = departmentIds[random.Next(departmentIds.Count)];
                    departmentsPositions.Add(new DepartmentPosition(
                        Guid.NewGuid(),
                        randomDeptId,
                        positionId));
                }

                var position = Position.Create(
                    positionId,
                    PositionName.Create(randomPositionName).Value,
                    null,
                    departmentsPositions);

                if (position.IsFailure)
                {
                    _logger.LogError("Position could not be created: {Error}", position.Error);
                    continue;
                }

                positions.Add(position.Value);
            }

            _dbContext.Set<Position>().AddRange(positions);
            await _dbContext.SaveChangesAsync(cancellationToken); // ✅ используем переданный токен

            _logger.LogInformation("{Count} positions seeded successfully.", positions.Count);
            return positions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed seeding positions");
            return positions;
        }
    }
}