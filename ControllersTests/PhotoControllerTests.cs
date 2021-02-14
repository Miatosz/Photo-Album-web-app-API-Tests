using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using ImageAlbumAPI.Controllers;
using ImageAlbumAPI.Data;
using ImageAlbumAPI.Dtos.GetDtos;
using ImageAlbumAPI.Models;
using ImageAlbumAPI.Repositories;
using ImageAlbumAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace ImageAlbumAPITests.ControllersTests
{
    public class PhotoControllerTests
    {
        PhotoController _controller;
        Mock<IPhotoService> _photoService;
        Mock<FakeUserManager> _userManager;
        IMapper _mapper;
        List<Photo> Photos;
        List<User> Users;


        [SetUp]
        public void Setup()
        {
            _userManager = new FakeUserManagerBuilder()
                .Build();

            var config = new MapperConfiguration(cfg => 
            { 
                cfg.CreateMap<Photo, GetPhotoDto>();
                cfg.CreateMap<GetPhotoDto, Photo>();
            });
            _mapper = config.CreateMapper(); 

            Photos = new List<Photo>
            {
                new Photo
                {
                    Id = 1,
                    Likes = new List<Like>()
                },
                new Photo
                {
                    Id = 2,
                    Likes = new List<Like>()
                },
                new Photo
                {
                    Id = 3,
                    Likes = new List<Like>()        
                }
            };

            Users = new List<User>
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
        public void Get_WhenCalled_ReturnsOkResult()
        {            
            var mockService = new Mock<IPhotoService>();
            mockService.Setup(service => service.Photos).Returns(Photos);
            var controller = new PhotoController(mockService.Object, _mapper, _userManager.Object);

            var result = controller.Get();

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
        }

        [Test]
        public void Get_WhenCalled_ReturnsAllPhotos()
        {           
            var mockService = new Mock<IPhotoService>();
            mockService.Setup(service => service.Photos).Returns(Photos);
            var controller = new PhotoController(mockService.Object, _mapper, _userManager.Object); 

            var okResult = controller.Get().Result as OkObjectResult;
            var items = okResult.Value as IEnumerable<GetPhotoDto>;

            Assert.IsInstanceOf<IEnumerable<GetPhotoDto>>(okResult.Value);
            Assert.AreEqual(3, items.Count());
        }

        [Test]
        public void GetById_WhenCalled_ReturnsOkResult()
        {
            var id = 1;
            var mockService = new Mock<IPhotoService>();
            mockService.Setup(service => service.GetPhotoById(id)).Returns(Photos.FirstOrDefault(c => c.Id == id));
            var controller = new PhotoController(mockService.Object, _mapper, _userManager.Object);             

            var result = controller.GetById(id);

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
        }

        [Test]
        public void GetById_WhenCalled_ReturnsPhotoById()
        {
            var id = 1;
            var mockService = new Mock<IPhotoService>();
            mockService.Setup(service => service.GetPhotoById(id)).Returns(Photos.FirstOrDefault(c => c.Id == id));            
            var controller = new PhotoController(mockService.Object, _mapper, _userManager.Object);             

            var result = controller.GetById(id).Result as OkObjectResult;
            var item = result.Value as GetPhotoDto;

            Assert.AreEqual(id, item.Id);
            Assert.IsInstanceOf<GetPhotoDto>(result.Value);
        }

        [Test]
        public void PostPhoto_WhenCalled_AddPhotoToRepo()
        {
            var mockService = new Mock<IPhotoService>();
            mockService.Setup(service => service.Photos).Returns(Photos);            
            var controller = new PhotoController(mockService.Object, _mapper, _userManager.Object); 
            var newPhoto = new Photo
            {
                Id = 4,
                Description = "test"
            };
            Photos.Add(newPhoto);

            var result = controller.PostPhoto(newPhoto).Result as OkObjectResult;
            var photo = result.Value as Photo;

            Assert.IsInstanceOf<Photo>(result.Value);
            Assert.AreEqual(Photos.FirstOrDefault(c => c.Id == 4).Id, photo.Id);
            Photos.Remove(newPhoto);
        }

        [Test]
        public void PostPhoto_WhenCalled_WhenPhotoAdded_ReturnOkResult()
        {
            var mockService = new Mock<IPhotoService>();
            mockService.Setup(service => service.Photos).Returns(Photos);            
            var controller = new PhotoController(mockService.Object, _mapper, _userManager.Object);             
            
            var result = controller.PostPhoto(new Photo());

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
        }

        [Test]
        public void PostPhoto_WhenCalled_WhenPhotoModelIsNotValid_ReturnBadRequest()
        {
            var mockService = new Mock<IPhotoService>();
            mockService.Setup(service => service.Photos).Returns(Photos);            
            var controller = new PhotoController(mockService.Object, _mapper, _userManager.Object); 
            var newPhoto = new Photo(){ DateOfAdd = null };

            controller.ModelState.AddModelError("Photo can't be null", "null photo");
            var result = controller.PostPhoto(newPhoto);

            Assert.IsInstanceOf<BadRequestResult>(result.Result);
        }

        [Test]
        public void LikePhoto_WhenCalled_ShouldCallLikePhotoAndReturnOkResult()
        {
            int photoId = 1;
            var mockService = new Mock<IPhotoService>();
            mockService.Setup(service => service.GetPhotoById(photoId)).Returns(Photos[photoId]);        
            var fakeUserManager = new FakeUserManagerBuilder()
                .Build();            
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "test1"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("custom-claim", "example claim value"),
            }, "mock"));
            fakeUserManager.Setup(u => u.FindByNameAsync("test1")).Returns(Task.FromResult(new User  {Id = "1", UserName = "test1" }));
            var controller = new PhotoController(mockService.Object, _mapper, fakeUserManager.Object);            
            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };
            controller.ControllerContext = context;
            
            var result = controller.LikePhoto(photoId);

            mockService.Verify(c => c.LikePhoto(It.IsAny<Photo>(), It.IsAny<string>()), Times.Once());
            Assert.IsInstanceOf<OkResult>(result);
        }

        [Test]
        public void LikePhoto_WhenCalled_IfUserAllreadyLikedPhoto_ShouldReturnBadRequestResult()
        {
            int photoId = 1;
            Photos[photoId].NumberOfLikes = 1;
            Photos[photoId].Likes = new List<Like>{ { new Like {Id = 1, UserId = "1"} } };
            var mockService = new Mock<IPhotoService>();
            mockService.Setup(service => service.GetPhotoById(photoId)).Returns(Photos[photoId]);        
            var fakeUserManager = new FakeUserManagerBuilder()
                .Build();            
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "test1"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("custom-claim", "example claim value"),
            }, "mock"));
            fakeUserManager.Setup(u => u.FindByNameAsync("test1")).Returns(Task.FromResult(new User  {Id = "1", UserName = "test1", Likes = new List<Like> { new Like { Id = 1 } }}));
            var controller = new PhotoController(mockService.Object, _mapper, fakeUserManager.Object);            
            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };
            controller.ControllerContext = context;

            
            var result = controller.LikePhoto(photoId);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public void LikePhoto_WhenCalled_IfPhotoIsNull_ShouldReturnNotFoundResult()
        {
            int photoId = 1;
            var mockService = new Mock<IPhotoService>();
            var controller = new PhotoController(mockService.Object, _mapper, _userManager.Object);  
            
            var result = controller.LikePhoto(photoId);

            Assert.IsInstanceOf<NotFoundResult>(result);       
        }

        [Test]
        public void UnlikePhoto_WhenCalled_IfPhotoIsNull_ShouldReturnNotFoundResult()
        {
            int photoId = 1;
            var mockService = new Mock<IPhotoService>();
            var controller = new PhotoController(mockService.Object, _mapper, _userManager.Object); 

            var result = controller.UnlikePhoto(photoId);

            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public void UnlikePhoto_WhenCalled_IfUserDidNotLikedPhoto_ShouldReturnBadRequestResult()
        {
            int photoId = 1;
            var mockService = new Mock<IPhotoService>();
            mockService.Setup(service => service.GetPhotoById(photoId)).Returns(Photos[photoId]);        
            var fakeUserManager = new FakeUserManagerBuilder()
                .Build();            
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "test1"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("custom-claim", "example claim value"),
            }, "mock"));
            fakeUserManager.Setup(u => u.FindByNameAsync("test1")).Returns(Task.FromResult(new User  {Id = "1", UserName = "test1"}));
            var controller = new PhotoController(mockService.Object, _mapper, fakeUserManager.Object);            
            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };
            controller.ControllerContext = context;

            
            var result = controller.UnlikePhoto(photoId);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public void UnlikePhoto_WhenCalled_ShouldCallUnLikePhotoAndReturnOkResult()
        {
            int photoId = 1;
            var mockService = new Mock<IPhotoService>();
            Photos[photoId].Likes.Add(new Like { Id = 1, UserId = "1"});
            Photos[photoId].NumberOfLikes = 1;
            mockService.Setup(service => service.GetPhotoById(photoId)).Returns(Photos[photoId]);          
            var fakeUserManager = new FakeUserManagerBuilder()
                .Build();            
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "test1"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("custom-claim", "example claim value"),
            }, "mock"));
            fakeUserManager.Setup(u => u.FindByNameAsync("test1")).Returns(Task.FromResult(new User  {Id = "1", UserName = "test1" }));
            var controller = new PhotoController(mockService.Object, _mapper, fakeUserManager.Object);            
            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };
            controller.ControllerContext = context;

            
            var result = controller.UnlikePhoto(photoId);


            mockService.Verify(service => service.UnlikePhoto(It.IsAny<Photo>(), It.IsAny<string>()), Times.Once());
            Assert.IsInstanceOf<OkResult>(result);
        } 

        [Test]
        public void AddComment_ShouldCallAddCommentAndReturnOkResult()
        {
            int photoId = 1;
            var mockService = new Mock<IPhotoService>();
            mockService.Setup(service => service.GetPhotoById(photoId)).Returns(Photos[photoId]); 
            var fakeUserManager = new FakeUserManagerBuilder()
                .Build();            
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "test1"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("custom-claim", "example claim value"),
            }, "mock"));
            fakeUserManager.Setup(u => u.FindByNameAsync("test1")).Returns(Task.FromResult(new User  {Id = "1", UserName = "test1" }));
            var controller = new PhotoController(mockService.Object, _mapper, fakeUserManager.Object);            
            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };
            controller.ControllerContext = context;

            
            var result = controller.AddComment(photoId, new Comment { Id = 1 });


            mockService.Verify(service => service.AddComment(It.IsAny<Photo>(), It.IsAny<Comment>()), Times.Once());
            Assert.IsInstanceOf<OkResult>(result);
        }   

        [Test]
        public void AddComment_IfPhotoIsNull_ShouldReturnNotFoundResult()
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
            fakeUserManager.Setup(u => u.FindByNameAsync("test1")).Returns(Task.FromResult(new User  {Id = "1", UserName = "test1" }));
            var controller = new PhotoController(mockService.Object, _mapper, fakeUserManager.Object);            
            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };
            controller.ControllerContext = context;
            
            var result = controller.AddComment(1, new Comment { Id = 1 });

            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public void RemoveComment_ShouldCallRemoveCommentAndReturnOkResult()
        {
            int photoId = 1;
            Photos[photoId].Comments = new List<Comment> { { new Comment { Id = 1, UserId = "1"} } };
            var mockService = new Mock<IPhotoService>();
            mockService.Setup(service => service.GetPhotoById(photoId)).Returns(Photos[photoId]); 
            var fakeUserManager = new FakeUserManagerBuilder()
                .Build();            
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "test1"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("custom-claim", "example claim value"),
            }, "mock"));
            fakeUserManager.Setup(u => u.FindByNameAsync("test1")).Returns(Task.FromResult(new User  {Id = "1", UserName = "test1" }));
            var controller = new PhotoController(mockService.Object, _mapper, fakeUserManager.Object);            
            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };
            controller.ControllerContext = context;

            
            var result = controller.RemoveComment(photoId, new Comment { Id = 1, UserId = "1" });


            mockService.Verify(service => service.RemoveComment(It.IsAny<Photo>(), It.IsAny<Comment>()), Times.Once());
            Assert.IsInstanceOf<OkResult>(result);
        }   

        [Test]
        public void RemoveComment_IfCommentNotExists_ShouldReturnNotFound()
        {
            int photoId = 1;
            Photos[photoId].Comments = new List<Comment>();
            var mockService = new Mock<IPhotoService>();
            mockService.Setup(service => service.GetPhotoById(photoId)).Returns(Photos[photoId]); 
            var fakeUserManager = new FakeUserManagerBuilder()
                .Build();            
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "test1"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("custom-claim", "example claim value"),
            }, "mock"));
            fakeUserManager.Setup(u => u.FindByNameAsync("test1")).Returns(Task.FromResult(new User  {Id = "1", UserName = "test1" }));
            var controller = new PhotoController(mockService.Object, _mapper, fakeUserManager.Object);            
            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };
            controller.ControllerContext = context;
            
            var result = controller.RemoveComment(photoId, new Comment { Id = 1, UserId = "1" });

            Assert.IsInstanceOf<NotFoundResult>(result);
        }   

        [Test]
        public void RemoveComment_IfPhotoIsNull_ShouldReturnNotFoundResult()
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
            fakeUserManager.Setup(u => u.FindByNameAsync("test1")).Returns(Task.FromResult(new User  {Id = "1", UserName = "test1" }));
            var controller = new PhotoController(mockService.Object, _mapper, fakeUserManager.Object);            
            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };
            controller.ControllerContext = context;
            
            var result = controller.RemoveComment(1, new Comment { Id = 1 });

            Assert.IsInstanceOf<NotFoundResult>(result);
        }

               [Test]
        public void AddReply_ShouldCallAddReplyAndReturnOkResult()
        {
            int photoId = 1;
            Photos[photoId].Comments = new List<Comment> { new Comment { Id = 1}};
            var mockService = new Mock<IPhotoService>();
            mockService.Setup(service => service.GetPhotoById(photoId)).Returns(Photos[photoId]); 
            var fakeUserManager = new FakeUserManagerBuilder()
                .Build();            
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "test1"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("custom-claim", "example claim value"),
            }, "mock"));
            fakeUserManager.Setup(u => u.FindByNameAsync("test1")).Returns(Task.FromResult(new User  {Id = "1", UserName = "test1" }));
            var controller = new PhotoController(mockService.Object, _mapper, fakeUserManager.Object);            
            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };
            controller.ControllerContext = context;

            
            var result = controller.AddReply(photoId, 1, new Reply { Id = 1 });


            mockService.Verify(service => service.AddReply(It.IsAny<Comment>(), It.IsAny<Reply>(), It.IsAny<Photo>()), Times.Once());
            Assert.IsInstanceOf<OkResult>(result);
        }   

        [Test]
        public void AddReply_IfCommentIsNull_ShouldReturnNotFoundResult()
        {
            var photoId = 1;
            Photos[photoId].Comments = new List<Comment>();
            var mockService = new Mock<IPhotoService>();
            mockService.Setup(service => service.GetPhotoById(photoId)).Returns(Photos[photoId]); 
            var fakeUserManager = new FakeUserManagerBuilder()
                .Build();            
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "test1"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("custom-claim", "example claim value"),
            }, "mock"));
            fakeUserManager.Setup(u => u.FindByNameAsync("test1")).Returns(Task.FromResult(new User  {Id = "1", UserName = "test1" }));
            var controller = new PhotoController(mockService.Object, _mapper, fakeUserManager.Object);            
            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };
            controller.ControllerContext = context;
            
            var result = controller.AddReply(photoId, 1, new Reply { Id = 1 });

            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public void DeletePhoto_ShouldCallDeletePhoto()
        {
            var photoId = 1;
            foreach (var photo in Photos)
            {
                photo.Album = new Album();
            } 
            Photos[photoId].Album.UserId = "1";
            var mockService = new Mock<IPhotoService>();
            mockService.Setup(service => service.GetPhotoById(photoId)).Returns(Photos[photoId]); 
            mockService.Setup(service => service.Photos).Returns(Photos);
            var fakeUserManager = new FakeUserManagerBuilder()
                .Build();            
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "test1"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("custom-claim", "example claim value"),
            }, "mock"));
            fakeUserManager.Setup(u => u.FindByNameAsync("test1")).Returns(Task.FromResult(new User  {Id = "1", UserName = "test1", Photos = new List<Photo> { Photos[photoId] } }));
            var controller = new PhotoController(mockService.Object, _mapper, fakeUserManager.Object);            
            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };
            controller.ControllerContext = context;

            controller.DeletePhoto(photoId);
            
            mockService.Verify(c => c.DeletePhoto(photoId), Times.Once());
        }

        [Test]
        public void DeletePhoto_IfUserPhotosDoesNotContainPhotoShouldNotCallDeletePhoto()
        {
            var photoId = 1;
            foreach (var photo in Photos)
            {
                photo.Album = new Album();
            } 
            var mockService = new Mock<IPhotoService>();
            mockService.Setup(service => service.GetPhotoById(photoId)).Returns(Photos[photoId]); 
            var fakeUserManager = new FakeUserManagerBuilder()
                .Build();            
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "test1"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("custom-claim", "example claim value"),
            }, "mock"));
            fakeUserManager.Setup(u => u.FindByNameAsync("test1")).Returns(Task.FromResult(new User  {Id = "1", UserName = "test1" }));
            var controller = new PhotoController(mockService.Object, _mapper, fakeUserManager.Object);            
            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };
            controller.ControllerContext = context;

            controller.DeletePhoto(photoId);

            mockService.Verify(c => c.DeletePhoto(photoId), Times.Never());
        }
        //To Fix
        // [Test]
        // public void PutPhoto_ShouldCallUpdatePhotoAndReturnOkResult()
        // {  
        //     var Photo = new Photo {Id = 1, Album = new Album {UserId = "1"}};
        //     foreach (var photo in Photos)
        //     {
        //         Photo.Album = new Album() {Photos = new List<Photo>(){Photo}};
        //     }
        //     var mockService = new Mock<IPhotoService>();
        //     mockService.Setup(service => service.Photos).Returns(Photos);
        //     var fakeUserManager = new FakeUserManagerBuilder()
        //         .Build();            
        //     var mockUrlHelper = new Mock<IUrlHelper>(MockBehavior.Strict);
        //     mockUrlHelper
        //         .Setup(x => x.IsLocalUrl(It.IsAny<string>()))
        //         .Returns(true)
        //         .Verifiable();
        //     var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        //     {
        //         new Claim(ClaimTypes.Name, "test1"),
        //         new Claim(ClaimTypes.NameIdentifier, "1"),
        //         new Claim("custom-claim", "example claim value"),
        //     }, "mock"));
        //     fakeUserManager.Setup(u => u.FindByNameAsync("test1")).Returns(Task.FromResult
        //         (new User  {Id = "1", UserName = "test1", Albums = new List<Album> {new Album {UserId = "1"}},Photos = new List<Photo>{Photo}}));
        //     var controller = new PhotoController(mockService.Object, _mapper, fakeUserManager.Object);            
        //     controller.Url = mockUrlHelper.Object;
        //     var context = new ControllerContext
        //     {
        //         HttpContext = new DefaultHttpContext
        //         {
        //             User = user
        //         }
        //     };
        //     controller.ControllerContext = context;  

        //     var result = controller.PutPhoto(Photo).Result as OkObjectResult;            

        //     Assert.IsInstanceOf<OkResult>(result);
        //     mockService.Verify(service => service.UpdatePhoto(It.IsAny<Photo>()), Times.Once()); 
        // }

        [Test]
        public void PutPhoto_IfUserPhotosNotContainsPhoto_ShouldReturnNotFoundResult()
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
            fakeUserManager.Setup(u => u.FindByNameAsync("test1")).Returns(Task.FromResult(new User  {Id = "1", UserName = "test1" }));
            var controller = new PhotoController(mockService.Object, _mapper, fakeUserManager.Object);            
            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };
            controller.ControllerContext = context;  

            var result = controller.PutPhoto(new Photo()).Result as NotFoundObjectResult;            

            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            mockService.Verify(service => service.UpdatePhoto(It.IsAny<Photo>()), Times.Never()); 
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