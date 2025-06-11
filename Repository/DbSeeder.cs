using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

public static class DbSeeder
{
    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        if (!await roleManager.RoleExistsAsync("Member"))
        {
            await roleManager.CreateAsync(new IdentityRole("Member"));
        }
    }

    public static async Task SeedAdminUserAsync(UserManager<IdentityUser> userManager)
    {
        // Định nghĩa email và mật khẩu cho tài khoản Admin
        string adminEmail = "admin@email.com";
        string adminPassword = "Admin@123"; // Bạn có thể thay đổi mật khẩu này

        // Kiểm tra xem tài khoản Admin đã tồn tại chưa
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            // Tạo tài khoản Admin
            var user = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true // Bỏ qua xác nhận email
            };

            var result = await userManager.CreateAsync(user, adminPassword);
            if (result.Succeeded)
            {
                // Gán vai trò "Admin" cho tài khoản
                await userManager.AddToRoleAsync(user, "Admin");
            }
            else
            {
                // Xử lý lỗi nếu tạo tài khoản thất bại
                throw new Exception("Failed to create admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}