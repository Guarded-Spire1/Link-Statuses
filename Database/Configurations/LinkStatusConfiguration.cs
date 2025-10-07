using LinkStatusDb.Models;
using Microsoft.EntityFrameworkCore;

namespace LinkStatusDb.Configurations
{
    public class LinkStatusConfiguration : IEntityTypeConfiguration<LinkStatusEntity>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<LinkStatusEntity> builder)
        {
            builder.HasKey(ls => ls.Id);

            builder.HasOne(ls => ls.Link)
                   .WithMany(l => l.LinkStatus)
                   .HasForeignKey(ls => ls.LinkId);
        }
    }
}
