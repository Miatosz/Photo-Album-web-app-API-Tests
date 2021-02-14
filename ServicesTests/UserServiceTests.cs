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
    public class UserServiceTests
    {
        Mock<IUserRepo> _userRepo;
        Mock<IPhotoRepo> _photoRepo;
        Mock<IAlbumRepo> _albumRepo;
        List<User> _users;
        List<Album> _albums;
        List<Photo> _photos;
        UserService _userService;
    
        [SetUp]
        public void Setup()
        {
            _albums = new List<Album>
            {
                new Album
                {
                    UserId = "1"
                },
                new Album
                {
                    UserId = "1"
                },
                new Album
                {
                    UserId = "2"
                }
            };

            _photos = new List<Photo>
            {
                new Photo
                {
                    Description = "1 photo",
                    Album = new Album { UserId = "1" }
                },
                new Photo
                {
                    Description = "2 photo",
                    Album = new Album { UserId = "1" }
                },
                new Photo
                {
                    Description = "3 photo",
                    Album = new Album { UserId = "2" }
                }
            };

            _users = new List<User>
            {
                new User
                {
                    Id = "1",
                    UserName = "user 1"
                },
                new User
                {
                    Id = "2",
                    UserName = "user 2"
                }
            };
        }

        [Test]
        public void GetUsers_ShouldReturnAllUsers()
        {
            _photoRepo = new Mock<IPhotoRepo>();
            _albumRepo = new Mock<IAlbumRepo>();
            _userRepo = new Mock<IUserRepo>();
            _userRepo.Setup(c => c.Users).Returns(_users);
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ImageAlbumDb")
                .Options;          

            using (var context = new AppDbContext(options))
            {   
                _userService = new UserService(context, _userRepo.Object, _photoRepo.Object, _albumRepo.Object);

                var result = _userService.GetUsers();

                Assert.AreEqual(2, result.Count());
                Assert.AreEqual("user 1", result.First(c => c.Id == "1").UserName);
            }
        }

        [Test]
        public void GetUserById_ShouldReturnUserById()
        {
            _photoRepo = new Mock<IPhotoRepo>();
            _albumRepo = new Mock<IAlbumRepo>();
            _userRepo = new Mock<IUserRepo>();
            _userRepo.Setup(c => c.Users).Returns(_users);
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ImageAlbumDb")
                .Options;          

            using (var context = new AppDbContext(options))
            {   
                _userService = new UserService(context, _userRepo.Object, _photoRepo.Object, _albumRepo.Object);

                var result = _userService.GetUserById("1");

                Assert.AreEqual("user 1", result.UserName);
            }
        }

        [Test]
        public void GetUserById_ShouldReturnNullIfUserNotExists()
        {
            _photoRepo = new Mock<IPhotoRepo>();
            _albumRepo = new Mock<IAlbumRepo>();
            _userRepo = new Mock<IUserRepo>();
            _userRepo.Setup(c => c.Users).Returns(_users);
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ImageAlbumDb")
                .Options;            

            using (var context = new AppDbContext(options))
            {   
                _userService = new UserService(context, _userRepo.Object, _photoRepo.Object, _albumRepo.Object);

                var result = _userService.GetUserById("3");

                Assert.IsNull(result);
            }
        }

        [Test]
        public void GetUserAlbums_ShouldReturnAllUserAlbums()
        {
            _photoRepo = new Mock<IPhotoRepo>();
            _albumRepo = new Mock<IAlbumRepo>();
            _userRepo = new Mock<IUserRepo>();
            _albumRepo.Setup(c => c.Albums).Returns(_albums);
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ImageAlbumDb")
                .Options;            

            using (var context = new AppDbContext(options))
            {   
                _userService = new UserService(context, _userRepo.Object, _photoRepo.Object, _albumRepo.Object);

                var result = _userService.GetUserAlbums("1");

                Assert.AreEqual(2, result.Count());
            }
        }

        [Test]
        public void GetUserPhotos_ShouldReturnAllUserPhotos()
        {
            _photoRepo = new Mock<IPhotoRepo>();
            _albumRepo = new Mock<IAlbumRepo>();
            _userRepo = new Mock<IUserRepo>();
            _photoRepo.Setup(c => c.Photos).Returns(_photos);
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ImageAlbumDb")
                .Options;        

            using (var context = new AppDbContext(options))
            {   
                _userService = new UserService(context, _userRepo.Object, _photoRepo.Object, _albumRepo.Object);

                var result = _userService.GetUserPhotos("1");

                Assert.AreEqual(2, result.Count());
            }
        }



    }
}