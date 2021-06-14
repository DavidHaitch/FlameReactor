﻿// <auto-generated />
using System;
using FlameReactor.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FlameReactor.DB.Migrations
{
    [DbContext(typeof(FlameReactorContext))]
    [Migration("20210610164137_Adjustments")]
    partial class Adjustments
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.7");

            modelBuilder.Entity("BreedingFlame", b =>
                {
                    b.Property<int>("BreedingsID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ParentsID")
                        .HasColumnType("INTEGER");

                    b.HasKey("BreedingsID", "ParentsID");

                    b.HasIndex("ParentsID");

                    b.ToTable("BreedingFlame");
                });

            modelBuilder.Entity("FlameReactor.DB.Models.AccessEvent", b =>
                {
                    b.Property<DateTimeOffset>("Timestamp")
                        .HasColumnType("TEXT");

                    b.Property<string>("IPAddress")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserAgent")
                        .HasColumnType("TEXT");

                    b.HasKey("Timestamp", "IPAddress");

                    b.ToTable("AccessEvents");
                });

            modelBuilder.Entity("FlameReactor.DB.Models.Breeding", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ChildID")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.ToTable("Breedings");
                });

            modelBuilder.Entity("FlameReactor.DB.Models.Flame", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("BirthID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("DisplayName")
                        .HasColumnType("TEXT");

                    b.Property<int>("Generation")
                        .HasColumnType("INTEGER");

                    b.Property<string>("GenomePath")
                        .HasColumnType("TEXT");

                    b.Property<string>("ImagePath")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<int>("Promiscuity")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Rating")
                        .HasColumnType("INTEGER");

                    b.Property<string>("VideoPath")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("BirthID")
                        .IsUnique();

                    b.ToTable("Flames");
                });

            modelBuilder.Entity("FlameReactor.DB.Models.InteractionEvent", b =>
                {
                    b.Property<DateTimeOffset>("Timestamp")
                        .HasColumnType("TEXT");

                    b.Property<string>("IPAddress")
                        .HasColumnType("TEXT");

                    b.Property<string>("Details")
                        .HasColumnType("TEXT");

                    b.Property<string>("InteractionType")
                        .HasColumnType("TEXT");

                    b.HasKey("Timestamp", "IPAddress");

                    b.ToTable("InteractionEvents");
                });

            modelBuilder.Entity("FlameReactor.DB.Models.Vote", b =>
                {
                    b.Property<string>("IPAddress")
                        .HasColumnType("TEXT");

                    b.Property<int>("FlameId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Adjustment")
                        .HasColumnType("INTEGER");

                    b.HasKey("IPAddress", "FlameId");

                    b.ToTable("Votes");
                });

            modelBuilder.Entity("BreedingFlame", b =>
                {
                    b.HasOne("FlameReactor.DB.Models.Breeding", null)
                        .WithMany()
                        .HasForeignKey("BreedingsID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FlameReactor.DB.Models.Flame", null)
                        .WithMany()
                        .HasForeignKey("ParentsID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("FlameReactor.DB.Models.Flame", b =>
                {
                    b.HasOne("FlameReactor.DB.Models.Breeding", "Birth")
                        .WithOne("Child")
                        .HasForeignKey("FlameReactor.DB.Models.Flame", "BirthID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Birth");
                });

            modelBuilder.Entity("FlameReactor.DB.Models.Breeding", b =>
                {
                    b.Navigation("Child");
                });
#pragma warning restore 612, 618
        }
    }
}
