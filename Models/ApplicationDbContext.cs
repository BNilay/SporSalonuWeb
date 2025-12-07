using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace webProje.Models
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{	
		//constructor
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
		{
		}

		public DbSet<Antrenor> Antrenorler { get; set; }
		public DbSet<SporSalonu> SporSalonlari { get; set; }
		public DbSet<Uye> Uyeler { get; set; }
		public DbSet<Randevu> Randevular { get; set; }
		public DbSet<Hizmet> Hizmetler { get; set; }


	}
}
