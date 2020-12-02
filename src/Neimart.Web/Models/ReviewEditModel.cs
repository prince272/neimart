using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Neimart.Core.Entities;

namespace Neimart.Web.Models
{
    public class ReviewEditModel
    {
        public bool IsEdit => Id != 0;

        public long Id { get; set; }

        public string Title { get; set; }

        [Display(Prompt = "Tell others what you think about this product. Would you recommend it, and why?")]
        public string Comment { get; set; }

        public int Rating { get; set; }

        [Display(Description = "Whether this review is approved or rejected.")]
        public bool Approved { get; set; }

        public string Reply { get; set; }
    }

    public class ReviewEditProfile : Profile
    {
        public ReviewEditProfile()
        {
            CreateMap<ReviewEditModel, Review>();
            CreateMap<Review, ReviewEditModel>();
        }
    }

    public class ReviewEditValidator : AbstractValidator<ReviewEditModel>
    {
        public ReviewEditValidator()
        {
            RuleFor(x => x.Title).NotNull().NotEmpty();
            RuleFor(x => x.Comment).NotNull().NotEmpty();
            RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        }
    }
}
