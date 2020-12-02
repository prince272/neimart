using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Neimart.Core;
using Neimart.Core.Entities;
using Neimart.Core.Events;
using Neimart.Core.Infrastructure.Imaging;
using Neimart.Core.Infrastructure.Web;
using Neimart.Core.Services;
using Neimart.Core.Settings;
using Neimart.Core.Utilities.Extensions;
using Neimart.Core.Utilities.Helpers;
using Neimart.Data;
using Neimart.Web.Services;
using Neimart.Web.Middlewares;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace Neimart.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IWebHostEnvironment Environment { get; }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(options =>
            {
                options.Company = new CompanyInfo
                {
                    Name = "Neimart",
                    Description = "Neimart is an ecommerce platform that provides an end-to-end business solution allowing you to start, grow, and manage your retail business.",
                    EstablishedOn = DateTimeOffset.Parse("2019-07-08"),
                    Address = "No.4 Agbaamo Street, Airport Accra, Ghana",
                    PhoneNumber = "+233547547258",


                    AdminEmail = Configuration.GetValue<string>("Sending:Smtp:Credentials:Admin:Email"),
                    AdminPassword = Configuration.GetValue<string>("Sending:Smtp:Credentials:Admin:Password"),

                    SupportEmail = Configuration.GetValue<string>("Sending:Smtp:Credentials:Support:Email"),
                    SupportPassword = Configuration.GetValue<string>("Sending:Smtp:Credentials:Support:Password"),

                    InfoEmail = Configuration.GetValue<string>("Sending:Smtp:Credentials:Info:Email"),
                    InfoPassword = Configuration.GetValue<string>("Sending:Smtp:Credentials:Info:Password"),

                    NotificationEmail = Configuration.GetValue<string>("Sending:Smtp:Credentials:Notification:Email"),
                    NotificationPassword = Configuration.GetValue<string>("Sending:Smtp:Credentials:Notification:Password"),

                    MapLink = "https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d75551.8964035619!2d-0.19444572554201875!3d5.610157527892059!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0xfdf9b079dcc55cf%3A0x373d56b9a01d602d!2s4%20Agbaamo%20St%2C%20Accra!5e0!3m2!1sen!2sgh!4v1595846305553!5m2!1sen!2sgh"
                };

                options.PlanRates = new List<decimal>
                {
                    25,
                    85,
                    255
                };
                options.PlanFeatures = new List<string[]>
                {
                    new string[] { "Unlimited products", "24/7 support", "Built-in reviews", "Automatic SEO Support" },
                    new string[] { "Unlimited products", "24/7 support", "Built-in reviews", "Automatic SEO Support", "Basic Designer Assist", "Featured on Neimart" },
                    new string[] { "Unlimited products", "24/7 support", "Built-in reviews", "Automatic SEO Support", "Pro Designer Assist", "Featured on Neimart", "Facebook & Google Ads" },
                };
                options.PlanTrialDays = 30;

                options.PaymentRate = 0.023m;

                options.CartMaxCount = 10;
                options.PageDefaultSize = 15;

                options.ThemeMode = ThemeMode.Light;
                options.ThemeStyle = ThemeStyle.Gradient;

                options.CurrencyCode = "GHS";
                options.CurrencyMinValue = 1;
                options.CurrencyMaxValue = 1000000;
                options.CountryCode = "GH";
                options.CurrencySymbol = "GH₵";
                options.PercentSymbol = "%";
                options.QuantityMaxValue = 100;

                options.ImageFileExtensions = new string[] { ".jpg", ".jpeg", ".png", ".gif" };
                options.DocumentFileExtensions = new string[] { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".pps", ".ppsx", ".odt", ".xls", ".xlsx", ".psd", ".txt", ".zip" };
                options.AudioFileExtensions = new string[] { ".mp3", ".ogg", ".wav", };
                options.VideoFileExtensions = new string[] { ".mp4", ".webm", ".swf", ".flv" };

                options.ImageFileMaxSize = 5242880; // 5MB
                options.DocumentFileMaxSize = 73400320; // 70MB
                options.AudioFileMaxSize = 73400320; // 70MB
                options.VideoFileMaxSize = 73400320; // 70MB
            });

            services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                        sqlServerOptions => sqlServerOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name));

                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                });

            services.AddIdentity<User, Role>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 0;
                options.Password.RequiredUniqueChars = 0;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters = null;
                options.User.RequireUniqueEmail = true;

                options.SignIn.RequireConfirmedAccount = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            })
                    .AddUserManager<UserService>()
                    .AddRoleManager<RoleService>()
                    .AddSignInManager<SignInService>()
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddDefaultTokenProviders();

            services.AddResponseCompression();

            services.AddAuthentication()
                    //.AddFacebook(options =>
                    //{
                    //    options.AppId = Configuration.GetValue<string>("Authentication:Facebook:AppId");
                    //    options.AppSecret = Configuration.GetValue<string>("Authentication:Facebook:AppSecret");
                    //    options.AccessDeniedPath = "/account/access-denied";
                    //})
                    .AddGoogle(options =>
                    {
                        options.ClientId = Configuration.GetValue<string>("Authentication:Google:ClientId");
                        options.ClientSecret = Configuration.GetValue<string>("Authentication:Google:ClientSecret");
                        options.AccessDeniedPath = "/account/access-denied";
                        options.SaveTokens = true;
                    });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.SlidingExpiration = true;

                options.ReturnUrlParameter = "returnUrl";
                options.LoginPath = "/account/signin";
                options.LogoutPath = "/account/signout";
                options.AccessDeniedPath = "/account/access-denied";
                options.SlidingExpiration = true;

                options.Events.OnRedirectToLogin = context =>
                {
                    string redirectUrl = new Uri((string)context.Request.Headers["Referer"] ?? context.Request.Path, UriKind.RelativeOrAbsolute).ToRelative();

                    redirectUrl = QueryHelpers.AddQueryString(options.LoginPath, "returnUrl", redirectUrl);

                    if (context.Request.IsAjax())
                    {
                        int statusCode = StatusCodes.Status401Unauthorized;
                        var error = new ProblemDetails
                        {
                            Status = statusCode,
                            Detail = "You need to sign into your account.",
                        };

                        error.Extensions.Add("redirectUrl", redirectUrl);

                        context.Response.StatusCode = statusCode;
                        context.Response.WriteAsync(error.ToJsonString());
                    }
                    else
                    {
                        context.Response.Redirect(redirectUrl);
                    }

                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    if (context.Request.IsAjax())
                    {
                        int statusCode = StatusCodes.Status403Forbidden;
                        string detail = "Access denied.";

                        context.Response.StatusCode = statusCode;
                        context.Response.WriteAsync(new { detail, statusCode }.ToJsonString());
                    }
                    else
                    {
                        context.Response.Redirect(context.RedirectUri);
                    }

                    return Task.CompletedTask;
                };
                options.Events.OnSignedIn = async context =>
                {
                    var mediator = context.HttpContext.RequestServices.GetRequiredService<IMediator>();
                    await mediator.Publish(new UserSignedIn(await context.HttpContext.GetMemberAsync()));
                };
            });

            var mvc = services.AddControllersWithViews(options =>
             {
                 // ASP.NET Core 2.2 Parameter Transformers for clean URL generation and slugs in Razor Pages or MVC
                 // source: https://www.hanselman.com/blog/ASPNETCore22ParameterTransformersForCleanURLGenerationAndSlugsInRazorPagesOrMVC.aspx
                 options.Conventions.Add(new RouteTokenTransformerConvention(
                                 new SlugifyParameterTransformer()));

                 options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());

                 // Best way to trim strings after data entry. Should I create a custom model binder?
                 // source: https://stackoverflow.com/questions/1718501/best-way-to-trim-strings-after-data-entry-should-i-create-a-custom-model-binder/59313908#59313908
                 int formValueProviderFactoryIndex = options.ValueProviderFactories.IndexOf(options.ValueProviderFactories.OfType<FormValueProviderFactory>().Single());
                 options.ValueProviderFactories[formValueProviderFactoryIndex] = new TrimmedFormValueProviderFactory();

                 int queryStringValueProviderFactoryIndex = options.ValueProviderFactories.IndexOf(options.ValueProviderFactories.OfType<QueryStringValueProviderFactory>().Single());
                 options.ValueProviderFactories[queryStringValueProviderFactoryIndex] = new TrimmedQueryStringValueProviderFactory();

                 // A Humanizer Display Metadata Provider for ASP .Net Core
                 // source: https://www.michael-whelan.net/using-humanizer-with-asp-dotnet-core/
                 options.ModelMetadataDetailsProviders.Add(new HumanizerMetadataProvider());
             })
                     .AddNewtonsoftJson(options =>
                     {
                         options.SerializerSettings.Converters.Add(new StringEnumConverter());
                         options.SerializerSettings.Converters.Add(new StringEnumConverter(new DefaultNamingStrategy()));
                         options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                     })
                     .AddFluentValidation(options =>
                     {
                         options.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());

                         // Client side validation for bool value #1314
                         // source: https://github.com/FluentValidation/FluentValidation/issues/1314
                         // This is the new bit:
                         options.ConfigureClientsideValidation(clientside =>
                          {
                             // Preserve the existing "EqualTo" clientside validator
                             // as this handles cross-property comparisons, which is a supported scenario.
                             var originalEqualFactory = clientside.ClientValidatorFactories[typeof(FluentValidation.Validators.EqualValidator)];
                             // Now add in our new one.
                             clientside.ClientValidatorFactories[typeof(FluentValidation.Validators.EqualValidator)] = (context, rule, validator) =>
                              {
                                  var originalClientsideValidator = originalEqualFactory(context, rule, validator);
                                  return new EqualToValueClientValidator(rule, validator, originalClientsideValidator);
                              };
                          });
                     })
                     .AddRazorRuntimeCompilation();

            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = false;
                options.ConstraintMap["slugify"] = typeof(SlugifyParameterTransformer);
            });
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
            });
            services.AddRecaptcha(options =>
            {
                options.SecretKey = Configuration.GetValue<string>("Validation:Recaptcha:SecretKey");
                options.SiteKey = Configuration.GetValue<string>("Validation:Recaptcha:SiteKey");
            });
            services.AddSkiaSharpProcessor();
            services.AddAntiforgery(options =>
            {
                options.SuppressXFrameOptionsHeader = true;
            });

            services.AddLocalFileClient(options =>
            {
                options.BasePath = Path.Combine(Environment.WebRootPath, "uploads");
                options.BaseUrl = "/uploads";
            });
            services.AddEasyCaching(options =>
            {
                options.UseInMemory("default");
            });
            services.AddCacheManager();
            services.AddUnitOfWork<AppDbContext>();
            services.AddAutoMapper(options =>
            {
                // Disable the flattening behavior to prevent unexpected mapping between an entity and other entity found in a model.
                options.DestinationMemberNamingConvention = new ExactMatchNamingConvention();
            }, Assembly.GetExecutingAssembly());
            services.AddMediatR(Assembly.GetExecutingAssembly());

            services.AddSmtpEmailSender(options =>
            {
                options.Server = Configuration.GetValue<string>("Sending:Smtp:Server");
                options.Port = Configuration.GetValue<int>("Sending:Smtp:Port");
                options.EnableSsl = false;
            });
            services.AddSmsSender();

            services.AddRazorViewRenderer();
            services.AddPaySwitch(options =>
            {
                options.MerchantId = Configuration.GetValue<string>("Paying:PaySwitch:MerchantId");
                options.ApiUser = Configuration.GetValue<string>("Paying:PaySwitch:ApiUser");
                options.ApiKey = Configuration.GetValue<string>("Paying:PaySwitch:ApiKey");
                options.Passcode = Configuration.GetValue<string>("Paying:PaySwitch:Passcode");
                options.Live = !Environment.IsDevelopment();
            });
            //services.AddFlutterwave(options =>
            //{
            //    options.PublicKey = Configuration.GetValue<string>("Paying:Flutterwave:PublicKey");
            //    options.SecretKey = Configuration.GetValue<string>("Paying:Flutterwave:SecretKey");
            //    options.EncryptionKey = Configuration.GetValue<string>("Paying:Flutterwave:EncryptionKey");
            //    options.Live = true;
            //});

            services.AddTransient<AppService>();
            services.AddTransient<MessageService>();
            services.AddTransient<TransactionService>();
            services.AddTransient<ProductService>();
            services.AddTransient<TagService>();
            services.AddTransient<CategoryService>();
            services.AddTransient<BannerService>();
            services.AddTransient<CartService>();
            services.AddTransient<ReviewService>();
            services.AddTransient<MediaService>();
            services.AddTransient<AddressService>();
            services.AddTransient<OrderService>();
            services.AddTransient<OrderItemService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/error/500");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/error/{0}");

            app.UseHttpsRedirection();

            // Enable compression
            app.UseResponseCompression();

            app.UseImageProcessor();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseTransaction();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapAreaControllerRoute(
                    areaName: "Portal",
                    name: "portal",
                    pattern: "portal/{controller:slugify=Home}/{action:slugify=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "account",
                    pattern: "account/{action:slugify=Index}/{id?}",
                    defaults: new { controller = "Account" });

                endpoints.MapControllerRoute(
                    name: "store",
                    pattern: "{storeSlug}/{action:slugify=Index}/{slug?}",
                    defaults: new { controller = "Store" },
                    constraints: new { storeSlug = new StoreSlugRouteConstraint(app.ApplicationServices) });

                endpoints.MapControllerRoute(
                    name: "uploads",
                    pattern: "uploads/{action=Index}/{id?}",
                    defaults: new { controller = "Media" });

                endpoints.MapControllerRoute(
                    name: "company",
                    pattern: "{action:slugify=Index}/{id?}",
                    defaults: new { controller = "Company" });

                endpoints.MapControllerRoute(
                    name: "test",
                    pattern: "test/{action=Index}/{id?}",
                    defaults: new { controller = "Test" });

            });
        }
    }
}