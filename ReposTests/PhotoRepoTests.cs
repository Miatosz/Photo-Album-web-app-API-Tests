using System.Collections.Generic;
using System.Linq;
using ImageAlbumAPI.Data;
using ImageAlbumAPI.Models;
using ImageAlbumAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace ImageAlbumAPITests.ReposTests
{
    public class PhotoRepoTests
    {
        List<Photo> _photos;
        PhotoRepo _photoRepo;

        [SetUp]
        public void Setup()
        {
            _photos = new List<Photo>
            {
                new Photo
                {
                    Id = 1,
                    Comments = new List<Comment>()
                },
                new Photo
                {
                    Id = 2 
                }
            };
        }

        [Test]
        public void Photos_ShouldReturnAllPhotos()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ImageAlbumDb")
                .Options;  
            using (var context = new AppDbContext(options))
            {
                context.Photos.AddRange(_photos.ToList());
                context.SaveChanges();
            }          

            using (var context = new AppDbContext(options))
            {   
                _photoRepo = new PhotoRepo(context);

                var result = _photoRepo.Photos;

                Assert.AreEqual(2, result.Count());
            }
        }

        [Test]
        public void AddPhoto_IfUserIdIsZeroShouldAddPhoto()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ImageAlbumDb")
                .Options;  
            using (var context = new AppDbContext(options))
            {
                context.Photos.AddRange(_photos.ToList());
                context.SaveChanges();
            }          

            using (var context = new AppDbContext(options))
            {   
                _photoRepo = new PhotoRepo(context);
                var photo = new Photo() { Id = 0 };

                _photoRepo.AddPhoto(photo);

                Assert.AreEqual(3, context.Photos.Count());
            }
        }

        [Test]
        public void DeletePhoto_IfPhotoExistsShouldDeletePhoto()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ImageAlbumDb")
                .Options;  
            using (var context = new AppDbContext(options))
            {
                context.Photos.AddRange(_photos.ToList());
                context.SaveChanges();
            }          

            using (var context = new AppDbContext(options))
            {   
                _photoRepo = new PhotoRepo(context);

                _photoRepo.DeletePhoto(2);

                Assert.AreEqual(1, context.Photos.Count());
            }
        }

        [Test]
        public void UpdatePhoto_IfPhotoExistsShouldUpdatePhoto()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ImageAlbumDb")
                .Options;  
            using (var context = new AppDbContext(options))
            {
                context.Photos.AddRange(_photos.ToList());
                context.SaveChanges();
            }          

            using (var context = new AppDbContext(options))
            {   
                _photoRepo = new PhotoRepo(context);

                _photoRepo.UpdatePhoto(new Photo() { Id = 1, Description = "changed"} );

                Assert.AreEqual("changed", context.Photos.First(c => c.Id == 1).Description);
            }
        }

        [Test]
        public void UpdateComments_IfPhotoExistsShouldUpdateComments()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ImageAlbumDb")
                .Options;  
            using (var context = new AppDbContext(options))
            {
                context.Photos.AddRange(_photos.ToList());
                context.SaveChanges();
            }          

            using (var context = new AppDbContext(options))
            {   
                _photoRepo = new PhotoRepo(context);

                _photoRepo.UpdateComments(new Photo() { Id = 1, Comments = new List<Comment>() { new Comment { Content = "comment" }}} );

                Assert.AreEqual(1, context.Photos.First(c => c.Id == 1).Comments.Count());
                Assert.AreEqual("comment", context.Photos.First(c => c.Id == 1).Comments.First().Content);
            }
        }
    }
}