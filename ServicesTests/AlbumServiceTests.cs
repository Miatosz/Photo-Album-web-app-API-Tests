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
    public class AlbumServiceTests
    {
        Mock<IAlbumRepo> _albumRepo;
        Mock<IPhotoRepo> _photoRepo;
        AlbumService _albumService;
        List<Album> _albums;
        List<Photo> _photos;

        [SetUp]
        public void Setup()
        {
            _albums = new List<Album>
            {
                new Album
                {
                    Id = 1,
                    Description = "1 album"
                },
                new Album
                {
                    Id = 2,
                    Description = "2 album"
                }
            };
            _photos = new List<Photo>
            {
                new Photo
                {
                    Id = 1,
                    AlbumId = 1
                },
                new Photo
                {
                    Id = 2,
                    AlbumId = 1
                }
            };
        }

        [Test]
        public void GetAlbumById_ShouldReturnAlbumById()
        {
            _photoRepo = new Mock<IPhotoRepo>();
            _albumRepo = new Mock<IAlbumRepo>();
            _albumRepo.Setup(c => c.Albums).Returns(_albums);
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ImageAlbumDb")
                .Options;          

            using (var context = new AppDbContext(options))
            {   
               _albumService = new AlbumService(context, _albumRepo.Object, _photoRepo.Object);

               var result = _albumService.GetAlbumById(1);

                Assert.AreEqual(_albums[0].Description, result.Description);
            }
        }

        [Test]
        public void GetAlbumPhotos_ShouldReturnAllPhotosFromAlbum()
        {
            _photoRepo = new Mock<IPhotoRepo>();
            _albumRepo = new Mock<IAlbumRepo>();
            _photoRepo.Setup(c => c.Photos).Returns(_photos);
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ImageAlbumDb")
                .Options;            

            using (var context = new AppDbContext(options))
            {   
               _albumService = new AlbumService(context, _albumRepo.Object, _photoRepo.Object);

               var result = _albumService.GetAlbumPhotos(1);

                Assert.AreEqual(2, result.Count());
            }
        }

        [Test]
        public void GetAlbums_ShouldReturnAllAlbums()
        {
            _photoRepo = new Mock<IPhotoRepo>();
            _albumRepo = new Mock<IAlbumRepo>();
            _albumRepo.Setup(c => c.Albums).Returns(_albums);
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ImageAlbumDb")
                .Options;           

            using (var context = new AppDbContext(options))
            {   
                _albumService = new AlbumService(context, _albumRepo.Object, _photoRepo.Object);

                var result = _albumService.GetAlbums();

                Assert.AreEqual(2, result.Count());
            }
        }
    }
}