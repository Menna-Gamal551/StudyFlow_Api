
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyFlow_Api.Data;
using StudyFlow_Api.Data.DTOs;
using StudyFlow_Api.Data.Models;
using StudyFlow_Api.Services;
using StudyFlow_Api.Services.IService;

namespace StudyFlow_Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<AppdbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("MyCon")));
            builder.Services.AddAutoMapper(op =>
            {
                op.CreateMap<Subject, SubjectDTO>().ReverseMap();
                op.CreateMap<Subject, CreateSubjectDTO>().ReverseMap();
                op.CreateMap<Subject, UpdateSubjectDTO>().ReverseMap();
                op.CreateMap<Assignment, CreateAssignmentDTO>().ReverseMap();
                op.CreateMap<Assignment, UpdateAssignmentDTO>().ReverseMap();
                op.CreateMap<User, UserDTO>().ReverseMap();
                op.CreateMap<Assignment, AssignmentDTO>().ForMember(
                    dest => dest.SubjectName,
                    op => op.MapFrom(src=>src.Subject!=null?src.Subject.SubjectName:null)
                    ); 
            }
            );
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
