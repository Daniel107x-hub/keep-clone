using backend.Models;
using Microsoft.EntityFrameworkCore;
public class KeepContext : DbContext {
    public DbSet<User> Users { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<Reminder> Reminders { get; set; }

    public string DbPath { get; }

    public KeepContext(){
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "keep.db");
    }


    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={DbPath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany(user => user.Notes)
            .WithOne(note => note.User)
            .HasForeignKey(note => note.UserId)
            .HasPrincipalKey(user => user.Id);
    }

}