using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuMejorPeso.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Assignment",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    durationMinutes = table.Column<int>(type: "int", nullable: false),
                    difficulty = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignment", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Branch",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    address = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branch", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Licenses",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    barcode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    startDate = table.Column<DateOnly>(type: "date", nullable: false),
                    endDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Licenses", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Pay",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    memberId = table.Column<int>(type: "int", nullable: false),
                    membershipId = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    amount = table.Column<int>(type: "int", nullable: false),
                    paymentMethod = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pay", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Specialty",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialty", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TypeMembreship",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    daysDuration = table.Column<int>(type: "int", nullable: false),
                    price = table.Column<float>(type: "float", nullable: false),
                    active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypeMembreship", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Classrooms",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    assignmentid = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    limitedPlace = table.Column<int>(type: "int", nullable: false),
                    branchId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classrooms", x => x.id);
                    table.ForeignKey(
                        name: "FK_Classrooms_Assignment_assignmentid",
                        column: x => x.assignmentid,
                        principalTable: "Assignment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Classrooms_Branch_branchId",
                        column: x => x.branchId,
                        principalTable: "Branch",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    userName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    passwordHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    salt = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Role = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    lastName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    lastAccess = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    branchId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.id);
                    table.ForeignKey(
                        name: "FK_User_Branch_branchId",
                        column: x => x.branchId,
                        principalTable: "Branch",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Memberships",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    typeid = table.Column<int>(type: "int", nullable: false),
                    state = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    pricePaid = table.Column<float>(type: "float", nullable: false),
                    debt = table.Column<float>(type: "float", nullable: false),
                    discount = table.Column<float>(type: "float", nullable: false),
                    startDate = table.Column<DateOnly>(type: "date", nullable: false),
                    endDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Memberships", x => x.id);
                    table.ForeignKey(
                        name: "FK_Memberships_TypeMembreship_typeid",
                        column: x => x.typeid,
                        principalTable: "TypeMembreship",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ScheduleClassrooms",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    classroomId = table.Column<int>(type: "int", nullable: false),
                    dayWeek = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    startTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    endTime = table.Column<TimeSpan>(type: "time(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleClassrooms", x => x.id);
                    table.ForeignKey(
                        name: "FK_ScheduleClassrooms_Classrooms_classroomId",
                        column: x => x.classroomId,
                        principalTable: "Classrooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ActivityRecord",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    action = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    userid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityRecord", x => x.id);
                    table.ForeignKey(
                        name: "FK_ActivityRecord_User_userid",
                        column: x => x.userid,
                        principalTable: "User",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Coaches",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    state = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    branchId = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    lastName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dni = table.Column<int>(type: "int", nullable: false),
                    phone = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    userId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coaches", x => x.id);
                    table.ForeignKey(
                        name: "FK_Coaches_Branch_branchId",
                        column: x => x.branchId,
                        principalTable: "Branch",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Coaches_User_userId",
                        column: x => x.userId,
                        principalTable: "User",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    birthdate = table.Column<DateOnly>(type: "date", nullable: false),
                    direction = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gender = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    licenseId = table.Column<int>(type: "int", nullable: true),
                    membershipId = table.Column<int>(type: "int", nullable: true),
                    note = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    lastName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dni = table.Column<int>(type: "int", nullable: false),
                    phone = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    userId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.id);
                    table.ForeignKey(
                        name: "FK_Members_Licenses_licenseId",
                        column: x => x.licenseId,
                        principalTable: "Licenses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Members_Memberships_membershipId",
                        column: x => x.membershipId,
                        principalTable: "Memberships",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Members_User_userId",
                        column: x => x.userId,
                        principalTable: "User",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClassroomCoaches",
                columns: table => new
                {
                    classroomsid = table.Column<int>(type: "int", nullable: false),
                    coachesid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassroomCoaches", x => new { x.classroomsid, x.coachesid });
                    table.ForeignKey(
                        name: "FK_ClassroomCoaches_Classrooms_classroomsid",
                        column: x => x.classroomsid,
                        principalTable: "Classrooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassroomCoaches_Coaches_coachesid",
                        column: x => x.coachesid,
                        principalTable: "Coaches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CoachSpecialty",
                columns: table => new
                {
                    coachesid = table.Column<int>(type: "int", nullable: false),
                    specialtiesid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachSpecialty", x => new { x.coachesid, x.specialtiesid });
                    table.ForeignKey(
                        name: "FK_CoachSpecialty_Coaches_coachesid",
                        column: x => x.coachesid,
                        principalTable: "Coaches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoachSpecialty_Specialty_specialtiesid",
                        column: x => x.specialtiesid,
                        principalTable: "Specialty",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ScheduleCoaches",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    coachId = table.Column<int>(type: "int", nullable: false),
                    dayWeek = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    startTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    endTime = table.Column<TimeSpan>(type: "time(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleCoaches", x => x.id);
                    table.ForeignKey(
                        name: "FK_ScheduleCoaches_Coaches_coachId",
                        column: x => x.coachId,
                        principalTable: "Coaches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Attendance",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    classId = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Memberid = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendance", x => x.id);
                    table.ForeignKey(
                        name: "FK_Attendance_Members_Memberid",
                        column: x => x.Memberid,
                        principalTable: "Members",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClassroomMembers",
                columns: table => new
                {
                    classroomsid = table.Column<int>(type: "int", nullable: false),
                    membersid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassroomMembers", x => new { x.classroomsid, x.membersid });
                    table.ForeignKey(
                        name: "FK_ClassroomMembers_Classrooms_classroomsid",
                        column: x => x.classroomsid,
                        principalTable: "Classrooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassroomMembers_Members_membersid",
                        column: x => x.membersid,
                        principalTable: "Members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityRecord_userid",
                table: "ActivityRecord",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_Memberid",
                table: "Attendance",
                column: "Memberid");

            migrationBuilder.CreateIndex(
                name: "IX_ClassroomCoaches_coachesid",
                table: "ClassroomCoaches",
                column: "coachesid");

            migrationBuilder.CreateIndex(
                name: "IX_ClassroomMembers_membersid",
                table: "ClassroomMembers",
                column: "membersid");

            migrationBuilder.CreateIndex(
                name: "IX_Classrooms_assignmentid",
                table: "Classrooms",
                column: "assignmentid");

            migrationBuilder.CreateIndex(
                name: "IX_Classrooms_branchId",
                table: "Classrooms",
                column: "branchId");

            migrationBuilder.CreateIndex(
                name: "IX_Coaches_branchId",
                table: "Coaches",
                column: "branchId");

            migrationBuilder.CreateIndex(
                name: "IX_Coaches_userId",
                table: "Coaches",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachSpecialty_specialtiesid",
                table: "CoachSpecialty",
                column: "specialtiesid");

            migrationBuilder.CreateIndex(
                name: "IX_Members_licenseId",
                table: "Members",
                column: "licenseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_membershipId",
                table: "Members",
                column: "membershipId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_userId",
                table: "Members",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_typeid",
                table: "Memberships",
                column: "typeid");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleClassrooms_classroomId",
                table: "ScheduleClassrooms",
                column: "classroomId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleCoaches_coachId",
                table: "ScheduleCoaches",
                column: "coachId");

            migrationBuilder.CreateIndex(
                name: "IX_User_branchId",
                table: "User",
                column: "branchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityRecord");

            migrationBuilder.DropTable(
                name: "Attendance");

            migrationBuilder.DropTable(
                name: "ClassroomCoaches");

            migrationBuilder.DropTable(
                name: "ClassroomMembers");

            migrationBuilder.DropTable(
                name: "CoachSpecialty");

            migrationBuilder.DropTable(
                name: "Pay");

            migrationBuilder.DropTable(
                name: "ScheduleClassrooms");

            migrationBuilder.DropTable(
                name: "ScheduleCoaches");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "Specialty");

            migrationBuilder.DropTable(
                name: "Classrooms");

            migrationBuilder.DropTable(
                name: "Coaches");

            migrationBuilder.DropTable(
                name: "Licenses");

            migrationBuilder.DropTable(
                name: "Memberships");

            migrationBuilder.DropTable(
                name: "Assignment");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "TypeMembreship");

            migrationBuilder.DropTable(
                name: "Branch");
        }
    }
}
