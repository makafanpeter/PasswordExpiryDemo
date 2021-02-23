using System;
using AutoMapper;
using PasswordPoliciesDemo.API.Infrastructure.Common.Mappings;
using PasswordPoliciesDemo.API.Infrastructure.Domain;

namespace PasswordPoliciesDemo.API.ViewModels
{
    public class UserProfile : BaseViewModel, IMapFrom<ApplicationUser>
    {
        public void Mapping(Profile profile)
        {
            profile.CreateMap<ApplicationUser, UserProfile>();
        }


        public string FirstName { get; set; }
        public string LastName { get; set; }


        public DateTimeOffset? LastLogin { get; set; }
        public string Username { get; set; }

        

    }


    public class RegisterRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }



    }



}