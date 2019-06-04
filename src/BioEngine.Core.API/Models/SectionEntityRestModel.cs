using System;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;

namespace BioEngine.Core.API.Models
{
    public abstract class SectionEntityRestModel<TEntity> : ContentEntityRestModel<TEntity>
        where TEntity : class, ISectionEntity, IContentEntity
    {
        public Guid[] SectionIds { get; set; }
        public Guid[] TagIds { get; set; }

        protected override async Task ParseEntityAsync(TEntity entity)
        {
            await base.ParseEntityAsync(entity);
            SectionIds = entity.SectionIds;
            TagIds = entity.TagIds;
        }

        protected override async Task<TEntity> FillEntityAsync(TEntity entity)
        {
            entity = await base.FillEntityAsync(entity);
            entity.SectionIds = SectionIds;
            entity.TagIds = TagIds;
            return entity;
        }
    }
}
