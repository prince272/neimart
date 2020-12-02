using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace Neimart.Core.Entities
{
    public class User : IdentityUser<long>, IEntity
    {
        public User()
        {
        }

        protected User(string userName)
            : base(userName)
        {
        }

        public string MobileIssuer { get; set; }

        public string MobileNumber { get; set; }

        public bool MobileSetup { get; set; }

        public string BankIssuer { get; set; }

        public string BankNumber { get; set; }

        public bool BankSetup { get; set; }

        public string UserCode { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName => $"{FirstName?.Trim()} {LastName?.Trim()}".Trim();

        public string Bio { get; set; }

        public string StoreSlug { get; set; }

        public string StoreName { get; set; }

        public string StoreDescription { get; set; }

        public StoreCategory StoreCategory { get; set; }

        public StoreStatus StoreStatus { get; set; }

        public StoreAccess StoreAccess { get; set; }

        public UserImage StoreLogo { get; set; }

        public long? StoreLogoId { get; set; }

        public virtual UserDocument StoreDocument { get; set; }

        public long? StoreDocumentId { get; set; }

        public bool StoreDeliveryRequired { get; set; }

        public PlanType StorePlanType { get; set; }

        public PlanPeriod StorePlanPeriod { get; set; }

        public bool StorePlanUpdated { get; set; }

        public DateTimeOffset StorePlanUpdatedOn { get; set; }

        public DateTimeOffset StorePlanEndedOn { get; set; }

        public TimeSpan StorePlanLeftSpan => new TimeSpan(Math.Max(0, (StorePlanEndedOn - DateTimeOffset.UtcNow).Ticks));

        public bool StorePlanEnded => DateTimeOffset.UtcNow >= StorePlanEndedOn;

        public bool StoreSetup { get; set; }

        public string StoreRegion { get; set; }

        public string StorePlace { get; set; }

        public string StoreStreet { get; set; }

        public string StorePostal { get; set; }

        public string StoreAddress => string.Join(", ", new[] { StoreRegion, StorePlace, StoreStreet, StorePostal }
                .Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim(',', ' ')));

        public List<Delivery> StoreDeliveries { get; set; } = new List<Delivery>();

        public ThemeMode StoreThemeMode { get; set; }

        public ThemeStyle StoreThemeStyle { get; set; }

        public virtual UserImage UserImage { get; set; }

        public long? UserImageId { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public DateTimeOffset UpdatedOn { get; set; }

        public bool Updated { get; set; }

        public bool Deleted { get; set; }

        public decimal Balance { get; set; }

        public string AboutNote { get; set; }

        public string TermsNote { get; set; }

        public string PrivacyNote { get; set; }

        public string ReturnsNote { get; set; }

        public string ReviewsNote { get; set; }


        public string FacebookLink { get; set; }

        public string TwitterLink { get; set; }

        public string YoutubeLink { get; set; }

        public string InstagramLink { get; set; }

        public string LinkedInLink { get; set; }

        public string PinterestLink { get; set; }

        public string MapLink { get; set; }

        public string WhatsAppNumber { get; set; }

        public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }

    public enum StoreStatus
    {
        Closed,
        Opened
    }

    public enum StoreAccess
    {
        Pending,
        Approved,
        Rejected
    }

    public enum PlanType
    {
        Basic, 
        Standard,
        Premium
    }

    public enum PlanPeriod
    {
        [Display(Name = "3 Months")]
        ThreeMonths = 3,
        [Display(Name = "6 Months")]
        SixMonths = 6,
        [Display(Name = "1 Year")]
        OneYear = 12
    }

    [Flags]
    public enum StoreCategory
    {
        [Display(Name = "Electronics"), StoreCategory(HexColor = "#e91e63", FaIcon = "fa-tv")]
        Electronics = 1 << 0,

        [Display(Name = "Sports, Arts & Outdoors"), StoreCategory(HexColor = "#1c2331", FaIcon = "fa-futbol")]
        Hobbies = 1 << 1,

        [Display(Name = "Education"), StoreCategory(HexColor = "#673ab7", FaIcon = "fa-books")]
        Education = 1 << 2,

        [Display(Name = "Softwares & Games"), StoreCategory(HexColor = "#3f51b5", FaIcon = "fa-gamepad")]
        Games = 1 << 3,

        [Display(Name = "Videos & Audios"), StoreCategory(HexColor = "#2196f3", FaIcon = "fa-photo-video")]
        Videos = 1 << 4,

        [Display(Name = "Home & Garden"), StoreCategory(HexColor = "#4caf50", FaIcon = "fa-flower-daffodil")]
        Garden = 1 << 5,

        [Display(Name = "Clothing & Beauty"), StoreCategory(HexColor = "#ffeb3b", FaIcon = "fa-tshirt")]
        Fashion = 1 << 6,

        [Display(Name = "Groceries"), StoreCategory(HexColor = "#ff5722", FaIcon = "fa-burger-soda")]
        Groceries = 1 << 7,

        [Display(Name = "Healthcare"), StoreCategory(HexColor = "#9933cc", FaIcon = "fa-spray-can")]
        Healthcare = 1 << 8,

        [Display(Name = "Instruments"), StoreCategory(HexColor = "#ff1744 ", FaIcon = "fa-guitar-electric")]
        Instruments = 1 << 9,

        [Display(Name = "Equipments"), StoreCategory(HexColor = "#ff8f00", FaIcon = "fa-tools")]
        Equipments = 1 << 10,

        [Display(Name = "Properties"), StoreCategory(HexColor = "#d500f9", FaIcon = "fa-house-user")]
        Properties = 1 << 11,

        [Display(Name = "Vehicles"), StoreCategory(HexColor = "#1c2331", FaIcon = "fa-cars")]
        Vehicles = 1 << 12,

        [Display(Name = "Pets and animals"), StoreCategory(HexColor = "#f44336", FaIcon = "fa-dog")]
        Animals = 1 << 13,

        [Display(Name = "Repair & Construction"), StoreCategory(HexColor = "#304ffe", FaIcon = "fa-toolbox")]
        Constructions = 1 << 14,
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class StoreCategoryAttribute : Attribute
    {
        public string FaIcon { get; set; }

        public string HexColor { get; set; }
    }
}