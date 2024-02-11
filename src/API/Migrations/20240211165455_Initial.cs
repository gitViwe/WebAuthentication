using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserDetails",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KanBanSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewTaskOpen = table.Column<bool>(type: "bit", nullable: false),
                    NewTaskName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserDetailId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KanBanSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KanBanSections_UserDetails_UserDetailId",
                        column: x => x.UserDetailId,
                        principalTable: "UserDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KanBanTaskItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KanBanSectionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KanBanTaskItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KanBanTaskItems_KanBanSections_KanBanSectionId",
                        column: x => x.KanBanSectionId,
                        principalTable: "KanBanSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KanBanSections_UserDetailId",
                table: "KanBanSections",
                column: "UserDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_KanBanTaskItems_KanBanSectionId",
                table: "KanBanTaskItems",
                column: "KanBanSectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KanBanTaskItems");

            migrationBuilder.DropTable(
                name: "KanBanSections");

            migrationBuilder.DropTable(
                name: "UserDetails");
        }
    }
}
