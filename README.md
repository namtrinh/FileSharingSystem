######################## Sản phẩm cuối cùng: Cho phép người dùng chia sẽ file lên hệ thống và quản lí file Người dùng khác có thể nhìn thấy được có thể xem trực tiếp hoặc tải xuống.

####### Cách khởi động file ########

Yêu cầu sử dụng Visual studio đã cài đặt ASP.NET and web development và hệ quản trị cơ sở dữ liệu Mysql
Dowload src và mở file FileSharingSystem.sln hoặc clone src từ github
Nếu trong thư migrate có bất cứ file nào -> xóa
Vào file appsetting.json cấu hình tài khoản mysql bao gồm username và password
Tạo sẳn database có tên là filesharingdb -> không cần tạo table
Tiến hành gõ lệnh trong: Tools/NuGet Packet Manager/Packet Manager Console
Add-Migration InitialCreate
Update-Database Sau khi migrate xong có thể tiến hành khởi động dự án
####### Một số chức năng chính #######

Đăng kí tài khoản ( bao gồm gửi email xác nhận, khi nhấn vào link được gửi đến mail thì tài khoản mới được tạo )

Đăng nhập

Quên mật khẩu

Trang chủ:

Chia sẽ file: Cho phép các định dạng sau: // Image formats ".jpg", ".jpeg", ".png", ".gif",

           // Document formats
           ".pdf", ".docx", ".doc", ".xls", ".xlsx", ".txt", ".rtf", ".ppt", ".pptx", ".odt", ".ods", ".odp", 
       
           // Audio formats
           ".mp3", ".wav", ".aac", ".flac", ".ogg", ".wma", ".m4a", 
       
           // Video formats
           ".mp4", ".avi", ".mov", ".mkv", ".flv", ".wmv", ".mpeg", ".3gp", 
       
           // Compressed/archive formats
           ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2", ".xz", 
       
           // Font files
           ".ttf", ".otf", ".woff", ".woff2", 
       
           // Other common files
           ".csv", ".ics", ".apk", ".iso"
Hiển thị danh sách file từ nhiều người dùng ( bao gồm thông tin ngày đăng tải, dung lượng, loại file)

Hiển thị danh sách file bản thân người dùng đăng

Tải file trực tiếp

Mở file trực tiếp từ trang web +Lưu ý : Hiện tại các định dạng có thể mở trực tiếp -File tài liệu có định dạng .pdf, .docx, .txt -Hình ảnh -Video +Còn lại các định dạng khác cần tải về mới có thể xem

Quản lí file :

Hiển thị file ( bao gồm thông tin ngày đăng tải, dung lượng, loại file)
Phân loại file( hình ảnh, file tài liệu (pdf, docs, ecel)
Xóa file
Tải về file
Tìm kiếm file theo tên
Xem profile của bản thân

Hiện thông tin cá nhân
Những file mà người dùng đó đăng
