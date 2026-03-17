using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace AGE.SignatureHub.Domain.Entities
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        public string? Description { get; set; }

        public ApplicationRole()
        {
            Id = Guid.NewGuid();
        }

        public ApplicationRole(string roleName, string? description = null) : base(roleName)
        {
            Id = Guid.NewGuid();
            Description = description;
        }
    }
}