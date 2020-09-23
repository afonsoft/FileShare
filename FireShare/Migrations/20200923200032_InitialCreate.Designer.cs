﻿// <auto-generated />
using System;
using FireShare.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FireShare.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20200923200032_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.8");

            modelBuilder.Entity("FireShare.Repository.Model.FileModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("Id")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreationDateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("CreationDateTime")
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("getdate()");

                    b.Property<string>("Hash")
                        .HasColumnName("Hash")
                        .HasColumnType("TEXT")
                        .HasMaxLength(4000);

                    b.Property<string>("IP")
                        .HasColumnName("IP")
                        .HasColumnType("TEXT")
                        .HasMaxLength(100);

                    b.Property<string>("Name")
                        .HasColumnName("Name")
                        .HasColumnType("TEXT")
                        .HasMaxLength(400);

                    b.Property<long>("Size")
                        .HasColumnName("Size")
                        .HasColumnType("INTEGER");

                    b.Property<string>("StorageName")
                        .HasColumnName("StorageName")
                        .HasColumnType("TEXT")
                        .HasMaxLength(400);

                    b.Property<string>("Type")
                        .HasColumnName("Type")
                        .HasColumnType("TEXT")
                        .HasMaxLength(250);

                    b.HasKey("Id");

                    b.ToTable("Files");
                });
#pragma warning restore 612, 618
        }
    }
}
