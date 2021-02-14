using System.Collections.Generic;
using System.Linq;
using ImageAlbumAPI.Data;
using ImageAlbumAPI.Models;
using ImageAlbumAPI.Repositories;
using ImageAlbumAPI.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace ImageAlbumAPITests.ServicesTests
{
    public class PhotoServiceTests
    {
        Mock<IUserRepo> _userRepo;
        Mock<IPhotoRepo> _photoRepo;
        PhotoService _photoService;
        List<User> _users;

        [SetUp]
        public void Setup()
        {
            _users = new List<User>
            {
                new User
                {
                    Id = "1",
                    UserName = "test1"
                },
                new User
                {
                    Id = "2",
                    UserName = "test2"
                }
            };
        }

        [Test]
        public void LikePhoto_ShouldAddLikeToPhotoAndCallUpdatePhoto()
        {
            _photoRepo = new Mock<IPhotoRepo>();
            _userRepo = new Mock<IUserRepo>();
            _userRepo.Setup(c => c.Users).Returns(_users);
            var photo = new Photo() { Id = 1, NumberOfLikes = 0, Likes = new List<Like>()};
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ImageAlbumDb")
                .Options;            

            using (var context = new AppDbContext(options))
            {   
               _photoService = new PhotoService(context, _photoRepo.Object, _userRepo.Object);

               _photoService.LikePhoto(photo, "1");

                Assert.AreEqual(1, photo.NumberOfLikes);
               _photoRepo.Verify(c => c.UpdatePhoto(photo), Times.Once());
            }
        }

        [Test]
        public void AddComment_ShouldAddCommentToPhotoAndCallUpdatePhoto()
        {
            _photoRepo = new Mock<IPhotoRepo>();
            _userRepo = new Mock<IUserRepo>();
            _userRepo.Setup(c => c.Users).Returns(_users);
            var photo = new Photo() { Id = 1};
            var comment = new Comment() { Id = 1, UserId = "1" };
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ImageAlbumDb")
                .Options;            

            using (var context = new AppDbContext(options))
            {   
               _photoService = new PhotoService(context, _photoRepo.Object, _userRepo.Object);

               _photoService.AddComment(photo, comment);

                Assert.AreEqual(1, photo.Comments.Count());
               _photoRepo.Verify(c => c.UpdatePhoto(photo), Times.Once());
            }
        }
        
        [Test]
        public void RemoveComment_ShouldRemoveCommentFromPhotoAndCallUpdatePhoto()
        {
            _photoRepo = new Mock<IPhotoRepo>();
            _userRepo = new Mock<IUserRepo>();
            _userRepo.Setup(c => c.Users).Returns(_users);
            var comment = new Comment() { Id = 1, UserId = "1" };
            var photo = new Photo() { Id = 1, Comments = new List<Comment>() {comment}};            
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ImageAlbumDb")
                .Options;         

            using (var context = new AppDbContext(options))
            {   
               _photoService = new PhotoService(context, _photoRepo.Object, _userRepo.Object);

               _photoService.RemoveComment(photo, comment);

                Assert.AreEqual(0, photo.Comments.Count());
               _photoRepo.Verify(c => c.UpdatePhoto(photo), Times.Once());
            }
        }

        
    }  
}