﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Storage;
using BioEngine.Core.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.API.Controllers
{
    public class PostsController : ContentController<Post, int, Entities.Post, Entities.PostRequestItem>
    {
        private readonly IEnumerable<EntityMetadata> _entityMetadataList;
        private readonly BioContext _dbContext;

        public PostsController(BaseControllerContext<Post, int> context, IEnumerable<EntityMetadata> entityMetadataList,
            BioContext dbContext) : base(context)
        {
            _entityMetadataList = entityMetadataList;
            _dbContext = dbContext;
        }

        private PostBlock CreateBlock(string type)
        {
            var blockType = _entityMetadataList.Where(entityMetadata =>
                    entityMetadata.EntityType.FullName == type &&
                    typeof(PostBlock).IsAssignableFrom(entityMetadata.EntityType))
                .Select(e => e.EntityType).FirstOrDefault();
            if (blockType != null)
            {
                return Activator.CreateInstance(blockType) as PostBlock;
            }

            return null;
        }

        protected override async Task<Post> MapDomainModelAsync(Entities.PostRequestItem restModel,
            Post domainModel = null)
        {
            domainModel = await base.MapDomainModelAsync(restModel, domainModel);
            if (domainModel.AuthorId == 0)
            {
                domainModel.AuthorId = CurrentUser.Id;
            }

            domainModel.Blocks = new List<PostBlock>();
            foreach (var contentBlock in restModel.Blocks)
            {
                PostBlock block;
                if (contentBlock.Id > 0)
                {
                    block = await _dbContext.Blocks.FirstOrDefaultAsync(b =>
                        b.Id == contentBlock.Id && b.Post == domainModel);
                }
                else
                {
                    block = CreateBlock(contentBlock.Type);
                }

                if (block != null)
                {
                    block.Id = contentBlock.Id;
                    block.Post = domainModel;
                    block.Position = contentBlock.Position;
                    block.SetData(contentBlock.Data);
                    domainModel.Blocks.Add(block);
                }
            }

            return domainModel;
        }

        public override async Task<ActionResult<StorageItem>> UploadAsync(string name)
        {
            using (var ms = new MemoryStream())
            {
                await Request.Body.CopyToAsync(ms);
                return await Storage.SaveFileAsync(ms.GetBuffer(), name,
                    $"posts/{DateTimeOffset.UtcNow.Year}/{DateTimeOffset.UtcNow.Month}");
            }
        }
    }
}