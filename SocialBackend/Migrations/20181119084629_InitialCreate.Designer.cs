﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SocialBackend.Models;

namespace SocialBackend.Migrations
{
    [DbContext(typeof(SocialBackendContext))]
    [Migration("20181119084629_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024");

            modelBuilder.Entity("SocialBackend.Models.User", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("authToken");

                    b.Property<string>("emailAddress");

                    b.Property<bool>("online");

                    b.Property<string>("password");

                    b.Property<string>("username");

                    b.HasKey("id");

                    b.ToTable("User");
                });
#pragma warning restore 612, 618
        }
    }
}
