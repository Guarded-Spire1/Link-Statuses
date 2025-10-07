using LinkStatusDb.Models;
using Microsoft.EntityFrameworkCore;

namespace LinkStatusDb.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<UserEntity> builder)
        {
            builder.HasKey(u => u.Id);

            builder.HasMany(u => u.Link)
                   .WithOne(l => l.User);
        }
    }
}
