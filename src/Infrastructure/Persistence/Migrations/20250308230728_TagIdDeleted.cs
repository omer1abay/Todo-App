using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Todo_App.Infrastructure.Persistence.Migrations
{
    public partial class TagIdDeleted : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoItems_Tags_TagsId",
                table: "TodoItems");

            migrationBuilder.DropIndex(
                name: "IX_TodoItems_TagsId",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "TagsId",
                table: "TodoItems");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TagsId",
                table: "TodoItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_TagsId",
                table: "TodoItems",
                column: "TagsId");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoItems_Tags_TagsId",
                table: "TodoItems",
                column: "TagsId",
                principalTable: "Tags",
                principalColumn: "Id");
        }
    }
}
