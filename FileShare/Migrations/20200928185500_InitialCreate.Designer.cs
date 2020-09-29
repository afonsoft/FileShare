﻿// <auto-generated />
using System;
using FileShare.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FileShare.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20200928185500_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.8");

            modelBuilder.Entity("FileShare.Repository.Model.ExtensionPermittedModel", b =>
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

                    b.Property<string>("Extension")
                        .HasColumnName("Extension")
                        .HasColumnType("TEXT")
                        .HasMaxLength(10);

                    b.HasKey("Id");

                    b.ToTable("PermittedExtension");

                    b.HasData(
                        new
                        {
                            Id = new Guid("d4bd2f98-4177-4f10-b089-78f665c39140"),
                            CreationDateTime = new DateTime(2020, 9, 28, 15, 54, 59, 442, DateTimeKind.Local).AddTicks(5122),
                            Extension = ".zip"
                        });
                });

            modelBuilder.Entity("FileShare.Repository.Model.FileModel", b =>
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
                        .HasMaxLength(200);

                    b.HasKey("Id");

                    b.ToTable("Files");
                });
#pragma warning restore 612, 618
        }
    }
}