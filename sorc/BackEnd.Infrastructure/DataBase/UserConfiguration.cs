using BackEnd.Domin.ValueObjects.ValueObjectsUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace BackEnd.Infrastructure.DataBase
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.Role).HasConversion<string>();

            builder.OwnsOne(u => u.Email, email =>
            {
                email.Property(e => e.Value)
                    .HasColumnName("Email")
                    .HasMaxLength(254)
                    .IsRequired();

                email.HasIndex(e => e.Value).IsUnique();
            });

            builder.Property(u => u.Name)
                .HasConversion(name => name.Value, value => new UserName(value));

            builder.Property(u => u.Phone)
                .HasConversion(phone => phone.Value, value => new PhoneNumber(value));

            builder.OwnsOne(u => u.Profile, profile =>
            {
                profile.Property(p => p.Url).HasColumnName("ProfileUrl");
                profile.Property(p => p.PublicId).HasColumnName("ProfilePublicId");
            });

            builder.OwnsOne(u => u.Password, password =>
            {
                password.Property(p => p.Hash).HasColumnName("PasswordHash");
                password.Property(p => p.Salt).HasColumnName("PasswordSalt");
            });
        }
    }
}
