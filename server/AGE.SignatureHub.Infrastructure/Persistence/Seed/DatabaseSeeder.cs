using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AGE.SignatureHub.Infrastructure.Persistence.Seed
{
    public class DatabaseSeeder
    {
        private readonly ApplicationDBContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(ApplicationDBContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                await SeedRolesAsync();

                await SeedAdminUserAsync();

                await SeedTestUsersAsync();

                _logger.LogInformation("Database seeding completed successfully.");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        private async Task SeedRolesAsync()
        {
            var roles = new List<ApplicationRole>
            {
                new ApplicationRole("Administrator", "Administrador do sistema com acesso total"),
                new ApplicationRole("User", "Usuário comum com acesso limitado"),
                new ApplicationRole("Manager", "Gerente com permissões intermediárias"),
                new ApplicationRole("Viewer", "Visualizador com acesso somente leitura")
            };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role.Name))
                {
                    var result = await _roleManager.CreateAsync(role);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Role '{RoleName}' created successfully.", role.Name);
                    }
                    else
                    {
                        _logger.LogError("Failed to create role '{RoleName}'. Errors: {Errors}", role.Name, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }
        }

        private async Task SeedAdminUserAsync()
        {
            var adminEmail = "desenvolvimento@age.mg.gov.br";

            var existingAdmin = await _userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Administrador do Sistema",
                    Department = "DIDTI",
                    Position = "Administrador",
                    RegistrationNumber = "ADM001",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(adminUser, "Admin@123456");

                if (result.Succeeded)
                {
                    _logger.LogInformation("Admin user created successfully.");

                    var addToRoleResult = await _userManager.AddToRoleAsync(adminUser, "Administrator");
                    if (addToRoleResult.Succeeded)
                    {
                        _logger.LogInformation("Admin user added to 'Administrator' role successfully.");
                    }
                    else
                    {
                        _logger.LogError("Failed to add admin user to 'Administrator' role. Errors: {Errors}", string.Join(", ", addToRoleResult.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    _logger.LogError("Failed to create admin user. Errors: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        private async Task SeedTestUsersAsync()
        {   
            var testUsers = new List<(string Email, string FullName, string Role, string Position)>
            {
                ("maria.silva@age.mg.gov.br", "Maria Silva", "Manager", "Procuradora"),
                ("joao.santos@age.mg.gov.br", "João Santos", "User", "Analista Jurídico"),
                ("ana.costa@age.mg.gov.br", "Ana Costa", "User", "Assistente"),
                ("carlos.oliveira@age.mg.gov.br", "Carlos Oliveira", "Viewer", "Estagiário")
            };

            foreach (var (email, fullName, role, position) in testUsers)
            {
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = fullName,
                        Department = "Jurídico",
                        Position = position,
                        RegistrationNumber = $"USR{Random.Shared.Next(1000, 9999)}",
                        EmailConfirmed = true,
                        IsActive = true
                    };

                    var result = await _userManager.CreateAsync(user, "Test@123456");

                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, role);
                        _logger.LogInformation("Test user {Email} created", email);
                    }
                }
            }
        }
    }
}