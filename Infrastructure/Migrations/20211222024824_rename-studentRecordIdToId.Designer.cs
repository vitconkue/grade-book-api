﻿// <auto-generated />
using System;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20211222024824_rename-studentRecordIdToId")]
    partial class renamestudentRecordIdToId
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.12")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("ApplicationCore.Entity.Assignment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int?>("ClassId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<int>("Priority")
                        .HasColumnType("integer");

                    b.Property<int>("Weight")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ClassId");

                    b.ToTable("Assignments");
                });

            modelBuilder.Entity("ApplicationCore.Entity.Class", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("InviteStringStudent")
                        .HasColumnType("text");

                    b.Property<string>("InviteStringTeacher")
                        .HasColumnType("text");

                    b.Property<int?>("MainTeacherId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Room")
                        .HasColumnType("text");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.HasIndex("MainTeacherId");

                    b.ToTable("Classes");
                });

            modelBuilder.Entity("ApplicationCore.Entity.ClassStudentsAccount", b =>
                {
                    b.Property<int>("ClassId")
                        .HasColumnType("integer");

                    b.Property<int>("StudentAccountId")
                        .HasColumnType("integer");

                    b.Property<int>("Id")
                        .HasColumnType("integer");

                    b.HasKey("ClassId", "StudentAccountId");

                    b.HasIndex("StudentAccountId");

                    b.ToTable("ClassStudentsAccounts");
                });

            modelBuilder.Entity("ApplicationCore.Entity.ClassTeachersAccount", b =>
                {
                    b.Property<int>("ClassId")
                        .HasColumnType("integer");

                    b.Property<int>("TeacherId")
                        .HasColumnType("integer");

                    b.Property<int>("Id")
                        .HasColumnType("integer");

                    b.HasKey("ClassId", "TeacherId");

                    b.HasIndex("TeacherId");

                    b.ToTable("ClassTeachersAccounts");
                });

            modelBuilder.Entity("ApplicationCore.Entity.StudentAssignmentGrade", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("AssignmentId")
                        .HasColumnType("integer");

                    b.Property<bool>("IsFinalized")
                        .HasColumnType("boolean");

                    b.Property<int>("Point")
                        .HasColumnType("integer");

                    b.Property<int>("StudentRecordId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("AssignmentId");

                    b.HasIndex("StudentRecordId");

                    b.ToTable("StudentAssignmentGrades");
                });

            modelBuilder.Entity("ApplicationCore.Entity.StudentRecord", b =>
                {
                    b.Property<int>("RecordId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("ClassId")
                        .HasColumnType("integer");

                    b.Property<string>("FullName")
                        .HasColumnType("text");

                    b.Property<string>("StudentIdentification")
                        .HasColumnType("text");

                    b.HasKey("RecordId");

                    b.HasIndex("ClassId");

                    b.ToTable("StudentsRecords");
                });

            modelBuilder.Entity("ApplicationCore.Entity.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("DefaultProfilePictureHex")
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .HasColumnType("text");

                    b.Property<string>("FirstName")
                        .HasColumnType("text");

                    b.Property<bool>("IsPasswordNotSet")
                        .HasColumnType("boolean");

                    b.Property<string>("LastName")
                        .HasColumnType("text");

                    b.Property<byte[]>("PasswordHash")
                        .HasColumnType("bytea");

                    b.Property<byte[]>("PasswordSalt")
                        .HasColumnType("bytea");

                    b.Property<string>("ProfilePictureUrl")
                        .HasColumnType("text");

                    b.Property<string>("StudentIdentification")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ApplicationCore.Entity.Assignment", b =>
                {
                    b.HasOne("ApplicationCore.Entity.Class", "Class")
                        .WithMany("ClassAssignments")
                        .HasForeignKey("ClassId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Class");
                });

            modelBuilder.Entity("ApplicationCore.Entity.Class", b =>
                {
                    b.HasOne("ApplicationCore.Entity.User", "MainTeacher")
                        .WithMany()
                        .HasForeignKey("MainTeacherId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("MainTeacher");
                });

            modelBuilder.Entity("ApplicationCore.Entity.ClassStudentsAccount", b =>
                {
                    b.HasOne("ApplicationCore.Entity.Class", "Class")
                        .WithMany("ClassStudentsAccounts")
                        .HasForeignKey("ClassId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ApplicationCore.Entity.User", "Student")
                        .WithMany("ClassStudentsAccounts")
                        .HasForeignKey("StudentAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Class");

                    b.Navigation("Student");
                });

            modelBuilder.Entity("ApplicationCore.Entity.ClassTeachersAccount", b =>
                {
                    b.HasOne("ApplicationCore.Entity.Class", "Class")
                        .WithMany("ClassTeachersAccounts")
                        .HasForeignKey("ClassId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ApplicationCore.Entity.User", "Teacher")
                        .WithMany("ClassTeachersAccounts")
                        .HasForeignKey("TeacherId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Class");

                    b.Navigation("Teacher");
                });

            modelBuilder.Entity("ApplicationCore.Entity.StudentAssignmentGrade", b =>
                {
                    b.HasOne("ApplicationCore.Entity.Assignment", "Assignment")
                        .WithMany("StudentAssignmentGrades")
                        .HasForeignKey("AssignmentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ApplicationCore.Entity.StudentRecord", "StudentRecord")
                        .WithMany("Grades")
                        .HasForeignKey("StudentRecordId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Assignment");

                    b.Navigation("StudentRecord");
                });

            modelBuilder.Entity("ApplicationCore.Entity.StudentRecord", b =>
                {
                    b.HasOne("ApplicationCore.Entity.Class", "Class")
                        .WithMany("Students")
                        .HasForeignKey("ClassId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Class");
                });

            modelBuilder.Entity("ApplicationCore.Entity.Assignment", b =>
                {
                    b.Navigation("StudentAssignmentGrades");
                });

            modelBuilder.Entity("ApplicationCore.Entity.Class", b =>
                {
                    b.Navigation("ClassAssignments");

                    b.Navigation("ClassStudentsAccounts");

                    b.Navigation("ClassTeachersAccounts");

                    b.Navigation("Students");
                });

            modelBuilder.Entity("ApplicationCore.Entity.StudentRecord", b =>
                {
                    b.Navigation("Grades");
                });

            modelBuilder.Entity("ApplicationCore.Entity.User", b =>
                {
                    b.Navigation("ClassStudentsAccounts");

                    b.Navigation("ClassTeachersAccounts");
                });
#pragma warning restore 612, 618
        }
    }
}
