using System;
using System.Collections.Generic;
using System.Linq;
using ImageAlbumAPI.Data;
using ImageAlbumAPI.Models;
using ImageAlbumAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace ImageAlbumAPITests.ReposTests
{
    public class AlbumRepoTests
    {
        List<Album> _albums;
        AlbumRepo _albumRepo;
        Random random = new Random();

        [SetUp]
        public void Setup()
        {
            _albums = new List<Album>
            {
                new Album
                {
                    Id = 1
                },
                new Album
                {
                    Id = 2 
                }
            };
        }

        [Test]
        public void Albums_ShouldReturnAllAlbums()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: random.Next().ToString())
                .Options;  
            using (var context = new AppDbContext(options))
            {
                context.Albums.AddRange(_albums.ToList());
                context.SaveChanges();
            }          

            using (var context = new AppDbContext(options))
            {   
                _albumRepo = new AlbumRepo(context);

                var result = _albumRepo.Albums;

                Assert.AreEqual(2, result.Count());
            }
        }

        [Test]
        public void AddAlbum_IfAlbumIdIsNotZeroShouldAddAlbum()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: random.Next().ToString())
                .Options;  
            using (var context = new AppDbContext(options))
            {
                context.Albums.AddRange(_albums.ToList());
                context.SaveChanges();
            }          

            using (var context = new AppDbContext(options))
            {   
                _albumRepo = new AlbumRepo(context);
                var album = new Album() { Id = 0 };

                _albumRepo.AddAlbum(album);

                Assert.AreEqual(3, context.Albums.Count());
            }
        }

        [Test]
        public void DeleteAlbum_IfAlbumExistsShouldDeleteAlbum()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: random.Next().ToString())
                .Options;  
            using (var context = new AppDbContext(options))
            {
                context.Albums.AddRange(_albums.ToList());
                context.SaveChanges();
            }          

            using (var context = new AppDbContext(options))
            {   
                _albumRepo = new AlbumRepo(context);

                _albumRepo.DeleteAlbum(2);

                Assert.AreEqual(1, context.Albums.Count());
            }
        }

        [Test]
        public void UpdateAlbum_IfAlbumExistsShouldUpdateAlbum()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: random.Next().ToString())
                .Options;  
            using (var context = new AppDbContext(options))
            {
                context.Albums.AddRange(_albums.ToList());
                context.SaveChanges();
            }          

            using (var context = new AppDbContext(options))
            {   
                _albumRepo = new AlbumRepo(context);

                _albumRepo.UpdateAlbum(new Album() { Id = 1, Description = "changed"} );

                Assert.AreEqual("changed", context.Albums.First(c => c.Id == 1).Description);
            }
        }

        
    }
}