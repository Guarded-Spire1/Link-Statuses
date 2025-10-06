using LinkStatusDb.Models;
using Microsoft.EntityFrameworkCore;

namespace Database.Configurations
{
    public class LinkConfiguration : IEntityTypeConfiguration<LinkEntity>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<LinkEntity> builder)
        {
            builder.HasKey(l => l.Id);

            builder.HasOne(l => l.User)
                   .WithMany(u => u.Link);

            builder.HasMany(l => l.LinkStatus)
                   .WithOne(ls => ls.Link);
        }
    }
}
