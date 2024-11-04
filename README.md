####### Cách khởi động file ########
- Yêu cầu sử dụng Visual studio đã cài đặt ASP.NET and web development và hệ quản trị cơ sở dữ liệu Mysql
- Dowload src và mở file FileSharingSystem.sln hoặc clone src từ github
- Nếu trong thư migrate có bất cứ file nào -> xóa 
- Vào file appsetting.json cấu hình tài khoản mysql bao gồm username và password
- Tạo sẳn database có tên là filesharingdb -> không cần tạo table
- Tiến hành gõ lệnh trong: Tools/NuGet Packet Manager/Packet Manager Console
     + Add-Migration InitialCreate
     + Update-Database
Sau khi migrate xong có thể tiến hành khởi động dự án

####### Một số chức năng chính #######
+ Đăng kí tài khoản ( bao gồm gửi email xác nhận, khi nhấn vào link được gửi đến mail thì tài khoản mới được tạo )
+ Đăng nhập
+ Quên mật khẩu

+ Trang chủ:
  - Hiển thị danh sách file từ nhiều người dùng ( bao gồm thông tin ngày đăng tải, dung lượng, loại file)
  - Hiển thị danh sách file bản thân người dùng đăng
  - Tải file trực tiếp
  - Mở file trực tiếp từ trang web ( đối với file word convert sang pdf để hiển thị được trên iframe, pdf thì không cần)

+ Chia sẽ file lên (Chỉ cho phép các file hình ảnh và các loại file tài liệu : .docx, .pdf , dung lượng file dưới 10mb, và file sẽ được quét virus trước khi đăng nên sẽ có thời gian chờ tầm 5s)

+ Quản lí file :
   - Hiển thị file ( bao gồm thông tin ngày đăng tải, dung lượng, loại file)
   - Phân loại file( hình ảnh, file tài liệu (pdf, docs, ecel)
   - Xóa file
   - Tải về file
   - Tìm kiếm file theo tên
+ Xem profile của bản thân
  - Hiện thông tin cá nhân
  - Những file mà người dùng đó đăng
  
     
