using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using ImageAlbumAPI.Controllers;
using ImageAlbumAPI.Dtos.GetDtos;
using ImageAlbumAPI.Models;
using ImageAlbumAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace ImageAlbumAPITests.ControllersTests
{
    public class AlbumControllerTests
    {
        Mock<FakeUserManager> _userManager;
        IMapper _mapper;
        Mock<IAlbumService> _albumService;
        List<Album> _albums;

        
        [SetUp]
        public void Setup()
        {
            var config = new MapperConfiguration(cfg => 
            { 
                cfg.CreateMap<Photo, GetPhotoDto>();
                cfg.CreateMap<GetPhotoDto, Photo>();
                cfg.CreateMap<GetAlbumDto, Album>();
                cfg.CreateMap<Album, GetAlbumDto>();
            });
            _mapper = config.CreateMapper(); 

            _userManager = new FakeUserManagerBuilder()
                .Build();

        _albums = new List<Album>
        {
            new Album
            {
                Id = 1,
                User = new User { Id = "1", UserName = "test1"},
                UserId = "1"
            },
            new Album
            {
                Id = 2,
                User = new User { Id = "1", UserName = "test2"},
                UserId = "1"
            }
        };


        }

        [Test]
        public void Get_ShouldReturnOkResult()
        {
            _albumService = new Mock<IAlbumService>();
            _albumService.Setup(a => a.Albums).Returns(_albums);
            var controller = new AlbumController(_albumService.Object, _mapper, _userManager.Object);

            var result = controller.Get().Result;

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public void Get_ShouldReturnAlbumsDtos()
        {
            _albumService = new Mock<IAlbumService>();
            _albumService.Setup(a => a.GetAlbums()).Returns(_albums);
            _albumService.Setup(a => a.GetAlbumById(It.IsAny<int>())).Returns(_albums[1]);
            var controller = new AlbumController(_albumService.Object, _mapper, _userManager.Object);

            var result = controller.Get().Result as OkObjectResult;
            var albums = result.Value as IEnumerable<GetAlbumDto>;

            Assert.IsInstanceOf<IEnumerable<GetAlbumDto>>(result.Value);
            Assert.AreEqual(_albums.Count(), albums.Count()); 
        }

        [Test]
        public void GetById_IfNotNull_ShouldReturnAlbumDto()
        {
            int id = 1;
            _albumService = new Mock<IAlbumService>();
            _albumService.Setup(a => a.GetAlbumById(id)).Returns(_albums.ElementAt(id));
            var controller = new AlbumController(_albumService.Object, _mapper, _userManager.Object);

            var result = controller.GetById(id).Result as OkObjectResult;
            var album = result.Value as GetAlbumDto;
        
            Assert.IsInstanceOf<GetAlbumDto>(result.Value);
            Assert.AreEqual(album.Id, _albums.ElementAt(id).Id);
        }

        [Test]
        public void GetById_IfNotNull_ShouldReturnOkResult()
        {
            int id = 1;
            _albumService = new Mock<IAlbumService>();
            _albumService.Setup(a => a.GetAlbumById(id)).Returns(_albums.ElementAt(id));
            var controller = new AlbumController(_albumService.Object, _mapper, _userManager.Object);

            var result = controller.GetById(id);

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
        }

        [Test]
        public void GetById_IfNull_ShouldReturnNotFoundResult()
        {
            int id = 1;
            _albumService = new Mock<IAlbumService>();
            var controller = new AlbumController(_albumService.Object, _mapper, _userManager.Object);

            var result = controller.GetById(id);

            Assert.IsInstanceOf<NotFoundResult>(result.Result);
        }

        [Test]
        public void PostAlbum_ShouldAddAlbum()
        {
            var newAlbum = new Album {Id = 3, Name = "testPost", User = new User  {Id = "1", UserName = "test1" }, UserId = "1", Photos = new List<Photo>()};
            _albums.Add(newAlbum);
            _albumService = new Mock<IAlbumService>();
            _albumService.Setup(a => a.Albums).Returns(_albums);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "test1"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("custom-claim", "example claim value"),
            }, "mock"));
            _userManager.Setup(u => u.FindByNameAsync("test1")).Returns(Task.FromResult(new User  {Id = "1", UserName = "test1" }));
            var controller = new AlbumController(_albumService.Object, _mapper, _userManager.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };
            

            var result = controller.PostAlbum(newAlbum).Result as OkObjectResult;
            var album = result.Value as Album;
            

            Assert.AreEqual(_albums.ElementAt(2).Id, album.Id);
            _albums.Remove(newAlbum); 
        }

        [Test]
        public void DeleteAlbum_IfUserAlbumsContaintsAlbum_ShouldDeleteAlbum()
        {
             var removeAlbum = new Album {Id = 3, Name = "testPost", User = new User  {Id = "1", UserName = "test1" }, UserId = "1", Photos = new List<Photo>()};
            _albums.Add(removeAlbum);
            _albumService = new Mock<IAlbumService>();
            _albumService.Setup(a => a.Albums).Returns(_albums);
            _albumService.Setup(a => a.GetAlbumById(3)).Returns(removeAlbum);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "test1"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("custom-claim", "example claim value"),}, "mock"));
            
            _userManager.Setup(u => u.FindByNameAsync("test1")).Returns(Task.FromResult(new User {Id = "1", UserName = "test1", Albums = new List<Album>() {removeAlbum}}));
            var controller = new AlbumController(_albumService.Object, _mapper, _userManager.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };

            controller.DeleteAlbum(removeAlbum.Id);
            
            _albumService.Verify(c => c.DeleteAlbum(3), Times.Once());
        }

        [Test]
        public void GetCurrentLoggedUser_ShouldReturnCurrentLoggedUser()
        {
            var mockService = new Mock<IPhotoService>();
            var fakeUserManager = new FakeUserManagerBuilder()
                .Build();          
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "test1"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("custom-claim", "example claim value"),
            }, "mock"));
            fakeUserManager.Setup(u => u.FindByNameAsync("test1")).Returns(Task.FromResult(new User()));
            var controller = new PhotoController(mockService.Object, _mapper, fakeUserManager.Object);      
            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };
            controller.ControllerContext = context;

            var result = controller.GetCurrentLoggedUser();

            Assert.IsInstanceOf<User>(result.Result);
        }
    }
}